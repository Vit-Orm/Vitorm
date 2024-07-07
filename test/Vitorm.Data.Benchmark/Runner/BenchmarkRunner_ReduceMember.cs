using BenchmarkDotNet.Attributes;

using Vit.Linq.ExpressionTree;

using Vitorm;

namespace App.Runner
{
    //[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
    [InProcess]
    public partial class BenchmarkRunner_ReduceMember
    {
        [Params(100)]
        public int N;

        [Params(true, false)]
        public bool queryJoin = true;

        //[Params(true, false)]
        public bool reduceMember = true;

        [Params(true, false)]
        public bool executeQuery = false;


        [GlobalSetup]
        public void Setup()
        {
            DataConvertArgument.CalculateToConstant_ManuallyReduceMember = reduceMember;
        }

        [Benchmark]
        public void Run()
        {
            Run(N, queryJoin);
        }


        public void Run(int N, bool queryJoin)
        {
            for (int i = 0; i < N; i++)
            {
                if (queryJoin) QueryJoin();
                else Query();
            }
        }

        public void QueryJoin()
        {
            var userSet = Data.Query<User>();

            var minId = 1;
            var config = new { maxId = 100, offsetId = 100 };

            var query =
                    from user in userSet
                    from father in userSet.Where(father => user.fatherId == father.id).DefaultIfEmpty()
                    from mother in userSet.Where(mother => user.motherId == mother.id).DefaultIfEmpty()
                    where user.id > minId && user.id < config.maxId
                    orderby user.id
                    select new
                    {
                        user,
                        father,
                        mother,
                        testId = user.id + config.offsetId,
                        hasFather = father.name != null ? true : false
                    }
                    ;

            query = query.Skip(1).Take(2);

            if (executeQuery)
            {
                var userList = query.ToList();
            }
            else
            {
                var sql = query.ToExecuteString();
            }
        }

        public void Query()
        {
            var minId = 1;
            var config = new { maxId = 100 };

            var userSet = Data.Query<User>();
            var query1 =
                    from user in userSet
                    where user.id > minId && user.id < config.maxId
                    orderby user.id
                    select user;

            var query = query1.Skip(1).Take(2);

            if (executeQuery)
            {
                var userList = query.ToList();
            }
            else
            {
                var sql = query.ToExecuteString();
            }
        }

    }




}