using System.Data;

using Vit.Orm.Entity;

namespace Vit.Orm.Sql.DataReader
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
                    column.Set(entity, value);
                    return true;
                }

                if (column.isPrimaryKey) return false;
                return true;
            }
        }
    }



}
