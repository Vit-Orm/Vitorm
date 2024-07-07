using System;

namespace Vitorm.Entity.LoaderAttribute
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EntityLoaderAttribute : Attribute
    {
        public Type Loader { get; set; }
    }
}
