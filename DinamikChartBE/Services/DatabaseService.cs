using DinamikChartBE.Interfaces;
using DinamikChartBE.Models;
using System.Data.SqlClient;

namespace DinamikChartBE.Services
{
    public class DatabaseService : IDatabaseService
    {
        public bool TestAndSaveConnection(ConnectionDetails connectionDetails, ISession session)
        {
            string connectionString = BuildConnectionString(connectionDetails);
            if (TestDatabaseConnection(connectionString))
            {
                session.SetString("Server", connectionDetails.Server);
                session.SetString("Username", connectionDetails.Username);
                session.SetString("Password", connectionDetails.Password);
                session.SetString("Database", connectionDetails.DatabaseName);
                return true;
            }

            return false;
        }

        public List<string> RetrieveStoredProcedures(ISession session)
        {
            string query = "SELECT name AS ProcedureName FROM sys.procedures";
            return ExecuteQuery(session, query);
        }

        public List<dynamic> ExecuteStoredProcedure(string procedureIdentifier, ISession session)
        {
            string query = $"EXEC {procedureIdentifier}";
            return ExecuteDynamicQuery(session, query);
        }

        public List<string> RetrieveViews(ISession session)
        {
            string query = "SELECT name AS ViewName FROM sys.views";
            return ExecuteQuery(session, query);
        }

        public List<dynamic> ExecuteViewQuery(string viewIdentifier, ISession session)
        {
            string query = $"SELECT * FROM {viewIdentifier}";
            return ExecuteDynamicQuery(session, query);
        }

        private string BuildConnectionString(ConnectionDetails connectionDetails)
        {
            return $"Server={connectionDetails.Server}; Database={connectionDetails.DatabaseName}; User Id={connectionDetails.Username}; Password={connectionDetails.Password};";
        }

        private bool TestDatabaseConnection(string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        private List<string> ExecuteQuery(ISession session, string query)
        {
            var result = new List<string>();
            string connectionString = GetConnectionStringFromSession(session);

            if (string.IsNullOrEmpty(connectionString))
                return result;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                result.Add(reader[0].ToString());
                            }
                        }
                    }
                }
            }
            catch
            {
                return result;
            }

            return result;
        }

        private List<dynamic> ExecuteDynamicQuery(ISession session, string query)
        {
            var result = new List<dynamic>();
            string connectionString = GetConnectionStringFromSession(session);

            if (string.IsNullOrEmpty(connectionString))
                return null;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var row = new
                                {
                                    Column1 = reader[0],
                                    Column2 = reader.FieldCount > 1 ? reader[1] : null
                                };
                                result.Add(row);
                            }
                        }
                    }
                }
            }
            catch
            {
                return null;
            }

            return result;
        }

        private string GetConnectionStringFromSession(ISession session)
        {
            string server = session.GetString("Server");
            string username = session.GetString("Username");
            string password = session.GetString("Password");
            string database = session.GetString("Database");

            if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(database))
            {
                return null;
            }

            return $"Server={server}; Database={database}; User Id={username}; Password={password};";
        }
    }
}
