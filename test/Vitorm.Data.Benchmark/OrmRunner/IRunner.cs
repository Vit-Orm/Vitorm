namespace App.OrmRunner
{
    public interface IRunner
    {
        void Run(RunConfig config);
    }

    public class RunConfig
    {
        public int repeatCount = 1;

        public bool executeQuery;

        public int? skip = 1;
        public int take;
        public bool queryJoin;

    }
}
