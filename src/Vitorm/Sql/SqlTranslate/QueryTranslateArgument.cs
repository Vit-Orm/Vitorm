using System;
using System.Collections.Generic;
using System.Linq;

using Vit.Linq.ExpressionNodes.ComponentModel;

namespace Vitorm.Sql.SqlTranslate
{
    public class QueryTranslateArgument
    {
        public SqlDbContext dbContext { get; protected set; }

        public Type resultEntityType { get; protected set; }


        public QueryTranslateArgument(SqlDbContext dbContext, Type resultEntityType)
        {
            this.dbContext = dbContext;
            this.resultEntityType = resultEntityType;
        }



        public IDbDataReader dataReader { get; set; }

        public Dictionary<string, object> sqlParam { get; protected set; }

        protected int paramIndex = 0;

        /// <summary>
        /// add sqlParam and get the generated sqlParam name
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string AddParamAndGetName(object value)
        {
            sqlParam ??= new();
            var paramName = "p" + (paramIndex++);

            sqlParam[paramName] = value;
            return paramName;
        }


        protected Stack<ExpressionNode> expressionTree;
        public IDisposable ExpressionTree_Push(ExpressionNode node)
        {
            expressionTree ??= new();
            expressionTree.Push(node);
            return expressionTreeDisposable ??= new Disposable { OnDispose = ExpressionTree_Pop };
        }
        protected IDisposable expressionTreeDisposable;
        protected void ExpressionTree_Pop()
        {
            expressionTree?.Pop();
        }
        public ExpressionNode ExpressionTree_Current()
        {
            if (expressionTree?.Any() == true)
                return expressionTree.Peek();
            return default;
        }
        class Disposable : IDisposable
        {
            public Action OnDispose;
            public void Dispose() => OnDispose?.Invoke();
        }

    }
}
