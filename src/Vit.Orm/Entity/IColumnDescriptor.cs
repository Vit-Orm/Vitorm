using System;

namespace Vit.Orm.Entity
{
    public interface IColumnDescriptor
    {
        bool isPrimaryKey { get; }
        string name { get; }
        Type type { get; }
        void Set(object entity, object value);
        object Get(object entity);
    }
}
