using System;
using System.Linq;

namespace Vitorm.Entity.Loader.EntityLoader_FromAttribute
{
    /// <summary>
    /// EntityLoader_FromAttribute
    /// </summary>
    public class EntityLoader : IEntityLoader
    {
        public static IEntityLoader GetEntityLoader(Type entityType)
        {
            var attr = entityType.GetCustomAttributes(true).FirstOrDefault(attr => attr is IEntityLoader);
            return attr as IEntityLoader;
        }

        public void CleanCache()
        {
        }

        public (bool success, IEntityDescriptor entityDescriptor) LoadDescriptor(Type entityType) => LoadDescriptorWithoutCache(entityType);
        public (bool success, IEntityDescriptor entityDescriptor) LoadDescriptorWithoutCache(Type entityType)
        {
            var entityLoader = GetEntityLoader(entityType);
            if (entityLoader == null) return default;
            return entityLoader.LoadDescriptor(entityType);
        }

    }
}
