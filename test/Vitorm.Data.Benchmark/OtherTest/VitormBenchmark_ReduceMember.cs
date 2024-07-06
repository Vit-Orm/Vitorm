using BenchmarkDotNet.Attributes;

using Vit.Linq.ExpressionTree;

using Vitorm;

namespace App.OtherTest
{
    //[Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [InProcess]
    public class VitormBenchmark_ReduceMember
    {
        [Params(100)]
        public int N;

        [Params(true, false)]
        public bool queryJoin;

        [Params(true, false)]
        public bool reduceMember;


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


        public static void Run(int N, bool queryJoin)
        {
            for (int i = 0; i < N; i++)
            {
                if (queryJoin) QueryJoin();
                else Query();
            }
        }

        public static void QueryJoin()
        {
            var userSet = Data.Query<User>();
            var query =
                    from user in userSet
                    from father in userSet.Where(father => user.fatherId == father.id).DefaultIfEmpty()
                    from mother in userSet.Where(mother => user.motherId == mother.id).DefaultIfEmpty()
                    where user.id > 1
                    orderby user.id
                    select new
                    {
                        user,
                        father,
                        mother,
                        testId = user.id + 100,
                        hasFather = father.name != null ? true : false
                    }
                    ;

            query = query.Skip(1).Take(2);

            var sql = query.ToExecuteString();
            //var userList = query.ToList();
        }

        public static void Query()
        {
            var userSet = Data.Query<User>();
            var query1 =
                    from user in userSet
                    where user.id > 1
                    orderby user.id
                    select user;

            var query = query1.Skip(1).Take(2);

            var sql = query.ToExecuteString();
            //var userList = query.ToList();
        }



        // Entity Definition
        [System.ComponentModel.DataAnnotations.Schema.Table("User")]
        public class User
        {
            [System.ComponentModel.DataAnnotations.Key]
            public int id { get; set; }
            public string name { get; set; }
            public DateTime? birth { get; set; }
            public int? fatherId { get; set; }
            public int? motherId { get; set; }
        }

    }




}