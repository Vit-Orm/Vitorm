using App.OrmRunner.SqlSugerRunner;

using SqlSugar;

using Vit.Core.Util.ConfigurationManager;

namespace App.OrmRunner
{
    public partial class Runner_SqlSuger : IRunner
    {
        static string provider = Appsettings.json.GetStringByPath("Vitorm.Data[0].provider");
        static string connectionString = Appsettings.json.GetStringByPath("Vitorm.Data[0].connectionString");
        public void Run(RunConfig config)
        {

            for (int i = 0; i < config.repeatCount; i++)
            {

                Action<SqlSugarClient> configAction;
                if (config.executeQuery)
                {
                    configAction = db => { };
                }
                else
                {
                    configAction = db =>
                    {
                        db.Aop.OnLogExecuting = (sql, pars) =>
                        {
                            //Console.WriteLine(sql);

                            var nativeSql = UtilMethods.GetNativeSql(sql, pars);
                            //Console.WriteLine(nativeSql);
                            sql = nativeSql;

                            //var sqlString = UtilMethods.GetSqlString(DbType.SqlServer, sql, pars);
                            //Console.WriteLine(sqlString);
                        };
                    };
                }
                using SqlSugarClient db = new SqlSugarClient(new ConnectionConfig
                {
                    DbType = provider switch { "Sqlite" => DbType.Sqlite, "MySql" => DbType.MySql, "SqlServer" => DbType.SqlServer },
                    ConnectionString = connectionString,
                    IsAutoCloseConnection = true,
                }, configAction);


                if (config.queryJoin) QueryExecute.QueryJoin(db, config);
                else QueryExecute.Query(db, config);
            }
        }
    }
}



namespace App.OrmRunner.SqlSugerRunner
{

    // Entity Definition
    [SugarTable("User")]
    public class User
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public int id { get; set; }
        public string name { get; set; }
        public DateTime? birth { get; set; }
        public int? fatherId { get; set; }
        public int? motherId { get; set; }
    }



    public class QueryExecute
    {

        public static void QueryJoin(SqlSugarClient db, RunConfig config)
        {
            var query = db.Queryable<User>().LeftJoin<User>((user, father) => user.fatherId == father.id)
                .LeftJoin<User>((user, father, mother) => user.fatherId == mother.id)
                .Where((user, father, mother) => user.id > 1 && user.id < 10000)
                .OrderBy((user, father, mother) => user.id, OrderByType.Asc)
                .Select((user, father, mother) =>
                new
                {
                    user,
                    father,
                    mother,
                    testId = user.id + 100,
                    hasFather = father.name != null ? true : false
                });


            Execute(query, config);
        }

        public static void Query(SqlSugarClient db, RunConfig config)
        {
            var query = db.Queryable<User>().Where(user => user.id > 1 && user.id < 10000).OrderBy(user => user.id, OrderByType.Asc);

            Execute(query, config);
        }


        public static void Execute<Result>(ISugarQueryable<Result> query, RunConfig config)
        {
            if (config.skip > 0) query = query.Skip(config.skip.Value);
            query = query.Take(config.take);

            if (config.executeQuery)
            {
                var userList = query.ToList();
                var rowCount = userList.Count();
                if (rowCount != config.take) throw new Exception($"query failed, expected row count : {config.take} , actual count: {rowCount} ");
            }
            else
            {
                var count = query.Count();
                //query.Single();

                //if (string.IsNullOrEmpty(sql))
                //    throw new Exception($"query failed, can not generated sql script");
            }
        }



    }
}
