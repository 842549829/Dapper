using System;
using System.Collections.Generic;

namespace Db.Data
{
    /// <summary>
    /// 解决不同数据库之间的兼容性问题的接口
    /// </summary>
    public interface IDataCompatible : IDisposable
    {
        /// <summary>
        /// 参数名前缀
        /// </summary>
        string ParameterPrefixName { get; }

        /// <summary>
        /// 用于翻页查询的Between条件的开始量变量名
        /// </summary>
        string BeginBetweenVariableName { get; }

        /// <summary>
        /// 用于翻页查询的Between条件的结束量变量名
        /// </summary>
        string EndBetweenVariableName { get; }

        /// <summary>
        /// 获取查询数据库的SQL脚本
        /// </summary>
        /// <param name="type">模型类型</param>
        /// <param name="where">查询条件</param>
        /// <param name="pager">分页器</param>
        /// <param name="columns">分页器</param>
        /// <returns>数据库查询脚本</returns>
        string GetSelectSql(Type type, string where, DataPager pager, IEnumerable<string> columns = null);

        /// <summary>
        /// 获取字段计算的SQL脚本
        /// </summary>
        /// <param name="type">模型类型</param>
        /// <param name="solver">列运算器类型</param>
        /// <param name="column">要计算的列名称</param>
        /// <param name="where">查询条件</param>
        /// <returns>数据库查询脚本</returns>
        string GetComputeSql(Type type, DataSolver solver, string column, string where);

        /// <summary>
        /// 获取空插脚本
        /// </summary>
        /// <returns>模拟插入数据行的脚本,用于强制返回受影响行数量大于0</returns>
        string GetInsertSQL();

        /// <summary>
        /// 获取用于插入记录的SQL脚本
        /// </summary>
        /// <param name="type">模型类型</param>
        /// <returns>用于插入记录的SQL脚本</returns>
        string GetInsertSQL(Type type);

        /// <summary>
        /// 获取用于删除记录的SQL脚本
        /// </summary>
        /// <param name="type">模型类型</param>
        /// <returns>用于删除记录的SQL脚本</returns>
        string GetDeleteSQL(Type type);

        /// <summary>
        /// 获取按条件删除记录的SQL脚本
        /// </summary>
        /// <param name="type">模型类型</param>
        /// <param name="where">删除条件</param>
        /// <returns>用于删除记录的SQL脚本</returns>
        string GetDeleteSQL(Type type, string where);

        /// <summary>
        /// 获取用于修改记录的SQL脚本
        /// </summary>
        /// <param name="type">模型类型</param>
        /// <param name="columnNames">要更新的字段集合</param>
        /// <param name="conditions">条件</param>
        /// <returns>用于修改记录的SQL脚本</returns>
        string GetUpdateSQL(Type type, string[] columnNames, string conditions);

        /// <summary>
        /// 获取用于还原数据库的SQL脚本
        /// </summary>
        /// <param name="backupFilename">备份文件完整路径</param>
        /// <param name="primary">前缀</param>
        /// <param name="suffix">后缀</param>
        /// <param name="destDirectory">目标数据库保存目录</param>
        /// <returns></returns>
        string GetRestoreDatabaseSQL(string backupFilename, string primary, string suffix, string destDirectory);
    }
}