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
    //[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
    [InProcess]
    public class BenchmarkRunner
    {

        //[Params(100)]
        public int N = 100;

        //[Params(true, false)]
        public bool executeQuery = true;

        [Params(false, true)]
        public bool queryJoin = false;


        [Params(10, 100, 1000)]
        public int take = 100;

        [Params(0, 10)]
        public int? skip = 10;


        [Params(typeof(Runner_Vitorm), typeof(Runner_EntityFramework))]
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