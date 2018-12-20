using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Db.Data
{
    /// <summary>
    /// System.Data.SqlClient 兼容性扩展
    /// </summary>
    [DataCompatible(ProviderName = "System.Data.SqlClient")]
    public sealed class SqlServerCompatible : IDataCompatible
    {
        private const string EmptyInsertSQL = "select * into #T{0} from (select 1 as TF) \"TT\"\r\nDROP TABLE #T{0}";

        private static readonly Regex TemplateCreateRule =
            new Regex(@"into (#[\S]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex NameSymbol = new Regex("[\\[\\]\\`\\\"]", RegexOptions.Compiled);

        private static readonly Regex operationRule = new Regex(@"\([^\)]+\)|\=", RegexOptions.Compiled);
        //private static IDictionary<Type, string> InsertSQLCache = new ConcurrentDictionary<Type, string>();
        //private static IDictionary<string, string> SelectSQLCache = new ConcurrentDictionary<string, string>();
        //private static IDictionary<string, string> DeleteSQLCache = new ConcurrentDictionary<string, string>();
        //private static IDictionary<string, string> UpdateSQLCache = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// 参数名前缀
        /// </summary>
        public string ParameterPrefixName => "@";

        /// <summary>
        /// 用于翻页查询的Between条件的开始量变量名
        /// </summary>
        public string BeginBetweenVariableName => "@@BeginRowNumber";

        /// <summary>
        /// 用于翻页查询的Between条件的结束量变量名
        /// </summary>
        public string EndBetweenVariableName => "@@EndRowNumber";

        /// <summary>
        /// 获取字段计算的SQL脚本
        /// </summary>
        /// <param name="type">模型类型</param>
        /// <param name="solver">列运算器类型</param>
        /// <param name="column">要计算的列名称</param>
        /// <param name="where">查询条件</param>
        /// <returns>数据库查询脚本</returns>
        public string GetComputeSql(Type type, DataSolver solver, string column, string where)
        {
            var chainTableNames = new List<string>();
            var table = DataTableDef.GetDataTable(type);
            var deep = table.TableChain.Count();
            IEnumerator<DataTableDef> chain = table.TableChain.GetEnumerator();
            var sb = new StringBuilder(deep * 0x50);
            if (chain.MoveNext())
            {
                chainTableNames.Add(chain.Current.TableName);
                sb.AppendFormat(
                    "select {0}({1}) from {2}",
                    Enum.GetName(typeof(DataSolver), solver),
                    solver == DataSolver.Count ? column : GetSafeName(column),
                    GetSafeName(chain.Current.TableName));
                sb.AppendLine();
                while (chain.MoveNext())
                {
                    if (chain.Current.TableType != DataTableType.Virtual)
                    {
                        var conditions = new List<string>();
                        chain.Current.ForeignKeyColumns.ForEach(fk =>
                        {
                            if (chainTableNames.Exists(tn =>
                                fk.MasterFullColumnName.StartsWith(tn, StringComparison.OrdinalIgnoreCase)))
                            {
                                conditions.Add(StringExtension.Format("{0}={1}", GetSafeName(fk.FullColumnName),
                                    GetSafeName(fk.MasterFullColumnName)));
                            }
                        });
                        if (conditions.Count > 0)
                        {
                            sb.AppendFormat(
                                "inner join {0} on {1}",
                                GetSafeName(chain.Current.TableName),
                                string.Join(" and ", conditions)
                            );
                            sb.AppendLine();
                        }

                        chainTableNames.Add(chain.Current.TableName);
                    }
                }

                if (!string.IsNullOrWhiteSpace(where))
                {
                    sb.AppendLine(" where " + where);
                    sb.AppendLine();
                }
            }

            return fixColumnName(sb.ToString(), table.ColumnNames);
        }

        /// <summary>
        /// 获取查询数据库的SQL脚本
        /// </summary>
        /// <param name="type">模型类型</param>
        /// <param name="where">查询条件</param>
        /// <param name="pager">分页器</param>
        /// <param name="columns">输出列</param>
        /// <returns>数据库查询脚本</returns>
        public string GetSelectSql(Type type, string where, DataPager pager, IEnumerable<string> columns = null)
        {
            //string key = Cryptography.SHA512Hash(
            //            type.FullName,
            //            where ?? String.Empty,
            //            pager == null ? String.Empty : pager.OrderColumn,
            //            pager == null ? String.Empty : pager.OrderType.ToString(),
            //            columns == null ? String.Empty : String.Join("", columns)
            //        );
            string selectSql;
            //if (!SelectSQLCache.TryGetValue(key, out selectSql))
            {
                selectSql = GetSelectSql(type, where, pager, null, true, columns);
                selectSql = SupplementDropTemplateSql(selectSql);
                //SelectSQLCache[key] = selectSql;
            }

            //HesERP.Core.ExceptionLogger.WriteDatabaseLog(selectSql);
            return selectSql;
        }

        /// <summary>
        /// 获取空插脚本
        /// </summary>
        /// <returns>模拟插入数据行的脚本,用于强制返回受影响行数量大于0</returns>
        public string GetInsertSQL()
        {
            return string.Format(EmptyInsertSQL, GetRandomString());
        }

        /// <summary>
        /// 获取用于插入记录的SQL脚本
        /// </summary>
        /// <param name="type">模型类型</param>
        /// <returns>记录插入脚本</returns>
        public string GetInsertSQL(Type type)
        {
            string m_insertSQL;
            //if (!InsertSQLCache.TryGetValue(type, out m_insertSQL))
            {
                var table = DataTableDef.GetDataTable(type);
                var deep = table.TableChain.Count();
                var sb = new StringBuilder(deep * 0x180);
                IEnumerator<DataTableDef> chain = table.TableChain.GetEnumerator();
                while (chain.MoveNext())
                {
                    if (chain.Current.TableType == DataTableType.Table && !chain.Current.Type.IsAbstract)
                    {
                        sb.AppendFormat(
                            "insert into {0}({1}) values(@{2})",
                            GetSafeName(chain.Current.TableName),
                            string.Join(",", GetSafeName(chain.Current.EditableColumns.Keys)),
                            string.Join(",@", chain.Current.EditableColumns.Values)
                        );
                        sb.AppendLine();

                        if (chain.Current.IdentityColumn != null)
                        {
                            sb.AppendFormat("select @@IDENTITY");
                            sb.AppendLine();
                            sb.AppendFormat("set @{0}=@@IDENTITY", chain.Current.IdentityColumn.Name);
                            sb.AppendLine();
                        }
                    }
                }

                m_insertSQL = sb.ToString();
                //InsertSQLCache[type] = m_insertSQL;
            }
            return m_insertSQL;
        }

        /// <summary>
        /// 获取按条件删除记录的SQL脚本
        /// </summary>
        /// <param name="type">模型类型</param>
        /// <param name="where">删除条件</param>
        /// <returns>记录删除脚本</returns>
        public string GetDeleteSQL(Type type, string where)
        {
            //string key = type.FullName + ":" + where;
            string m_deleteSQL;
            //if (!DeleteSQLCache.TryGetValue(key, out m_deleteSQL))
            {
                var set = GetSelectSql(type, where, null, null, false);
                m_deleteSQL = set + InternalGetDeleteSQL(type);
                m_deleteSQL = SupplementDropTemplateSql(m_deleteSQL);
                //DeleteSQLCache[key] = m_deleteSQL;
            }
            return m_deleteSQL;
        }

        /// <summary>
        /// 获取用于删除记录的SQL脚本
        /// </summary>
        /// <param name="type">模型类型</param>
        /// <returns>记录删除脚本</returns>
        public string GetDeleteSQL(Type type)
        {
            var table = DataTableDef.GetDataTable(type).TableChain.First();
            var conditions = new List<string>();
            foreach (var keyColumn in table.PrimaryKeyColumns)
            {
                conditions.Add(
                    string.Format(
                        "{0}.{1}=@{2}",
                        GetSafeName(table.TableName),
                        GetSafeName(keyColumn.ColumnName),
                        keyColumn.Name
                    ));
            }

            return GetDeleteSQL(type, string.Join(" and ", conditions));
        }

        /// <summary>
        /// 获取用于修改记录的SQL脚本
        /// </summary>
        /// <param name="type">模型类型</param>
        /// <param name="columnNames">要更新的字段集合</param>
        /// <param name="conditions">条件</param>
        /// <returns>记录修改脚本</returns>
        public string GetUpdateSQL(Type type, string[] columnNames, string conditions)
        {
            //string key = type.FullName;
            //if (columnNames != null && columnNames.Length > 0)
            //{
            //    List<string> sortedcolumnNames = new List<string>(columnNames);
            //    sortedcolumnNames.Sort();
            //    key += ":" + String.Join("|", sortedcolumnNames);
            //}
            //if (!String.IsNullOrWhiteSpace(conditions))
            //{
            //    key += ":" + conditions;
            //}
            string m_updateSQL;
            //if (!UpdateSQLCache.TryGetValue(key, out m_updateSQL))
            {
                var table = DataTableDef.GetDataTable(type);
                var deep = table.TableChain.Count;
                var sb = new StringBuilder(deep * 0x180);
                var chain = table.TableChain.Reverse<DataTableDef>().GetEnumerator();
                ;
                while (chain.MoveNext())
                {
                    if (chain.Current.TableType == DataTableType.Table)
                    {
                        var safeTableName = GetSafeName(chain.Current.TableName);
                        var updateColumns = new List<string>();
                        if (columnNames != null && columnNames.Length > 0)
                        {
                            foreach (var columnName in columnNames)
                            {
                                var clearColumnName = columnName.Replace("\"", "");
                                var isOperation = clearColumnName.IndexOf('=') > 0;
                                if (isOperation)
                                {
                                    clearColumnName = clearColumnName.Substring(0, clearColumnName.IndexOf('='));
                                }

                                string editableColumn;
                                if (clearColumnName.StartsWith(chain.Current.TableName,
                                    StringComparison.OrdinalIgnoreCase))
                                {
                                    clearColumnName = clearColumnName.Substring(chain.Current.TableName.Length + 1);

                                    if (chain.Current.EditableColumns.TryGetValue(clearColumnName, out editableColumn))
                                    {
                                        if (!chain.Current.ColumnDictionary[editableColumn].PrimaryKey)
                                        {
                                            if (isOperation)
                                            {
                                                updateColumns.Add(columnName);
                                            }
                                            else
                                            {
                                                updateColumns.Add(string.Format("{0}=@{1}", GetSafeName(columnName),
                                                    editableColumn));
                                            }
                                        }
                                    }
                                }
                                else if (chain.Current.EditableColumns.TryGetValue(clearColumnName, out editableColumn))
                                {
                                    if (!chain.Current.ColumnDictionary[editableColumn].PrimaryKey)
                                    {
                                        if (!chain.Current.ColumnDictionary[editableColumn].PrimaryKey)
                                        {
                                            if (isOperation)
                                            {
                                                updateColumns.Add(columnName);
                                            }
                                            else
                                            {
                                                updateColumns.Add(string.Format("{0}=@{1}", GetSafeName(columnName),
                                                    editableColumn));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (var column in chain.Current.EditableColumns)
                            {
                                if (!chain.Current.ColumnDictionary[column.Value].PrimaryKey)
                                {
                                    updateColumns.Add(string.Format("{0}=@{1}", GetSafeName(column.Key), column.Value));
                                }
                            }
                        }

                        if (updateColumns.Count > 0)
                        {
                            if (string.IsNullOrWhiteSpace(conditions))
                            {
                                var _conditions = new List<string>();
                                foreach (var keyColumn in chain.Current.PrimaryKeyColumns)
                                {
                                    _conditions.Add(
                                        string.Format(
                                            "{0}=@{1}",
                                            GetSafeName(keyColumn.ColumnName),
                                            keyColumn.Name
                                        ));
                                }

                                sb.AppendFormat(
                                    "update {0} set {1} where {2}",
                                    safeTableName,
                                    string.Join(",", updateColumns),
                                    string.Join(" and ", _conditions)
                                );
                            }
                            else
                            {
                                sb.AppendFormat(
                                    "update {0} set {1} where {2}",
                                    safeTableName,
                                    string.Join(",", updateColumns),
                                    conditions
                                );
                            }

                            sb.AppendLine();
                        }
                    }
                }

                m_updateSQL = fixColumnName(sb.ToString(), table.ColumnNames);
                //UpdateSQLCache[key] = m_updateSQL;
            }
            return m_updateSQL;
        }

        /// <summary>
        /// 获取还原备份的数据库的 SQL 代码
        /// </summary>
        /// <param name="backupFilename">备份路径名称</param>
        /// <param name="primary">备份的数据库原来的名称（数据库前缀）</param>
        /// <param name="suffix">数据库的名称后缀(企业GUID)</param>
        /// <param name="destDirectory">目标还原目录</param>
        /// <returns>还原备份的数据库的 SQL 代码</returns>
        public string GetRestoreDatabaseSQL(string backupFilename, string primary, string suffix, string destDirectory)
        {
            var restoreSql = @"
RESTORE DATABASE [{0}]
FROM  DISK = N'{1}'
WITH  FILE = 1,
MOVE N'{2}' TO N'{3}\{0}.mdf',  
MOVE N'{2}_log' TO N'{3}\{0}_log.ldf',  NOUNLOAD,  STATS = 10
select @@error
";
            return string.Format(restoreSql, primary + "." + suffix, backupFilename, primary, destDirectory);
        }

        /// <summary>
        /// 释放托管资源
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 获取安全名称
        /// </summary>
        /// <param name="name">非安全名称</param>
        /// <returns></returns>
        public string GetSafeName(string name)
        {
            return "\"" + NameSymbol.Replace(name, "").Replace(".", "\".\"") + "\"";
        }

        /// <summary>
        /// 获取安全名称
        /// </summary>
        /// <param name="names">非安全名称</param>
        /// <returns></returns>
        public IEnumerable<string> GetSafeName(IEnumerable<string> names)
        {
            var safeNames = new List<string>();
            foreach (var name in names)
            {
                safeNames.Add(GetSafeName(name));
            }

            return safeNames;
        }

        private string GetRandomString()
        {
            return new Random().Next(10000000, 99999999).ToString();
        }

        private string GetRownumColumnName(DataPager pager)
        {
            return string.Format(
                "row_number() over (order by {0} {1}) rownum",
                GetSafeName(pager.OrderColumn), pager.OrderType == DataOrderType.DESC ? "desc" : "asc"
            );
        }

        private string GetOrderCondition(DataPager pager)
        {
            if (string.IsNullOrWhiteSpace(pager.OrderColumn))
            {
                return string.Empty;
            }

            return string.Format(
                "order by {0} {1}",
                GetSafeName(pager.OrderColumn), pager.OrderType == DataOrderType.DESC ? "desc" : "asc"
            );
        }


        private string GetBetweenCondition(DataPager pager)
        {
            return string.Format(
                "(rownum between {0} and {1})",
                pager.PageSize * (pager.CurrentPage - 1) + 1,
                pager.PageSize * pager.CurrentPage
            );
        }

        private bool IsOperation(string column)
        {
            return operationRule.IsMatch(column);
        }

        private IEnumerable<string> GetOutColumnExNames(IEnumerable<string> outColumnNames, DataTableDef table)
        {
            var columnExNames = new List<string>(outColumnNames.Count());
            foreach (var outColumnName in outColumnNames)
            {
                columnExNames.Add(string.Format("{0} \"{1}\"", outColumnName,
                    table.ColumnMapping[outColumnName.ToLower()].Name));
            }

            return columnExNames;
        }

        private string GetSelectSql(Type type, string where, DataPager pager, DataTableDef parentTable, bool outSet,
            IEnumerable<string> columns = null)
        {
            var table = DataTableDef.GetDataTable(type);
            var deep = table.TableChain.Count();
            var sb = new StringBuilder(deep * 0x180);
            IEnumerator<DataTableDef> chain = table.TableChain.GetEnumerator();
            if (chain.MoveNext())
            {
                DataTableDef masterTable = null;
                var tempTablefixName = GetRandomString();
                var chainTableNames = new List<string>();
                IEnumerable<string> outColumnNames = table.ColumnNames;
                chainTableNames.Add(chain.Current.TableName);
                var groupby = string.Empty;
                if (columns != null)
                {
                    outColumnNames = outColumnNames.Intersect(columns);
                }

                masterTable = chain.Current;
                var ExNames = GetOutColumnExNames(outColumnNames, table);
                if (columns != null)
                {
                    var opts = columns.Except(outColumnNames).Where(opt => IsOperation(opt));
                    if (opts.Any())
                    {
                        opts = opts.Where(o => !string.IsNullOrWhiteSpace(o));
                        groupby = " group by " + string.Join(",", outColumnNames);
                        ExNames = ExNames.Concat(opts);
                    }
                }

                if (pager != null)
                {
                    //sb.AppendFormat(
                    //    "select {0}{1},{2} into #TEMP_{3} from {4}",
                    //    columns == null ? String.Empty : "distinct ",
                    //    GetRownumColumnName(pager),
                    //    String.Join(",", ExNames),
                    //    tempTablefixName,
                    //    GetSafeName(masterTable.TableName));
                    //sb.AppendLine();

                    sb.AppendFormat(
                        "select {0},{1} into #TEMP_{2} from {3}",
                        GetRownumColumnName(pager),
                        string.Join(",", ExNames),
                        tempTablefixName,
                        GetSafeName(masterTable.TableName));
                    sb.AppendLine();
                }
                else
                {
                    //sb.AppendFormat(
                    //    "select {0}{1} into #{2} from {3}",
                    //    columns == null ? String.Empty : "distinct ",
                    //    String.Join(",", ExNames),
                    //    table.Name,
                    //    GetSafeName(masterTable.TableName));
                    //sb.AppendLine();
                    sb.AppendFormat(
                        "select {0} into #{1} from {2}",
                        string.Join(",", ExNames),
                        table.Name,
                        GetSafeName(masterTable.TableName));
                    sb.AppendLine();
                }

                while (chain.MoveNext())
                {
                    if (chain.Current.TableType != DataTableType.Virtual)
                    {
                        var conditions = new List<string>();
                        chain.Current.ForeignKeyColumns.ForEach(fk =>
                        {
                            if (chainTableNames.Exists(tn =>
                                fk.MasterFullColumnName.StartsWith(tn, StringComparison.OrdinalIgnoreCase)))
                            {
                                conditions.Add(StringExtension.Format("{0}={1}", GetSafeName(fk.FullColumnName),
                                    GetSafeName(fk.MasterFullColumnName)));
                            }
                        });
                        if (conditions.Count > 0)
                        {
                            sb.AppendFormat(
                                "inner join {0} on {1}",
                                GetSafeName(chain.Current.TableName),
                                string.Join(" and ", conditions)
                            );
                            sb.AppendLine();
                        }

                        chainTableNames.Add(chain.Current.TableName);
                    }
                }

                if (parentTable != null)
                {
                    var foreignKeyColumns = table.FindChainForeignKeyColumnsInChain(parentTable);
                    if (foreignKeyColumns.Count > 0)
                    {
                        var conditions = new List<string>();
                        foreignKeyColumns.ForEach(fk =>
                        {
                            conditions.Add(StringExtension.Format(
                                "#{0}.{1}={2}",
                                parentTable.Name,
                                GetSafeName(
                                    fk.MasterFullColumnName.Substring(fk.MasterFullColumnName.LastIndexOf('.') + 1)),
                                GetSafeName(fk.FullColumnName)));
                        });
                        DataColumnDef deletedFlagColumn;
                        if (table.ColumnDictionary.TryGetValue("Deleted", out deletedFlagColumn))
                        {
                            conditions.Add(StringExtension.Format("{0}=0",
                                GetSafeName(deletedFlagColumn.FullColumnName)));
                        }

                        sb.AppendFormat(
                            //"inner join #{0} {0} on [{1}].[{2}]={3}",
                            "where exists (select top 1 1 from #{0} where {1})",
                            parentTable.Name,
                            string.Join(" AND ", conditions)
                        );
                        if (table.PrimaryKeyColumns.Any())
                        {
                            sb.AppendLine();
                            sb.AppendFormat(
                                "order by {0} asc",
                                string.Join(",", table.PrimaryKeyColumns.Select(c => GetSafeName(c.FullColumnName)))
                            );
                        }

                        sb.AppendLine();
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(where))
                    {
                        sb.AppendLine(" where " + where);
                    }
                }

                sb.AppendLine(groupby);

                if (pager != null)
                {
                    sb.AppendLine();
                    sb.AppendFormat(
                        "select count(1) TotalRecord from #TEMP_{0}", tempTablefixName);
                    sb.AppendLine();
                    sb.AppendFormat(
                        "select * into #{0} from #TEMP_{1} where (rownum between {2} and {3}) order by rownum asc",
                        table.Name,
                        tempTablefixName,
                        BeginBetweenVariableName,
                        EndBetweenVariableName
                    );
                }

                if (outSet)
                {
                    sb.AppendLine();
                    sb.AppendFormat("select * FROM #{0}", table.Name);
                }

                sb.AppendLine();
                IEnumerable<DataColumnDef> children = table.Children;
                //if (columns != null)
                //{
                //    children = children.Where(def => columns.Contains(def.FullColumnName));
                //}
                foreach (var child in children)
                {
                    if (child.DataType.GetInterface("IEnumerable") != null)
                    {
                        sb.AppendLine(
                            GetSelectSql(
                                child.DataType.GetGenericArguments()[0],
                                "0=1",
                                null,
                                columns == null || columns.Contains(child.FullColumnName) ? table : null,
                                outSet
                            )
                        );
                    }
                    else
                    {
                        sb.AppendLine(
                            GetSelectSql(
                                child.DataType,
                                "0=1",
                                null,
                                columns == null || columns.Contains(child.FullColumnName) ? table : null,
                                outSet
                            )
                        );
                    }
                }
            }

            return fixColumnName(sb.ToString(), table.ColumnNames);
        }

        private string fixColumnName(string sql, ICollection<string> columnNames)
        {
            var regex = new Regex(
                $@"\b({string.Join("|", columnNames.Select(column => column.Replace(".", "\\.")))})\b");
            return regex.Replace(sql,
                match => { return char.IsLetter(match.Value[0]) ? GetSafeName(match.Value) : match.Value; });
        }

        private string SupplementDropTemplateSql(string sql)
        {
            var supplementSelectSql = new StringBuilder(sql.Length * 2);
            supplementSelectSql.AppendLine(sql);
            foreach (Match macth in TemplateCreateRule.Matches(sql))
            {
                supplementSelectSql.AppendFormat("DROP TABLE {0}\r\n", macth.Groups[1].Value);
            }

            return supplementSelectSql.ToString();
        }

        private string InternalGetDeleteSQL(Type type)
        {
            var table = DataTableDef.GetDataTable(type);
            var deep = table.TableChain.Count();
            var sb = new StringBuilder(deep * 0x100);
            var children = table.Children;
            for (var i = children.Count; i > 0;)
            {
                var column = children[--i];
                if (column.DataType.GetInterface("IEnumerable") != null)
                {
                    sb.AppendLine(InternalGetDeleteSQL(column.DataType.GetGenericArguments()[0]));
                }
                else
                {
                    sb.AppendLine(InternalGetDeleteSQL(column.DataType));
                }
            }

            var masterTable = table.TableChain.Last();
            var chain = table.TableChain.Reverse<DataTableDef>().GetEnumerator();
            while (chain.MoveNext())
            {
                if (chain.Current.TableType == DataTableType.Table)
                {
                    var conditions = new List<string>();
                    foreach (var keyColumn in chain.Current.PrimaryKeyColumns)
                    {
                        conditions.Add(
                            string.Format(
                                "{0}.{1}=#{2}.{1}",
                                GetSafeName(chain.Current.TableName),
                                GetSafeName(keyColumn.ColumnName),
                                masterTable.Name
                            ));
                    }

                    sb.AppendFormat(
                        "delete {0} from #{1} where {2}",
                        GetSafeName(chain.Current.TableName),
                        masterTable.Name,
                        string.Join(" and ", conditions)
                    );
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }
    }
}