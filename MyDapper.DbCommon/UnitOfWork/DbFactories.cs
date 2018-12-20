using System;
using System.Data;
using System.Data.SqlClient;

namespace MyDapper.DbCommon.UnitOfWork
{
    /// <summary>
    /// 创建数据链接工厂
    /// </summary>
    public static class DbFactories
    {
        /// <summary>
        /// 创建数据链接
        /// </summary>
        /// <param name="name">
        /// 链接字符名称
        /// </param>
        /// <param name="parameters">parameters</param>
        /// <returns>
        /// The IDbConnection<see cref="IDbConnection"/>.
        /// </returns>
        public static DbFactoriesResult GetConnection(string name, params object[] parameters)
        {
            // 读取配置文件
            //IConfiguration configuration = Configuration.GetConfigurationRootByJson("appsetion.json");
            //DbConnection conn = new SqlConnection(configuration["ConnectionString"]);
            IDbConnection conn;
            var providerName = "system.data.sqlclient";
            var connectionString = string.Format("Data Source=192.168.100.142;Initial Catalog=test;uid=hpadmin;pwd=cdhpadmin2013;MultipleActiveResultSets=True", parameters);
            switch (providerName.ToLower())
            {
                case "mysql.data.mysqlclient":
                    throw new NotImplementedException();
                case "oracle.data.oracleclient":
                    throw new NotImplementedException();
                case "access.data.accessclient":
                    throw new NotImplementedException();
                default:
                    conn = new SqlConnection(connectionString);
                    break;
            }

            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            return new DbFactoriesResult
            {
                DbConnection = conn,
                ProviderName = providerName
            };
        }
    }

    public class DbFactoriesResult
    {
        public IDbConnection DbConnection { get; set; }

        public string ProviderName { get; set; }
    }
}
