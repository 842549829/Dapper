using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MyDapper.DbCommon.UnitOfWork;

namespace MyDapper.DbCommon.Repositories
{
    public class BulkCopyRepository : BaseRepository
    {
        private readonly IBulkCopyRepository _bulkCopyRepository;

        public BulkCopyRepository(IUnitOfWork unit) : base(unit, null)
        {
            switch (unit.ProviderName.ToLower())
            {
                case "mysql.data.mysqlclient":
                    throw new NotImplementedException();
                case "oracle.data.oracleclient":
                    throw new NotImplementedException();
                case "access.data.accessclient":
                    throw new NotImplementedException();
                default:
                    _bulkCopyRepository = new SqlBulkCopyRepository(unit);
                    break;
            }
        }

        /// <summary>
        /// Sql批量插入
        /// </summary>
        /// <typeparam name="T">泛型T</typeparam>
        /// <param name="dataList">数据列表</param>
        public void BatchInsert<T>(List<T> dataList)
        {
            _bulkCopyRepository.BatchInsert(dataList);
        }

        /// <summary>
        /// Sql批量更新
        /// </summary>
        /// <typeparam name="T">泛型T</typeparam>
        /// <param name="dataList">数据列表</param>
        public void BatchUpdate<T>(List<T> dataList)
        {
            _bulkCopyRepository.BatchUpdate(dataList);
        }

        public void BatchUpdate<T>(List<T> dataList, params Expression<Func<T, string>>[] predicates)
        {
            _bulkCopyRepository.BatchUpdate(dataList, predicates);
        }
    }
}