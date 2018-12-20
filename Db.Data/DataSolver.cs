namespace Db.Data
{
    /// <summary>
    /// 列运算器类型
    /// </summary>
    public enum DataSolver
    {
        /// <summary>
        /// 求和
        /// </summary>
        Sum,

        /// <summary>
        /// 平均值
        /// </summary>
        Avg,

        /// <summary>
        /// 最大值
        /// </summary>
        Max,

        /// <summary>
        /// 最小值
        /// </summary>
        Min,

        /// <summary>
        /// 行数统计
        /// </summary>
        Count
    }
}