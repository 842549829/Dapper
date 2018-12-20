using System;

namespace Db.Data
{
    /// <summary>
    /// 数据库兼容性扩展接口的特征
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DataCompatibleAttribute : Attribute
    {
        private string m_providerName;

        /// <summary>
        /// 数据供应器名称
        /// </summary>
        public string ProviderName
        {
            get => m_providerName;
            set => m_providerName = value.ToLower();
        }
    }
}