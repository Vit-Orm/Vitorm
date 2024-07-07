using System;
using System.Data;

namespace Vitorm.Sql.DataReader
{
    public class DataReader_FirstRow : DataReader
    {
        public bool nullable = true;
        public override object ReadData(IDataReader reader)
        {
            if (reader.Read())
            {
                return entityReader.ReadEntity(reader);
            }
            if (!nullable) throw new InvalidOperationException("Sequence contains no elements");
            return default;
        }
    }
}
