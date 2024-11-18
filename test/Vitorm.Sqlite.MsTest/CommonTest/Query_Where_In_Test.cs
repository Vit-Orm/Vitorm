using System.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Vit.Linq;

namespace Vitorm.MsTest.CommonTest
{

    [TestClass]
    public class Query_Where_In_Test
    {

        [TestMethod]
        public void Test_In()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            {
                var ids = new[] { 1, 2 };
                var userList = userQuery.Where(m => ids.Contains(m.id)).OrderBy(m => m.id).ToList();

                var strIds = String.Join(',', userList.Select(m => m.id));
                Assert.AreEqual(2, userList.Count);
                Assert.AreEqual("1,2", strIds);
            }


            {
                var ids = new List<int> { 1, 2 };
                var userList = userQuery.Where(m => ids.Contains(m.id)).OrderBy(m => m.id).ToList();

                var strIds = String.Join(',', userList.Select(m => m.id));
                Assert.AreEqual(2, userList.Count);
                Assert.AreEqual("1,2", strIds);
            }

        }



    }
}
