using Microsoft.VisualStudio.TestTools.UnitTesting;

using Vitorm.MsTest.CommonTest;
using Vitorm.MsTest.Sqlite3;
using Vitorm.MsTest.Sqlite4;
using Vitorm.MsTest.Sqlite5;

namespace Vitorm.MsTest.CustomTest
{

    [TestClass]
    public partial class DataProvider_Test
    {

        [TestMethod]
        public void Test_GetDataProviderByEntity()
        {
            var name = Guid.NewGuid().ToString();
            Init<User3>(name);

            Assert.AreEqual(name, Data.Get<User3>(1).name);
            using var dbContext = Data.DataProvider<User3>()?.CreateDbContext();
            Assert.AreEqual(name, dbContext.Get<User3>(1).name);
        }

        [TestMethod]
        public void Test_GetDataProviderByName()
        {
            var name = Guid.NewGuid().ToString();
            Init<User3>(name);

            using var dbContext = Data.DataProvider("Sqlite3")?.CreateDbContext();
            Assert.AreEqual(name, dbContext.Get<UserBase>(1).name);
        }

        [TestMethod]
        public void Test_GetDataProviderByNamespace()
        {
            var name = Guid.NewGuid().ToString();
            Init<User3>(name);

            using var dbContext = Data.DataProvider("Vitorm.MsTest.Sqlite3")?.CreateDbContext();
            Assert.AreEqual(name, dbContext.Get<UserBase>(1).name);
        }


        [TestMethod]
        public void Test_DataProviderByNamespaces()
        {
            // Sqlite4
            {
                var name = Guid.NewGuid().ToString();
                Init<User4>(name);

                using var dbContext = Data.DataProvider("Vitorm.MsTest.Sqlite4,Vitorm.MsTest.Sqlite5")?.CreateDbContext();
                Assert.AreEqual(name, dbContext.Get<UserBase>(1).name);
            }

            // Sqlite5
            {
                var name = Guid.NewGuid().ToString();
                Init<User5>(name);

                Assert.AreEqual(name, Data.Get<User5>(1).name);
            }
        }



        static void Init<User>(string name) where User : UserBase, new()
        {
            Data.TryDropTable<User>();
            Data.TryCreateTable<User>();
            Data.Add(new User { id = 1, name = name });
        }
    }
}


namespace Vitorm.MsTest.Sqlite3
{
    public class User3 : UserBase
    {
    }
}
namespace Vitorm.MsTest.Sqlite4
{
    public class User4 : UserBase
    {
    }
}
namespace Vitorm.MsTest.Sqlite5
{
    public class User5 : UserBase
    {
    }
}



