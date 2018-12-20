using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using FastMember;
using MyDapper.Model;

namespace MyDapper.Core.BatchOperation
{
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

        public static void Inserts()
        {
            const int count = 10000;
            List<Order> orders = new List<Order>();
            List<Product> products = new List<Product>();
            for (var i = 0; i < count; i++)
            {
                Product product = new Product
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = $"商品{i}",
                    Price = i * 0.8M
                };
                products.Add(product);
                Order order = new Order
                {
                    Id = Guid.NewGuid().ToString(),
                    ProductId = product.Id,
                    Remake = "suggestions",
                    Status = 1
                };
                orders.Add(order);
            }
            var productsDataTable = Batch.ConvertToDataTable(products);
            var ordersDataTable = Batch.ConvertToDataTable(orders);
            Dictionary<string, DataTable> dataTables = new Dictionary<string, DataTable>
            {

                { "Product", productsDataTable},
                { "Orders",ordersDataTable}
            };

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Inserts(SqLiteHelper.SqlServerConnection, dataTables);
            stopwatch.Stop();
            Console.WriteLine("耗时：" + stopwatch.ElapsedMilliseconds);
        }

        public static void Inserts(string connectionString, Dictionary<string, DataTable> dataTables, int batchSize = 0)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        SqlCommand command = new SqlCommand();
                        command.Transaction = transaction;
                        command.CommandText = "update ....";
                        command.ExecuteNonQuery();


                        foreach (var item in dataTables)
                        {
                            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                            {
                                bulkCopy.BatchSize = batchSize;
                                bulkCopy.DestinationTableName = item.Key;
                                bulkCopy.WriteToServer(item.Value);
                            }
                        }
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

    public class BatchExtension
    {
        public static void Insert()
        {
            List<Product> products = new List<Product>();
            for (int i = 0; i < 10000; i++)
            {
                Product product = new Product
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = $"的干法国防科大和{i}",
                    Price = i
                };
                products.Add(product);
            }

            var copyParameters = new[]
            {
                nameof(Product.Id),
                nameof(Product.Name),
                nameof(Product.Price)
            };

            using (var sqlCopy = new SqlBulkCopy(SqLiteHelper.SqlServerConnection))
            {
                sqlCopy.DestinationTableName = "[Product]";
                sqlCopy.BatchSize = 500;
                using (var reader = ObjectReader.Create(products, copyParameters))
                {
                    sqlCopy.WriteToServer(reader);
                }
            }
        }

        public static void Inserts()
        {
            const int count = 10000;
            List<Order> orders = new List<Order>();
            List<Product> products = new List<Product>();
            for (var i = 0; i < count; i++)
            {
                Product product = new Product
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = $"商品{i}",
                    Price = i * 0.8M
                };
                products.Add(product);
                Order order = new Order
                {
                    Id = Guid.NewGuid().ToString(),
                    ProductId = product.Id,
                    Remake = "suggestions",
                    Status = 1
                };
                orders.Add(order);
            }

            var productsItems = new Items
            {
                Type = typeof(Product),
                Enumerable = products,
                Members = new[] { nameof(Product.Id), nameof(Product.Name), nameof(Product.Price) }
            };
            var ordersItems = new Items
            {
                Type = typeof(Order),
                Enumerable = orders,
                Members = new[] { nameof(Order.Id), nameof(Order.ProductId), nameof(Order.Status), nameof(Order.Remake) }
            };
            Dictionary<string, Items> items = new Dictionary<string, Items>
            {

                { "Product", productsItems},
                { "Orders",ordersItems}
            };

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Inserts(SqLiteHelper.SqlServerConnection, items);
            stopwatch.Stop();
            Console.WriteLine("耗时：" + stopwatch.ElapsedMilliseconds);
        }


        public static void Inserts(string connectionString, Dictionary<string, Items> items, int batchSize = 0)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        foreach (var item in items)
                        {
                            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                            {
                                bulkCopy.BatchSize = batchSize;
                                bulkCopy.DestinationTableName = item.Key;
                                using (var reader = new ObjectReader(item.Value.Type, item.Value.Enumerable, item.Value.Members))
                                {
                                    bulkCopy.WriteToServer(reader);
                                }
                            }
                        }
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

}