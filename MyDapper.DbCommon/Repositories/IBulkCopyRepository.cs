using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MyDapper.DbCommon.Repositories
{
    public interface IBulkCopyRepository
    {
        void BatchInsert<T>(List<T> dataList);

        void BatchUpdate<T>(List<T> dataList);

        void BatchUpdate<T>(List<T> dataList, string field, string where);

        void BatchUpdate<T>(List<T> dataList, params Expression<Func<T, string>>[] predicates);
    }
}
