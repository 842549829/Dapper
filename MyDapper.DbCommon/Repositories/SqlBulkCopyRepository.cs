using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using FastMember;
using MyDapper.DbCommon.UnitOfWork;

namespace MyDapper.DbCommon.Repositories
{
    public class SqlBulkCopyRepository : IBulkCopyRepository
    {
        private readonly IUnitOfWork _unit;

        public SqlBulkCopyRepository(IUnitOfWork unit)
        {
            _unit = unit;
        }

        public void BatchInsert<T>(List<T> dataList)
        {
            var sqlConnection = (SqlConnection)_unit.Connection;
            var sqlTransaction = (SqlTransaction)_unit.Transaction;
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(sqlConnection, SqlBulkCopyOptions.Default, sqlTransaction))
            {
                var type = typeof(T);
                bulkCopy.DestinationTableName = BulkCopyRepositoryExtension.GetTableName(type);
                using (var reader = new ObjectReader(type, dataList, BulkCopyRepositoryExtension.GetFields(type)))
                {
                    bulkCopy.WriteToServer(reader);
                }
            }
        }

        public void BatchUpdate<T>(List<T> dataList)
        {
            var type = typeof(T);
            var tempTableName = "#TmpTable";
            var dataTableName = BulkCopyRepositoryExtension.GetTableName(type);
            var sqlConnection = (SqlConnection)_unit.Connection;
            var sqlTransaction = (SqlTransaction)_unit.Transaction;
            var sqlCommand = (SqlCommand)_unit.Command;
            sqlCommand.CommandText = $"SELECT * INTO {tempTableName} FROM {dataTableName} WHERE 1 = 2"; ;
            sqlCommand.ExecuteNonQuery();
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(sqlConnection, SqlBulkCopyOptions.Default, sqlTransaction))
            {

                bulkCopy.DestinationTableName = tempTableName;
                using (var reader = new ObjectReader(type, dataList, BulkCopyRepositoryExtension.GetFields(type)))
                {
                    bulkCopy.WriteToServer(reader);
                }
            }
            sqlCommand.CommandText = $"UPDATE T SET T.Name =Temp.Name  FROM {dataTableName} T INNER JOIN {tempTableName} Temp ON T.Id=Temp.Id; DROP TABLE {tempTableName};";
            sqlCommand.ExecuteNonQuery();
        }

        public void BatchUpdate<T>(List<T> dataList, string field, string where)
        {
            var type = typeof(T);
            var tempTableName = "#TmpTable";
            var dataTableName = BulkCopyRepositoryExtension.GetTableName(type);
            var sqlConnection = (SqlConnection)_unit.Connection;
            var sqlTransaction = (SqlTransaction)_unit.Transaction;
            var sqlCommand = (SqlCommand)_unit.Command;
            sqlCommand.CommandText = $"SELECT * INTO {tempTableName} FROM {dataTableName} WHERE 1 = 2"; ;
            sqlCommand.ExecuteNonQuery();
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(sqlConnection, SqlBulkCopyOptions.Default, sqlTransaction))
            {

                bulkCopy.DestinationTableName = tempTableName;
                using (var reader = new ObjectReader(type, dataList, BulkCopyRepositoryExtension.GetFields(type)))
                {
                    bulkCopy.WriteToServer(reader);
                }
            }
            sqlCommand.CommandText = $"UPDATE T SET {field} FROM {dataTableName} T INNER JOIN {tempTableName} Temp ON {where}; DROP TABLE {tempTableName};";
            sqlCommand.ExecuteNonQuery();
        }

        public void BatchUpdate<T>(List<T> dataList, params Expression<Func<T, string>>[] predicates)
        {
            var fields = ParseColumnPredicates(predicates);
            StringBuilder updateFields = new StringBuilder();
            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                updateFields.Append(i == 0 ? $"T.{field}=Temp.{field}" : $",T.{field}=Temp.{field}");
            }

            var type = typeof(T);
            var id = GetProperties(type);
            var innerJoin = $"T.{id}=Temp.{id}";

            var tempTableName = $"#TmpTable{type.Name}";
            var dataTableName = BulkCopyRepositoryExtension.GetTableName(type);
            var sqlConnection = (SqlConnection)_unit.Connection;
            var sqlTransaction = (SqlTransaction)_unit.Transaction;
            var sqlCommand = (SqlCommand)_unit.Command;
            sqlCommand.CommandText = $"SELECT * INTO {tempTableName} FROM {dataTableName} WHERE 1 = 2";
            sqlCommand.ExecuteNonQuery();
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(sqlConnection, SqlBulkCopyOptions.Default, sqlTransaction))
            {

                bulkCopy.DestinationTableName = tempTableName;
                using (var reader = new ObjectReader(type, dataList, BulkCopyRepositoryExtension.GetFields(type)))
                {
                    bulkCopy.WriteToServer(reader);
                }
            }
            sqlCommand.CommandText = $"UPDATE T SET {updateFields} FROM {dataTableName} T INNER JOIN {tempTableName} Temp ON {innerJoin}; DROP TABLE {tempTableName};";
            sqlCommand.ExecuteNonQuery();
        }

        private List<string> ParseColumnPredicates<T>(Expression<Func<T, string>>[] predicates)
        {
            List<string> columns = new List<string>(predicates.Length * 8);
            foreach (Expression<Func<T, string>> exp in predicates)
            {
                columns.AddRange(LambdaHelper.Parse(exp).Split(','));
            }
            return columns;
        }

        public static string GetProperties(Type type)
        {
            System.Reflection.PropertyInfo[] properties = type.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (properties.Length <= 0)
            {
                return "Id";
            }
            foreach (System.Reflection.PropertyInfo item in properties)
            {
                if (item.GetCustomAttributes(typeof(KeyAttribute), false).FirstOrDefault() is KeyAttribute)
                {
                    return item.Name;
                }
            }
            return "Id";
        }
    }
}