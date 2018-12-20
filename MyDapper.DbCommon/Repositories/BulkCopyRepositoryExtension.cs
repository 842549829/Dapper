using System;
using System.Collections.Generic;
using System.Reflection;

namespace MyDapper.DbCommon.Repositories
{
    /// <summary>
    /// BulkCopyRepository扩展类
    /// </summary>
    public class BulkCopyRepositoryExtension
    {
        /// <summary>
        /// 获取字段
        /// </summary>
        /// <param name="type">实体类型</param>
        /// <returns>字段集合</returns>
        public static string[] GetFields(Type type)
        {
            PropertyInfo[] propertyInfo = type.GetProperties();
            List<string> propertyInfoList = new List<string>();
            foreach (PropertyInfo item in propertyInfo)
            {
                propertyInfoList.Add(item.Name);
            }
            return propertyInfoList.ToArray();
        }

        /// <summary>
        /// 获取表名
        /// </summary>
        /// <param name="type">实体类型</param>
        /// <returns>表名</returns>
        public static string GetTableName(Type type)
        {
            return type.Name;
        }
    }
}