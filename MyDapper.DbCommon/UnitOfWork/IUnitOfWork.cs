using System;
using System.Data;

namespace MyDapper.DbCommon.UnitOfWork
{
    /// <summary>
    ///     工作单元接口
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Conn
        /// </summary>
        IDbConnection Connection { get; }

        /// <summary>
        /// Transaction
        /// </summary>
        IDbTransaction Transaction { get; }

        /// <summary>
        /// Command
        /// </summary>
        IDbCommand Command { get; }

        /// <summary>
        /// 提供程序名称
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        ///     提交
        /// </summary>
        void Complete();

        /// <summary>
        ///     回滚
        /// </summary>
        void Rollback();
    }
}