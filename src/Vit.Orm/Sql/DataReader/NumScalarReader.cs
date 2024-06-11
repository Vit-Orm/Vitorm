using System;
using System.Data;

using Vitorm.Sql;

namespace Vitorm.DataReader
{
    public class NumScalarReader : IDbDataReader
    {
        public object ReadData(IDataReader reader)
        {
            if (reader.Read())
            {
                var count = reader.GetValue(0);
                return Convert.ToInt32(count);
            }
            return -1;
        }
    }
}
