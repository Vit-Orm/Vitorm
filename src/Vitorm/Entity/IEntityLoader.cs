using System;

namespace Vitorm.Entity
{
    public interface IEntityLoader
    {
        void CleanCache();
        (bool success, IEntityDescriptor entityDescriptor) LoadDescriptor(Type entityType);
        (bool success, IEntityDescriptor entityDescriptor) LoadDescriptorWithoutCache(Type entityType);
    }
}
