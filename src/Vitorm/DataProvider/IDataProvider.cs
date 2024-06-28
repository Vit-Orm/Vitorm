using System.Collections.Generic;

namespace Vitorm.DataProvider
{
    public interface IDataProvider : IDbContext
    {
        void Init(Dictionary<string, object> config);
    }
}
