using System;

namespace Vit.Orm.Sql.DataReader
{

    class ValueReader : SqlFieldReader, IArgReader
    {
        public string argName { get; set; }

        public string argUniqueKey { get; set; }

        public Type argType { get => valueType; }

        public ValueReader(EntityReader entityReader, Type valueType, string argUniqueKey, string argName, string sqlFieldName)
                     : base(entityReader.sqlFields, valueType, sqlFieldName)
        {
            this.argUniqueKey = argUniqueKey;
            this.argName = argName;
        }
    }


}
