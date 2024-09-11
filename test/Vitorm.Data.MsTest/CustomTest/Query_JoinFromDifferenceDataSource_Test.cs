using System.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Vitorm.MsTest.CommonTest;

namespace Vitorm.MsTest.CustomTest
{
    [TestClass]
    public class Query_JoinFromDifferenceDataSource_Test
    {

        [TestMethod]
        public void Test()
        {
            new Sqlite_Test().Init();
            new SqliteReadOnly_Test().Init("SqliteReadOnly_Test.db");
            new SqliteReadOnly_Test().Init("SqliteReadOnly_Test.readonly.db");

            var userQuery = Data.Query<Sqlite.User>();

            var userQuery2 = Data.Query<Sqlite2.User>();

            var userQuery3 = Data.Query<MySql.User>();


            {
                try
                {
                    var list = (from user in userQuery
                                from father in userQuery2.Where(father => user.fatherId == father.id)
                                select new { user, father })
                            .ToList();
                    Assert.Fail("should not able to join table from different datasource");
                }
                catch (Exception ex)
                {
                }
            }


            {
                try
                {
                    var list = (from user in userQuery
                                from father in userQuery3.Where(father => user.fatherId == father.id)
                                select new { user, father })
                            .ToList();

                    Assert.Fail("should not able to join table from different datasource");
                }
                catch (Exception ex)
                {
                }
            }


        }



    }
}
