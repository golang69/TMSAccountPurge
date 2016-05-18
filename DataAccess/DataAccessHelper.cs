using System.Data;
using System.Data.SqlClient;

namespace DataAccess
{
	public class DataAccessHelper
	{
		public void BulkCopy(DataTable dotNetTable, string databaseTable, bool truncateTable, string connectionString)
		{
			if (truncateTable)
			{
				using (var connection = new SqlConnection(connectionString))
				{
					var sql = string.Format("TRUNCATE TABLE {0}", databaseTable);

					using (var sqlCommand = new SqlCommand(sql, connection))
					{
						connection.Open();
						sqlCommand.ExecuteNonQuery();
					}
				}
			}

			using (var connection = new SqlConnection(connectionString))
			{
				using (var bulkCopy = new SqlBulkCopy(connectionString, SqlBulkCopyOptions.UseInternalTransaction))
				{
					bulkCopy.DestinationTableName = databaseTable;
					connection.Open();
					bulkCopy.WriteToServer(dotNetTable);
				}
			}
		}
	}
}