using System;
using System.Reflection;
using Vitorm.Entity.Loader;

namespace Vitorm.Entity.LoaderAttribute
{
    public class EntityLoaderFromAttribute : IEntityLoader
    {
        public void CleanCache()
        {
        }

        public IEntityDescriptor LoadDescriptor(Type entityType)
        {
            var loaderType = entityType.GetCustomAttribute<EntityLoaderAttribute>()?.Loader;
            if (loaderType == null || !typeof(IEntityLoader).IsAssignableFrom(loaderType)) return null;
            var loader = Activator.CreateInstance(loaderType) as IEntityLoader;
            return loader?.LoadDescriptor(entityType);
        }
    }
}
