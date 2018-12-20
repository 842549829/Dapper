namespace Db.Data
{
    /// <summary>
    /// 字段类型
    /// </summary>
    public enum DataColumnType
    {
        /// <summary>
        /// 值类型
        /// </summary>
        Value,

        /// <summary>
        /// 子表
        /// </summary>
        Table,

        /// <summary>
        /// 虚拟字段，用于以不同的形式显示字段的内容，如数据库中的时间戳以传统方式显示
        /// </summary>
        Virtual
    }
}