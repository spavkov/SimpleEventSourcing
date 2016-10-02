using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventSourcing.Library.Persistence;
using EventSourcing.Library.Persistence.Exceptions;

namespace EventSourcing.Persistence.SqlServerCompact
{
    public class SqlServerCompactAppendOnlyStore : IAppendOnlyStore
    {
        private string connectionString;

        private bool EventStoreTableExists(SqlCeConnection connection, string tableName)
        {
            using (var command = new SqlCeCommand())
            {
                command.Connection = connection;
                var sql = string.Format("SELECT COUNT(*) FROM information_schema.tables WHERE table_name = '{0}'", tableName);
                command.CommandText = sql;
                var count = Convert.ToInt32(command.ExecuteScalar());
                return (count > 0);
            }
        }

        public void Initialize(Dictionary<string, string> configuration)
        {
            string databasePath, databaseName;
            if (!configuration.TryGetValue("DatabasePath", out databasePath))
            {
                throw new Exception("Could not find DatabasePath in EventStore configuration");
            }

            if (!configuration.TryGetValue("DatabaseName", out databaseName))
            {
                throw new Exception("Could not find DatabasePath in EventStore configuration");
            }

            if (!databaseName.Contains("."))
            {
                databaseName = databaseName + ".sdf";
            }

            var dbFile = Path.Combine(databasePath, databaseName);
            connectionString = "DataSource=\"" + dbFile + "\";";
            if (!File.Exists(dbFile))
            {
                using (var engine = new SqlCeEngine(connectionString))
                {
                    engine.CreateDatabase();
                }
            }

            using (var conn = new SqlCeConnection(connectionString))
            {
                conn.Open();

                if (!EventStoreTableExists(conn, "Events"))
                {
                    const string txt =
                        @"CREATE TABLE Events
                            ([Id] [int] PRIMARY KEY IDENTITY,
	                        [Name] [nvarchar](50) NOT NULL,
	                        [Version] [int] NOT NULL,
	                        [Data] [image] NOT NULL)";

                    using (var cmd = new SqlCeCommand(txt, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public void Append(string name, byte[] data, int expectedVersion = -1)
        {
            using (var conn = new SqlCeConnection(connectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    const string sql = @"SELECT MAX(Version) FROM Events WHERE Name=@name";

                    int version = 0;
                    using (var cmd = new SqlCeCommand(sql, conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@name", name);
                        var result = cmd.ExecuteScalar();
                        if (result.GetType() != typeof(DBNull))
                        {
                            version = (int)result;
                            if (expectedVersion >= 0)
                            {
                                if (version != expectedVersion)
                                {
                                    throw new AppendOnlyStoreConcurrencyException(version, expectedVersion, name);
                                }
                            }
                        }
                    }
                    const string txt = @"INSERT INTO Events (Name, Version, Data) VALUES (@name, @version, @data)";

                    using (var cmd = new SqlCeCommand(txt, conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@version", version + 1);
                        cmd.Parameters.AddWithValue("@data", data);
                        cmd.ExecuteNonQuery();
                    }
                    tx.Commit();
                }
            }
        }

        public IEnumerable<DataWithVersion> ReadRecords(string name, long afterVersion, int maxCount)
        {
            using (var conn = new SqlCeConnection(connectionString))
            {
                conn.Open();
                const string sql = @"SELECT TOP (@take) Data, Version FROM Events WHERE Name = @p1 AND Version > @skip ORDER BY Version";
                using (var cmd = new SqlCeCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@p1", name);
                    cmd.Parameters.AddWithValue("@take", maxCount);
                    cmd.Parameters.AddWithValue("@skip", afterVersion);


                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var data = (byte[])reader["Data"];
                            var version = (int)reader["Version"];
                            yield return new DataWithVersion(version, data);
                        }
                    }
                }
            }
        }

        public IEnumerable<DataWithName> ReadRecords(long afterVersion, int maxCount)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                const string sql = @"SELECT TOP (@take) Data, Name FROM Events WHERE Id > @skip ORDER BY Id";
                using (var cmd = new SqlCommand(sql, conn))
                {

                    cmd.Parameters.AddWithValue("@take", maxCount);
                    cmd.Parameters.AddWithValue("@skip", afterVersion);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var data = (byte[])reader["Data"];
                            var name = (string)reader["Name"];
                            yield return new DataWithName(name, data);
                        }
                    }
                }
            }
        }
    }
}
