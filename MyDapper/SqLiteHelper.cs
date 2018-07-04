using System.Data;
using MySql.Data.MySqlClient;

namespace MyDapper
{
    public class SqLiteHelper
    {
        private const string Mysqlconnection = "server=192.168.3.100;port=3306;database=test;charset=utf8;uid=root;pwd=admin;Allow Zero Datetime=True;SslMode = none";

        public static IDbConnection OpenConnection()
        {
            IDbConnection connection = new MySqlConnection(Mysqlconnection);
            if (connection.State != ConnectionState.Closed)
            {
                connection.Close();
            }
            connection.Open();
            return connection;
        }
    }
}