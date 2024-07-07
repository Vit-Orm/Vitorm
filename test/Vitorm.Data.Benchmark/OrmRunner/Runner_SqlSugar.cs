using SqlSugar;

using Vit.Core.Util.ConfigurationManager;


namespace App.OrmRunner
{
    public class Runner_SqlSuger : IRunner
    {
        RunConfig config;

        int? skip => config.skip;
        int? take => config.take;
        bool executeQuery => config.executeQuery;


        static string connectionString = Appsettings.json.GetStringByPath("Vitorm.Data[0].connectionString");


        SqlSugarClient db;
        public string sql;
        public void Run(RunConfig config)
        {
            this.config = config;


            for (int i = 0; i < config.repeatCount; i++)
            {

                Action<SqlSugarClient> configAction;
                if (executeQuery)
                {
                    configAction = db => { };
                }
                else
                {
                    configAction = db =>
                    {
                        db.Aop.OnLogExecuting = (sql, pars) =>
                        {
                            //Console.WriteLine(sql); //输出sql,查看执行sql 性能无影响

                            //获取原生SQL推荐 5.1.4.63  性能OK
                            var nativeSql = UtilMethods.GetNativeSql(sql, pars);
                            //Console.WriteLine(nativeSql);
                            sql = nativeSql;

                            //获取无参数化SQL 对性能有影响，特别大的SQL参数多的，调试使用
                            //var sqlString = UtilMethods.GetSqlString(DbType.SqlServer, sql, pars);
                            //Console.WriteLine(sqlString);
                        };
                    };
                }
                using SqlSugarClient db = new SqlSugarClient(new ConnectionConfig
                {
                    DbType = DbType.Sqlite,
                    ConnectionString = connectionString,
                    IsAutoCloseConnection = true,
                }, configAction);
                this.db = db;

                if (config.queryJoin) QueryJoin();
                else Query();
            }
        }

        #region Executor
        int exceptUserId = 1;
        public void QueryJoin()
        {
            var minId = 1;
            var config = new { maxId = 10000 };
            var offsetId = 100;

            //var query =
            //        from user in userSet
            //        from father in userSet.Where(father => user.fatherId == father.id).DefaultIfEmpty()
            //        from mother in userSet.Where(mother => user.motherId == mother.id).DefaultIfEmpty()
            //        where user.id > minId && user.id < config.maxId && user.id != exceptUserId
            //        orderby user.id
            //        select new
            //        {
            //            user,
            //            father,
            //            mother,
            //            testId = user.id + offsetId,
            //            hasFather = father.name != null ? true : false
            //        }
            //        ;

            var query = db.Queryable<User>().LeftJoin<User>((user, father) => user.fatherId == father.id)
                .LeftJoin<User>((user, father, mother) => user.fatherId == mother.id)
                .Where((user, father, mother) => user.id > minId && user.id < config.maxId && user.id != exceptUserId)
                .OrderBy((user, father, mother) => user.id, OrderByType.Asc)
                .Select((user, father, mother) =>
                new
                {
                    user,
                    father,
                    mother,
                    testId = user.id + offsetId,
                    hasFather = father.name != null ? true : false
                });
            

            Execute(query);
        }

        public void Query()
        {

            var minId = 1;
            var config = new { maxId = 10000 };

            //var query =
            //        from user in userSet
            //        where user.id > minId && user.id < config.maxId && user.id != exceptUserId
            //        orderby user.id
            //        select user;

            var query = db.Queryable<User>().Where(user => user.id > minId && user.id < config.maxId && user.id != exceptUserId).OrderBy(user => user.id, OrderByType.Asc);

            Execute(query);
        }
        #endregion

        public void Execute<Result>(ISugarQueryable<Result> query)
        {
            if (skip.HasValue) query = query.Skip(skip.Value);
            if (take.HasValue) query = query.Take(take.Value);

            if (executeQuery)
            {
                var userList = query.ToList();
                var rowCount = userList.Count();
                if (rowCount != take) throw new Exception($"query failed, expected row count : {take} , actual count: {rowCount} ");
            }
            else
            {
                sql = null;
                var count = query.Count();
                //query.Single();

                //if (string.IsNullOrEmpty(sql))
                //    throw new Exception($"query failed, can not generated sql script");
            }
        }


 



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


    }
}
