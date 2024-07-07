using System.Data;

using Vitorm.Entity;

namespace Vitorm.Sql.DataReader
{
    partial class ModelReader
    {
        class EntityPropertyReader : SqlFieldReader
        {
            public IColumnDescriptor column { get; protected set; }

            public EntityPropertyReader(IColumnDescriptor column, int sqlColumnIndex)
                : base(column.type, sqlColumnIndex)
            {
                this.column = column;
            }
            public bool Read(IDataReader reader, object entity)
            {
                var value = Read(reader);
                if (value != null)
                {
                    column.SetValue(entity, value);
                    return true;
                }

                if (column.isKey) return false;
                return true;
            }
        }
    }



}
