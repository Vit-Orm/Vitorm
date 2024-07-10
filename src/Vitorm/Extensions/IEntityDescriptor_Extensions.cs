using Vitorm.Entity;
using Vitorm.Entity.DataAnnotations;

namespace Vitorm
{
    public static partial class IEntityDescriptor_Extensions
    {
        public static IEntityDescriptor WithTable(this IEntityDescriptor entityDescriptor, string tableName)
            => entityDescriptor == null ? null : new EntityDescriptorWithAlias(entityDescriptor, tableName);

        public static IEntityDescriptor GetOriginEntityDescriptor(this IEntityDescriptor entityDescriptor)
        {
            while (entityDescriptor is EntityDescriptorWithAlias entityDescriptorWithAlias)
                entityDescriptor = entityDescriptorWithAlias.originEntityDescriptor;
            return entityDescriptor;
        }

        public static string GetOriginTable(this IEntityDescriptor entityDescriptor) => GetOriginEntityDescriptor(entityDescriptor)?.tableName;

    }
}
