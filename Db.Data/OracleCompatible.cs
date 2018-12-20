using System;
using System.Collections.Generic;
using System.Text;

namespace Db.Data
{
    /// <summary>
    /// System.Data.OracleClient 兼容性扩展
    /// </summary>
    [DataCompatible(ProviderName = "System.Data.OracleClient")]
    public sealed class OracleCompatible : IDataCompatible
    {
        /// <summary>
        /// 
        /// </summary>
        public string ParameterPrefixName
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// 
        /// </summary>
        public string BeginBetweenVariableName
        {
            get { throw new NotImplementedException(); }
        }
        /// <summary>
        /// 
        /// </summary>
        public string EndBetweenVariableName
        {
            get { throw new NotImplementedException(); }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="where"></param>
        /// <param name="pager"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public string GetSelectSql(Type type, string where, DataPager pager, IEnumerable<string> columns = null)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="solver"></param>
        /// <param name="column"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public string GetComputeSql(Type type, DataSolver solver, string column, string where)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetInsertSQL()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetInsertSQL(Type type)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetDeleteSQL(Type type)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public string GetDeleteSQL(Type type, string where)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="columnNames"></param>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public string GetUpdateSQL(Type type, string[] columnNames, string conditions)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="backupFilename"></param>
        /// <param name="primary"></param>
        /// <param name="suffix"></param>
        /// <param name="destDirectory"></param>
        /// <returns></returns>
        public string GetRestoreDatabaseSQL(string backupFilename, string primary, string suffix, string destDirectory)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
