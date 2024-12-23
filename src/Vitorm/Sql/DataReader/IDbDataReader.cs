

namespace Vitorm.Sql.DataReader
{
    public interface IDbDataReader
    {
        object ReadData(System.Data.IDataReader reader);
    }
}
