using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace App.OrmRunner
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
    [Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.Method)]
    [InProcess]
    public class BenchmarkRunner
    {

        //[Params(1, 100)]
        public int N = 1;

        //[Params(true, false)]
        public bool executeQuery = true;

        [Params(false, true)]
        public bool queryJoin = true;


        [Params(1, 1000)]
        //[Params(100)]
        public int take = 1;

        //[Params(0, 10)]
        public int? skip = null;


        [Params(typeof(Runner_Vitorm), typeof(Runner_EntityFramework), typeof(Runner_SqlSuger))]
        public Type runner;




        IRunner queryTest;

        [GlobalSetup]
        public void Setup()
        {
            queryTest = Activator.CreateInstance(runner) as IRunner;
        }

        [Benchmark]
        public void Run()
        {
            var config = new RunConfig { repeatCount = N, executeQuery = executeQuery, queryJoin = queryJoin, skip = skip, take = take };
            queryTest.Run(config);
        }
    }


}