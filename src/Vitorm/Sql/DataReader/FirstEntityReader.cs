using System;
using System.Data;

namespace Vitorm.Sql.DataReader
{
    public class FirstEntityReader : EntityReader
    {
        public bool nullable = true;
        public override object ReadData(IDataReader reader)
        {
            if (reader.Read())
            {
                return entityConstructor.ReadEntity(reader);
            }
            if (!nullable) throw new InvalidOperationException("Sequence contains no elements");
            return default;
        }
    }
}
