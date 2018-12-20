using System;
using System.Linq.Expressions;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Db.Data
{
    /// <summary>
    /// 查询结果分页控制器
    /// </summary>
    public class DataPager
    {
        /// <summary>
        /// 当前查询的页
        /// </summary>
        public virtual int CurrentPage { set; get; } = 1;

        /// <summary>
        /// 每页输出的数据集合行数
        /// </summary>
        public virtual int PageSize { set; get; } = 10;

        /// <summary>
        /// 正确匹配条件的所有数据记录总数
        /// </summary>
        public virtual int TotalCount { set; get; }

        /// <summary>
        /// 总页数
        /// </summary>
        public virtual int TotalPages
        {
            get
            {
                if (PageSize > 0)
                {
                    return (int)Math.Ceiling(TotalCount / (decimal)PageSize);
                }

                return 0;
            }
        }

        /// <summary>
        /// 排序字段
        /// </summary>
        //[Obsolete("OrderColumn属性是不健壮的，建议改用DataPager<T>的OrderColumnExpression")]
        public virtual string OrderColumn { set; get; }

        /// <summary>
        /// 排序方式
        /// </summary>
        public virtual DataOrderType OrderType { set; get; }
    }

    /// <summary>
    /// 查询结果分页控制器
    /// </summary>
    /// <typeparam name="T">查询模型泛型</typeparam>
    public class DataPager<T> : DataPager
    {
        private Expression<Func<T, object>> m_OrderColumnExpression;

        /// <summary>
        /// Lambda形势的排序字段
        /// </summary>
        [XmlIgnore]
        [JsonIgnore]
        public Expression<Func<T, object>> OrderColumnExpression
        {
            set
            {
                m_OrderColumnExpression = value;
                OrderColumn = LambdaHelper.Parse(value).SqlString;
            }
            get => m_OrderColumnExpression;
        }
    }
}