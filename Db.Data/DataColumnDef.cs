using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Db.Data
{
    /// <summary>
    /// 数据库字段(列)的定义与模型间的关系定义
    /// </summary>
    public class DataColumnDef
    {
        /// <summary>
        /// m_memberInfo
        /// </summary>
        private readonly MemberInfo m_memberInfo;

        /// <summary>
        /// m_memberType
        /// </summary>
        private readonly MemberTypes m_memberType;

        /// <summary>
        /// m_set
        /// </summary>
        private readonly MethodInfo m_set;

        /// <summary>
        /// 数据库字段(列)的定义与模型件的关系定义
        /// </summary>
        /// <param name="memberInfo">字段属性元数据</param>
        public DataColumnDef(MemberInfo memberInfo)
        {
            m_memberInfo = memberInfo;
            m_memberType = memberInfo.MemberType;
            DataType = memberInfo.MemberType == MemberTypes.Field ? ((FieldInfo)memberInfo).FieldType : ((PropertyInfo)memberInfo).PropertyType;
            if (m_memberType == MemberTypes.Property)
            {
                m_set = ((PropertyInfo)memberInfo).GetSetMethod();
            }

            ColumnName = Name = memberInfo.Name;
            var atts = memberInfo.GetCustomAttributes(typeof(DataColumnAttribute), false);
            if (atts.Length > 0)
            {
                DataColumnAttribute attr = (DataColumnAttribute)atts[0];
                if (!string.IsNullOrWhiteSpace(attr.ColumnName))
                {
                    ColumnName = attr.ColumnName;
                }

                PrimaryKey = attr.PrimaryKey;
                Identity = attr.Identity;
                if (attr.ColumnType == DataColumnType.Value && (ForeignKey = attr.ForeignKey) &&
                    !string.IsNullOrWhiteSpace(attr.MasterFullColumnName))
                {
                    MasterFullColumnName = attr.MasterFullColumnName;
                }

                ColumnType = attr.ColumnType;
                Description = attr.Description;
                MaxLength = attr.MaxLength;
                FixedLength = attr.FixedLength;
            }

            if (string.IsNullOrWhiteSpace(Description))
            {
                atts = memberInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (atts.Length > 0)
                {
                    if (atts[0] is DescriptionAttribute attr)
                    {
                        Description = attr.Description;
                    }
                }
            }

            DataTableDef table = DataTableDef.GetDataTable(m_memberInfo.DeclaringType);
            FullColumnName = string.Format("{0}.{1}", table.TableName, ColumnName);
        }

        /// <summary>
        /// 程序中的模型字段的名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 数据库中字段的名称
        /// </summary>
        public string ColumnName { get; }

        /// <summary>
        /// 数据库中字段的完整名称
        /// </summary>
        public string FullColumnName { get; }

        /// <summary>
        /// 是否是主键列
        /// </summary>
        public bool PrimaryKey { get; }

        /// <summary>
        /// 是否是标识列
        /// </summary>
        public bool Identity { get; }

        /// <summary>
        /// 是否是外键
        /// </summary>
        public bool ForeignKey { get; }

        /// <summary>
        /// 外键列的主键完整名称
        /// ex.  [table1].[field1]
        /// </summary>
        public string MasterFullColumnName { get; }

        /// <summary>
        /// 字段描述
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// 模型中字段的类型  Value=值类型  Table=子表  Virtual=虚拟字段
        /// </summary>
        public DataColumnType ColumnType { get; }

        /// <summary>
        /// 字段数据类型
        /// </summary>
        public Type DataType { get; }

        /// <summary>
        /// 字段大小
        /// </summary>
        public int MaxLength { get; }

        /// <summary>
        /// 固定大小
        /// </summary>
        public bool FixedLength { set; get; }


        /// <summary>
        /// 表类类型
        /// </summary>
        internal Type DefType { set; get; }

        /// <summary>
        /// 获取模型中当前字段的内容
        /// </summary>
        /// <param name="model">模型实例</param>
        /// <returns>字段的内容</returns>
        public object GetValue(object model)
        {
            object value;
            if (m_memberType == MemberTypes.Property)
            {
                value = ((PropertyInfo)m_memberInfo).GetValue(model, null);
            }
            else
            {
                value = ((FieldInfo)m_memberInfo).GetValue(model);
            }

            if (DataType == typeof(DateTime) && DateTime.MinValue.Equals(value))
            {
                value = new DateTime(1753, 1, 1);
            }
            else if (value != null && value is string && MaxLength > 0)
            {
                var str_value = value as string;
                if (MaxLength < str_value.Length)
                {
                    value = str_value.Substring(0, MaxLength);
                }
            }
            else if (value == null && typeof(IEnumerable).IsAssignableFrom(DataType))
            {
                var constructor = DataType.GetConstructors().Where(ci => !ci.GetParameters().Any()).FirstOrDefault();
                if (constructor != null)
                {
                    value = constructor.Invoke(new object[] { });
                }
            }

            return value;
        }

        /// <summary>
        /// 设置模型中当前字段的内容
        /// </summary>
        /// <param name="model">模型实例</param>
        /// <param name="value">字段的内容</param>
        public void SetValue(object model, object value)
        {
            if (value != DBNull.Value)
            {
                var val = value;
                if (value != null && value.GetType() != ((PropertyInfo)m_memberInfo).PropertyType)
                {
                    val = value.ToString().AsType(((PropertyInfo)m_memberInfo).PropertyType);
                }

                if (m_memberType == MemberTypes.Property)
                {
                    if (m_set != null)
                    {
                        m_set.Invoke(model, new[] { val });
                    }
                }
                else
                {
                    ((FieldInfo)m_memberInfo).SetValue(model, val);
                }
            }
        }
    }
}