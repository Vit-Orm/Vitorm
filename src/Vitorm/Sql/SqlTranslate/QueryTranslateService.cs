using System;

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
            return $"select count(*) from ({BuildQuery(arg, stream)}) u;";
        }

        protected override string ReadSelect(QueryTranslateArgument arg, CombinedStream stream, string prefix = "select")
        {
            switch (stream.method)
            {
                case "Count":
                case "" or null or "ToList" or nameof(Orm_Extensions.ToExecuteString):
                    {
                        var reader = new DataReader.DataReader();
                        return prefix + " " + BuildReader(arg, stream, reader);
                    }
                case "FirstOrDefault" or "First" or "LastOrDefault" or "Last":
                    {
                        stream.take = 1;
                        stream.skip = null;

                        if (stream.method.Contains("Last"))
                            ReverseOrder(arg, stream);

                        var nullable = stream.method.Contains("OrDefault");
                        var reader = new DataReader_FirstRow { nullable = nullable };
                        return prefix + " " + BuildReader(arg, stream, reader);
                    }
            }
            throw new NotSupportedException("not supported method: " + stream.method);
        }



    }
}
