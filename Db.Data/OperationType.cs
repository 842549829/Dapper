namespace Db.Data
{
    /// <summary>
    /// 字段运算类型
    /// </summary>
    public enum OperationType
    {
        /// <summary>
        /// 加
        /// </summary>
        Add,

        /// <summary>
        /// 减
        /// </summary>
        Subtract,

        /// <summary>
        /// 乘
        /// </summary>
        Multiply,

        /// <summary>
        /// 除
        /// </summary>
        Divide,

        /// <summary>
        /// 模
        /// </summary>
        Mod,

        /// <summary>
        /// 余
        /// </summary>
        Remainder,

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