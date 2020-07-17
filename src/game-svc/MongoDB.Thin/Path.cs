using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MongoDB.Thin
{
    public static class Path
    {
        public static string From<TDocument, TField>(Expression<Func<TDocument, TField>> targetField)
        {
            var memberExpression = targetField.Body as MemberExpression;
            if (memberExpression == null)
            {
                var unaryExpression = targetField.Body as UnaryExpression;
                if (unaryExpression != null && unaryExpression.NodeType == ExpressionType.Convert)
                    memberExpression = unaryExpression.Operand as MemberExpression;
            }

            if (memberExpression == null) throw new InvalidOperationException();

            var result = memberExpression.ToString();
            result = result.Substring(result.IndexOf('.') + 1);

            return result;
        }

        public static string From<TDocument, TArrayDocument>(
            Expression<Func<TDocument, IEnumerable<TArrayDocument>>> targetArray, Expression<Func<TArrayDocument, object>> targetField)
        {
            return
                From(targetArray) +
                "." +
                From(targetField);
        }
    }
}
