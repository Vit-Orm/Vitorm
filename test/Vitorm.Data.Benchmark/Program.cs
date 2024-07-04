using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

using Vitorm;
using Vitorm.Sql;

namespace App
{

    public class VitormBenchmark
    {
        [Params(10000)]
        public int N;

        [Params("Data", "Vitorm")]
        public string dataType;

        [Params(false, true)]
        public bool queryJoin;

        //[Params(false, true)]
        //public bool reduceMember;

        [GlobalSetup]
        public void Setup()
        {
        }

        [Benchmark]
        public void Run()
        {
            Run(N, dataType, queryJoin);
        }


        public static void Run(int N, string dataType, bool queryJoin)
        {
            for (int i = 0; i < N; i++)
            {
                var query = GetQuery(dataType);
                if (queryJoin) QueryJoin(query);
                else Query(query);
            }
        }

        static IQueryable<User> GetQuery(string dataType)
        {
            var connectionString = "data source=sqlite.db;";
            return dataType == "Data" ? Data.Query<User>() : new SqlDbContext().UseSqlite(connectionString).AutoDisposeAfterQuery().Query<User>();
        }


        public static void QueryJoin(IQueryable<User> userSet)
        {
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
                    };

            query = query.Skip(1).Take(2);

            var sql = query.ToExecuteString();
            //var userList = query.ToList();
        }

        public static void Query(IQueryable<User> userSet)
        {
            var query1 =
                    from user in userSet
                    where user.id > 1
                    orderby user.id
                    select user;

            var query = query1.Skip(1).Take(2);

            var sql = query.ToExecuteString();
            //var userList = query.ToList();
        }



    }

    public class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<VitormBenchmark>();
        }

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