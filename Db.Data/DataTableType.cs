namespace Db.Data
{
    /// <summary>
    /// 表类型
    /// </summary>
    public enum DataTableType
    {
        /// <summary>
        /// 实体表（可增删改查）
        /// </summary>
        Table,

        /// <summary>
        /// 视图（仅可用于查）
        /// </summary>
        View,

        /// <summary>
        /// 虚拟表(仅作为子查询的容器)
        /// </summary>
        Virtual
    }
}