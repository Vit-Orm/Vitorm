using Microsoft.VisualStudio.TestTools.UnitTesting;

using Vit.Core.Module.Serialization;
using Vit.Linq;
using Vit.Linq.ComponentModel;
using Vit.Linq.FilterRules.ComponentModel;

namespace Vitorm.MsTest.CommonTest
{

    [TestClass]
    public class Query_Filter_Test
    {

        [TestMethod]
        public void Test()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            {
                var strFilter = "{'field':'id',  'operator': '>',  'value': 1 }".Replace("'", "\"");
                var filter = Json.Deserialize<FilterRule>(strFilter);

                var items = userQuery.Where(filter).OrderByDescending(u => u.id).ToList();
                Assert.AreEqual(5, items.Count);
                Assert.AreEqual(6, items[0].id);
            }

            {
                var strPagedQuery = "{ 'filter':{'field':'id',  'operator': '>',  'value': 1 },  'orders':[{'field':'id','asc':false}],  'page':{'pageSize':2, 'pageIndex':1}  }".Replace("'", "\"");
                var pagedQuery = Json.Deserialize<PagedQuery>(strPagedQuery);

                var pageData = userQuery.ToPageData(pagedQuery);
                Assert.AreEqual(5, pageData.totalCount);
                Assert.AreEqual(6, pageData.items[0].id);
            }

        }

    }
}
