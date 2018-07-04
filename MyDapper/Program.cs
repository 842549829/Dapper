using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Dapper;

namespace MyDapper
{
    class Program
    {
        static void Main(string[] args)
        {

            var i = QueryPaging(new User { Name = "Dapper01" }, 3, 56);
        }

        /// <summary>
        /// 添加单条数据
        /// </summary>
        /// <returns>结果</returns>
        public static int InsertWithSql()
        {
            using (var conn = SqLiteHelper.OpenConnection())
            {
                User user = new User
                {
                    Name = "Dapper01",
                    Address = "周口",
                    Age = "15"
                };
                const string sql = "INSERT INTO User(Name,Address,Age)VALUES(@Name,@Address,@Age); SELECT LAST_INSERT_ID();";
                var obj = conn.ExecuteScalar(sql, user);
                if (obj != null)
                {
                    var rows = Convert.ToInt32(obj);
                    return rows;
                }
                return 0;
            }
        }

        /// <summary>
        /// 批量添加
        /// </summary>
        /// <returns>结果</returns>
        public static int InsertWithSqls()
        {
            using (var conn = SqLiteHelper.OpenConnection())
            {
                List<User> users = new List<User>();
                for (int i = 0; i < 50; i++)
                {
                    User user = new User
                    {
                        Name = "Dapper" + i,
                        Address = "周口",
                        Age = "15" + i
                    };
                    users.Add(user);
                }

                const string sql = "INSERT INTO User(Name,Address,Age)VALUES(@Name,@Address,@Age);";
                var rows = conn.Execute(sql, users);
                return rows;
            }
        }

        /// <summary>
        ///  多表添加
        /// </summary>
        /// <returns>结果</returns>
        public static int InsertWithSqlss()
        {
            int rows = 0;
            using (var conn = SqLiteHelper.OpenConnection())
            {
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        List<User> users = new List<User>();
                        for (int i = 0; i < 50; i++)
                        {
                            User user = new User
                            {
                                Name = "Dapper" + i,
                                Address = "周口",
                                Age = "15" + i
                            };
                            users.Add(user);
                        }
                        const string userSql = "INSERT INTO User(Name,Address,Age)VALUES(@Name,@Address,@Age);";
                        rows = conn.Execute(userSql, users);

                        List<Role> roles = new List<Role>();
                        for (int i = 0; i < 20; i++)
                        {
                            Role role = new Role
                            {
                                Name = "Role" + i
                            };
                            roles.Add(role);
                        }


                        const string roleSql = "INSERT INTO Role(Name) VALUES(@Name);";
                        rows += conn.Execute(roleSql, roles);

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                    }
                }

                return rows;
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <returns>结果</returns>
        public static int DeleteWithSql()
        {
            using (var conn = SqLiteHelper.OpenConnection())
            {
                User user = new User { Id = 15 };
                const string sql = "DELETE FROM User WHERE Id=@Id;";
                return conn.Execute(sql, user);
            }
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <returns>结果</returns>
        public static int UpdateWithSql()
        {
            using (var conn = SqLiteHelper.OpenConnection())
            {
                User user = new User
                {
                    Id = 14,
                    Name = "Dapper03",
                    Address = "太康",
                    Age = "687"
                };
                const string sql = "UPDATE User SET Name=@Name,Address=@Address,Age=@Age WHERE Id=@Id;";
                return conn.Execute(sql, user);
            }
        }

        /// <summary>
        /// 单条数据查询
        /// </summary>
        /// <returns>User</returns>
        public static User QueryUser()
        {
            using (var conn = SqLiteHelper.OpenConnection())
            {
                const string sql = "SELECT * FROM User WHERE Id=@Id;";
                var user = conn.Query<User>(sql, new { Id = 12 }).SingleOrDefault();
                return user;
            }
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns>Users</returns>
        public static IEnumerable<User> QueryUsers()
        {
            using (var conn = SqLiteHelper.OpenConnection())
            {
                const string sql = "SELECT * FROM User WHERE Age=@Age;";
                var users = conn.Query<User>(sql, new { Age = "15" });
                return users;
            }
        }

        /// <summary>
        /// 1对1关联查询
        /// </summary>
        /// <returns>Users</returns>
        public static IEnumerable<User> QueryUsers1()
        {
            using (var conn = SqLiteHelper.OpenConnection())
            {
                const string sql = "SELECT * FROM `User` as u  INNER JOIN Role as r ON u.Id = r.Id;";
                var users = conn.Query<User, Role, User>(sql, (user, role) =>
                {
                    user.Role = role;
                    return user;
                });
                return users;
            }
        }

        /// <summary>
        /// 1对多关联查询
        /// </summary>
        /// <returns>Users</returns>
        public static IEnumerable<User> QueryUsers2()
        {
            using (var conn = SqLiteHelper.OpenConnection())
            {
                const string sql = "SELECT * FROM `User` as u  INNER JOIN Role as r ON u.Id = r.Id;";
                var users = conn.Query<User, Role, User>(sql, (user, role) =>
                {
                    if (user.Roles == null)
                    {
                        user.Roles = new List<Role>();
                    }
                    user.Roles.Add(role);
                    return user;

                }, splitOn: "Id");
                return users;
            }
        }

        /// <summary>
        /// 多对多关联查询
        /// </summary>
        /// <returns>Users</returns>
        public static IEnumerable<User> QueryUsers3()
        {
            using (var conn = SqLiteHelper.OpenConnection())
            {
                const string sql = "SELECT a.*,c.* FROM `User` AS a LEFT JOIN Relation b ON a.id = b.UserId LEFT JOIN Role c ON c.id=b.RoleId;";
                var users = conn.Query<User, Role, User>(sql, (user, role) =>
                {
                    if (role != null)
                    {
                        if (user.Roles == null)
                        {
                            user.Roles = new List<Role>();
                        }
                        user.Roles.Add(role);
                    }

                    return user;

                }, splitOn: "Id");
                return users;
            }
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="condition">查询条件</param>
        /// <param name="pageIndex">开始页</param>
        /// <param name="pageSize">页大小</param>
        /// <returns>结果集</returns>
        public static IEnumerable<User> QueryPaging(User condition, int pageIndex, int pageSize)
        {
            using (var conn = SqLiteHelper.OpenConnection())
            {
                DynamicParameters dp = new DynamicParameters(condition);
                StringBuilder sql = new StringBuilder("SELECT * FROM User");
                sql.Append(" WHERE 1 = 1 ");
                if (!string.IsNullOrWhiteSpace(condition.Name))
                {
                    sql.Append(" AND Name = @Name ");
                }
                if (!string.IsNullOrWhiteSpace(condition.Age))
                {
                    sql.Append(" AND Age = @Age ");
                }
                sql.Append(" LIMIT @StartRow,@Size");
                dp.Add("@StartRow", pageIndex);
                dp.Add("@Size", pageSize);
                var users = conn.Query<User>(sql.ToString(), dp);
                return users;
            }
        }
    }
}