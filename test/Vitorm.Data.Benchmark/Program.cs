
using BenchmarkDotNet.Running;

namespace App
{

    public partial class Program
    {
        static void Main(string[] args)
        {
            // #1 init
            //App.Runner.EnvSetup.InitDb();



            // #2
            //new App.OrmRunner.Runner_Vitorm().Query(take: 100);
            //new App.OrmRunner.Runner_Vitorm().QueryJoin(take: 100);
            //new App.OrmRunner.Runner_EntityFramework().Query(take: 100);
            //new App.OrmRunner.Runner_EntityFramework().QueryJoin(take: 100);
            var summary = BenchmarkRunner.Run<App.OrmRunner.BenchmarkRunner>();



            // #3
            //new App.Runner.ReduceMember.BenchmarkRunner().QueryJoin();
            //BenchmarkRunner.Run<App.Runner.BenchmarkRunner_ReduceMember>();

        }
    }

}