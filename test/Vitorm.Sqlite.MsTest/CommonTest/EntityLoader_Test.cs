using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vitorm.MsTest.CommonTest
{
    [TestClass]
    public class EntityLoader_Test
    {
        [TestMethod]
        public void Test_EntityDescriptor()
        {
            using var dbContext = DataSource.CreateDbContext();

            {
                var entityDescriptor = dbContext.GetEntityDescriptor<User>();
                Assert.AreEqual("id", entityDescriptor.key?.columnName);
                Assert.AreEqual("User", entityDescriptor.tableName);
            }

            {
                var entityDescriptor = dbContext.GetEntityDescriptor<User2>();
                Assert.IsNull(entityDescriptor.key);
                Assert.AreEqual("User", entityDescriptor.tableName);
            }

            {
                var entityDescriptor = dbContext.GetEntityDescriptor<User3>();
                Assert.IsNull(entityDescriptor);
            }
        }


        #region Custom Entity

        public class User
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        [Vitorm.Entity.Loader.DataAnnotations.StrictEntityLoader]
        [Table("User")]
        public class User2 : User { }


        [Vitorm.Entity.Loader.DataAnnotations.StrictEntityLoader]
        public class User3 : User { }

        #endregion




    }
}
