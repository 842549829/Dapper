using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Db.Data
{
    public partial class DataFactory : IDisposable
    {
        /// <summary>
        /// 解析lambda
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicates">lambda集合</param>
        /// <returns>field</returns>
        public static List<string> ParseColumnPredicates<T>(params Expression<Func<T, string>>[] predicates)
        {
            List<string> columns = new List<string>(predicates.Length * 8);
            foreach (Expression<Func<T, string>> exp in predicates)
            {
                string[] _columns = LambdaHelper.Parse(exp).SqlString.Split(',');
                columns.AddRange(_columns);
            }
            return columns;
        }

        public  string ParseColumnPredicates<T>(Expression<Func<T, bool>> predicates)
        {
            return LambdaHelper.Parse(dataCompatible, predicates).SqlString;
        }

        /// <summary>
        /// 解析lambda
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicates">lambda集合</param>
        /// <returns></returns>
        public string[] ParsePredicate<T>(params Expression<Func<T, string>>[] predicates)
        {
            List<string> columns = new List<string>(predicates.Length * 8);
            foreach (Expression<Func<T, string>> exp in predicates)
            {
                string[] _columns = LambdaHelper.Parse(dataCompatible, exp).SqlString.Split(',');
                columns.AddRange(_columns);
            }
            return columns.ToArray();
        }

        /// <summary>
        /// 释放托管资源
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
