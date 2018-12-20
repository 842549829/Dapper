using System.Collections.Generic;

namespace Db.Data
{
    /// <summary>
    /// Lambda表达式解析器
    /// </summary>
    internal interface ILambdaParser
    {
        /// <summary>
        /// Lambda表达式解析结果SQL语句
        /// </summary>
        string SqlString { get; }

        /// <summary>
        /// Lambda表达式解析结果参数集
        /// </summary>
        IEnumerable<DataParameter> Parameters { get; }
    }
}