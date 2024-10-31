using Microsoft.VisualStudio.TestTools.UnitTesting;

using Vit.Core.Module.Serialization;
using Vit.Linq;
using Vit.Linq.ComponentModel;
using Vit.Linq.FilterRules.ComponentModel;

namespace Vitorm.MsTest.CommonTest
{

    [TestClass]
    public class Query_FilterRule_Test
    {

        [TestMethod]
        public void Test_PagedQuery()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            {
                var strPagedQuery = "{ 'filter':{'field':'id',  'operator': '>',  'value': 1 },  'orders':[{'field':'id','asc':false}],  'page':{'pageSize':2, 'pageIndex':1}  }".Replace("'", "\"");
                var pagedQuery = Json.Deserialize<PagedQuery>(strPagedQuery);

                var pageData = userQuery.ToPageData(pagedQuery);
                Assert.AreEqual(5, pageData.totalCount);
                Assert.AreEqual(6, pageData.items[0].id);
            }

        }



        [TestMethod]
        public void Test_IsNull()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            {
                var strFilter = "{'field':'fatherId',  'operator': 'IsNull' }".Replace("'", "\"");
                var filter = Json.Deserialize<FilterRule>(strFilter);

                var items = userQuery.Where(filter).OrderByDescending(u => u.id).ToList();
                var ids = items.Select(m => m.id).Distinct().ToList();
                Assert.AreEqual(3, ids.Count);
                Assert.AreEqual(0, ids.Except(new int[] { 4, 5, 6 }).Count());
            }
        }

        [TestMethod]
        public void Test_IsNotNull()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            {
                var strFilter = "{'field':'fatherId',  'operator': 'IsNotNull' }".Replace("'", "\"");
                var filter = Json.Deserialize<FilterRule>(strFilter);

                var items = userQuery.Where(filter).OrderByDescending(u => u.id).ToList();
                var ids = items.Select(m => m.id).Distinct().ToList();
                Assert.AreEqual(3, ids.Count);
                Assert.AreEqual(0, ids.Except(new int[] { 1, 2, 3 }).Count());
            }
        }

        [TestMethod]
        public void Test_In()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            {
                var strFilter = "{'field':'id',  'operator': 'In',  'value': [1,2,3] }".Replace("'", "\"");
                var filter = Json.Deserialize<FilterRule>(strFilter);

                var items = userQuery.Where(filter).OrderByDescending(u => u.id).ToList();
                var ids = items.Select(m => m.id).Distinct().ToList();
                Assert.AreEqual(3, ids.Count);
                Assert.AreEqual(0, ids.Except(new int[] { 1, 2, 3 }).Count());
            }
        }

        [TestMethod]
        public void Test_NotIn()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            {
                var strFilter = "{'field':'id',  'operator': 'NotIn',  'value': [1,2,3] }".Replace("'", "\"");
                var filter = Json.Deserialize<FilterRule>(strFilter);

                var items = userQuery.Where(filter).OrderByDescending(u => u.id).ToList();
                var ids = items.Select(m => m.id).Distinct().ToList();
                Assert.AreEqual(3, ids.Count);
                Assert.AreEqual(0, ids.Except(new int[] { 4, 5, 6 }).Count());
            }
        }


        [TestMethod]
        public void Test_NumberCompare()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            // #1 >
            {
                var strFilter = "{'field':'id',  'operator': '>',  'value': 3 }".Replace("'", "\"");
                var filter = Json.Deserialize<FilterRule>(strFilter);

                var items = userQuery.Where(filter).OrderByDescending(u => u.id).ToList();
                var ids = items.Select(m => m.id).Distinct().ToList();
                Assert.AreEqual(3, ids.Count);
                Assert.AreEqual(0, ids.Except(new int[] { 4, 5, 6 }).Count());
            }

            // #2 >=
            {
                var strFilter = "{'field':'id',  'operator': '>=',  'value': 4 }".Replace("'", "\"");
                var filter = Json.Deserialize<FilterRule>(strFilter);

                var items = userQuery.Where(filter).OrderByDescending(u => u.id).ToList();
                var ids = items.Select(m => m.id).Distinct().ToList();
                Assert.AreEqual(3, ids.Count);
                Assert.AreEqual(0, ids.Except(new int[] { 4, 5, 6 }).Count());
            }

            // #3 <
            {
                var strFilter = "{'field':'id',  'operator': '<',  'value': 4 }".Replace("'", "\"");
                var filter = Json.Deserialize<FilterRule>(strFilter);

                var items = userQuery.Where(filter).OrderByDescending(u => u.id).ToList();
                var ids = items.Select(m => m.id).Distinct().ToList();
                Assert.AreEqual(3, ids.Count);
                Assert.AreEqual(0, ids.Except(new int[] { 1, 2, 3 }).Count());
            }

            // #4 <=
            {
                var strFilter = "{'field':'id',  'operator': '<=',  'value': 3 }".Replace("'", "\"");
                var filter = Json.Deserialize<FilterRule>(strFilter);

                var items = userQuery.Where(filter).OrderByDescending(u => u.id).ToList();
                var ids = items.Select(m => m.id).Distinct().ToList();
                Assert.AreEqual(3, ids.Count);
                Assert.AreEqual(0, ids.Except(new int[] { 1, 2, 3 }).Count());
            }

            // #5.1  =
            {
                var strFilter = "{'field':'id',  'operator': '=',  'value': 3 }".Replace("'", "\"");
                var filter = Json.Deserialize<FilterRule>(strFilter);

                var items = userQuery.Where(filter).OrderByDescending(u => u.id).ToList();
                var ids = items.Select(m => m.id).Distinct().ToList();
                Assert.AreEqual(1, ids.Count);
                Assert.AreEqual(0, ids.Except(new int[] { 3 }).Count());
            }
            // #5.2 =
            {
                var strFilter = "{'field':'fatherId',  'operator': '=',  'value': null }".Replace("'", "\"");
                var filter = Json.Deserialize<FilterRule>(strFilter);

                var items = userQuery.Where(filter).OrderByDescending(u => u.id).ToList();
                var ids = items.Select(m => m.id).Distinct().ToList();
                Assert.AreEqual(3, ids.Count);
                Assert.AreEqual(0, ids.Except(new int[] { 4, 5, 6 }).Count());
            }

            // #6.1 !=
            {
                var strFilter = "{'field':'id',  'operator': '!=',  'value': 3 }".Replace("'", "\"");
                var filter = Json.Deserialize<FilterRule>(strFilter);

                var items = userQuery.Where(filter).OrderByDescending(u => u.id).ToList();
                var ids = items.Select(m => m.id).Distinct().ToList();
                Assert.AreEqual(5, ids.Count);
                Assert.AreEqual(0, ids.Except(new int[] { 1, 2, 4, 5, 6 }).Count());
            }

            // #6.2 !=
            {
                var strFilter = "{'field':'fatherId',  'operator': '!=',  'value': null }".Replace("'", "\"");
                var filter = Json.Deserialize<FilterRule>(strFilter);

                var items = userQuery.Where(filter).OrderByDescending(u => u.id).ToList();
                var ids = items.Select(m => m.id).Distinct().ToList();
                Assert.AreEqual(3, ids.Count);
                Assert.AreEqual(0, ids.Except(new int[] { 1, 2, 3 }).Count());
            }
        }


        [TestMethod]
        public void Test_String()
        {
            using var dbContext = DataSource.CreateDbContextForWriting();
            var userQuery = dbContext.Query<User>();

            var users = userQuery.Where(m => m.id >= 5).ToList();
            users[0].name = "";
            users[1].name = null;
            dbContext.UpdateRange(users);
            DataSource.WaitForUpdate();


            // #1 Contains
            {
                var strFilter = "{'field':'name',  'operator': 'Contains',  'value': '46' }".Replace("'", "\"");
                var filter = Json.Deserialize<FilterRule>(strFilter);

                var items = userQuery.Where(filter).OrderByDescending(u => u.id).ToList();
                var ids = items.Select(m => m.id).Distinct().ToList();
                Assert.AreEqual(2, ids.Count);
                Assert.AreEqual(0, ids.Except(new int[] { 1, 2 }).Count());
            }

            // #2 NotContain
            {
                var strFilter = "{'field':'name',  'operator': 'NotContain',  'value': '46' }".Replace("'", "\"");
                var filter = Json.Deserialize<FilterRule>(strFilter);

                var items = userQuery.Where(filter).OrderByDescending(u => u.id).ToList();
                var ids = items.Select(m => m.id).Distinct().ToList();
                Assert.AreEqual(3, ids.Count);
                Assert.AreEqual(0, ids.Except(new int[] { 3, 4, 5 }).Count());
            }


            // #3 StartsWith
            {
                var strFilter = "{'field':'name',  'operator': 'StartsWith',  'value': 'u14' }".Replace("'", "\"");
                var filter = Json.Deserialize<FilterRule>(strFilter);

                var items = userQuery.Where(filter).OrderByDescending(u => u.id).ToList();
                var ids = items.Select(m => m.id).Distinct().ToList();
                Assert.AreEqual(1, ids.Count);
                Assert.AreEqual(0, ids.Except(new int[] { 1 }).Count());
            }

            // #4 EndsWith
            {
                var strFilter = "{'field':'name',  'operator': 'EndsWith',  'value': '46' }".Replace("'", "\"");
                var filter = Json.Deserialize<FilterRule>(strFilter);

                var items = userQuery.Where(filter).OrderByDescending(u => u.id).ToList();
                var ids = items.Select(m => m.id).Distinct().ToList();
                Assert.AreEqual(2, ids.Count);
                Assert.AreEqual(0, ids.Except(new int[] { 1, 2 }).Count());
            }

        }


    }
}
