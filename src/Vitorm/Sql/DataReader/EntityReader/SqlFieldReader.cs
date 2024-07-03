using System;
using System.Collections.Generic;
using System.Data;

namespace Vitorm.Sql.DataReader
{

    class SqlFieldReader
    {
        public int sqlFieldIndex { get; set; }
        protected Type valueType { get; set; }
        protected Type underlyingType;


        public SqlFieldReader(List<string> sqlFields, Type valueType, string sqlFieldName)
        {
            this.valueType = valueType;
            underlyingType = TypeUtil.GetUnderlyingType(valueType);

            sqlFieldIndex = sqlFields.IndexOf(sqlFieldName);
            if (sqlFieldIndex < 0)
            {
                sqlFieldIndex = sqlFields.Count;
                sqlFields.Add(sqlFieldName);
            }
        }



        public object Read(IDataReader reader)
        {
            var value = reader.GetValue(sqlFieldIndex);
            return TypeUtil.ConvertToUnderlyingType(value, underlyingType);
        }


    }



}
