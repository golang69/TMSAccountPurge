using System;
using System.Data;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Transactions;
using Model;

namespace Business
{
    public class TMSAccountService
    {
        private readonly TmsAccountPurgeSettings _settings;

        public TMSAccountService(TmsAccountPurgeSettings settings)
        {
            _settings = settings;
        }

        public void ProcessTmsAccounts()
        {
            if (_settings.TruncateTmsPurgeCriteria) TruncateTmsPurgeCriteria();

            if (_settings.ReadTmsAccounts) ReadInTmsAccounts();
            if (_settings.PurgeTmsAccounts) PurgeTmsAccounts();
            if (_settings.ProcessIncomingMessages) ProcessIncomingMessages();

            if (_settings.ArchivePurgeCriteria) ArchivePurgeCriteria();
        }

        private void ReadInTmsAccounts()
        {
            var myStopWatch = Stopwatch.StartNew();
            using (var reader = new StreamReader(_settings.AccountPurgeFile))
            {
                while (true)
                {
                    var line = reader.ReadLine();
                    if (line == null) break;
                    _settings.CpiTmsAccountPurge.Rows.Add(new object[] { long.Parse(line.TrimStart('0'), NumberStyles.Any) });
                }
            }

            _settings.CpiTmsAccountPurge.AcceptChanges();
            _settings.Helper.BulkCopy(_settings.CpiTmsAccountPurge, _settings.CpiTmsAccountPurgeDbTable,
                _settings.TruncateTable, _settings.TfmDcConnectionString);
            myStopWatch.Stop();
            _settings.Log.Info(String.Format("[{0}] TMS Accounts loaded in {1:N0} seconds", _settings.CpiTmsAccountPurge.Rows.Count,
                myStopWatch.Elapsed.TotalSeconds));
            _settings.CpiTmsAccountPurge.Clear();
        }

        private void PurgeTmsAccounts()
        {
            var myStopWatch = Stopwatch.StartNew();
            using (var connection = new SqlConnection(_settings.FenergoConnectionString))
            {
                var command = new SqlCommand { CommandText = _settings.PurgeTmsAccountsSP, Connection = connection, CommandType = CommandType.StoredProcedure };
                var beforeStatusParameter = new SqlParameter("@beforeStatus", dbType: SqlDbType.Char, size: 1);
                var afterStatusParameter = new SqlParameter("@afterStatus", dbType: SqlDbType.Char, size: 1);
                var updatedByParameter = new SqlParameter("@updatedBy", dbType: SqlDbType.NVarChar, size: 50);

                beforeStatusParameter.Value = _settings.BeforeStatus;
                afterStatusParameter.Value = _settings.AfterStatus;
                updatedByParameter.Value = _settings.UpdatedBy;

                command.Parameters.AddRange(values: new SqlParameter[] { beforeStatusParameter, afterStatusParameter, updatedByParameter });
                connection.Open();

                var purged = command.ExecuteNonQuery();
                myStopWatch.Stop();
                _settings.Log.Info(String.Format("[{0}] TMS Accounts marked as purged in {1:N0} seconds", purged, myStopWatch.Elapsed.TotalSeconds));
            }
        }

