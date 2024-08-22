using System.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vitorm.MsTest.CommonTest
{

    [TestClass]
    public class Query_Count_Test
    {

        [TestMethod]
        public void Test_Count()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            TestAllCase(userQuery);
        }



        [TestMethod]
        public void Test_SkipAndTake_Count()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            TestAllCase(userQuery, new Config { skip = 0 });
            TestAllCase(userQuery, new Config { skip = 1 });
            TestAllCase(userQuery, new Config { skip = 10 });

            TestAllCase(userQuery, new Config { take = 0 });
            TestAllCase(userQuery, new Config { take = 2 });
            TestAllCase(userQuery, new Config { take = 20 });


            TestAllCase(userQuery, new Config { skip = 0, take = 0 });
            TestAllCase(userQuery, new Config { skip = 0, take = 2 });
            TestAllCase(userQuery, new Config { skip = 0, take = 20 });

            TestAllCase(userQuery, new Config { skip = 1, take = 0 });
            TestAllCase(userQuery, new Config { skip = 1, take = 2 });
            TestAllCase(userQuery, new Config { skip = 1, take = 20 });

            TestAllCase(userQuery, new Config { skip = 10, take = 0 });
            TestAllCase(userQuery, new Config { skip = 10, take = 2 });
            TestAllCase(userQuery, new Config { skip = 10, take = 20 });
        }


        [TestMethod]
        public void Test_Distinct_Count()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            TestAllCase(userQuery, new Config { distinct = true });
        }




        [TestMethod]
        public void Test_Distinct_SkipAndTake_Count()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            TestAllCase(userQuery, new Config { distinct = true, skip = 1, take = 100 });
        }





        void TestAllCase(IQueryable<User> users, Config config = null)
        {

            #region no orderBy
            {
                // single table
                Test(users.Select(user => user.fatherId), config);
                Test(users.Select(user => new { user.fatherId }), config);

                Test(users.Select(user => user), config);
                Test(users.Select(user => new { user }), config);
                Test(users.Select(user => new { user, user.fatherId }), config);
                Test(users.Select(user => new { user.id, user.fatherId }), config);



                // joinedTable
                Test(users.SelectMany(user => users.Where(father => father.id == user.fatherId), (user, father) => user.fatherId), config);
                Test(users.SelectMany(user => users.Where(father => father.id == user.fatherId), (user, father) => new { user.fatherId }), config);

                Test(users.SelectMany(user => users.Where(father => father.id == user.fatherId), (user, father) => user), config);
                Test(users.SelectMany(user => users.Where(father => father.id == user.fatherId), (user, father) => new { user }), config);
                Test(users.SelectMany(user => users.Where(father => father.id == user.fatherId), (user, father) => new { user, user.id }), config);

                Test(users.SelectMany(user => users.Where(father => father.id == user.fatherId), (user, father) => new { user, father }), config);



                // groupedTable Lambda Expression
                {
                    var query =
                         users
                        .GroupBy(user => new { user.fatherId, user.motherId })
                        .Select(userGroup => new
                        {
                            userGroup.Key.fatherId,
                            userGroup.Key.motherId,
                            sumId = userGroup.Sum(m => m.id),
                        });

                    Test(query, config);
                }
                // groupedTable Linq Expression
                {
                    var query =
                         from user in users
                         group user by new { user.fatherId, user.motherId } into userGroup
                         select new
                         {
                             userGroup.Key.fatherId,
                             userGroup.Key.motherId,
                             sumId = userGroup.Sum(m => m.id),
                         };

                    Test(query, config);
                }
            }
            #endregion

            #region with orderBy
            {
                // single table
                Test(users.OrderBy(m => m.fatherId).Select(user => user.fatherId), config);
                Test(users.OrderBy(m => m.fatherId).Select(user => new { user.fatherId }), config);

                Test(users.OrderBy(m => m.fatherId).Select(user => user), config);
                Test(users.OrderBy(m => m.fatherId).Select(user => new { user }), config);
                Test(users.OrderBy(m => m.fatherId).Select(user => new { user, user.fatherId }), config);
                Test(users.OrderBy(m => m.fatherId).Select(user => new { user.id, user.fatherId }), config);



                // joinedTable
                Test(users.SelectMany(user => users.Where(father => father.id == user.fatherId), (user, father) => user.fatherId).OrderBy(m => m), config);
                Test(users.SelectMany(user => users.Where(father => father.id == user.fatherId), (user, father) => new { user.fatherId }).OrderBy(m => m.fatherId), config);

                Test(users.SelectMany(user => users.Where(father => father.id == user.fatherId), (user, father) => user).OrderBy(m => m.fatherId), config);
                Test(users.SelectMany(user => users.Where(father => father.id == user.fatherId), (user, father) => new { user }).OrderBy(m => m.user.fatherId), config);
                Test(users.SelectMany(user => users.Where(father => father.id == user.fatherId), (user, father) => new { user, user.id }).OrderBy(m => m.user.fatherId), config);

                Test(users.SelectMany(user => users.Where(father => father.id == user.fatherId), (user, father) => new { user, father }).OrderBy(m => m.user.fatherId), config);


                // order by alias column
                {
                    Test(users.Select(user => user.fatherId).OrderBy(m => m), config);
                    Test(users.Select(user => new { user.fatherId }).OrderBy(m => m.fatherId), config);

                    Test(users.Select(user => user).OrderBy(m => m.fatherId), config);
                    Test(users.Select(user => new { user }).OrderBy(m => m.user.fatherId), config);
                    Test(users.Select(user => new { user, user.fatherId }).OrderBy(m => m.fatherId), config);
                    Test(users.Select(user => new { user.id, user.fatherId }).OrderBy(m => m.fatherId), config);

                    Test(users.Select(user => new { user.id, fid = user.fatherId ?? -1 }).OrderBy(m => m.id), config);
                }



                // groupedTable Lambda Expression
                {
                    var query =
                         users
                        .GroupBy(user => new { user.fatherId, user.motherId })
                        .OrderBy(m => m.Key.fatherId)
                        .Select(userGroup => new
                        {
                            userGroup.Key.fatherId,
                            userGroup.Key.motherId,
                            sumId = userGroup.Sum(m => m.id),
                        });

                    Test(query, config);
                }
                // groupedTable Linq Expression
                {
                    var query =
                         from user in users
                         group user by new { user.fatherId, user.motherId } into userGroup
                         orderby userGroup.Key.fatherId
                         select new
                         {
                             userGroup.Key.fatherId,
                             userGroup.Key.motherId,
                             sumId = userGroup.Sum(m => m.id),
                         };

                    Test(query, config);
                }

            }
            #endregion


        }



        void Test<Entity>(IQueryable<Entity> query, Config config = null)
        {
            if (config?.distinct == true) query = query.Distinct();
            if (config?.skip.HasValue == true) query = query.Skip(config.skip.Value);
            if (config?.take.HasValue == true) query = query.Take(config.take.Value);

            var sql = query.ToExecuteString();

            var rows = query.ToList();
            int expectedCount = rows.Count;


            var count = query.Count();
            Assert.AreEqual(expectedCount, count);
        }
        class Config
        {
            public bool? distinct;

            public int? skip;
            public int? take;
        }
    }
}
