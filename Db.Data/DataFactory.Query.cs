using System;
using System.Linq;
using System.Linq.Expressions;

namespace Db.Data
{
    public partial class DataFactory
    {
        private readonly IDataCompatible dataCompatible;

        public DataFactory(string providerName)
        {
            if (providerName == "")
            {
                dataCompatible = new MySqlCompatible();
            }
            else
            {
                dataCompatible = new SqlServerCompatible();
            }
        }

        public string GetDataPagerSql<T>(DataPager pager, Expression<Func<T, bool>> predicate,
            params Expression<Func<T, string>>[] columnPredicates)
        {
            var columns = ParsePredicate(columnPredicates);
            var parser = LambdaHelper.Parse(dataCompatible, predicate);
            if (parser != null)
            {
                return GetDataPagerSql<T>(parser.SqlString, pager, columns, parser.Parameters.ToArray());
            }

            return GetDataPagerSql<T>(columns);
        }

        /// <summary>
        /// 读取全部数据集
        /// </summary>
        /// <param name="columns">要查询的字段</param>
        /// <returns>全部数据集</returns>
        public string GetDataPagerSql<T>(string[] columns = null)
        {
            return GetDataPagerSql<T>(null, null, columns);
        }

        /// <summary>
        /// 根据条件忽略缓存读取数据集
        /// </summary>
        /// <param name="whereStr">条件</param>
        /// <param name="columns">要查询的字段</param>
        /// <param name="parameters">条件采用的参数</param>
        /// <param name="pager">分页器</param>
        /// <returns>结果数据集</returns>
        public string GetDataPagerSql<T>(string whereStr, DataPager pager, string[] columns,
            params DataParameter[] parameters)
        {
            var tableType = typeof(T);
            var sql = dataCompatible.GetSelectSql(tableType, whereStr, pager, columns);
            return sql;
        }
    }
}