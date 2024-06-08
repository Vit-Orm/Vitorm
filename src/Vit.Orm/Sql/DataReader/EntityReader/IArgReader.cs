using System;
using System.Data;

namespace Vit.Orm.Sql.DataReader
{
    public interface IArgReader
    {
        string argUniqueKey { get; }
        Type argType { get; }
        string argName { get; }
        object Read(IDataReader reader);
    }

}
