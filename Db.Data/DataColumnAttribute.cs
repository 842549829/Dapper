using System;

namespace Db.Data
{
    /// <summary>
    /// 数据列特征
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DataColumnAttribute : Attribute
    {
        /// <summary>
        /// 是否是标识列
        /// </summary>
        public bool Identity { get; set; }

        /// <summary>
        /// 是否是主键
        /// </summary>
        public bool PrimaryKey { get; set; }

        /// <summary>
        /// 是否是索引列
        /// </summary>
        public bool Index { get; set; }

        /// <summary>
        /// 是否是外键列
        /// </summary>
        public bool ForeignKey { get; set; }

        /// <summary>
        /// 主表主键列完整名称
        ///     ex.  [table1].[field1]
        /// </summary>
        public string MasterFullColumnName { get; set; }

        /// <summary>
        /// 字段描述
        /// </summary>
        public string Description { set; get; }

        /// <summary>
        /// 字段大小
        /// </summary>
        public int MaxLength { set; get; }

        /// <summary>
        /// 固定大小
        /// </summary>
        public bool FixedLength { set; get; }

        /// <summary>
        /// 数据库中字段的名称
        /// </summary>
        public string ColumnName { set; get; }

        /// <summary>
        /// 字段类型 Value=值类型,Table=子表类型
        /// </summary>
        public DataColumnType ColumnType { get; set; } = DataColumnType.Value;
    }
}