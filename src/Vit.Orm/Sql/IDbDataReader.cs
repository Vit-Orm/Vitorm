 

namespace Vit.Orm.Sql
{
    public interface IDbDataReader
    {
        object ReadData(System.Data.IDataReader reader);
    }
}
