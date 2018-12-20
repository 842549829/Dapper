using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Db.Data
{
    /// <summary>
    /// Lambda表达式转换
    /// </summary>
    public static class LambdaHelper
    {
        /// <summary>
        /// SQL IN 查询的Lambda实现
        /// </summary>
        /// <typeparam name="T">字段数据类型的泛型</typeparam>
        /// <param name="obj">字段实体</param>
        /// <param name="array">IN 查询的参数集合</param>
        /// <returns>查询的结果</returns>
        public static bool In<T>(this T obj, IEnumerable<T> array)
        {
            return array.Contains(obj);
        }

        /// <summary>
        /// SQL IN 查询的Lambda实现
        /// </summary>
        /// <typeparam name="T">字段数据类型的泛型</typeparam>
        /// <param name="obj">字段实体</param>
        /// <param name="array">IN 查询的参数集合</param>
        /// <returns>查询的结果</returns>
        public static bool In<T>(this T obj, params T[] array)
        {
            return array.Contains(obj);
        }

        /// <summary>
        /// SQL NOT IN 查询的Lambda实现
        /// </summary>
        /// <typeparam name="T">字段数据类型的泛型</typeparam>
        /// <param name="obj">字段实体</param>
        /// <param name="array">IN 查询的参数集合</param>
        /// <returns>查询的结果</returns>
        public static bool NotIn<T>(this T obj, IEnumerable<T> array)
        {
            return !array.Contains(obj);
        }

        /// <summary>
        /// 复合型复杂条件匹配
        /// </summary>
        /// <typeparam name="T">模型数据类型的泛型</typeparam>
        /// <param name="obj">模型实体</param>
        /// <param name="exps">复杂条件的lambda表达式集合</param>
        /// <returns>查询的结果</returns>
        public static bool Complex<T>(this T obj, IEnumerable<Expression> exps)
        {
            return true;
        }

        /// <summary>
        /// 任意型复杂条件匹配
        /// </summary>
        /// <typeparam name="T">模型数据类型的泛型</typeparam>
        /// <param name="obj">模型实体</param>
        /// <param name="exps">复杂条件的lambda表达式集合</param>
        /// <returns>查询的结果</returns>
        public static bool Optional<T>(this T obj, IEnumerable<Expression> exps)
        {
            return true;
        }

        /// <summary>
        /// SQL NOT IN 查询的Lambda实现
        /// </summary>
        /// <typeparam name="T">字段数据类型的泛型</typeparam>
        /// <param name="obj">字段的对象引用</param>
        /// <param name="array">IN 查询的参数集合</param>
        /// <returns>查询的结果</returns>
        public static bool NotIn<T>(this T obj, params T[] array)
        {
            return !array.Contains(obj);
        }

        /// <summary>
        /// 字段[集合]的Lambda实现
        /// </summary>
        /// <typeparam name="T">数据表模型类型的泛型</typeparam>
        /// <param name="obj">模型的对象引用</param>
        /// <param name="array">要包含的Lambda形式的字段集合</param>
        /// <returns>查询的结果</returns>
        public static string With<T>(this T obj, params object[] array)
        {
            return null;
        }

        /// <summary>
        /// 字段排除[集合]的Lambda实现
        /// </summary>
        /// <typeparam name="T">数据表模型类型的泛型</typeparam>
        /// <param name="obj">模型的对象引用</param>
        /// <param name="array">要排除的Lambda形式的字段集合</param>
        /// <returns>查询的结果</returns>
        public static string Without<T>(this T obj, params object[] array)
        {
            return null;
        }


        /// <summary>
        /// 排除字段[集合]的Lambda实现
        /// </summary>
        /// <typeparam name="T">数据表模型类型的泛型</typeparam>
        /// <param name="obj">模型的对象引用</param>
        /// <param name="array">要排除的Lambda形式的字段集合</param>
        /// <returns>查询的结果</returns>
        public static string Exclude<T>(this T obj, params object[] array)
        {
            return null;
        }

        /// <summary>
        /// SQL Update 字段的运算实现
        /// </summary>
        /// <typeparam name="T">数据表模型类型的泛型</typeparam>
        /// <param name="obj">模型的对象引用</param>
        /// <param name="type">字段运算类型</param>
        /// <returns>查询的结果</returns>
        public static object Operation<T>(this T obj, OperationType type)
        {
            return null;
        }

        /// <summary>
        /// SQL Like '%?%' 查询的Lambda实现
        /// </summary>
        /// <param name="str">字段的对象引用</param>
        /// <param name="likeStr">Like 查询的参数</param>
        /// <returns>查询的结果</returns>
        public static bool Like(this string str, string likeStr)
        {
            return true;
        }

        /// <summary>
        /// SQL Len(???) 查询的实现
        /// 针对查询字段使用Len函数 sql
        /// </summary>
        /// <param name="str"></param>
        /// <param name="lenStr"></param>
        /// <returns></returns>
        public static bool Len(this string str, int lenStr)
        {
            return true;
        }

        /// <summary>
        /// SQL NOT Like '%?%' 查询的Lambda实现
        /// </summary>
        /// <param name="str">字段的对象引用</param>
        /// <param name="likeStr">Like 查询的参数</param>
        /// <returns>查询的结果</returns>
        public static bool NotLike(this string str, string likeStr)
        {
            return true;
        }

        /// <summary>
        /// 解析lambda表达式
        /// </summary>
        /// <param name="compatible">compatible</param>
        /// <param name="lambda">lambda 表达式</param>
        /// <returns>lambda表达式解析器</returns>
        internal static ILambdaParser Parse(IDataCompatible compatible, LambdaExpression lambda)
        {
            if (lambda != null)
            {
                var type = lambda.GetType().GetGenericArguments()[0].GetGenericArguments()[0];
                var parser = new LambdaParser(compatible, type);
                if (lambda.Body is BinaryExpression be)
                {
                    parser.Parse(be);
                }
                else
                {
                    parser.Parse(lambda);
                }

                return parser;
            }

            return new LambdaParser(compatible, null);
        }

        /// <summary>
        /// 解析lambda表达式
        /// </summary>
        /// <param name="lambda">lambda 表达式</param>
        /// <returns>lambda表达式解析器</returns>
        internal static ILambdaParser Parse(LambdaExpression lambda)
        {
            return Parse(null, lambda);
        }

        internal static string ConvertToString(object value)
        {
            if (value == null) return "''";
            var type = value.GetType();
            if (type.IsEnum) return Convert.ToInt32(value).ToString();
            if (type == typeof(string)) return $"'{value.ToString().Replace("'", "''")}'";
            if (type == typeof(Guid)) return $"'{value}'";
            if (type == typeof(bool)) return value.Equals(true) ? "1" : "0";
            return value.ToString();
        }

        /// <summary>
        /// Lambda表达式解析器
        /// </summary>
        private sealed class LambdaParser : ILambdaParser
        {
            private readonly IDataCompatible m_compatible;

            private readonly List<DataParameter> m_parameters = new List<DataParameter>();
            private readonly DataTableDef m_tableDef;

            /// <summary>
            /// Lambda表达式解析器构造函数
            /// </summary>
            /// <param name="compatible">数据库兼容性接口实现</param>
            /// <param name="type">Lambda表达式的输入类型</param>
            internal LambdaParser(IDataCompatible compatible, Type type)
            {
                m_compatible = compatible;
                if (type != null)
                {
                    m_tableDef = DataTableDef.GetDataTable(type);
                }
            }

            /// <summary>
            /// Lambda表达式解析结果SQL语句
            /// </summary>
            public string SqlString { get; private set; } = string.Empty;

            /// <summary>
            /// Lambda表达式解析结果参数集
            /// </summary>
            public IEnumerable<DataParameter> Parameters => m_parameters;

            /// <summary>
            /// 解析Lambda表达式
            /// </summary>
            /// <param name="be">Lambda表达式</param>
            internal void Parse(LambdaExpression be)
            {
                SqlString = ExpressionParse(be.Body);
            }

            /// <summary>
            /// 解析二元运算
            /// </summary>
            /// <param name="be">二元运算表达式</param>
            internal void Parse(BinaryExpression be)
            {
                SqlString = ExpressionParse(be.Left, be.Right, be.NodeType);
            }

            /// <summary>
            /// 解析二元运算
            /// </summary>
            /// <param name="left">二元运算的左边表达式</param>
            /// <param name="right">二元运算的右边表达式</param>
            /// <param name="type">二元运算的关系运算符</param>
            /// <returns></returns>
            private string ExpressionParse(Expression left, Expression right, ExpressionType type)
            {
                //处理左边
                var leftExpressionStr = ExpressionParse(left);
                //处理关系运算符
                var expressionTypeCast = ExpressionTypeCast(type);
                //处理右边
                string columnName = null;
                var column = FindColumnDef(left);
                if (column != null)
                {
                    columnName = column.ColumnName;
                }
               

                var rightExpressionStr = columnName == null ? ExpressionParse(right) : ExpressionParse(right, null, columnName);
                
                var parameter = m_parameters.SingleOrDefault(n => n.ParameterName == rightExpressionStr);
                if (rightExpressionStr == null || parameter != null && parameter.Value == DBNull.Value)
                {
                    if (expressionTypeCast == "=")
                    {
                        expressionTypeCast = string.Empty;
                        rightExpressionStr = " is null";
                        if (parameter != null) m_parameters.Remove(parameter);
                    }
                    else if (expressionTypeCast == "<>")
                    {
                        expressionTypeCast = string.Empty;
                        rightExpressionStr = " is not null";
                        if (parameter != null) m_parameters.Remove(parameter);
                    }
                }

                return StringExtension.Format("({0}{1}{2})", leftExpressionStr, expressionTypeCast, rightExpressionStr);
            }

            /// <summary>
            /// 解析Lambda表达式的结果值
            /// </summary>
            /// <param name="exp">lambda表达式</param>
            /// <returns>结果值</returns>
            private object ExpressionValue(Expression exp)
            {
                //方法表达式
                if (exp is MethodCallExpression)
                {
                    var mce = (MethodCallExpression)exp;
                    var arguments = new object[mce.Arguments.Count];
                    for (var i = 0; i < mce.Arguments.Count; i++)
                    {
                        arguments[i] = ExpressionValue(mce.Arguments[i]);
                    }

                    return mce.Method.Invoke(ExpressionValue(mce.Object), arguments);
                }
                //字段、属性表达式

                if (exp is MemberExpression)
                {
                    var me = (MemberExpression)exp;
                    object obj = null;
                    if (me.Expression != null)
                    {
                        obj = ExpressionValue(me.Expression);
                        if (obj == null)
                        {
                            return null;
                        }
                    }

                    if (me.Member.MemberType == MemberTypes.Property)
                    {
                        return ((PropertyInfo)me.Member).GetValue(obj, null);
                    }

                    return ((FieldInfo)me.Member).GetValue(obj);
                }
                //值类型表达式

                if (exp is ConstantExpression)
                {
                    var ce = (ConstantExpression)exp;
                    return ce.Value;
                }

                return null;
            }

            /// <summary>
            /// 查找表达式对应的数据库字段的完整表达形式
            /// </summary>
            /// <param name="exp"></param>
            /// <returns></returns>
            private DataColumnDef FindColumnDef(Expression exp)
            {
                if (exp is MemberExpression)
                {
                    var me = (MemberExpression)exp;
                    if (me.Expression != null)
                    {
                        if (me.Expression.NodeType == ExpressionType.Parameter)
                        {
                            return m_tableDef.Columns.FirstOrDefault(o => o.Name == me.Member.Name);
                        }

                        if (me.Expression.NodeType == ExpressionType.MemberAccess)
                        {
                            var column = FindColumnDef(me.Expression);
                            if (column != null)
                            {
                                if (me.Expression.Type.Name.Equals("Nullable`1"))
                                {
                                    return column;
                                }

                                var _tableDef = DataTableDef.GetDataTable(column.DataType);
                                return _tableDef.Columns.FirstOrDefault(o => o.Name == me.Member.Name);
                            }
                        }
                        else if (me.Expression.NodeType == ExpressionType.Convert)
                        {
                            var ue = (UnaryExpression)me.Expression;
                            var _tableDef = DataTableDef.GetDataTable(ue.Operand.Type);
                            return _tableDef.Columns.FirstOrDefault(o => o.Name == me.Member.Name);
                        }
                    }
                }

                return null;
            }

            /// <summary>
            /// 解析Lambda表达式
            /// </summary>
            /// <param name="exp">Lambda表达式</param>
            /// <param name="format">解析结果的格式化字符串</param>
            /// <param name="parameter">参数名</param>
            /// <returns>Lambda表达式的解析结果字符串</returns>
            private string ExpressionParse(Expression exp, string format = null, string parameter = null)
            {
                //二元表达式
                if (exp is BinaryExpression be)
                {
                    return ExpressionParse(be.Left, be.Right, be.NodeType);
                }
                //字段、属性表达式

                if (exp is MemberExpression me)
                {
                    var column = FindColumnDef(me);
                    if (column != null)
                    {
                        return column.ColumnName;
                    }

                    var parameterValue = ExpressionValue(me);
                    if (format != null && parameterValue is string)
                    {
                        parameterValue = string.Format(format, parameterValue);
                    }

                    if (parameterValue != null)
                    {
                        if (!(parameterValue is string) && parameterValue is IEnumerable)
                        {
                            var parameterValues = ((IEnumerable)parameterValue).GetEnumerator();
                            var parameterNames = new List<string>();
                            while (parameterValues.MoveNext())
                            {
                                var parameterName = string.Format("{0}P{1}", m_compatible.ParameterPrefixName,
                                    m_parameters.Count);
                                if (parameter != null)
                                {
                                    parameterName = $"{m_compatible.ParameterPrefixName}{parameter}";
                                }
                                parameterNames.Add(parameterName);
                                m_parameters.Add(new DataParameter(parameterName,
                                    parameterValues.Current ?? DBNull.Value));
                            }

                            return string.Join(",", parameterNames);
                        }

                        {
                            var parameterName = string.Format("{0}P{1}", m_compatible.ParameterPrefixName,m_parameters.Count);
                            if (parameter != null)
                            {
                                parameterName = $"{m_compatible.ParameterPrefixName}{parameter}";
                            }
                            m_parameters.Add(new DataParameter(parameterName, parameterValue));
                            return parameterName;
                        }
                    }

                    return null;
                }
                //创建数组的表达式

                if (exp is NewArrayExpression ae1)
                {
                    var tmpstr = new StringBuilder();
                    foreach (var ex in ae1.Expressions)
                    {
                        tmpstr.Append(ExpressionParse(ex));
                        tmpstr.Append(",");
                    }

                    return tmpstr.Length > 0 ? tmpstr.ToString(0, tmpstr.Length - 1) : "";
                }
                //构造函数表达式

                if (exp is NewExpression ae)
                {
                    var exts = new string[ae.Arguments.Count];
                    for (var i = 0; i < exts.Length; i++)
                    {
                        exts[i] = ExpressionParse(ae.Arguments[i]);
                    }

                    return string.Join(",", exts);
                }
                //方法调用表达式

                if (exp is MethodCallExpression mce)
                {
                    switch (mce.Method.Name)
                    {
                        case "Like":
                            return string.Format("({0} like {1})", ExpressionParse(mce.Arguments[0]),
                                ExpressionParse(mce.Arguments[1]));
                        case "NotLike":
                            return string.Format("({0} Not like {1})", ExpressionParse(mce.Arguments[0]),
                                ExpressionParse(mce.Arguments[1]));
                        case "Contains":
                            return string.Format("({0} like {1})", ExpressionParse(mce.Object),
                                ExpressionParse(mce.Arguments[0], "%{0}%"));
                        case "StartsWith":
                            return string.Format("({0} like {1})", ExpressionParse(mce.Object),
                                ExpressionParse(mce.Arguments[0], "{0}%"));
                        case "EndsWith":
                            return string.Format("({0} like {1})", ExpressionParse(mce.Object),
                                ExpressionParse(mce.Arguments[0], "%{0}"));
                        case "In":
                            {
                                var parameters = ExpressionParse(mce.Arguments[1]);
                                if (!string.IsNullOrEmpty(parameters))
                                {
                                    var _parameterNames =
                                        parameters.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                    var _parameters = m_parameters.GetRange(m_parameters.Count - _parameterNames.Length,
                                        _parameterNames.Length);
                                    var str_parameters = _parameters.Select(p => ConvertToString(p.Value));
                                    m_parameters.RemoveRange(m_parameters.Count - _parameterNames.Length,
                                        _parameterNames.Length);
                                    return string.Format("({0} In ({1}))", ExpressionParse(mce.Arguments[0]),
                                        string.Join(",", str_parameters));
                                }

                                return "(1<>1)";
                            }
                        case "NotIn":
                            {
                                var parameters = ExpressionParse(mce.Arguments[1]);
                                if (!string.IsNullOrEmpty(parameters))
                                {
                                    var _parameterNames =
                                        parameters.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                    var _parameters = m_parameters.GetRange(m_parameters.Count - _parameterNames.Length,
                                        _parameterNames.Length);
                                    var str_parameters = _parameters.Select(p => ConvertToString(p.Value));
                                    m_parameters.RemoveRange(m_parameters.Count - _parameterNames.Length,
                                        _parameterNames.Length);
                                    return string.Format("({0} Not In ({1}))", ExpressionParse(mce.Arguments[0]),
                                        string.Join(",", str_parameters));
                                }

                                return "(1=1)";
                            }
                        case "With":
                            return ExpressionParse(mce.Arguments[1]);
                        case "Exclude":
                        case "Without":
                            var without = ExpressionParse(mce.Arguments[1]).Split(',');
                            var cloumns = DataTableDef.GetDataTable(mce.Arguments[0].Type).Columns
                                .Select(column => column.FullColumnName);
                            return string.Join(",", cloumns.Except(without));
                        case "Operation":
                            {
                                var column = FindColumnDef(mce.Arguments[0]);
                                var type = (OperationType)ExpressionValue(mce.Arguments[1]);
                                switch (type)
                                {
                                    case OperationType.Add:
                                        return string.Format("{0}={0}+{1}{2}", column.FullColumnName,
                                            m_compatible.ParameterPrefixName, column.Name);
                                    case OperationType.Subtract:
                                        return string.Format("{0}={0}-{1}{2}", column.FullColumnName,
                                            m_compatible.ParameterPrefixName, column.Name);
                                    case OperationType.Multiply:
                                        return string.Format("{0}={0}*{1}{2}", column.FullColumnName,
                                            m_compatible.ParameterPrefixName, column.Name);
                                    case OperationType.Divide:
                                        return string.Format("{0}={0}/{1}{2}", column.FullColumnName,
                                            m_compatible.ParameterPrefixName, column.Name);
                                    case OperationType.Max:
                                        return string.Format("Max({0}) {1}", column.FullColumnName, column.Name);
                                    case OperationType.Min:
                                        return string.Format("Min({0}) {1}", column.FullColumnName, column.Name);
                                    case OperationType.Avg:
                                        return string.Format("Avg({0}) {1}", column.FullColumnName, column.Name);
                                    case OperationType.Sum:
                                        return string.Format("Sum({0}) {1}", column.FullColumnName, column.Name);
                                    case OperationType.Count:
                                        return string.Format("Count({0}) {1}", column.FullColumnName, column.Name);
                                    default:
                                        return string.Format("{0}={0}%{1}{2}", column.FullColumnName,
                                            m_compatible.ParameterPrefixName, column.Name);
                                }
                            }
                        case "Complex":
                        case "Optional":
                            var complex = new List<string>();
                            var exps = ExpressionValue(mce.Arguments[1]) as IList;
                            foreach (var item in exps)
                            {
                                complex.Add(ExpressionParse((item as LambdaExpression)?.Body));
                            }

                            return string.Format("({0})",
                                string.Join(mce.Method.Name == "Complex" ? " And " : " Or ", complex));
                        case "Len": //针对查询字段添加 len函数
                            return string.Format("(len({0})={1})", ExpressionParse(mce.Arguments[0]),
                                ExpressionParse(mce.Arguments[1]));
                        default:
                            {
                                var parameterName = string.Format("{0}P{1}", m_compatible.ParameterPrefixName,
                                    m_parameters.Count);
                                if (parameter != null)
                                {
                                    parameterName = $"{m_compatible.ParameterPrefixName}{parameter}";
                                }
                                var parameterValue = ExpressionValue(mce);
                                m_parameters.Add(new DataParameter(parameterName, parameterValue ?? DBNull.Value));
                                return parameterName;
                            }
                    }
                }
                //常量值表达式

                if (exp is ConstantExpression ce)
                {
                    string parameterName = string.Format("{0}P{1}", m_compatible.ParameterPrefixName, m_parameters.Count);
                    if (parameter != null)
                    {
                        parameterName = $"{m_compatible.ParameterPrefixName}{parameter}";
                    }
                    var parameterValue = ce.Value;

                    if (format != null && parameterValue is string)
                    {
                        parameterValue = string.Format(format, parameterValue);
                    }

                    m_parameters.Add(new DataParameter(parameterName, parameterValue ?? DBNull.Value));
                    return parameterName;
                }
                //一元运算表达式

                if (exp is UnaryExpression ue)
                {
                    return ExpressionParse(ue.Operand);
                }

                return null;
            }

            /// <summary>
            /// 解析关系运算符
            /// </summary>
            /// <param name="type">关系运算符类型</param>
            /// <returns>关系运算符的字符串形式</returns>
            private string ExpressionTypeCast(ExpressionType type)
            {
                switch (type)
                {
                    case ExpressionType.And:
                        return " & ";
                    case ExpressionType.AndAlso:
                        return " AND ";
                    case ExpressionType.Equal:
                        return "=";
                    case ExpressionType.GreaterThan:
                        return ">";
                    case ExpressionType.GreaterThanOrEqual:
                        return ">=";
                    case ExpressionType.LessThan:
                        return "<";
                    case ExpressionType.LessThanOrEqual:
                        return "<=";
                    case ExpressionType.NotEqual:
                        return "<>";
                    case ExpressionType.Or:
                        return " | ";
                    case ExpressionType.OrElse:
                        return " Or ";
                    case ExpressionType.Add:
                    case ExpressionType.AddChecked:
                        return "+";
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractChecked:
                        return "-";
                    case ExpressionType.Divide:
                        return "/";
                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyChecked:
                        return "*";
                    default:
                        return null;
                }
            }
        }
    }
}