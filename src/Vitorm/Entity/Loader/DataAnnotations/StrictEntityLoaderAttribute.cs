using System;

namespace Vitorm.Entity.Loader.DataAnnotations
{
    /// <summary>
    /// strictMode to load EntityDescriptor: must specify TableAttribute , if not specify KeyAttribute will not have key column
    /// </summary>
    public class StrictEntityLoaderAttribute : Attribute, IEntityLoader
    {
        public void CleanCache()
        {
        }

        public (bool success, IEntityDescriptor entityDescriptor) LoadDescriptor(Type entityType) => LoadDescriptorWithoutCache(entityType);

        public (bool success, IEntityDescriptor entityDescriptor) LoadDescriptorWithoutCache(Type entityType) => EntityLoader.LoadFromType(entityType, strictMode: true);

    }
}
