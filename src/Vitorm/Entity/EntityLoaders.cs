using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Vitorm.Entity
{
    public class EntityLoaders : IEntityLoader
    {
        public readonly static EntityLoaders Instance = new();


        public List<IEntityLoader> loaders = new();
        public EntityLoaders()
        {
            loaders.Add(new Loader.EntityLoader_FromAttribute.EntityLoader());

            loaders.Add(new Loader.DataAnnotations.EntityLoader());
        }

        readonly ConcurrentDictionary<Type, IEntityDescriptor> descriptorCache = new();

        public void CleanCache()
        {
            descriptorCache.Clear();
            foreach (var loader in loaders) loader.CleanCache();
        }

        public (bool success, IEntityDescriptor entityDescriptor) LoadDescriptor(Type entityType)
        {
            if (descriptorCache.TryGetValue(entityType, out var entityDescriptor))
                return (true, entityDescriptor);

            var result = LoadDescriptorWithoutCache(entityType);
            if (result.success)
                descriptorCache[entityType] = result.entityDescriptor;

            return result;
        }

        public (bool success, IEntityDescriptor entityDescriptor) LoadDescriptorWithoutCache(Type entityType)
        {
            foreach (var loader in loaders)
            {
                var result = loader.LoadDescriptor(entityType);
                if (result.success) return result;
            }
            return default;
        }

    }
}
