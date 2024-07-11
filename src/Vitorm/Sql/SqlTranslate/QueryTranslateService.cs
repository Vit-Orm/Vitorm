using System;
using System.Linq;

using Vit.Linq;

using Vitorm.Sql.DataReader;
using Vitorm.StreamQuery;

namespace Vitorm.Sql.SqlTranslate
{
    public class QueryTranslateService : BaseQueryTranslateService
    {
        /* //sql
        select u.id, u.name, u.birth ,u.fatherId ,u.motherId,    father.name,  mother.name
        from User u
        inner join User father on u.fatherId = father.id 
        left join User mother on u.motherId = mother.id
        where u.id > 1
        limit 1,5;
         */




        public QueryTranslateService(SqlTranslateService sqlTranslator) : base(sqlTranslator)
        {
        }


        public override string BuildCountQuery(QueryTranslateArgument arg, CombinedStream stream)
        {
            // select count(*) from (select distinct fatherid,motherId from "User" u) u;
            return $"select count(*) from ({BuildQuery(arg, stream)}) u";
        }

        protected override string ReadSelect(QueryTranslateArgument arg, CombinedStream stream, string prefix = "select")
        {
            switch (stream.method)
            {
                case nameof(Queryable.Count):
                case "" or null or nameof(Enumerable.ToList) or nameof(Orm_Extensions.ToExecuteString):
                case nameof(Queryable_Extensions.ToListAndTotalCount) or nameof(Queryable_Extensions.TotalCount):
                    {
                        var reader = new DataReader.DataReader();
                        return prefix + " " + BuildDataReader(arg, stream, reader);
                    }
                case nameof(Queryable.FirstOrDefault) or nameof(Queryable.First) or nameof(Queryable.LastOrDefault) or nameof(Queryable.Last):
                    {
                        stream.take = 1;
                        stream.skip = null;

                        if (stream.method.Contains("Last"))
                            ReverseOrder(arg, stream);

                        var nullable = stream.method.Contains("OrDefault");
                        var reader = new DataReader_FirstRow { nullable = nullable };
                        return prefix + " " + BuildDataReader(arg, stream, reader);
                    }
            }
            throw new NotSupportedException("not supported method: " + stream.method);
        }



    }
}
