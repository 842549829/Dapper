using System;
using System.Collections.Generic;
using MyDapper.Core.BatchOperation;
using MyDapper.DbCommon.Repositories;
using MyDapper.DbCommon.UnitOfWork;
using MyDapper.Model;

namespace MyDapper.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            BatchB();
            //Batch.Update();
        }


        public static void BatchA()
        {
            const int count = 10000;
            List<Orders> orders = new List<Orders>();
            List<Product> products = new List<Product>();
            for (var i = 0; i < count; i++)
            {
                Product product = new Product
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = $"的肯定分拣单{i}",
                    Price = i * 0.8M
                };
                products.Add(product);
                Orders order = new Orders
                {
                    Id = Guid.NewGuid().ToString(),
                    ProductId = product.Id,
                    Remake = "是开发大概几点",
                    Status = 1
                };
                orders.Add(order);
            }

            using (IUnitOfWork unitOfWork = new UnitOfWork("code"))
            {
                using (BulkCopyRepository sqlBulkCopyRepository = new BulkCopyRepository(unitOfWork))
                {
                    try
                    {
                        sqlBulkCopyRepository.BatchInsert(orders);
                        sqlBulkCopyRepository.BatchInsert(products);
                        unitOfWork.Complete();
                    }
                    catch (Exception)
                    {
                        unitOfWork.Rollback();
                    }
                }
            }
        }


        public static void BatchB()
        {
            var list = Batch.GetList();
            List<Product> products = new List<Product>();
            for (int i = 0; i < list.Count; i++)
            {
                Product product = new Product
                {
                    Id = list[i],
                    Name = $"撒旦撒旦撒旦所多多所",
                    Price = 500000
                };
                products.Add(product);
            }

            using (IUnitOfWork unitOfWork = new UnitOfWork("code"))
            {
                try
                {
                    BulkCopyRepository sqlBulkCopyRepository = new BulkCopyRepository(unitOfWork);
                    sqlBulkCopyRepository.BatchUpdate(products, m => m.With(m.Name, m.Price));
                    unitOfWork.Complete();
                }
                catch (Exception exception)
                {
                    unitOfWork.Rollback();
                }
            }
        }
    }
}

