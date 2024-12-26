using System.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vitorm.MsTest.CustomTest
{

    [TestClass]
    public class ReadEntity_Test
    {

        [TestMethod]
        public void ReadValue()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();


            // ReadValue
            {
                using var reader = dbContext.ExecuteReader("select userId,userName,userBirth,userFatherId,userMotherId from `User`  where userId<=4 order by userId");

                var ids = reader.ReadValue<int>().ToList();

                Assert.AreEqual("1,2,3,4", String.Join(",", ids));
            }

            // ReadValue
            {
                using var reader = dbContext.ExecuteReader("select userId,userName,userBirth,userFatherId,userMotherId from `User`  where userId<=4 order by userId");

                var names = reader.ReadValue<string>(1).ToList();

                Assert.AreEqual("u146,u246,u356,u400", String.Join(",", names));
            }

            // ReadValue
            {
                using var reader = dbContext.ExecuteReader("select userId,userName,userBirth,userFatherId,userMotherId from `User`  where userId<=4 order by userId");

                var ids = reader.ReadValue<int?>("UserFatherId").ToList();

                Assert.AreEqual("4,4,5,", String.Join(",", ids));
            }

        }



        [TestMethod]
        public void ReadTuple()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            // ReadTuple
            {
                using var reader = dbContext.ExecuteReader("select userId,userName,userBirth,userFatherId,userMotherId from `User`  where userId<=4 order by userId");

                List<(int id, string name)> userList = reader.ReadTuple<int, string>().ToList();

                Assert.AreEqual("1,2,3,4", String.Join(",", userList.Select(u => u.id)));
            }

            // ReadTuple by indexes
            {
                using var reader = dbContext.ExecuteReader("select userId,userName,userBirth,userFatherId,userMotherId from `User`  where userId<=4 order by userId");

                List<(int id, int? fatherId)> userList = reader.ReadTuple<int, int?>([0, 3]).ToList();

                Assert.AreEqual("1,2,3,4", String.Join(",", userList.Select(u => u.id)));

                Assert.AreEqual("4,4,5,", String.Join(",", userList.Select(u => u.fatherId)));
            }


            // ReadTuple by columnNames
            {
                using var reader = dbContext.ExecuteReader("select userId,userName,userBirth,userFatherId,userMotherId from `User`  where userId<=4 order by userId");

                List<(int id, int? fatherId)> userList = reader.ReadTuple<int, int?>(["userId", "userFatherId"]).ToList();

                Assert.AreEqual("1,2,3,4", String.Join(",", userList.Select(u => u.id)));

                Assert.AreEqual("4,4,5,", String.Join(",", userList.Select(u => u.fatherId)));
            }
        }


        [TestMethod]
        public void ReadTuple3()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            // ReadTuple
            {
                using var reader = dbContext.ExecuteReader("select userId,userName,userBirth,userFatherId,userMotherId from `User`  where userId<=4 order by userId");

                List<(int id, string name, DateTime? birth)> userList = reader.ReadTuple<int, string, DateTime?>().ToList();

                Assert.AreEqual("1,2,3,4", String.Join(",", userList.Select(u => u.id)));
            }

            // ReadTuple by indexes
            {
                using var reader = dbContext.ExecuteReader("select userId,userName,userBirth,userFatherId,userMotherId from `User`  where userId<=4 order by userId");

                List<(int id, int? fatherId, int? motherId)> userList = reader.ReadTuple<int, int?, int?>([0, 3, 4]).ToList();

                Assert.AreEqual("1,2,3,4", String.Join(",", userList.Select(u => u.id)));
                Assert.AreEqual("4,4,5,", String.Join(",", userList.Select(u => u.fatherId)));
                Assert.AreEqual("6,6,6,", String.Join(",", userList.Select(u => u.motherId)));
            }


            // ReadTuple by columnNames
            {
                using var reader = dbContext.ExecuteReader("select userId,userName,userBirth,userFatherId,userMotherId from `User`  where userId<=4 order by userId");

                List<(int id, int? fatherId, int? motherId)> userList = reader.ReadTuple<int, int?, int?>(["userId", "userFatherId", "UserMotherId"]).ToList();

                Assert.AreEqual("1,2,3,4", String.Join(",", userList.Select(u => u.id)));
                Assert.AreEqual("4,4,5,", String.Join(",", userList.Select(u => u.fatherId)));
                Assert.AreEqual("6,6,6,", String.Join(",", userList.Select(u => u.motherId)));
            }
        }


        [TestMethod]
        public void ReadTuple4()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            // ReadTuple
            {
                using var reader = dbContext.ExecuteReader("select userId,userName,userBirth,userFatherId,userMotherId from `User`  where userId<=4 order by userId");

                List<(int id, string name, DateTime? birth, int? fatherId)> userList = reader.ReadTuple<int, string, DateTime?, int?>().ToList();

                Assert.AreEqual("1,2,3,4", String.Join(",", userList.Select(u => u.id)));
                Assert.AreEqual("4,4,5,", String.Join(",", userList.Select(u => u.fatherId)));
            }

            // ReadTuple by indexes
            {
                using var reader = dbContext.ExecuteReader("select userId,userName,userBirth,userFatherId,userMotherId from `User`  where userId<=4 order by userId");

                List<(int id, int? fatherId, int? motherId, string name)> userList = reader.ReadTuple<int, int?, int?, string>([0, 3, 4, 1]).ToList();

                Assert.AreEqual("1,2,3,4", String.Join(",", userList.Select(u => u.id)));
                Assert.AreEqual("4,4,5,", String.Join(",", userList.Select(u => u.fatherId)));
                Assert.AreEqual("6,6,6,", String.Join(",", userList.Select(u => u.motherId)));
                Assert.AreEqual("u146,u246,u356,u400", String.Join(",", userList.Select(u => u.name)));
            }


            // ReadTuple by columnNames
            {
                using var reader = dbContext.ExecuteReader("select userId,userName,userBirth,userFatherId,userMotherId from `User`  where userId<=4 order by userId");

                List<(int id, int? fatherId, int? motherId, string name)> userList = reader.ReadTuple<int, int?, int?, string>(["userId", "userFatherId", "UserMotherId", "userName"]).ToList();

                Assert.AreEqual("1,2,3,4", String.Join(",", userList.Select(u => u.id)));
                Assert.AreEqual("4,4,5,", String.Join(",", userList.Select(u => u.fatherId)));
                Assert.AreEqual("6,6,6,", String.Join(",", userList.Select(u => u.motherId)));
                Assert.AreEqual("u146,u246,u356,u400", String.Join(",", userList.Select(u => u.name)));
            }
        }



        [TestMethod]
        public void ReadEntity()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            // ReadEntity
            {
                using var reader = dbContext.ExecuteReader("select * from `User`  where userId<=4 order by userId");

                var userList = reader.ReadEntity<User>().ToList();

                Assert.AreEqual("1,2,3,4", String.Join(",", userList.Select(u => u.id)));
                Assert.AreEqual("4,4,5,", String.Join(",", userList.Select(u => u.fatherId)));
                Assert.AreEqual("6,6,6,", String.Join(",", userList.Select(u => u.motherId)));
                Assert.AreEqual("u146,u246,u356,u400", String.Join(",", userList.Select(u => u.name)));
            }
        }


        [TestMethod]
        public void ReadEntity2()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            // ReadEntity :  try read all columns for each entities
            {
                using var reader = dbContext.ExecuteReader("select userId,userName,userBirth,userFatherId,userMotherId from `User`  where userId<=4 order by userId");

                var userList = reader.ReadEntity<User, User, (int id, string name, int? fatherId, int? motherId)>((u0, u1) => (u0.id, u0.name, u1.fatherId, u1.motherId)).ToList();

                Assert.AreEqual("1,2,3,4", String.Join(",", userList.Select(u => u.id)));
                Assert.AreEqual("u146,u246,u356,u400", String.Join(",", userList.Select(u => u.name)));
                Assert.AreEqual("4,4,5,", String.Join(",", userList.Select(u => u.fatherId)));
                Assert.AreEqual("6,6,6,", String.Join(",", userList.Select(u => u.motherId)));
            }

            // ReadEntity splitOn column not exist 
            {
                using var reader = dbContext.ExecuteReader("select userId,userName,userBirth,userFatherId,userMotherId from `User`  where userId<=4 order by userId");

                var userList = reader.ReadEntity<User, User, (int id, string name, int? fatherId, int? motherId)>((u0, u1) => (u0.id, u0.name, u1.fatherId, u1.motherId), "dummyField").ToList();

                Assert.AreEqual("1,2,3,4", String.Join(",", userList.Select(u => u.id)));
                Assert.AreEqual("u146,u246,u356,u400", String.Join(",", userList.Select(u => u.name)));
                Assert.AreEqual("4,4,5,", String.Join(",", userList.Select(u => u.fatherId)));
                Assert.AreEqual("6,6,6,", String.Join(",", userList.Select(u => u.motherId)));
            }

            // ReadEntity splitOn
            {
                using var reader = dbContext.ExecuteReader("select userId,userName,userBirth,userFatherId,userMotherId from `User`  where userId<=4 order by userId");

                var userList = reader.ReadEntity<User, User, (int id, string name, int? fatherId, int? motherId)>((u0, u1) => (u0.id, u0.name, u1.fatherId, u1.motherId), "userFatherId").ToList();

                Assert.AreEqual("1,2,3,4", String.Join(",", userList.Select(u => u.id)));
                Assert.AreEqual("u146,u246,u356,u400", String.Join(",", userList.Select(u => u.name)));
                Assert.AreEqual("4,4,5,", String.Join(",", userList.Select(u => u.fatherId)));
                Assert.AreEqual("6,6,6,", String.Join(",", userList.Select(u => u.motherId)));
            }

            // ReadEntity splitIndex
            {
                using var reader = dbContext.ExecuteReader("select userId,userName,userBirth,userFatherId,userMotherId from `User`  where userId<=4 order by userId");

                var userList = reader.ReadEntity<User, User, (int id, string name, int? fatherId, int? motherId)>((u0, u1) => (u0.id, u0.name, u1.fatherId, u1.motherId), 3).ToList();

                Assert.AreEqual("1,2,3,4", String.Join(",", userList.Select(u => u.id)));
                Assert.AreEqual("u146,u246,u356,u400", String.Join(",", userList.Select(u => u.name)));
                Assert.AreEqual("4,4,5,", String.Join(",", userList.Select(u => u.fatherId)));
                Assert.AreEqual("6,6,6,", String.Join(",", userList.Select(u => u.motherId)));
            }
        }


        [TestMethod]
        public void ReadEntity3()
        {
            using var dbContext = DataSource.CreateDbContext();
            var userQuery = dbContext.Query<User>();

            // ReadEntity :  try read all columns for each entities
            {
                using var reader = dbContext.ExecuteReader("select userId,userName,userBirth,userFatherId,userMotherId from `User`  where userId<=4 order by userId");

                var userList = reader.ReadEntity<User, User, User, (int id, string name, int? fatherId, int? motherId)>((u0, u1, u2) => (u0.id, u0.name, u1.fatherId, u2.motherId)).ToList();

                Assert.AreEqual("1,2,3,4", String.Join(",", userList.Select(u => u.id)));
                Assert.AreEqual("u146,u246,u356,u400", String.Join(",", userList.Select(u => u.name)));
                Assert.AreEqual("4,4,5,", String.Join(",", userList.Select(u => u.fatherId)));
                Assert.AreEqual("6,6,6,", String.Join(",", userList.Select(u => u.motherId)));
            }

            // ReadEntity splitOn column not exist 
            {
                using var reader = dbContext.ExecuteReader("select userId,userName,userBirth,userFatherId,userMotherId from `User`  where userId<=4 order by userId");

                var userList = reader.ReadEntity<User, User, User, (int id, string name, int? fatherId, int? motherId)>((u0, u1, u2) => (u0.id, u0.name, u1.fatherId, u2.motherId), "dummyField", "dummyField").ToList();

                Assert.AreEqual("1,2,3,4", String.Join(",", userList.Select(u => u.id)));
                Assert.AreEqual("u146,u246,u356,u400", String.Join(",", userList.Select(u => u.name)));
                Assert.AreEqual("4,4,5,", String.Join(",", userList.Select(u => u.fatherId)));
                Assert.AreEqual("6,6,6,", String.Join(",", userList.Select(u => u.motherId)));
            }

            // ReadEntity splitOns
            {
                using var reader = dbContext.ExecuteReader("select userId,userName,userBirth,userFatherId,userMotherId from `User`  where userId<=4 order by userId");

                var userList = reader.ReadEntity<User, User, User, (int id, string name, int? fatherId, int? motherId)>((u0, u1, u2) => (u0.id, u0.name, u1.fatherId, u2.motherId), "userFatherId", "userMotherId").ToList();

                Assert.AreEqual("1,2,3,4", String.Join(",", userList.Select(u => u.id)));
                Assert.AreEqual("u146,u246,u356,u400", String.Join(",", userList.Select(u => u.name)));
                Assert.AreEqual("4,4,5,", String.Join(",", userList.Select(u => u.fatherId)));
                Assert.AreEqual("6,6,6,", String.Join(",", userList.Select(u => u.motherId)));
            }


            // ReadEntity splitOns
            {
                using var reader = dbContext.ExecuteReader("select userId,userName,userBirth,   userFatherId,userId,  userMotherId,userId from `User`  where userId<=4 order by userId");

                var userList = reader.ReadEntity<User, User, User, (int id, string name, int? fatherId, int? motherId)>((u0, u1, u2) => (u0.id, u0.name, u1.fatherId, u2.motherId), "userFatherId", "userMotherId").ToList();

                Assert.AreEqual("1,2,3,4", String.Join(",", userList.Select(u => u.id)));
                Assert.AreEqual("u146,u246,u356,u400", String.Join(",", userList.Select(u => u.name)));
                Assert.AreEqual("4,4,5,", String.Join(",", userList.Select(u => u.fatherId)));
                Assert.AreEqual("6,6,6,", String.Join(",", userList.Select(u => u.motherId)));
            }

            // ReadEntity splitIndex
            {
                using var reader = dbContext.ExecuteReader("select userId,userName,userBirth,userFatherId,userMotherId from `User`  where userId<=4 order by userId");

                var userList = reader.ReadEntity<User, User, User, (int id, string name, int? fatherId, int? motherId)>((u0, u1, u2) => (u0.id, u0.name, u1.fatherId, u2.motherId), 3, 4).ToList();

                Assert.AreEqual("1,2,3,4", String.Join(",", userList.Select(u => u.id)));
                Assert.AreEqual("u146,u246,u356,u400", String.Join(",", userList.Select(u => u.name)));
                Assert.AreEqual("4,4,5,", String.Join(",", userList.Select(u => u.fatherId)));
                Assert.AreEqual("6,6,6,", String.Join(",", userList.Select(u => u.motherId)));
            }
        }




    }
}
