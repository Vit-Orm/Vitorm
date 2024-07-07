using System.Data;

namespace Vitorm.Sql.DataReader.EntityReader.EntityConstructor
{
    public interface IValueReader
    {
        object Read(IDataReader reader);
    }

}
