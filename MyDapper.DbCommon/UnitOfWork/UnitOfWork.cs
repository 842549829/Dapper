using System.Data;

namespace MyDapper.DbCommon.UnitOfWork
{
    /// <summary>
    /// 工作单元
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
        /// 设置事务隔离级别
        /// </summary>
        /// <param name="name"> The connection String.</param>
        /// <param name="parameters">parameters</param>
        public UnitOfWork(string name, params object[] parameters)
        {
            var coon = DbFactories.GetConnection(name);
            Connection = coon.DbConnection;
            ProviderName = coon.ProviderName;
            Command = Connection.CreateCommand();
            Transaction = Connection.BeginTransaction();
            Command.Transaction = Transaction;
        }

        /// <summary>
        /// 提供程序名称
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// 链接
        /// </summary>
        public IDbConnection Connection { get; set; }

        /// <summary>
        /// 事务
        /// </summary>
        public IDbTransaction Transaction { get; set; }

        /// <summary>
        /// SQL执行
        /// </summary>
        /// <summary>
        /// IDbCommand
        /// </summary>
        public IDbCommand Command { get; private set; }

        /// <summary>
        /// 提交事务
        /// </summary>
        public virtual void Complete()
        {
            Transaction.Commit();
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        public virtual void Rollback()
        {
            Transaction.Rollback();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void Dispose()
        {
            if (Transaction != null)
            {
                Transaction.Dispose();
                Transaction = null;
            }

            if (Command != null)
            {
                Command.Dispose();
                Command = null;
            }

            if (Connection != null)
            {
                Connection.Dispose();
                Connection.Close();
                Connection = null;
            }
        }
    }
}
