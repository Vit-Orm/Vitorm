using Microsoft.VisualStudio.TestTools.UnitTesting;

using Vit.Linq;

namespace Vitorm.MsTest.CommonTest
{

    [TestClass]
    public partial class Event_Test
    {
        class AssertDisposable : IDisposable
        {
            public AssertDisposable()
            {
                executeString = null;
            }
            public void Dispose()
            {
                Assert.IsNotNull(executeString);
            }
        }
        static string executeString;

        static Event_Test()
        {
            DbContext.event_DefaultOnExecuting = (arg) =>
            {
                executeString = arg.executeString;
            };
        }


        [TestMethod]
        public void Test()
        {
            using var dbContext = DataSource.CreateDbContextForWriting();
            AssertDisposable assertDisposable;

            // TryCreateTable
            using (assertDisposable = new())
            {
                dbContext.TryCreateTable<User>();
            }


            var newUserList = User.NewUsers(7, 4, forAdd: true);
            // Add
            using (assertDisposable = new())
            {
                dbContext.Add(newUserList[0]);
            }

            // AddRange
            using (assertDisposable = new())
            {
                dbContext.AddRange(newUserList.Skip(1));
            }


            // Get
            using (assertDisposable = new())
            {
                var user = dbContext.Get<User>(1);
            }

            // Query
            using (assertDisposable = new())
            {
                var result = dbContext.Query<User>().ToList();
            }
            using (assertDisposable = new())
            {
                var result = dbContext.Query<User>().Count();
            }
            using (assertDisposable = new())
            {
                var result = dbContext.Query<User>().ToExecuteString();
            }
            using (assertDisposable = new())
            {
                var result = dbContext.Query<User>().FirstOrDefault();
            }
            using (assertDisposable = new())
            {
                var result = dbContext.Query<User>().ToListAndTotalCount();
            }

        }


    }
}
