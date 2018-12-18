using System.Data;
using MySql.Data.MySqlClient;

namespace MyDapper
{
    public class SqLiteHelper
    {
        public const string MysqlConnection = "server=192.168.3.100;port=3306;database=test;charset=utf8;uid=root;pwd=admin;Allow Zero Datetime=True;SslMode = none";

        public const string SqlServerConnection = "Data Source=192.168.100.142;Initial Catalog=test;uid=hpadmin;pwd=cdhpadmin2013;MultipleActiveResultSets=True";

        public static IDbConnection OpenConnection()
        {
            IDbConnection connection = new MySqlConnection(MysqlConnection);
            if (connection.State != ConnectionState.Closed)
            {
                connection.Close();
            }
            connection.Open();
            return connection;
        }
    }
}