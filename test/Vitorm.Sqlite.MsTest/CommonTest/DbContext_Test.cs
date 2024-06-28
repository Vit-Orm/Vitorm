using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vitorm.MsTest.CommonTest
{
    [TestClass]
    public class DbContext_Test
    {
        [TestMethod]
        public void EntityDescriptor_Test()
        {
            using var dbContext = DataSource.CreateDbContext();
            var entityDescriptor = dbContext.GetEntityDescriptor(typeof(User));
            var key = entityDescriptor.key;

            Assert.AreEqual("id", key.name);
        }

    }
}
