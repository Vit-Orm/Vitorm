using System.Linq;

using Vitorm.Entity;

namespace Vit.Extensions.Vitorm_Extensions
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
            return data?.allColumns.FirstOrDefault(m => m.propertyName == propertyName)?.columnName;
        }
    }
}