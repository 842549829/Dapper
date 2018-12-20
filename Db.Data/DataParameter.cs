using System;
using System.Data;
using System.Data.Common;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Db.Data
{
    /// <summary>
    /// 执行SQL 语句时采用的参数
    /// </summary>
    public class DataParameter
    {
        private DbParameter m_DbParameter;
        private readonly object m_entity;

        private object m_value;

        /// <summary>
        /// 执行SQL 语句时采用的参数
        /// </summary>
        /// <param name="parameterName">参数名称</param>
        public DataParameter(string parameterName)
        {
            ParameterName = parameterName;
        }

        /// <summary>
        /// 执行SQL 语句时采用的参数
        /// </summary>
        /// <param name="parameterName">参数名称</param>
        /// <param name="value">参数值</param>
        public DataParameter(string parameterName, object value)
            : this(parameterName)
        {
            m_value = value ?? DBNull.Value;
        }

        /// <summary>
        /// 执行SQL 语句时采用的参数
        /// </summary>
        /// <param name="parameterName">参数名称</param>
        /// <param name="column">模型字段关系定义</param>
        /// <param name="entity">模型实体</param>
        internal DataParameter(string parameterName, DataColumnDef column, object entity)
            : this(parameterName)
        {
            Column = column;
            m_entity = entity;
            object value = column.GetValue(entity);
            if (column.DataType == typeof(DateTime) && DateTime.MinValue.Equals(value))
            {
                m_value = DBNull.Value;
            }
            //else if(column.DataType == typeof(String) && !column.ForeignKey)
            //{
            //    m_value = value ?? String.Empty;
            //}
            else
            {
                m_value = value ?? DBNull.Value;
            }
        }

        /// <summary>
        /// 获取和设置参数输出属性
        /// </summary>
        public bool Output { get; set; } = false;

        /// <summary>
        /// 参数名称
        /// </summary>
        public string ParameterName { set; get; }

        /// <summary>
        /// 数据库中字段的完整名称
        /// </summary>
        [XmlIgnore]
        [JsonIgnore]
        public DataColumnDef Column { get; } = null;

        /// <summary>
        /// 外键对应的主键字段的完整名称
        /// </summary>

        [XmlIgnore]
        [JsonIgnore]
        public string MasterFullColumnName
        {
            get
            {
                if (Column != null)
                {
                    return Column.MasterFullColumnName;
                }

                return null;
            }
        }

        /// <summary>
        /// 参数值
        /// </summary>
        public object Value
        {
            set
            {
                m_value = value;
                if (m_DbParameter != null)
                {
                    m_DbParameter.Value = m_value;
                }

                if (m_entity != null && Column != null)
                {
                    Column.SetValue(m_entity, m_value);
                }
            }
            get
            {
                if (m_DbParameter != null)
                {
                    return m_DbParameter.Value;
                }

                return m_value;
            }
        }

        internal DbParameter CreateDbParameter(DbCommand comm)
        {
            m_DbParameter = comm.CreateParameter();
            m_DbParameter = comm.CreateParameter();
            m_DbParameter.ParameterName = ParameterName;
            m_DbParameter.Direction = Output ? ParameterDirection.InputOutput : ParameterDirection.Input;
            m_DbParameter.Value = m_value;
            return m_DbParameter;
        }
    }
}