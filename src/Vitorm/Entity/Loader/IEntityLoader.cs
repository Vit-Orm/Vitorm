using System;

namespace Vitorm.Entity.Loader
{
    public interface IEntityLoader
    {
        void CleanCache();
        IEntityDescriptor LoadDescriptor(Type entityType);
    }
}
