
# Vitorm.SqlServer
Vitorm.SqlServer is a lightweight yet comprehensive ORM for SqlServer that strikes the perfect balance between simplicity and functionality.     
Similar to Dapper in its lightweight design, Vitorm offers an easy-to-use, efficient interface for data access.     
However, it doesn't stop there; Vitorm goes beyond basic ORM capabilities to provide a rich feature set akin to Entity Framework.     
This means you get the best of both worlds: the performance and simplicity of Dapper with the robust features and flexibility of Entity Framework, making Vitorm an ideal choice for developers seeking a powerful yet streamlined ORM solution.
> source address: [https://github.com/VitormLib/Vitorm/tree/master/src/Vitorm.SqlServer](https://github.com/VitormLib/Vitorm/tree/master/src/Vitorm.SqlServer "https://github.com/VitormLib/Vitorm/tree/master/src/Vitorm.SqlServer")        

![](https://img.shields.io/github/license/VitormLib/Vitorm.svg)  
![](https://img.shields.io/github/repo-size/VitormLib/Vitorm.svg)  ![](https://img.shields.io/github/last-commit/VitormLib/Vitorm.svg)  
 

| Build | NuGet |
| -------- | -------- |
|![](https://github.com/VitormLib/Vitorm/workflows/ki_devops3/badge.svg) | [![](https://img.shields.io/nuget/v/Vitorm.SqlServer.svg)](https://www.nuget.org/packages/Vitorm.SqlServer) ![](https://img.shields.io/nuget/dt/Vitorm.SqlServer.svg) |




# Vitorm Documentation
This guide will walk you through the steps to set up and use Vitorm.SqlServer.

supported features:

| feature    |  method   |  remarks   |     |
| --- | --- | --- | --- |
|  create table   |  TryCreateTable   |     |     |
|  drop table   |  TryDropTable   |     |     |
| --- | --- | --- | --- |
|  create records   |  Add AddRange   |     |     |
|  retrieve  records |  Query Get   |     |     |
|  update records   |  Update UpdateRange ExecuteUpdate  |     |     |
|  delete records   |  Delete DeleteRange DeleteByKey DeleteByKeys ExecuteDelete   |     |     |
| --- | --- | --- | --- |
|  change table   |  ChangeTable    |  change mapping table from database   |   |
|  change database  |  ChangeDatabase   | change database to be connected  |   |
| --- | --- | --- | --- |
|  collection total count   |  TotalCount    |  Collection Total Count without Take and Skip   |   |
|  collection total count and list  |  ToListAndTotalCount   | query List and TotalCount at on request  |   |
|     |     |   |   |


## Installation
Before using Vitorm.SqlServer, install the necessary package:    
``` bash
dotnet add package Vitorm.SqlServer
```

## Minimum viable demo
``` csharp
using Vitorm;
namespace App
{
    public class Program_Min
    {
        static void Main2(string[] args)
        {
            // #1 Init
            using var dbContext = new Vitorm.Sql.SqlDbContext();
            dbContext.UseSqlServer("Server=localhost;Database=dev-orm;User ID=sa;Password=Admin0123;TrustServerCertificate=true;");

            // #2 Query
            var user = dbContext.Get<User>(1);
            var users = dbContext.Query<User>().Where(u => u.name.Contains("li")).ToList();
        }

        // Entity Definition
        [System.ComponentModel.DataAnnotations.Schema.Table("User")]
        public class User
        {
            [System.ComponentModel.DataAnnotations.Key]
            public int id { get; set; }
            public string name { get; set; }
            public DateTime? birth { get; set; }
            public int? fatherId { get; set; }
        }
    }
}
```


## Full Example
> This example provides a comprehensive guide to utilizing Vitorm for basic and advanced database operations while maintaining lightweight performance.    
``` csharp
using Vitorm;

namespace App
{
    public class Program_Min
    {
        static void Main(string[] args)
        {
            // #1 Configures Vitorm
            using var dbContext = new Vitorm.Sql.SqlDbContext();
            dbContext.UseSqlServer("Server=localhost;Database=dev-orm;User ID=sa;Password=123456;TrustServerCertificate=true;");

            // #2 Create Table
            dbContext.TryDropTable<User>();
            dbContext.TryCreateTable<User>();

            // #3 Insert Records
            dbContext.Add(new User { id = 1, name = "lith" });
            dbContext.AddRange(new[] {
                new User { id = 2, name = "lith", fatherId = 1 },
                new User { id = 3, name = "lith", fatherId = 1 }
            });

            // #4 Query Records
            {
                var user = dbContext.Get<User>(1);
                var users = dbContext.Query<User>().Where(u => u.name.Contains("li")).ToList();
                var sql = dbContext.Query<User>().Where(u => u.name.Contains("li")).ToExecuteString();
            }

            // #5 Update Records
            dbContext.Update(new User { id = 1, name = "lith1" });
            dbContext.UpdateRange(new[] {
                new User { id = 2, name = "lith2", fatherId = 1 },
                new User { id = 3, name = "lith3", fatherId = 2 }
            });
            dbContext.Query<User>().Where(u => u.name.Contains("li"))
                .ExecuteUpdate(u => new User { name = "Lith" + u.id });

            // #6 Delete Records
            dbContext.Delete<User>(new User { id = 1, name = "lith1" });
            dbContext.DeleteRange(new[] {
                new User { id = 2, name = "lith2", fatherId = 1 },
                new User { id = 3, name = "lith3", fatherId = 2 }
            });
            dbContext.DeleteByKey<User>(1);
            dbContext.DeleteByKeys<User, int>(new[] { 1, 2 });
            dbContext.Query<User>().Where(u => u.name.Contains("li"))
                .ExecuteDelete();

            // #7 Join Queries
            {
                var query =
                        from user in dbContext.Query<User>()
                        from father in dbContext.Query<User>().Where(father => user.fatherId == father.id).DefaultIfEmpty()
                        where father != null
                        orderby user.id
                        select new { user, father };

                var sql = query.ToExecuteString();
                var users = query.ToList();
            }

            // #8 Transactions
            {
                using var tran1 = dbContext.BeginTransaction();
                dbContext.Update(new User { id = 4, name = "u4001" });

                using (var tran2 = dbContext.BeginTransaction())
                {
                    dbContext.Update(new User { id = 4, name = "u4002" });
                    // will rollback
                }

                using (var tran2 = dbContext.BeginTransaction())
                {
                    dbContext.Update(new User { id = 4, name = "u4002" });
                    tran2.Rollback();
                }

                using (var tran2 = dbContext.BeginTransaction())
                {
                    dbContext.Update(new User { id = 4, name = "u4003" });
                    tran2.Commit();
                }

                tran1.Commit();
            }
        }

        // Entity Definition
        [System.ComponentModel.DataAnnotations.Schema.Table("User")]
        public class User
        {
            [System.ComponentModel.DataAnnotations.Key]
            public int id { get; set; }
            public string name { get; set; }
            public DateTime? birth { get; set; }
            public int? fatherId { get; set; }
        }
    }
}
```

## Explanation   
1. **Setup**: Initializes the database and configures Vitorm.
2. **Create Table**: Drops and recreates the `User` table.
3. **Insert Records**: Adds single and multiple user records.
4. **Query Records**: Retrieves user records using various querying methods.
5. **Update Records**: Updates single and multiple user records.
6. **Delete Records**: Deletes single and multiple user records.
7. **Join Queries**: Performs a join operation between user and father records.
8. **Transactions**: Demonstrates nested transactions and rollback/commit operations.
9. **Database Functions**: Uses custom database functions in queries.



# Vitorm.Data Documentation    
Vitorm.Data is a static class that allows you to use Vitorm without explicitly creating or disposing of a DbContext.    
 
## Installation    
Before using Vitorm.Data, install the necessary package:    
``` bash
dotnet add package Vitorm.Data
dotnet add package Vitorm.SqlServer
```

## Config settings
``` json
// appsettings.json
{
  "Vitorm": {
    "Data": [
      {
        "provider": "SqlServer",
        "namespace": "App",
        "connectionString": "Server=localhost;Database=dev-orm;User ID=sa;Password=123456;TrustServerCertificate=true;"
      }
    ]
  }
}
```

## Minimum viable demo
> After configuring the `appsettings.json` file, you can directly perform queries without any additional configuration or initialization, `Vitorm.Data` is that easy to use.    
``` csharp
using Vitorm;
namespace App
{
    public class Program_Min
    {
        static void Main2(string[] args)
        {
            //  Query Records
            var user = Data.Get<User>(1);
            var users = Data.Query<User>().Where(u => u.name.Contains("li")).ToList();
        }

        // Entity Definition
        [System.ComponentModel.DataAnnotations.Schema.Table("User")]
        public class User
        {
            [System.ComponentModel.DataAnnotations.Key]
            public int id { get; set; }
            public string name { get; set; }
            public DateTime? birth { get; set; }
            public int? fatherId { get; set; }
        }
    }
}

```

## Full Example    
``` csharp
using Vitorm;

namespace App
{
    public class Program
    {
        static void Main(string[] args)
        {
            // #1 No need to init Vitorm.Data

            // #2 Create Table
            Data.TryDropTable<User>();
            Data.TryCreateTable<User>();

            // #3 Insert Records
            Data.Add(new User { id = 1, name = "lith" });
            Data.AddRange(new[] {
                new User { id = 2, name = "lith", fatherId = 1 },
                new User { id = 3, name = "lith", fatherId = 1 }
            });

            // #4 Query Records
            {
                var user = Data.Get<User>(1);
                var users = Data.Query<User>().Where(u => u.name.Contains("li")).ToList();
                var sql = Data.Query<User>().Where(u => u.name.Contains("li")).ToExecuteString();
            }

            // #5 Update Records
            Data.Update(new User { id = 1, name = "lith1" });
            Data.UpdateRange(new[] {
                new User { id = 2, name = "lith2", fatherId = 1 },
                new User { id = 3, name = "lith3", fatherId = 2 }
            });
            Data.Query<User>().Where(u => u.name.Contains("li"))
                .ExecuteUpdate(u => new User { name = "Lith" + u.id });

            // #6 Delete Records
            Data.Delete<User>(new User { id = 1, name = "lith1" });
            Data.DeleteRange(new[] {
                new User { id = 2, name = "lith2", fatherId = 1 },
                new User { id = 3, name = "lith3", fatherId = 2 }
            });
            Data.DeleteByKey<User>(1);
            Data.DeleteByKeys<User, int>(new[] { 1, 2 });
            Data.Query<User>().Where(u => u.name.Contains("li"))
                .ExecuteDelete();

            // #7 Join Queries
            {
                var query =
                        from user in Data.Query<User>()
                        from father in Data.Query<User>().Where(father => user.fatherId == father.id).DefaultIfEmpty()
                        where father != null
                        orderby user.id
                        select new { user, father };

                var sql = query.ToExecuteString();
                var users = query.ToList();
            }

            // #8 Transactions
            {
                using var dbContext = Data.DataProvider<User>().CreateSqlDbContext();
                using var tran1 = dbContext.BeginTransaction();

                dbContext.Update(new User { id = 4, name = "u4001" });

                using (var tran2 = dbContext.BeginTransaction())
                {
                    dbContext.Update(new User { id = 4, name = "u4002" });
                    // will rollback
                }

                using (var tran2 = dbContext.BeginTransaction())
                {
                    dbContext.Update(new User { id = 4, name = "u4002" });
                    tran2.Rollback();
                }

                using (var tran2 = dbContext.BeginTransaction())
                {
                    dbContext.Update(new User { id = 4, name = "u4003" });
                    tran2.Commit();
                }

                tran1.Commit();
            }
        }

        // Entity Definition
        [System.ComponentModel.DataAnnotations.Schema.Table("User")]
        public class User
        {
            [System.ComponentModel.DataAnnotations.Key]
            public int id { get; set; }
            public string name { get; set; }
            public DateTime? birth { get; set; }
            public int? fatherId { get; set; }
        }
    }
}
```


[Test Example](https://github.com/VitormLib/Vitorm/tree/master/test/Vitorm.SqlServer.MsTest)    
