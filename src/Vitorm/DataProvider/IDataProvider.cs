using System.Collections.Generic;

namespace Vitorm.DataProvider
{
    public interface IDataProvider : IDbContext
    {
        DbContext CreateDbContext();
        void Init(Dictionary<string, object> config);
    }
}
