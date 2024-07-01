using Microsoft.VisualStudio.TestTools.UnitTesting;
using User = Vitorm.MsTest.ClickHouse.User;
using Vitorm.Sql;

namespace Vitorm.MsTest.ClickHouse
{
    public class User : Vitorm.MsTest.UserBase
    {
    }
}


namespace Vitorm.MsTest
{

    [TestClass]
    public partial class ClickHouse_Test : UserTest<User>
    {

        [TestMethod]
        public void Test()
        {
            Init();

            Test_DbContext();
            //Test_Transaction();
            Test_Get();
            Test_Query();
            Test_QueryJoin();
            Test_ToExecuteString();
            //Test_ExecuteUpdate();
            Test_ExecuteDelete();
            Test_Create();
            //Test_Update();
            Test_Delete();
        }

        public override User NewUser(int id, bool forAdd = false) => new User { id = id, name = "testUser" + id };

        public override void WaitForUpdate() => Thread.Sleep(1000);

        public void Init()
        {
            using var dbContext = Data.DataProvider<User>()?.CreateDbContext() as SqlDbContext;

            dbContext.Drop<User>();
            dbContext.Create<User>();

            var users = new List<User> {
                    new User { id=1, name="u146", fatherId=4, motherId=6 },
                    new User { id=2, name="u246", fatherId=4, motherId=6 },
                    new User { id=3, name="u356", fatherId=5, motherId=6 },
                    new User { id=4, name="u400" },
                    new User { id=5, name="u500" },
                    new User { id=6, name="u600" },
                };
            users.ForEach(user => { user.birth = DateTime.Parse("2021-01-01 00:00:00").AddHours(user.id); });

            dbContext.AddRange(users);

            WaitForUpdate();

        }


    }
}
