
# Vitorm
Vitorm is a lightweight yet comprehensive ORM that strikes the perfect balance between simplicity and functionality.     
Similar to Dapper in its lightweight design, Vitorm offers an easy-to-use, efficient interface for data access.     
However, it doesn't stop there; Vitorm goes beyond basic ORM capabilities to provide a rich feature set akin to Entity Framework.     
This means you get the best of both worlds: the performance and simplicity of Dapper with the robust features and flexibility of Entity Framework, making Vitorm an ideal choice for developers seeking a powerful yet streamlined ORM solution.
> written by ChatGPT :  "Help me write a description for Vitorm. It is a lightweight ORM, similar to Dapper, but it is feature-rich, akin to EF. Please emphasize its lightweight nature and comprehensive functionality."    
> source address: [https://github.com/VitormLib/Vitorm](https://github.com/VitormLib/Vitorm "https://github.com/VitormLib/Vitorm")        

![](https://img.shields.io/github/license/VitormLib/Vitorm.svg)  
![](https://img.shields.io/github/repo-size/VitormLib/Vitorm.svg)  ![](https://img.shields.io/github/last-commit/VitormLib/Vitorm.svg)  
 

| Build | NuGet |
| -------- | -------- |
|![](https://github.com/VitormLib/Vitorm/workflows/ki_devops3/badge.svg) | [![](https://img.shields.io/nuget/v/Vitorm.svg)](https://www.nuget.org/packages/Vitorm) ![](https://img.shields.io/nuget/dt/Vitorm.svg) |



| Database | Supported | Code | NuGet |
| -------- | -------- | -------- | -------- |
| MySql         |   √   | [MySql](src/develop/src/Vitorm.MySql)             |  [![](https://img.shields.io/nuget/v/Vitorm.MySql.svg)](https://www.nuget.org/packages/Vitorm.MySql) ![](https://img.shields.io/nuget/dt/Vitorm.MySql.svg)   |
| SqlServer     |   √   | [SqlServer](src/develop/src/Vitorm.SqlServer)     |  [![](https://img.shields.io/nuget/v/Vitorm.SqlServer.svg)](https://www.nuget.org/packages/Vitorm.SqlServer) ![](https://img.shields.io/nuget/dt/Vitorm.SqlServer.svg)   |
| Sqlite        |   √   | [Sqlite](src/develop/src/Vitorm.Sqlite)           |  [![](https://img.shields.io/nuget/v/Vitorm.Sqlite.svg)](https://www.nuget.org/packages/Vitorm.Sqlite) ![](https://img.shields.io/nuget/dt/Vitorm.Sqlite.svg)   |
| ElasticSearch |   √   | [ElasticSearch](https://github.com/VitormLib/Vitorm.ElasticSearch)     |  [![](https://img.shields.io/nuget/v/Vitorm.ElasticSearch.svg)](https://www.nuget.org/packages/Vitorm.ElasticSearch) ![](https://img.shields.io/nuget/dt/Vitorm.ElasticSearch.svg)   |
| ClickHouse    |   √   | [ClickHouse](https://github.com/VitormLib/Vitorm.ClickHouse)     |  [![](https://img.shields.io/nuget/v/Vitorm.ClickHouse.svg)](https://www.nuget.org/packages/Vitorm.ClickHouse) ![](https://img.shields.io/nuget/dt/Vitorm.ClickHouse.svg)   |
| Oracle        |   ×   |      |      |




# Vitorm Documentation
This guide will walk you through the steps to set up and use Vitorm with SQLite.

## Installation
Before using Vitorm, install the necessary package:

``` bash
dotnet add package Vitorm.Sqlite
```

## Using Vitorm
> This example provides a comprehensive guide to utilizing Vitorm for basic and advanced database operations while maintaining lightweight performance.
``` csharp
using Vit.Extensions.Vitorm_Extensions;
using Vitorm;

namespace App
{
    public class Program
    {
        static void Main(string[] args)
        {
            // #1 Create an empty SQLite database file and configures Vitorm
            File.WriteAllBytes("sqlite.db", new byte[0]);
            using var dbContext = new Vitorm.Sql.SqlDbContext();
            dbContext.UseSqlite("data source=sqlite.db");

            // #2 Create Table
            dbContext.Drop<User>();
            dbContext.Create<User>();

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

            // #9 Database Functions
            {
                // select * from User where IIF(t0.fatherId is not null, true, false);
                var query = dbContext.Query<User>().Where(u => DbFunction.Call<bool>("IIF", u.fatherId != null, true, false));
                var sql = query.ToExecuteString();
                var userList = query.ToList();
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

1. **Setup**: Initializes the SQLite database and configures Vitorm.
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

Before using Vitorm, install the necessary package:
``` bash
dotnet add package Vitorm.Data
dotnet add package Vitorm.Sqlite
```

## Config settings
``` json
// appsettings.json
{
  "Vitorm": {
    "Data": [
      {
        "provider": "Vitorm.Sqlite.DataProvider",
        "assemblyFile": "Vitorm.Sqlite.dll",
        "namespace": "App",
        "connectionString": "data source=sqlite.db;"
      }
    ]
  }
}
```


## Using Vitorm.Data
``` csharp
 using Vit.Extensions.Vitorm_Extensions;
using Vitorm;

namespace App
{
    public class Program
    {
        static void Main(string[] args)
        {
            // #1 Create an empty SQLite database file and configures Vitorm.Data
            File.WriteAllBytes("sqlite.db", new byte[0]);

            // #2 Create Table
            Data.Drop<User>();
            Data.Create<User>();

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

            // #9 Database Functions
            {
                // select * from User where IIF(t0.fatherId is not null, true, false);
                var query = Data.Query<User>().Where(u => DbFunction.Call<bool>("IIF", u.fatherId != null, true, false));
                var sql = query.ToExecuteString();
                var userList = query.ToList();
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

# Examples  
- [CRUD](test/Vitorm.Sqlite.MsTest/CommonTest/CRUD_Test.cs)    
- [ExecuteDelete](test/Vitorm.Sqlite.MsTest/CommonTest/Orm_Extensions_ExecuteDelete_Test.cs)    
- [ExecuteUpdate](test/Vitorm.Sqlite.MsTest/CommonTest/Orm_Extensions_ExecuteUpdate_Test.cs)    
- [ToExecuteString](test/Vitorm.Sqlite.MsTest/CommonTest/Orm_Extensions_ToExecuteString_Test.cs)    
    
- [Query_LinqMethods](test/Vitorm.Sqlite.MsTest/CommonTest/Query_LinqMethods_Test.cs)  
- [Query_Distinct](test/Vitorm.Sqlite.MsTest/CommonTest/Query_LinqMethods_Distinct_Test.cs)  
    
- [Query_String](test/Vitorm.Sqlite.MsTest/CommonTest/Query_Type_String_Test.cs)  
- [Query_String_Like](test/Vitorm.Sqlite.MsTest/CommonTest/Query_Type_String_Like_Test.cs)  
- [Query_String_Caculate](test/Vitorm.Sqlite.MsTest/CommonTest/Query_Type_String_Caculate_Test.cs)  
    
- [Query_Numric](test/Vitorm.Sqlite.MsTest/CommonTest/Query_Type_Numric_Test.cs)  
- [Query_Numric_Caculate](test/Vitorm.Sqlite.MsTest/CommonTest/Query_Type_Numric_Caculate_Test.cs)  
    
- [Query_DateTime](test/Vitorm.Sqlite.MsTest/CommonTest/Query_Type_DateTime_Test.cs)  
    
- [InnerJoin_BySelectMany](test/Vitorm.Sqlite.MsTest/CommonTest/Query_InnerJoin_BySelectMany_Test.cs)  
- [InnerJoin_ByJoin](test/Vitorm.Sqlite.MsTest/CommonTest/Query_InnerJoin_ByJoin_Test.cs)  
- [LeftJoin_BySelectMany](test/Vitorm.Sqlite.MsTest/CommonTest/Query_LeftJoin_BySelectMany_Test.cs)  
- [LeftJoin_ByGroupJoin](test/Vitorm.Sqlite.MsTest/CommonTest/Query_LeftJoin_ByGroupJoin_Test.cs)  
    
- [GroupBy](test/Vitorm.Sqlite.MsTest/CommonTest/Query_Group_Test.cs)  
- [Transaction](test/Vitorm.Sqlite.MsTest/CommonTest/Transaction_Test.cs)  
    
- [MySql-DbFunction](test/Vitorm.MySql.MsTest/CustomTest/DbFunction_Test.cs)  
- [SqlServer-DbFunction](test/Vitorm.SqlServer.MsTest/CustomTest/DbFunction_Test.cs)  
- [Sqlite-DbFunction](test/Vitorm.Sqlite.MsTest/CustomTest/DbFunction_Test.cs)  

