
using BenchmarkDotNet.Running;

namespace App
{

    public partial class Program
    {
        static void Main(string[] args)
        {
            // #1 init
            App.Runner.EnvSetup.InitDb();



            // #2 
            //new App.OrmRunner.Runner_Vitorm().Run(new() { take = 100, queryJoin = true, executeQuery = true });
            //new App.OrmRunner.Runner_EntityFramework().Run(new() { take = 100, queryJoin = true, executeQuery = true });
            //new App.OrmRunner.Runner_SqlSuger().Run(new() { repeatCount = 100, take = 1000, queryJoin = true, executeQuery = true });
            //new App.OrmRunner.Runner_SqlSuger().Run(new() { repeatCount = 100, take = 1000, queryJoin = false, executeQuery = true });
            var summary = BenchmarkRunner.Run<App.OrmRunner.BenchmarkRunner>();



            // #3
            //new App.Runner.BenchmarkRunner_ReduceMember().Run();
            //BenchmarkRunner.Run<App.Runner.BenchmarkRunner_ReduceMember>();

        }
    }

}