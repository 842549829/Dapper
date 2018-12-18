using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace MyDapper.BatchOperation
{

    /* 表结构
        DROP TABLE [dbo].[Product]
        GO
        CREATE TABLE [dbo].[Product] (
        [Id] varchar(36) NOT NULL ,
        [Name] varchar(255) NOT NULL ,
        [Price] decimal(18,4) NOT NULL 
        )
        GO
        ALTER TABLE [dbo].[Product] ADD PRIMARY KEY ([Id])
        GO
     */

    public class Batch
    {
        public static List<string> GetList()
        {
            List<string> list = new List<string>();
            using (SqlConnection conn = new SqlConnection(SqLiteHelper.SqlServerConnection))
            {
                using (SqlCommand command = new SqlCommand("SELECT TOP 5000 Id FROM Product", conn))
                {
                    conn.Open();
                    var data = command.ExecuteReader();
                    while (data.Read())
                    {
                        list.Add(data["Id"].ToString());
                    }
                }
            }

            return list;
        }

        public static void Update()
        {
            var list = GetList();
            List<Product> products = new List<Product>();
            for (int i = 0; i < list.Count; i++)
            {
                Product product = new Product
                {
                    Id = list[i],
                    Name = $"默认{i}",
                    Price = (decimal)i * 5
                };
                products.Add(product);
            }
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Update(SqLiteHelper.SqlServerConnection, products, "Product");
            stopwatch.Stop();
            Console.WriteLine("耗时：" + stopwatch.ElapsedMilliseconds);
        }

        public static void Update<T>(string connectionString, List<T> list, string destinationTableName)
        {
            var dt = ConvertToDataTable(list);
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    using (SqlCommand command = new SqlCommand(string.Empty, connection))
                    {
                        try
                        {
                            command.Transaction = transaction;
                            command.CommandText = "CREATE TABLE #TmpTable(Id varchar(36),Name varchar(255),Price decimal(18,4))";
                            command.ExecuteNonQuery();
                            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                            {
                                bulkCopy.BulkCopyTimeout = 660;
                                bulkCopy.DestinationTableName = "#TmpTable";
                                bulkCopy.WriteToServer(dt);
                                bulkCopy.Close();
                            }
                            command.CommandTimeout = 300;
                            command.CommandText = "UPDATE T SET T.Name =Temp.Name  FROM " + destinationTableName + " T INNER JOIN #TmpTable Temp ON T.Id=Temp.Id; DROP TABLE #TmpTable;";
                            command.ExecuteNonQuery();
                            transaction.Commit();
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                        }
                    }
                }
            }
        }

        public static void Insert()
        {
            List<Product> products = new List<Product>();
            for (int i = 0; i < 10000; i++)
            {
                Product product = new Product
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = $"商品{i}",
                    Price = (decimal)i
                };
                products.Add(product);
            }
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Insert(SqLiteHelper.SqlServerConnection, products, "Product");
            stopwatch.Stop();
            Console.WriteLine("耗时：" + stopwatch.ElapsedMilliseconds);
        }

        public static void Insert<T>(string connectionString, List<T> dataList, string destinationTableName, int batchSize = 0)
        {
            DataTable dataTable = ConvertToDataTable(dataList);
            Insert(connectionString, dataTable, destinationTableName, batchSize);
        }

        public static void Insert(string connectionString, DataTable dataTable, string destinationTableName, int batchSize = 0)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                    {
                        bulkCopy.BatchSize = batchSize;
                        bulkCopy.DestinationTableName = destinationTableName;
                        try
                        {
                            bulkCopy.WriteToServer(dataTable);
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            transaction.Rollback();
                        }
                    }
                }
            }
        }

        public static DataTable ConvertToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
            {
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                {
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                }
                table.Rows.Add(row);
            }
            return table;
        }
    }

    public class Product
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }
    }
}