﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

using Vitorm.MsTest.MySql.Issue_004;


namespace Vitorm.MsTest.Issue000_099.Issues
{
    /// <summary>
    /// https://github.com/VitormLib/Vitorm/issues/4
    /// support schema name of table for mysql
    /// </summary>
    [TestClass]
    public class Issue_004_Test
    {
        [TestMethod]
        public void Test()
        {
            var name = Guid.NewGuid().ToString();

            // #1 Init
            {
                using var dbContext = Data.DataProvider<MyUser>().CreateSqlDbContext();
                dbContext.Execute(@"
create schema IF NOT EXISTS `schemaTest`;
use `schemaTest`;
drop table if exists `schemaTest`.`MyUser`;
CREATE TABLE IF NOT EXISTS `schemaTest`.`MyUser` (`id` int NOT NULL primary key,  `name` varchar(1000) DEFAULT NULL);
insert into `schemaTest`.`MyUser`(`id`,name) values(1,@name);
", param: new Dictionary<string, object> { ["name"] = name });
            }

            // #2 Assert
            {
                var user = Data.Get<MyUser>(1);
                Assert.AreEqual(name, user.name);
            }
        }
    }
}

// Entity Definition
namespace Vitorm.MsTest.MySql.Issue_004
{
    [System.ComponentModel.DataAnnotations.Schema.Table("MyUser", Schema = "schemaTest")]
    public class MyUser
    {
        [System.ComponentModel.DataAnnotations.Key]
        public int id { get; set; }
        public string name { get; set; } 
    }
}