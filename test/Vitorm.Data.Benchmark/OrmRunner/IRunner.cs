namespace App.OrmRunner
{
    public interface IRunner
    {
        void Run(RunConfig config); 
    }

    public class RunConfig 
    {
        public int repeatCount;

        public int take;
        public bool queryJoin;
    }
}
