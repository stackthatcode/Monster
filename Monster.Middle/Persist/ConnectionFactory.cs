using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Push.Foundation.Utilities.Helpers;

namespace Monster.Middle.Persist
{
    public class ConnectionFactory
    {
        private readonly string _connectionString;
        public static string ConnectionStringOverride = null;

        public ConnectionFactory()
        {
            _connectionString =

                !ConnectionStringOverride.IsNullOrEmpty()

                    ? ConnectionStringOverride

                    : ConfigurationManager
                        .ConnectionStrings["DefaultConnection"]
                        .ConnectionString;
        }

        public IDbConnection Make()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
