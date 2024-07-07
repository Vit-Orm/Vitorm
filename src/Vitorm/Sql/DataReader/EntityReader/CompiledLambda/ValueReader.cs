using System;

namespace Vitorm.Sql.DataReader.EntityReader.CompiledLambda
{

    class ValueReader : SqlFieldReader, IArgReader
    {
        public string argName { get; set; }

        public string argUniqueKey { get; set; }

        public Type entityType { get => valueType; }

        public ValueReader(Type valueType, string argUniqueKey, string argName, int sqlColumnIndex)
                     : base(valueType, sqlColumnIndex)
        {
            this.argUniqueKey = argUniqueKey;
            this.argName = argName;
        }
    }


}
