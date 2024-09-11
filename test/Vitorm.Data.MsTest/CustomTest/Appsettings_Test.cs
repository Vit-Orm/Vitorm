using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vitorm.MsTest.CustomTest
{
    [TestClass]
    public partial class Appsettings_Test
    {

        [TestMethod]
        public void Test_GetDataProviderByName()
        {
            Data.Init("appsettings.Development.json");

            using var dbContext = Data.DataProvider("Sqlite-develop")?.CreateSqlDbContext();
            Assert.AreEqual("data source=sqlite.develop.db;", dbContext.dbConnection.ConnectionString);
        }


    }
}



