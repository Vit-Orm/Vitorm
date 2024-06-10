using System;
using System.Linq;
using Vit.Linq.ExpressionTree.ComponentModel;
using Vit.Orm.DataReader;
using Vit.Orm.Sql.DataReader;
using Vit.Linq.ExpressionTree.CollectionsQuery;
using Vit.Extensions.Linq_Extensions;
using System.IO;


namespace Vit.Orm.Sql.SqlTranslate
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


        protected override string ReadSelect(QueryTranslateArgument arg, CombinedStream stream, string prefix = "select")
        {
            switch (stream.method)
            {
                case "Count":
                    {
                        var reader = new NumScalarReader();
                        if (arg.dataReader == null) arg.dataReader = reader;
                        return prefix + " " + "count(*)";
                    }
                case "" or null or "ToList" or nameof(Queryable_Extensions.ToExecuteString):
                    {
                        var reader = new EntityReader();
                        return prefix + " " + BuildReader(arg, stream, reader);
                    }
                case "FirstOrDefault" or "First" or "LastOrDefault" or "Last":
                    {
                        stream.take = 1;
                        stream.skip = null;

                        if (stream.method.Contains("Last"))
                            ReverseOrder(arg, stream);

                        var nullable = stream.method.Contains("OrDefault");
                        var reader = new FirstEntityReader { nullable = nullable };
                        return prefix + " " + BuildReader(arg,stream,reader);
                    }
            }
            throw new NotSupportedException("not supported method: " + stream.method);
        }
     


    }
}
