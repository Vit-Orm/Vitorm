namespace App.QueryTest
{
    public interface IBenchmarkQuery
    {
        void Query(QueryConfig config); 
    }

    public class QueryConfig 
    {
        public int repeatCount;

        public int take;
        public bool queryJoin;
    }
}