        private void ProcessIncomingMessages()
        {
            var messagesMarked = 0;
            var messagesScanned = 0;

            var createdOn = GetOldestAccountMessage();

            while (true)
            {
                using (var t = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
                {
                    using (var context = new TFMEntities())
                    {
                        ((IObjectContextAdapter)context).ObjectContext.CommandTimeout = _settings.CommandTimeout;
                        _settings.IncomingMessageIdList = (context.IncomingMessages.Where(msg => msg.Format == _settings.AccountMessage &&
                                                                                                 msg.CreatedOn > createdOn &&
                                                                                                 msg.MessageStatusId == _settings.NewMessage)
                                                                                    .OrderBy(msg => msg.CreatedOn)
                                                                                    .Select(msg => new IncomingMsgExtract
                                                                                    {
                                                                                        IncomingMessageId = msg.IncomingMessageId,
                                                                                        CreatedOn = msg.CreatedOn,
                                                                                        XmlMessage = msg.XmlMessage
                                                                                    })
                                                                                    .Take(_settings.PurgeBlockSize)
                                                                                    .OrderBy(msg => msg.CreatedOn)).ToList();
                    }
                }

                if (_settings.IncomingMessageIdList.Count > 0)
                {
                    var item = _settings.IncomingMessageIdList[_settings.IncomingMessageIdList.Count - 1];
                    createdOn = item.CreatedOn;
                    messagesScanned += _settings.IncomingMessageIdList.Count;
                }
                else
                {
                    Thread.Sleep(5000);
                    _settings.Log.Info(String.Format("No TMS account candidate messages found."));
                    _settings.Log.Info(String.Format("[{0}] Incoming account messages scanned.", messagesScanned));
                    _settings.Log.Info(String.Format("[{0}] Incoming account messages marked with {1}", messagesMarked, _settings.NotProcessed));
                    createdOn = GetOldestAccountMessage();
                    continue;
                }

                foreach (var msg in _settings.IncomingMessageIdList)
                {
                    if (msg.XmlMessage.Contains(@"AcctStatus=""P"""))
                    {
                        _settings.IncomingMessageIds.Rows.Add(new object[] { msg.IncomingMessageId });
                    }
                }

                if (_settings.IncomingMessageIds.Rows.Count > 0)
                {
                    var myStopWatch = Stopwatch.StartNew();
                    _settings.IncomingMessageIds.AcceptChanges();
                    _settings.Helper.BulkCopy(_settings.IncomingMessageIds, _settings.CpiTmsAccountMsgIdsDbTable, false, _settings.TfmDcConnectionString);

                    if (_settings.MarkIncomingMessages) messagesMarked += UpdateIncomingMessages(_settings.IncomingMessageIds);
                    myStopWatch.Stop();
                    _settings.Log.Info(String.Format("[{0}] Purge messages loaded in {1:N0} seconds", _settings.IncomingMessageIds.Rows.Count, myStopWatch.Elapsed.TotalSeconds));
                    Console.WriteLine("\t[{0}] Purge messages loaded in {1:N0} seconds", _settings.IncomingMessageIds.Rows.Count, myStopWatch.Elapsed.TotalSeconds);
                }

                _settings.IncomingMessageIds.Clear();
            }
        }

        private int UpdateIncomingMessages(DataTable messageIds)
        {
            using (var connection = new SqlConnection(_settings.TfmConnectionString))
            {
                var command = new SqlCommand { CommandText = _settings.MarkIncomingMessagesSP, Connection = connection, CommandType = CommandType.StoredProcedure };

                var messageIdsParameter = new SqlParameter("@incomingMessageIds", dbType: SqlDbType.Structured);
                var messageStatusIdParameter = new SqlParameter("@messageStatusId", dbType: SqlDbType.TinyInt);

                messageIdsParameter.Value = messageIds;
                messageStatusIdParameter.Value = _settings.NotProcessed;

                command.Parameters.AddRange(values: new SqlParameter[] { messageIdsParameter, messageStatusIdParameter });
                connection.Open();

                return command.ExecuteNonQuery();
            }
        }

        private DateTime GetOldestAccountMessage()
        {
            var returnDate = DateTime.MinValue;

            using (var context = new TFMEntities())
            {
                var start = context.IncomingMessages.Where(msg => msg.Format == _settings.AccountMessage && msg.MessageStatusId == _settings.NewMessage)
                                                    .OrderBy(msg => msg.CreatedOn)
                                                    .FirstOrDefault();

                if (start != null) returnDate = start.CreatedOn;
            }
            return returnDate;
        }

        #region Initialize
        private void TruncateTmsPurgeCriteria()
        {
            using (var connection = new SqlConnection(_settings.TfmDcConnectionString))
            {
                var sql = new StringBuilder();
                var accountIdTableParam = new SqlParameter("@CpiTmsAccountPurge", _settings.CpiTmsAccountPurgeDbTable);
                var accountMsgIdTableParam = new SqlParameter("@CpiTmsAccountMsgIds", _settings.CpiTmsAccountMsgIdsDbTable);

                sql.Append("TRUNCATE TABLE ").Append(accountIdTableParam.Value).Append("; TRUNCATE TABLE ").Append(accountMsgIdTableParam.Value).Append(";");

                using (var sqlCommand = new SqlCommand(sql.ToString(), connection))
                {
                    connection.Open();
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }
        #endregion

        #region Archival
        private void ArchivePurgeCriteria()
        {
            using (var connection = new SqlConnection(_settings.TfmDcConnectionString))
            {
                var sql = new StringBuilder();
                var command = new SqlCommand { Connection = connection };

                var accountIdTableParam = new SqlParameter("@CpiTmsAccountPurge", _settings.CpiTmsAccountPurgeDbTable);
                var accountIdArchiveTableParam = new SqlParameter("@CpiTmsAccountPurgeArchive", string.Concat(_settings.CpiTmsAccountPurgeDbTable, DateTime.Now.Year));

                sql.Append("SELECT * INTO ").Append(accountIdArchiveTableParam.Value).Append(" FROM ").Append(accountIdTableParam.Value);

                command.CommandText = sql.ToString();
                command.Connection = connection;
                connection.Open();
                var rows = command.ExecuteNonQuery();
                _settings.Log.Info(String.Format("[{0}] TMS Accounts archived to {1}", rows, accountIdArchiveTableParam.Value));
            }

            using (var connection = new SqlConnection(_settings.TfmDcConnectionString))
            {
                var sql = new StringBuilder();
                var command = new SqlCommand { Connection = connection };

                var accountMsgIdTableParam = new SqlParameter("@CpiTmsAccountMsgIds", _settings.CpiTmsAccountMsgIdsDbTable);
                var accountMsgIdArchiveTableParam = new SqlParameter("@CpiTmsAccountMsgIdsArchive", string.Concat(_settings.CpiTmsAccountMsgIdsDbTable, DateTime.Now.Year));

                sql.Append("SELECT * INTO ").Append(accountMsgIdArchiveTableParam.Value).Append(" FROM ").Append(accountMsgIdTableParam.Value);

                command.CommandText = sql.ToString();
                command.Connection = connection;
                connection.Open();
                var rows = command.ExecuteNonQuery();
                _settings.Log.Info(String.Format("[{0}] TMS Account message Ids archived to {1}", rows, accountMsgIdArchiveTableParam.Value));
            }
        }
        #endregion

    }
}