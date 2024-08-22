using Microsoft.VisualStudio.TestTools.UnitTesting;

using Vit.Core.Module.Serialization;
using Vit.Linq;
using Vit.Linq.ComponentModel;

namespace Vitorm.MsTest.CommonTest
{

    [TestClass]
    public class Query_FilterRule_WithJoin_Test
    {

        [TestMethod]
        public void Test()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            {
                var query =
                    from user in userQuery
                    from father in userQuery.Where(father => user.fatherId == father.id)
                    select new { user, father };


                var strPagedQuery = "{ 'filter':{'field':'father.name',  'operator': '=',  'value': 'u400' },  'orders':[{'field':'user.id','asc':false}],  'page':{'pageSize':2, 'pageIndex':1}  }".Replace("'", "\"");
                var pagedQuery = Json.Deserialize<PagedQuery>(strPagedQuery);


                var pageData = query.ToPageData(pagedQuery);

                Assert.AreEqual(2, pageData.totalCount);
                Assert.AreEqual(2, pageData.items[0].user.id);
            }

        }

    }
}
