using Vitorm.Entity;

namespace Vitorm
{
    public interface IDbSet
    {
        IEntityDescriptor entityDescriptor { get; }
        IEntityDescriptor ChangeTable(string tableName);
        IEntityDescriptor ChangeTableBack();
    }
}
