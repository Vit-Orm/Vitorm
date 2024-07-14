using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

using Vit.Linq.ExpressionTree;

using Vitorm;
using Vitorm.Sql;

namespace App.Runner
{
    class Config : ManualConfig
    {
        // https://benchmarkdotnet.org/articles/configs/configs.html
        public Config()
        {
            WithOptions(ConfigOptions.DisableOptimizationsValidator);
        }
    }

    [Config(typeof(Config))]
    //[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
    [InProcess]
    public partial class BenchmarkRunner_ReduceMember
    {
        //[Params(100)]
        public int N = 2;

        //[Params(true, false)]
        public bool executeQuery = false;

        //[Params(false, true)]
        public bool queryJoin = true;


        [Params(10, 100, 1000)]
        public int take = 100;
        //[Params(0, 10)]
        public int? skip = 10;



        //[Params(true, false)]
        public bool reduceMember = true;

        [Params("EntityConstructor", "CompiledLambda")]
        public string entityReader = "EntityConstructor";


        IQueryable<User> userQuery;
        public IQueryable<User> GetQueryable() => userQuery;


        [GlobalSetup]
        public void Setup()
        {
            ToDataArgument.CalculateToConstant_ManuallyReduceMember = reduceMember;

            if (entityReader == "CompiledLambda")
                SqlDbContext.defaultEntityReaderType = typeof(global::Vitorm.Sql.DataReader.EntityReader.CompiledLambda.EntityReader);
            else
                SqlDbContext.defaultEntityReaderType = typeof(global::Vitorm.Sql.DataReader.EntityReader.EntityConstructor.EntityReader);

        }

        [Benchmark]
        public void Run()
        {
            userQuery = Data.Query<User>();

            for (int i = 0; i < N; i++)
            {
                if (queryJoin) QueryJoin();
                else Query();
            }
        }


        #region Executor
        int exceptUserId = 1;
        public void QueryJoin()
        {
            var userSet = GetQueryable();

            var minId = 1;
            var config = new { maxId = 10000 };
            var offsetId = 100;

            var query =
                    from user in userSet
                    from father in userSet.Where(father => user.fatherId == father.id).DefaultIfEmpty()
                    from mother in userSet.Where(mother => user.motherId == mother.id).DefaultIfEmpty()
                    where user.id > minId && user.id < config.maxId && user.id != exceptUserId
                    orderby user.id
                    select new
                    {
                        user,
                        father,
                        mother,
                        testId = user.id + offsetId,
                        hasFather = father.name != null ? true : false
                    }
                    ;

            Execute(query);
        }

        public void Query()
        {
            var userSet = GetQueryable();

            var minId = 1;
            var config = new { maxId = 10000 };

            var query =
                    from user in userSet
                    where user.id > minId && user.id < config.maxId && user.id != exceptUserId
                    orderby user.id
                    select user;

            Execute(query);
        }
        #endregion


        public void Execute<Result>(IQueryable<Result> query)
        {
            if (skip > 0) query = query.Skip(skip.Value);
            query = query.Take(take);

            if (executeQuery)
            {
                var userList = query.ToList();
                var rowCount = userList.Count();
                if (rowCount != take) throw new Exception($"query failed, expected row count : {take} , actual count: {rowCount} ");
            }
            else
            {
                var sql = query.ToExecuteString();
                if (string.IsNullOrEmpty(sql)) throw new Exception($"query failed, can not generated sql script");
            }
        }

    }




}