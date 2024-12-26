using System.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vitorm.MsTest.CustomTest
{

    [TestClass]
    public class Procedure_Test
    {

        [TestMethod]
        public void Procedure()
        {
            using var dbContext = DataSource.CreateDbContext();

            // create procedure
            {
                dbContext.Execute(@"
                    DROP PROCEDURE IF EXISTS GetUser;

                    CREATE PROCEDURE GetUser(IN uid INT)
                    BEGIN
                        SELECT * FROM `User` WHERE userId = uid order by userId;
                        SELECT * FROM `User` WHERE userId != uid order by userId;
                    END
                    ");
            }


            // ExecuteReader
            {
                using var reader = dbContext.ExecuteReader("GetUser", new() { ["uid"] = 1 }, isProcedure: true);

                var userList = reader.ReadEntity<User>().ToList();
                Assert.AreEqual("1", String.Join(",", userList.Select(u => u.id)));
            }

            // ExecuteReader
            {
                using var reader = dbContext.ExecuteReader("GetUser", new() { ["uid"] = 1 }, isProcedure: true);
                {
                    var userList = reader.ReadEntity<User>().ToList();
                    Assert.AreEqual("1", String.Join(",", userList.Select(u => u.id)));
                }
                reader.NextResult();
                {
                    var userList = reader.ReadEntity<User>().ToList();
                    Assert.AreEqual("2,3,4,5,6", String.Join(",", userList.Select(u => u.id)));
                }
            }

            // ExecuteReader
            {
                using var reader = dbContext.ExecuteReader("GetUser", new() { ["uid"] = 0 }, isProcedure: true);
                {
                    var userList = reader.ReadEntity<User>().ToList();
                    Assert.AreEqual("", String.Join(",", userList.Select(u => u.id)));
                }
                reader.NextResult();
                {
                    var userList = reader.ReadEntity<User>().ToList();
                    Assert.AreEqual("1,2,3,4,5,6", String.Join(",", userList.Select(u => u.id)));
                }
            }

            // ExecuteReader
            {
                var userId = dbContext.ExecuteScalar("GetUser", new() { ["uid"] = 1 }, isProcedure: true);
                Assert.AreEqual(1, userId);
            }
        }



        [TestMethod]
        public async Task ProcedureAsync()
        {
            using var dbContext = DataSource.CreateDbContext();

            // create procedure
            {
                await dbContext.ExecuteAsync(@"
                    DROP PROCEDURE IF EXISTS GetUser;

                    CREATE PROCEDURE GetUser(IN uid INT)
                    BEGIN
                        SELECT * FROM `User` WHERE userId = uid order by userId;
                        SELECT * FROM `User` WHERE userId != uid order by userId;
                    END
                    ");
            }


            // ExecuteReader
            {
                using var reader = await dbContext.ExecuteReaderAsync("GetUser", new() { ["uid"] = 1 }, isProcedure: true);

                var userList = reader.ReadEntity<User>().ToList();
                Assert.AreEqual("1", String.Join(",", userList.Select(u => u.id)));
            }

            // ExecuteReader
            {
                using var reader = await dbContext.ExecuteReaderAsync("GetUser", new() { ["uid"] = 1 }, isProcedure: true);
                {
                    var userList = reader.ReadEntity<User>().ToList();
                    Assert.AreEqual("1", String.Join(",", userList.Select(u => u.id)));
                }
                reader.NextResult();
                {
                    var userList = reader.ReadEntity<User>().ToList();
                    Assert.AreEqual("2,3,4,5,6", String.Join(",", userList.Select(u => u.id)));
                }
            }

            // ExecuteReader
            {
                using var reader = await dbContext.ExecuteReaderAsync("GetUser", new() { ["uid"] = 0 }, isProcedure: true);
                {
                    var userList = reader.ReadEntity<User>().ToList();
                    Assert.AreEqual("", String.Join(",", userList.Select(u => u.id)));
                }
                reader.NextResult();
                {
                    var userList = reader.ReadEntity<User>().ToList();
                    Assert.AreEqual("1,2,3,4,5,6", String.Join(",", userList.Select(u => u.id)));
                }
            }

            // ExecuteReader
            {
                var userId = await dbContext.ExecuteScalarAsync("GetUser", new() { ["uid"] = 1 }, isProcedure: true);
                Assert.AreEqual(1, userId);
            }
        }



    }
}
