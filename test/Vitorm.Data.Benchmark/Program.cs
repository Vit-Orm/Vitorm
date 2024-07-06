using App.QueryTest;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;

namespace App
{

    public class Program
    {
        static void Main(string[] args)
        {
            QueryTest_Vitorm.InitDb();

            //new QueryTest_Vitorm().Query(take: 100);
            //new QueryTest_Vitorm().QueryJoin(take: 100);

            //new QueryTest_EntityFramework().Query(take: 100);
            //new QueryTest_EntityFramework().QueryJoin(take: 100);


            var summary = BenchmarkRunner.Run<VitormBenchmark>();
            //BenchmarkRunner.Run<OtherTest.VitormBenchmark_ReduceMember>();
        }


        [Orderer(SummaryOrderPolicy.FastestToSlowest)]
        [InProcess]
        public class VitormBenchmark
        {
            [Params(100)]
            public int N;

            [Params(10, 100, 500, 1000)]
            public int rowCount;

            [Params(typeof(QueryTest_Vitorm), typeof(QueryTest_EntityFramework))]
            public Type testType;

            [Params(true, false)]
            public bool queryJoin;


            IBenchmarkQuery queryTest;

            [GlobalSetup]
            public void Setup()
            {
                queryTest = Activator.CreateInstance(testType) as IBenchmarkQuery;
            }

            [Benchmark]
            public void Run()
            {
                var config = new QueryConfig { repeatCount = N, queryJoin = queryJoin, take = rowCount };
                queryTest.Query(config);
            }
        }
    }

}