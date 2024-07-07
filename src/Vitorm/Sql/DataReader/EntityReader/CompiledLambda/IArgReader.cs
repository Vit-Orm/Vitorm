using System;
using System.Data;

namespace Vitorm.Sql.DataReader.EntityReader.CompiledLambda
{
    public interface IArgReader
    {
        string argUniqueKey { get; }
        Type entityType { get; }
        string argName { get; }
        object Read(IDataReader reader);
    }

}
