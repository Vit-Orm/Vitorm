﻿using System.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vitorm.MsTest.CommonTest
{

    [TestClass]
    public class Orm_Extensions_ExecuteDelete_Test
    {

        [TestMethod]
        public void Test_ExecuteDelete()
        {
            {
                using var dbContext = DataSource.CreateDbContext();
                var userQuery = dbContext.Query<User>();

                var query = from user in userQuery
                            from father in userQuery.Where(father => user.fatherId == father.id).DefaultIfEmpty()
                            where user.id <= 5 && father != null
                            select new
                            {
                                user,
                                father
                            };

                var rowCount = query.ExecuteDelete();

                Assert.AreEqual(3, rowCount);

                var newUsers = userQuery.OrderBy(m => m.id).ToList();
                Assert.AreEqual(3, newUsers.Count());
                Assert.AreEqual(4, newUsers.First().id);
                Assert.AreEqual(6, newUsers.Last().id);
            }

            {
                using var dbContext = DataSource.CreateDbContext();
                var userQuery = dbContext.Query<User>();

                var rowCount = userQuery.Where(m => m.id == 2 || m.id == 4).ExecuteDelete();

                Assert.AreEqual(2, rowCount);

                var newUsers = userQuery.OrderBy(m => m.id).ToList();
                Assert.AreEqual(4, newUsers.Count());
                Assert.AreEqual(1, newUsers.First().id);
                Assert.AreEqual(3, newUsers[1].id);
                Assert.AreEqual(5, newUsers[2].id);
            }
        }
    }
}
