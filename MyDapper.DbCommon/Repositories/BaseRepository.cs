using System;
using System.Data;
using MyDapper.DbCommon.UnitOfWork;

namespace MyDapper.DbCommon.Repositories
{
    /// <summary>
    /// 数据库持久化基类
    /// </summary>
    public abstract class BaseRepository : IDisposable
    {
        /// <summary>
        /// 数据库链接
        /// </summary>
        protected IDbConnection Connection;

        /// <summary>
        /// Command对象
        /// </summary>
        protected IDbCommand Cmd;

        /// <summary>
        /// 工作单元接口
        /// </summary>
        protected IUnitOfWork UnitOfWork { get; }

        /// <summary>
        /// 数据库链接
        /// </summary>
        protected string Name { get; }

        /// <summary>
        /// 提供程序名称
        /// </summary>
        protected string ProviderName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseRepository"/> class.
        /// 构造函数
        /// </summary>
        /// <param name="unit">工作单元</param>
        /// <param name="name">数据库链接</param>
        /// <param name="parameters">parameters</param>
        protected BaseRepository(IUnitOfWork unit, string name, params object[] parameters)
        {
            Name = name;
            UnitOfWork = unit;
            if (unit != null)
            {
                Cmd = unit.Command;
                Connection = unit.Connection;
                ProviderName = unit.ProviderName;
            }
            else
            {
                if (Connection == null)
                {
                    var coon = DbFactories.GetConnection(name);
                    Connection = coon.DbConnection;
                    ProviderName = coon.ProviderName;
                }

                if (Connection.State != ConnectionState.Open)
                {
                    Connection.Open();
                }

                Cmd = Connection.CreateCommand();
            }
        }

        #region Parameter
        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>参数</returns>
        protected IDbDataParameter AddParameter(string name)
        {
            IDbDataParameter param = CreateParameter(name);
            Cmd.Parameters.Add(param);
            return param;
        }

        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="value">参数值</param>
        /// <returns>参数</returns>
        protected IDbDataParameter AddParameter(string name, object value)
        {
            IDbDataParameter param = CreateParameter(name, value);
            Cmd.Parameters.Add(param);
            return param;
        }

        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="value">参数值</param>
        /// <param name="type">参数类型</param>
        /// <returns>参数</returns>
        protected IDbDataParameter AddParameter(string name, object value, DbType type)
        {
            IDbDataParameter param = CreateParameter(name, value, type);
            Cmd.Parameters.Add(param);
            return param;
        }

        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="value">参数值</param>
        /// <param name="type">参数类型</param>
        /// <param name="direction">参数输出类型</param>
        /// <returns>参数</returns>
        protected IDbDataParameter AddParameter(string name, object value, DbType type, ParameterDirection direction)
        {
            IDbDataParameter param = CreateParameter(name, value, type, direction);
            Cmd.Parameters.Add(param);
            return param;
        }

        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="value">参数值</param>
        /// <param name="type">参数类型</param>
        /// <param name="direction">参数输出类型</param>
        /// <param name="size">参数大小</param>
        /// <returns>参数</returns>
        protected IDbDataParameter AddParameter(string name, object value, DbType type, ParameterDirection direction, int size)
        {
            IDbDataParameter param = CreateParameter(name, value, type, direction, size);
            Cmd.Parameters.Add(param);
            return param;
        }

        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="value">参数值</param>
        /// <param name="type">参数类型</param>
        /// <param name="direction">参数输出类型</param>
        /// <param name="size">参数大小</param>
        /// <param name="scale">显示数字参数的规模(精确到小数点后几位)</param>
        /// <returns>参数</returns>
        protected IDbDataParameter AddParameter(string name, object value, DbType type, ParameterDirection direction, int size, byte scale)
        {
            IDbDataParameter param = CreateParameter(name, value, type, direction, size, scale);
            Cmd.Parameters.Add(param);
            return param;
        }

        /// <summary>
        /// 清除参数
        /// </summary>
        protected void ClearParameters()
        {
            Cmd.Parameters.Clear();
        }
        #endregion

        #region  ExecuteReader

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="type">读取类型(sql文本,存储过程,表)</param>
        /// <param name="behavior">提供了一个查询的结果的描述和对数据库的影响</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>IDataReader</returns>
        protected IDataReader ExecuteReader(string sql, CommandType type, CommandBehavior behavior, int timeout)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentNullException("sql");
            }

