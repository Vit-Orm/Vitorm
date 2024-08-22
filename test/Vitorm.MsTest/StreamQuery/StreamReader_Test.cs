using System.Linq.Expressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Vit.Core.Module.Serialization;
using Vit.Linq;
using Vit.Linq.ExpressionNodes;
using Vit.Linq.ExpressionNodes.ComponentModel;

using Vitorm.StreamQuery;

namespace Vitorm.MsTest.StreamQuery
{

    [TestClass]
    public class StreamReader_Test
    {
        IQueryable<int> GetQuery()
        {
            var convertService = ExpressionConvertService.Instance;
            var queryTypeName = "TestQuery";

            Func<Expression, Type, object> QueryExecutor = (expression, type) =>
            {
                ExpressionNode_Lambda node;

                // #1 Code to Data
                // query => query.Where().OrderBy().Skip().Take().Select().ToList();
                var isArgument = QueryableBuilder.CompareQueryByName(queryTypeName);
                node = convertService.ConvertToData_LambdaNode(expression, autoReduce: true, isArgument: isArgument);
                var strNode = Json.Serialize(node);


                // #2 convert to CombinedStream
                var streamReader = new Vitorm.StreamQuery.StreamReader();
                streamReader.AddMethodCallConvertor(Queryable_Extensions_Batch.Convert);

                var stream = streamReader.ReadFromNode(node) as CombinedStream;
                var strStream = Json.Serialize(stream);

                if (nameof(Queryable_Extensions_Batch.Batch) == stream.method)
                {
                    var batchSize = (int)stream.methodArguments[0];
                    var result = Enumerable.Repeat(0, 10).Select(i => Enumerable.Range(0, batchSize).ToList());
                    return result;
                }

                throw new NotSupportedException("Method not support:" + stream.method);
            };

            return QueryableBuilder.Build<int>(QueryExecutor, queryTypeName);
        }


        [TestMethod]
        public void Test()
        {
            var query = GetQuery();

            var result = query.Batch(15);

            Assert.AreEqual(10, result.Count());

            foreach (var list in result)
            {
                Assert.AreEqual(15, list.Count);
            }

        }


    }
}
