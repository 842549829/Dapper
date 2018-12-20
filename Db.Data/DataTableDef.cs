using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Db.Data
{
    /// <summary>
    /// 数据表结构关系
    /// </summary>
    public sealed class DataTableDef
    {
        private static readonly object sync = new object();
        private static readonly IDictionary<Type, DataTableDef> MappingTable = new Dictionary<Type, DataTableDef>(100);

        private List<DataColumnDef> _PrimaryKeyColumns;
        private List<DataColumnDef> m_chainForeignKeyColumns;

        /// <summary>
        /// 表链外键字段映射
        /// </summary>
        private readonly Dictionary<string, List<DataColumnDef>> m_chainForeignKeyColumnsMapping =
            new Dictionary<string, List<DataColumnDef>>();

        private List<DataColumnDef> m_children;

        /// <summary>
        /// 数据表结构关系
        /// </summary>
        /// <param name="type">模型类型</param>
        private DataTableDef(Type type)
        {
            Type = type;
            TableName = Name = type.Name;
            var chain = new List<DataTableDef>();
            var atts = type.GetCustomAttributes(typeof(DataTableAttribute), false);
            if (atts.Length > 0)
            {
                if (atts[0] is DataTableAttribute attr)
                {
                    Cache = attr.Cache;
                    CacheExpires = attr.CacheExpires;
                    Description = attr.Description;
                    TableType = attr.TableType;
                    if (!string.IsNullOrWhiteSpace(attr.TableName))
                    {
                        TableName = attr.TableName;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(Description))
            {
                atts = type.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (atts.Length > 0)
                {
                    if (atts[0] is DescriptionAttribute attr)
                    {
                        Description = attr.Description;
                    }
                }
            }

            if (type.BaseType != typeof(object) && !type.BaseType.IsInterface /* && !type.BaseType.IsAbstract */)
            {
                BaseTable = GetDataTable(type.BaseType);
                chain.AddRange(BaseTable.TableChain);
            }

            chain.Add(this);
            TableChain = chain;
        }

        /// <summary>
        /// 完整表链中的外键集合
        /// </summary>
        public List<DataColumnDef> ChainForeignKeyColumns
        {
            get
            {
                if (m_chainForeignKeyColumns == null)
                {
                    var table = this;
                    m_chainForeignKeyColumns = new List<DataColumnDef>();
                    while (table != null)
                    {
                        m_chainForeignKeyColumns.AddRange(table.ForeignKeyColumns);
                        table = table.BaseTable;
                    }
                }

                return m_chainForeignKeyColumns;
            }
        }

        /// <summary>
        /// 类型名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 表名称
        /// </summary>
        public string TableName { get; }

        /// <summary>
        /// 表描述
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// 表类型
        /// </summary>
        public DataTableType TableType { get; }

        /// <summary>
        /// 类类型
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// 基表
        /// </summary>
        public DataTableDef BaseTable { get; }

        /// <summary>
        /// 是否启用缓存
        /// </summary>
        public bool Cache { get; }

        /// <summary>
        /// 缓存时间
        /// </summary>
        public int CacheExpires { get; }

        /// <summary>
        /// 自增标识字段
        /// </summary>
        public DataColumnDef IdentityColumn { get; private set; }

        /// <summary>
        /// 完整表链字段集合
        /// </summary>
        public Dictionary<string, DataColumnDef> ColumnMapping { get; private set; }

        /// <summary>
        /// 属性字段集合
        /// </summary>
        public Dictionary<string, DataColumnDef> ColumnDictionary { get; private set; }

        /// <summary>
        /// 完整表链字段集合
        /// </summary>
        public ICollection<string> ColumnNames
        {
            get
            {
                return ColumnMapping.Values.Where(column => column.ColumnType == DataColumnType.Value).ToList()
                    .ConvertAll(column => column.FullColumnName);
            }
        }

        /// <summary>
        /// 完整表链字段集合
        /// </summary>
        public ICollection<DataColumnDef> Columns => ColumnMapping.Values;

        /// <summary>
        /// 本地字段集合
        /// </summary>
        public List<DataColumnDef> DeclaredOnlyColumns { get; private set; }

        /// <summary>
        /// 字段列远程本地名称映射表
        /// </summary>
        public Dictionary<string, string> EditableColumns { get; private set; }

        /// <summary>
        /// 主键字段集合
        /// </summary>
        public List<DataColumnDef> PrimaryKeyColumns
        {
            get
            {
                if (TableType == DataTableType.Virtual && BaseTable != null)
                {
                    return BaseTable.PrimaryKeyColumns;
                }

                return _PrimaryKeyColumns;
            }
            private set => _PrimaryKeyColumns = value;
        }

        /// <summary>
        /// 外键字段集合
        /// </summary>
        public List<DataColumnDef> ForeignKeyColumns { get; private set; }

        /// <summary>
        /// 数据表继承链
        /// </summary>
        public List<DataTableDef> TableChain { get; }

        /// <summary>
        /// 子表
        /// </summary>
        public List<DataColumnDef> Children
        {
            get
            {
                var children = new List<DataColumnDef>();
                children.AddRange(m_children);
                if (BaseTable != null)
                {
                    children.AddRange(BaseTable.Children);
                }

                return children;
            }
        }

        /// <summary>
        /// 字段列解析
        /// </summary>
        /// <param name="type">模型类型</param>
        private void ParseColumns(Type type)
        {
            var foreignKeyColumns = new List<DataColumnDef>();
            var primaryKeyColumns = new List<DataColumnDef>();
            var columnDictionary = new Dictionary<string, DataColumnDef>();
            var columnMapping = new Dictionary<string, DataColumnDef>();
            var declaredOnlyColumns = new List<DataColumnDef>();
            //PropertyInfo[] properties = type.GetProperties(BindingFlags.DeclaredOnly);
            var members = new List<MemberInfo>();
            members.AddRange(type.GetProperties(BindingFlags.Public | BindingFlags.Instance));
            members.AddRange(type.GetFields(BindingFlags.Public | BindingFlags.Instance));
            foreach (var memberInfo in members)
            {
                var table = GetDataTable(memberInfo.DeclaringType);
                var column = new DataColumnDef(memberInfo);
                column.DefType = type;
                if (table.TableType != DataTableType.Virtual && column.ColumnType == DataColumnType.Value)
                {
                    columnDictionary[column.Name] = column;
                }

                if (column.ColumnType == DataColumnType.Table || table.TableType != DataTableType.Virtual &&
                    column.ColumnType != DataColumnType.Virtual)
                {
                    columnMapping[column.FullColumnName.ToLower()] = column;
                    if (memberInfo.DeclaringType.Equals(type))
                    {
                        declaredOnlyColumns.Add(column);
                        if (column.Identity)
                        {
                            IdentityColumn = column;
                        }

                        if (column.PrimaryKey)
                        {
                            primaryKeyColumns.Add(column);
                        }

                        if (column.ForeignKey)
                        {
                            foreignKeyColumns.Add(column);
                        }
                    }
                }
            }

            PrimaryKeyColumns = primaryKeyColumns;
            ForeignKeyColumns = foreignKeyColumns;
            DeclaredOnlyColumns = declaredOnlyColumns;
            ColumnMapping = columnMapping;
            ColumnDictionary = columnDictionary;
            GenerateColumnsMap();
        }

        /// <summary>
        /// 生成字段映射
        /// </summary>
        private void GenerateColumnsMap()
        {
            EditableColumns = new Dictionary<string, string>();
            m_children = new List<DataColumnDef>();
            foreach (var column in DeclaredOnlyColumns)
            {
                if (column.ColumnType == DataColumnType.Value)
                {
                    if (!column.Identity)
                    {
                        EditableColumns[column.ColumnName] = column.Name;
                    }
                }
                else if (column.ColumnType == DataColumnType.Table)
                {
                    m_children.Add(column);
                }
            }
        }

        /// <summary>
        /// 获取表结构
        /// </summary>
        /// <param name="type">模型类型</param>
        /// <returns>表结构</returns>
        public static DataTableDef GetDataTable(Type type)
        {
            lock (sync)
            {
                DataTableDef table;
                if (!MappingTable.TryGetValue(type, out table))
                {
                    table = new DataTableDef(type);
                    MappingTable[type] = table;
                    table.ParseColumns(type);
                }

                return table;
            }
        }

        /// <summary>
        /// 查找表链中对应主表的外键集合
        /// </summary>
        /// <param name="keyTable">主表</param>
        /// <returns>主键列的定义</returns>
        public List<DataColumnDef> FindChainForeignKeyColumnsInChain(DataTableDef keyTable)
        {
            if (!m_chainForeignKeyColumnsMapping.TryGetValue(keyTable.TableName, out var fkColumns))
            {
                var chainForeignKeyColumns = ChainForeignKeyColumns;
                var table = keyTable;
                fkColumns = new List<DataColumnDef>();
                while (table != null)
                {
                    var table1 = table;
                    fkColumns.AddRange(chainForeignKeyColumns.Where(fk => fk.MasterFullColumnName.StartsWith(table1.TableName + ".", StringComparison.OrdinalIgnoreCase)));
                    table = table.BaseTable;
                }

                m_chainForeignKeyColumnsMapping[keyTable.TableName] = fkColumns;
            }

            return fkColumns;
        }
    }
}