using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using DataAccess;
using log4net;
using Model;

namespace Business
{
	public class TmsAccountPurgeSettings
	{

		public ILog Log { get; private set; }
		public DataAccessHelper Helper { get; private set; }
		public List<IncomingMsgExtract> IncomingMessageIdList { get; set; }

		public int PurgeBlockSize { get; private set; }

		public bool ReadTmsAccounts { get; private set; }
		public bool PurgeTmsAccounts { get; private set; }
		public bool ProcessIncomingMessages { get; private set; }
		public bool MarkIncomingMessages { get; private set; }
		public bool ArchivePurgeCriteria { get; private set; }
		public bool TruncateTmsPurgeCriteria { get; private set; }

		public string AccountMessage { get; set; }
		public char BeforeStatus { get; private set; }
		public char AfterStatus { get; private set; }
		public string UpdatedBy { get; private set; }

		public byte NewMessage { get; set; }
		public byte NotProcessed { get; private set; }

		public string AccountPurgeFile { get; private set; }
		public bool TruncateTable { get; private set; }

		public string PurgeTmsAccountsSP { get; private set; }
		public string MarkIncomingMessagesSP { get; private set; }

		public string CpiTmsAccountPurgeDbTable { get; private set; }
		public string CpiTmsAccountMsgIdsDbTable { get; private set; }

		public DataTable CpiTmsAccountPurge { get; private set; }
		private readonly DataColumn[] _columnsTmsAccount = { new DataColumn("[SourceId]", typeof(long)) };
		public DataTable IncomingMessageIds { get; private set; }
		private readonly DataColumn[] _columnsIncomingMessage = { new DataColumn("[IncomingMessageId]", typeof(Guid)) };

		public string TfmConnectionString { get; private set; }
		public string FenergoConnectionString { get; private set; }
		public string TfmDcConnectionString { get; private set; }

		public int CommandTimeout { get; set; }

		public TmsAccountPurgeSettings()
		{
			log4net.Config.XmlConfigurator.Configure();
			Log = LogManager.GetLogger(this.GetType());
			Helper = new DataAccessHelper();
			IncomingMessageIdList = new List<IncomingMsgExtract>(500);

			PurgeBlockSize = int.Parse(ConfigurationManager.AppSettings["PurgeBlockSize"]);

			ReadTmsAccounts = bool.Parse(ConfigurationManager.AppSettings["ReadTmsAccounts"]);
			PurgeTmsAccounts = bool.Parse(ConfigurationManager.AppSettings["PurgeTmsAccounts"]);
			ProcessIncomingMessages = bool.Parse(ConfigurationManager.AppSettings["ProcessIncomingMessages"]);
			MarkIncomingMessages = bool.Parse(ConfigurationManager.AppSettings["MarkIncomingMessages"]);
			ArchivePurgeCriteria = bool.Parse(ConfigurationManager.AppSettings["ArchivePurgeCriteria"]);
			TruncateTmsPurgeCriteria = bool.Parse(ConfigurationManager.AppSettings["TruncateTmsPurgeCriteria"]);

			AccountMessage = ConfigurationManager.AppSettings["AccountMessage"];
			BeforeStatus = char.Parse(ConfigurationManager.AppSettings["BeforeStatus"]);
			AfterStatus = char.Parse(ConfigurationManager.AppSettings["AfterStatus"]);
			UpdatedBy = ConfigurationManager.AppSettings["UpdatedBy"] + DateTime.Now.Year;

			NewMessage = byte.Parse(ConfigurationManager.AppSettings["NewMessage"]);
			NotProcessed = byte.Parse(ConfigurationManager.AppSettings["NotProcessed"]);

			AccountPurgeFile = ConfigurationManager.AppSettings["AccountPurgeFile"];
			TruncateTable = bool.Parse(ConfigurationManager.AppSettings["TruncateTable"]);

			PurgeTmsAccountsSP = ConfigurationManager.AppSettings["PurgeTmsAccountsSP"];
			MarkIncomingMessagesSP = ConfigurationManager.AppSettings["MarkIncomingMessagesSP"];

			CpiTmsAccountPurgeDbTable = ConfigurationManager.AppSettings["CpiTmsAccountPurgeDbTable"];
			CpiTmsAccountPurge = new DataTable(CpiTmsAccountPurgeDbTable);
			CpiTmsAccountPurge.Columns.AddRange(_columnsTmsAccount);

			CpiTmsAccountMsgIdsDbTable = ConfigurationManager.AppSettings["CpiTmsAccountMsgIdsDbTable"];
			IncomingMessageIds = new DataTable(CpiTmsAccountMsgIdsDbTable);
			IncomingMessageIds.Columns.AddRange(_columnsIncomingMessage);

			TfmConnectionString = ConfigurationManager.ConnectionStrings["TFM"].ConnectionString;
			FenergoConnectionString = ConfigurationManager.ConnectionStrings["FENERGO"].ConnectionString;
			TfmDcConnectionString = ConfigurationManager.ConnectionStrings["TFM_DC"].ConnectionString;

			CommandTimeout = int.Parse(ConfigurationManager.AppSettings["PurgeBlockSize"]);
		}
	}
}