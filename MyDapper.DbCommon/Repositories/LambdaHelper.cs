using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MyDapper.DbCommon.Repositories
{
    public static class LambdaHelper
    {
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

        public static string Parse(LambdaExpression lambda)
        {
            return ExpressionParse(lambda.Body);
        }

        /// <summary>
        /// 解析Lambda表达式
        /// </summary>
        /// <param name="exp">Lambda表达式</param>
        /// <returns>Lambda表达式的解析结果字符串</returns>
        private static string ExpressionParse(Expression exp)
        {
            //二元表达式
            if (exp is BinaryExpression be)
            {
                return ExpressionParse(be.Left);
            }
            //字段、属性表达式

            if (exp is MemberExpression me)
            {
                var column = FindColumnDef(me);
                return column;
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
                   
                    case "With":
                        return ExpressionParse(mce.Arguments[1]);
                    default:
                        return null;
                }
            }

            //一元运算表达式
            if (exp is UnaryExpression ue)
            {
                return ExpressionParse(ue.Operand);
            }

            return null;
        }

        private static string FindColumnDef(Expression exp)
        {
            if (exp is MemberExpression)
            {
                var me = (MemberExpression)exp;
                if (me.Expression != null)
                {
                    if (me.Expression.NodeType == ExpressionType.Parameter)
                    {
                        return me.Member.Name;
                    }
                }
            }

            return null;
        }
    }
}
