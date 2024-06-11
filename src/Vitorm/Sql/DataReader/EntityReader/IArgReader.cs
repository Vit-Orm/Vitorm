using System;
using System.Data;

namespace Vitorm.Sql.DataReader
{
    public interface IArgReader
    {
        string argUniqueKey { get; }
        Type argType { get; }
        string argName { get; }
        object Read(IDataReader reader);
    }

}
