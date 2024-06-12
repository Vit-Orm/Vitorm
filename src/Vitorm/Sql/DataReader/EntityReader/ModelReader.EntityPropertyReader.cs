using System.Data;

using Vitorm.Entity;

namespace Vitorm.Sql.DataReader
{
    partial class ModelReader
    {
        class EntityPropertyReader : SqlFieldReader
        {
            IColumnDescriptor column;

            public EntityPropertyReader(EntityReader entityReader, IColumnDescriptor column, bool isPrimaryKey, string sqlFieldName)
                : base(entityReader.sqlFields, column.type, sqlFieldName)
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
