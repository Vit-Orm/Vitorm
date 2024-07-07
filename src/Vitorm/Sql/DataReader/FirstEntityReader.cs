using System;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Vitorm.Sql.DataReader
{
    public class FirstEntityReader : EntityReader
    {
        public bool nullable = true;
        public override object ReadData(IDataReader reader)
        {
            return new Func<IDataReader, string>(ReadEntity<string>)
              .GetMethodInfo().GetGenericMethodDefinition().MakeGenericMethod(entityType)
              .Invoke(this, new object[] { reader });
        }

        Entity ReadEntity<Entity>(IDataReader reader)
        {
            if (reader.Read())
            {
                var lambdaArgs = entityArgReaders.Select(m => m.Read(reader)).ToArray();
                var obj = (Entity)lambdaCreateEntity.DynamicInvoke(lambdaArgs);
                return obj;
            }
            if (!nullable) throw new InvalidOperationException("Sequence contains no elements");
            return default;
        }
    }
}
