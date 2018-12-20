using System;

namespace Db.Data
{
    /// <summary>
    /// 数据表特征
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class DataTableAttribute : Attribute
    {
        /// <summary>
        /// 是否启用缓存
        /// </summary>
        public bool Cache { get; set; }

        /// <summary>
        /// 缓存时间
        /// </summary>
        public int CacheExpires { get; set; }

        /// <summary>
        /// 表名称
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { set; get; }

        /// <summary>
        /// 字段类型 Table=实体表（可增删改查）View=视图（仅可用于查） Virtual=虚拟表(仅作为子查询的容器)
        /// </summary>
        public DataTableType TableType { get; set; } = DataTableType.Table;
    }
}