using System.Linq;

using Vitorm.Entity;

namespace Vitorm
{
    public static partial class IEntityDescriptor_Extensions
    {
        /// <summary>
        /// get database column name by entity property name
        /// </summary>
        /// <param name="data"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static string GetColumnNameByPropertyName(this IEntityDescriptor data, string propertyName)
        {
            return data?.properties.FirstOrDefault(m => m.propertyName == propertyName)?.columnName;
        }
    }
}