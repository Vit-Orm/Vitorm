using System;
using System.Data;

namespace Vitorm.Sql.DataReader
{

    class SqlFieldReader
    {
        public int sqlColumnIndex { get; set; }
        protected Type valueType { get; set; }
        protected Type underlyingType;


        public SqlFieldReader(Type valueType, int sqlColumnIndex)
        {
            this.valueType = valueType;
            underlyingType = TypeUtil.GetUnderlyingType(valueType);

            this.sqlColumnIndex = sqlColumnIndex;
        }



        public object Read(IDataReader reader)
        {
            var value = reader.GetValue(sqlColumnIndex);
            return TypeUtil.ConvertToUnderlyingType(value, underlyingType);
        }


    }



}