            Cmd.CommandText = sql;
            Cmd.CommandType = type;
            Cmd.CommandTimeout = timeout;
            return Cmd.ExecuteReader(behavior);
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="type">读取类型(sql文本,存储过程,表)</param>
        /// <param name="behavior">提供了一个查询的结果的描述和对数据库的影响</param>
        /// <returns>IDataReader</returns>
        protected IDataReader ExecuteReader(string sql, CommandType type, CommandBehavior behavior)
        {
            return ExecuteReader(sql, type, behavior, 0);
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="type">读取类型(sql文本,存储过程,表)</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>IDataReader</returns>
        protected IDataReader ExecuteReader(string sql, CommandType type, int timeout)
        {
            return ExecuteReader(sql, type, CommandBehavior.Default, timeout);
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="type">读取类型(sql文本,存储过程,表)</param>
        /// <returns>IDataReader</returns>
        protected IDataReader ExecuteReader(string sql, CommandType type)
        {
            return ExecuteReader(sql, type, CommandBehavior.Default, 0);
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="behavior">提供了一个查询的结果的描述和对数据库的影响</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>IDataReader</returns>
        protected IDataReader ExecuteReader(string sql, CommandBehavior behavior, int timeout)
        {
            return ExecuteReader(sql, CommandType.Text, behavior, timeout);
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="behavior">提供了一个查询的结果的描述和对数据库的影响</param>
        /// <returns>IDataReader</returns>
        protected IDataReader ExecuteReader(string sql, CommandBehavior behavior)
        {
            return ExecuteReader(sql, CommandType.Text, behavior, 0);
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>IDataReader</returns>
        protected IDataReader ExecuteReader(string sql, int timeout)
        {
            return ExecuteReader(sql, CommandType.Text, CommandBehavior.Default, timeout);
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>IDataReader</returns>
        protected IDataReader ExecuteReader(string sql)
        {
            return ExecuteReader(sql, CommandType.Text, CommandBehavior.Default, 0);
        }

        #endregion

        #region ExecuteScalar
        /// <summary>
        /// ExecuteScalar
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="type">执行类型</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>结果</returns>
        protected object ExecuteScalar(string sql, CommandType type, int timeout)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentNullException("sql");
            }

            Cmd.CommandText = sql;
            Cmd.CommandType = type;
            Cmd.CommandTimeout = timeout;
            object result = Cmd.ExecuteScalar();
            return result == DBNull.Value ? null : result;
        }

        /// <summary>
        /// ExecuteScalar
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>结果</returns>
        protected object ExecuteScalar(string sql)
        {
            return ExecuteScalar(sql, CommandType.Text, 0);
        }
        #endregion

        #region ExecuteNonQuery

        /// <summary>
        /// ExecuteNonQuery
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="type">执行类型</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>结果</returns>
        protected int ExecuteNonQuery(string sql, CommandType type, int timeout)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentNullException("sql");
            }

            Cmd.CommandText = sql;
            Cmd.CommandType = type;
            Cmd.CommandTimeout = timeout;
            return Cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// ExecuteNonQuery
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="type">执行类型</param>
        /// <returns>结果</returns>
        protected int ExecuteNonQuery(string sql, CommandType type)
        {
            return ExecuteNonQuery(sql, type, 0);
        }

        /// <summary>
        /// ExecuteNonQuery
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>结果</returns>
        protected int ExecuteNonQuery(string sql, int timeout)
        {
            return ExecuteNonQuery(sql, CommandType.Text, timeout);
        }

        /// <summary>
        /// ExecuteNonQuery
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>结果</returns>
        protected int ExecuteNonQuery(string sql)
        {
            return ExecuteNonQuery(sql, CommandType.Text, 0);
        }

        #endregion

        #region CreateParameter
        /// <summary>
        /// 创建参数
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>参数</returns>
        private IDbDataParameter CreateParameter(string name)
        {
            IDbDataParameter param = Cmd.CreateParameter();
            param.ParameterName = name;
            return param;
        }

        /// <summary>
        /// 创建参数
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="value">参数值</param>
        /// <returns>参数</returns>
        private IDbDataParameter CreateParameter(string name, object value)
        {
            IDbDataParameter param = CreateParameter(name);
            param.Value = value ?? DBNull.Value;
            return param;
        }

        /// <summary>
        /// 创建参数
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="value">参数值</param>
        /// <param name="type">参数类型</param>
        /// <returns>参数</returns>
        private IDbDataParameter CreateParameter(string name, object value, DbType type)
        {
            IDbDataParameter param = CreateParameter(name, value);
            param.DbType = type;
            return param;
        }

        /// <summary>
        /// 创建参数
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="value">参数值</param>
        /// <param name="type">参数类型</param>
        /// <param name="direction">参数输出类型</param>
        /// <returns>参数</returns>
        private IDbDataParameter CreateParameter(string name, object value, DbType type, ParameterDirection direction)
        {
            IDbDataParameter param = CreateParameter(name, value, type);
            param.Direction = direction;
            return param;
        }

        /// <summary>
        /// 创建参数
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="value">参数值</param>
        /// <param name="type">参数类型</param>
        /// <param name="direction">参数输出类型</param>
        /// <param name="size">参数大小</param>
        /// <returns>参数</returns>
        private IDbDataParameter CreateParameter(string name, object value, DbType type, ParameterDirection direction, int size)
        {
            IDbDataParameter param = CreateParameter(name, value, type, direction);
            param.Size = size;
            return param;
        }

        /// <summary>
        /// 创建参数
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="value">参数值</param>
        /// <param name="type">参数类型</param>
        /// <param name="direction">参数输出类型</param>
        /// <param name="size">参数大小</param>
        /// <param name="scale">显示数字参数的规模(精确到小数点后几位)</param>
        /// <returns>参数</returns>
        private IDbDataParameter CreateParameter(string name, object value, DbType type, ParameterDirection direction, int size, byte scale)
        {
            IDbDataParameter param = CreateParameter(name, value, type, direction, size);
            param.Scale = scale;
            return param;
        }
        #endregion

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (Cmd != null)
            {
                Cmd.Dispose();
                Cmd = null;
            }

            if (Connection == null)
            {
                return;
            }

            if (Connection.State == ConnectionState.Open)
            {
                Connection.Close();
            }

            Connection.Dispose();
            Connection = null;
        }
    }
}
