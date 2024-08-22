
# Vitorm
Vitorm is a lightweight yet comprehensive ORM that strikes the perfect balance between simplicity and functionality.    
Similar to Dapper in its lightweight design, Vitorm offers an easy-to-use, efficient interface for data access.    
However, it doesn't stop there; Vitorm goes beyond basic ORM capabilities to provide a rich feature set akin to Entity Framework.    
This means you get the best of both worlds: the performance and simplicity of Dapper with the robust features and flexibility of Entity Framework, making Vitorm an ideal choice for developers seeking a powerful yet streamlined ORM solution.
> source address: [https://github.com/VitormLib/Vitorm](https://github.com/VitormLib/Vitorm "https://github.com/VitormLib/Vitorm")    

![](https://img.shields.io/github/license/VitormLib/Vitorm.svg)  
![](https://img.shields.io/github/repo-size/VitormLib/Vitorm.svg)  ![](https://img.shields.io/github/last-commit/VitormLib/Vitorm.svg)  
 

| Build | NuGet |
| -------- | -------- |
|![](https://github.com/VitormLib/Vitorm/workflows/ki_devops3/badge.svg) | [![](https://img.shields.io/nuget/v/Vitorm.svg)](https://www.nuget.org/packages/Vitorm) ![](https://img.shields.io/nuget/dt/Vitorm.svg) |



| Database | Supported | Code | NuGet |
| -------- | -------- | -------- | -------- |
| MySql         |   √   | [MySql](https://github.com/VitormLib/Vitorm/tree/master/src/Vitorm.MySql)             |  [![](https://img.shields.io/nuget/v/Vitorm.MySql.svg)](https://www.nuget.org/packages/Vitorm.MySql) ![](https://img.shields.io/nuget/dt/Vitorm.MySql.svg)   |
| SqlServer     |   √   | [SqlServer](https://github.com/VitormLib/Vitorm/tree/master/src/Vitorm.SqlServer)     |  [![](https://img.shields.io/nuget/v/Vitorm.SqlServer.svg)](https://www.nuget.org/packages/Vitorm.SqlServer) ![](https://img.shields.io/nuget/dt/Vitorm.SqlServer.svg)   |
| Sqlite        |   √   | [Sqlite](https://github.com/VitormLib/Vitorm/tree/master/src/Vitorm.Sqlite)           |  [![](https://img.shields.io/nuget/v/Vitorm.Sqlite.svg)](https://www.nuget.org/packages/Vitorm.Sqlite) ![](https://img.shields.io/nuget/dt/Vitorm.Sqlite.svg)   |
| ElasticSearch |   √   | [ElasticSearch](https://github.com/VitormLib/Vitorm.ElasticSearch)     |  [![](https://img.shields.io/nuget/v/Vitorm.ElasticSearch.svg)](https://www.nuget.org/packages/Vitorm.ElasticSearch) ![](https://img.shields.io/nuget/dt/Vitorm.ElasticSearch.svg)   |
| ClickHouse    |   √   | [ClickHouse](https://github.com/VitormLib/Vitorm.ClickHouse)     |  [![](https://img.shields.io/nuget/v/Vitorm.ClickHouse.svg)](https://www.nuget.org/packages/Vitorm.ClickHouse) ![](https://img.shields.io/nuget/dt/Vitorm.ClickHouse.svg)   |
| Oracle        |   ×   |      |      |



# Vitorm Documentation
This guide will walk you through the steps to set up and use Vitorm with SQLite.

supported features:

| feature    |  method   |  remarks   |     |
| --- | --- | --- | --- |
|  create table   |  TryCreateTable   |     |     |
|  drop table   |  TryDropTable   |     |     |
|  truncate table   |  Truncate   |     |     |
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
Before using Vitorm, install the necessary package:    
``` bash
dotnet add package Vitorm.Sqlite
```

## Minimum viable demo
> code address: [Program_Min.cs](https://github.com/VitormLib/Vitorm/tree/master/test/Vitorm.Sqlite.Console/Program_Min.cs)    
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
            dbContext.UseSqlite("data source=sqlite.db");

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
> code address: [Program.cs](https://github.com/VitormLib/Vitorm/tree/master/test/Vitorm.Sqlite.Console/Program.cs)    
``` csharp
using Vitorm;

namespace App
{
    public class Program
    {
        static void Main(string[] args)
        {
            // #1 Configures Vitorm
            using var dbContext = new Vitorm.Sql.SqlDbContext();
            dbContext.UseSqlite("data source=sqlite.db");

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
            dbContext.Truncate<User>();

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
dotnet add package Vitorm.Sqlite
```

## Config settings
``` json
// appsettings.json
{
  "Vitorm": {
    "Data": [
      {
        "provider": "Sqlite",
        "namespace": "App",
        "connectionString": "data source=sqlite.db;"
      }
    ]
  }
}
```

## Minimum viable demo
> After configuring the `appsettings.json` file, you can directly perform queries without any additional configuration or initialization, `Vitorm.Data` is that easy to use.    
> code address: [Program_Min.cs](https://github.com/VitormLib/Vitorm/tree/master/test/Vitorm.Data.Console/Program_Min.cs)    
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
> code address: [Program.cs](https://github.com/VitormLib/Vitorm/tree/master/test/Vitorm.Data.Console/Program.cs)    
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
            Data.Truncate<User>();

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


# Comparison of performance with other ORMs
> Through benchmarks, there may be slight variations depending on the database source, for reference only.
> code address: [Vitorm.Data.Benchmark](https://github.com/VitormLib/Vitorm/tree/master/test/Vitorm.Data.Benchmark)    

## SqlServer 
| Method | queryJoin | take | runner                 | Mean         | Error     | StdDev    |
|------- |---------- |----- |----------------------- |-------------:|----------:|----------:|
| Run    | False     | 1    | Runner_EntityFramework |     5.380 ms | 0.0449 ms | 0.0398 ms |
| Run    | False     | 1    | Runner_SqlSuger        |     4.929 ms | 0.0588 ms | 0.0550 ms |
| Run    | False     | 1    | Runner_Vitorm          |     4.925 ms | 0.0506 ms | 0.0474 ms |
| Run    | False     | 1000 | Runner_EntityFramework |     6.009 ms | 0.0963 ms | 0.0853 ms |
| Run    | False     | 1000 | Runner_SqlSuger        |     5.616 ms | 0.1114 ms | 0.1488 ms |
| Run    | False     | 1000 | Runner_Vitorm          |     5.539 ms | 0.1060 ms | 0.1262 ms |
| Run    | True      | 1    | Runner_EntityFramework |     5.453 ms | 0.0571 ms | 0.0534 ms |
| Run    | True      | 1    | Runner_SqlSuger        |     5.601 ms | 0.0477 ms | 0.0423 ms |
| Run    | True      | 1    | Runner_Vitorm          |     5.337 ms | 0.0596 ms | 0.0528 ms |
| Run    | True      | 1000 | Runner_EntityFramework | 1,706.222 ms | 4.4435 ms | 3.9391 ms |
| Run    | True      | 1000 | Runner_SqlSuger        |   125.150 ms | 2.3678 ms | 2.3255 ms |
| Run    | True      | 1000 | Runner_Vitorm          |    21.027 ms | 0.3564 ms | 0.3334 ms |

## MySql 
| Method | queryJoin | take | runner                 | Mean         | Error       | StdDev      |
|------- |---------- |----- |----------------------- |-------------:|------------:|------------:|
| Run    | False     | 1    | Runner_EntityFramework |   1,231.1 μs |    47.43 μs |   139.84 μs |
| Run    | False     | 1    | Runner_SqlSuger        |     873.7 μs |    22.33 μs |    65.49 μs |
| Run    | False     | 1    | Runner_Vitorm          |     636.7 μs |    21.15 μs |    62.37 μs |
| Run    | False     | 1000 | Runner_EntityFramework |   4,764.8 μs |    56.52 μs |    52.87 μs |
| Run    | False     | 1000 | Runner_SqlSuger        |   1,906.4 μs |    33.21 μs |    42.00 μs |
| Run    | False     | 1000 | Runner_Vitorm          |   2,159.5 μs |    43.16 μs |    56.12 μs |
| Run    | True      | 1    | Runner_EntityFramework |   1,775.0 μs |    35.01 μs |    80.45 μs |
| Run    | True      | 1    | Runner_SqlSuger        |   1,747.5 μs |    34.05 μs |    44.27 μs |
| Run    | True      | 1    | Runner_Vitorm          |   1,292.5 μs |    25.72 μs |    41.54 μs |
| Run    | True      | 1000 | Runner_EntityFramework |   6,784.4 μs |   102.05 μs |    95.45 μs |
| Run    | True      | 1000 | Runner_SqlSuger        | 113,634.4 μs | 1,128.58 μs | 1,055.67 μs |
| Run    | True      | 1000 | Runner_Vitorm          |   3,175.2 μs |    62.02 μs |    88.95 μs |

## Sqlite
| Method | queryJoin | take | runner                 | Mean          | Error      | StdDev     |
|------- |---------- |----- |----------------------- |--------------:|-----------:|-----------:|
| Run    | False     | 1    | Runner_EntityFramework |     115.32 μs |   0.831 μs |   0.778 μs |
| Run    | False     | 1    | Runner_SqlSuger        |      81.66 μs |   0.747 μs |   0.699 μs |
| Run    | False     | 1    | Runner_Vitorm          |      45.64 μs |   0.350 μs |   0.328 μs |
| Run    | False     | 1000 | Runner_EntityFramework |   1,386.81 μs |  13.684 μs |  12.800 μs |
| Run    | False     | 1000 | Runner_SqlSuger        |     674.82 μs |   4.938 μs |   4.619 μs |
| Run    | False     | 1000 | Runner_Vitorm          |     769.88 μs |   6.020 μs |   5.631 μs |
| Run    | True      | 1    | Runner_EntityFramework |     220.27 μs |   1.916 μs |   1.793 μs |
| Run    | True      | 1    | Runner_SqlSuger        |     484.52 μs |   6.746 μs |   5.980 μs |
| Run    | True      | 1    | Runner_Vitorm          |     167.89 μs |   1.352 μs |   1.264 μs |
| Run    | True      | 1000 | Runner_EntityFramework |   1,962.25 μs |  10.031 μs |   8.377 μs |
| Run    | True      | 1000 | Runner_SqlSuger        | 103,179.50 μs | 534.265 μs | 446.135 μs |
| Run    | True      | 1000 | Runner_Vitorm          |   1,684.39 μs |  21.895 μs |  18.283 μs |




# Examples  
- [CRUD](test/Vitorm.Sqlite.MsTest/CommonTest/CRUD_Test.cs)    
- [ExecuteDelete](test/Vitorm.Sqlite.MsTest/CommonTest/Orm_Extensions_ExecuteDelete_Test.cs)    
- [ExecuteUpdate](test/Vitorm.Sqlite.MsTest/CommonTest/Orm_Extensions_ExecuteUpdate_Test.cs)    
- [ToExecuteString](test/Vitorm.Sqlite.MsTest/CommonTest/Orm_Extensions_ToExecuteString_Test.cs)    
    
- [Query_Method_Test](test/Vitorm.Sqlite.MsTest/CommonTest/Query_Method_Test.cs)  
- [Query_Distinct_Test](test/Vitorm.Sqlite.MsTest/CommonTest/Query_Distinct_Test.cs)  
    
- [Property_String_Test](test/Vitorm.Sqlite.MsTest/CommonTest/Property_String_Test.cs)  
- [Property_String_Like_Test](test/Vitorm.Sqlite.MsTest/CommonTest/Property_String_Like_Test.cs)  
- [Property_String_Calculate_Test](test/Vitorm.Sqlite.MsTest/CommonTest/Property_String_Calculate_Test.cs)  
    
- [Property_Numeric_Test](test/Vitorm.Sqlite.MsTest/CommonTest/Property_Numeric_Test.cs)  
- [Property_Numeric_Calculate_Test](test/Vitorm.Sqlite.MsTest/CommonTest/Property_Numeric_Calculate_Test.cs)  
    
- [Property_DateTime_Test](test/Vitorm.Sqlite.MsTest/CommonTest/Property_DateTime_Test.cs)  
    
- [InnerJoin_BySelectMany](test/Vitorm.Sqlite.MsTest/CommonTest/Query_InnerJoin_BySelectMany_Test.cs)  
- [InnerJoin_ByJoin](test/Vitorm.Sqlite.MsTest/CommonTest/Query_InnerJoin_ByJoin_Test.cs)  
- [LeftJoin_BySelectMany](test/Vitorm.Sqlite.MsTest/CommonTest/Query_LeftJoin_BySelectMany_Test.cs)  
- [LeftJoin_ByGroupJoin](test/Vitorm.Sqlite.MsTest/CommonTest/Query_LeftJoin_ByGroupJoin_Test.cs)  
    
- [GroupBy](test/Vitorm.Sqlite.MsTest/CommonTest/Query_Group_Test.cs)  
- [Transaction](test/Vitorm.Sqlite.MsTest/CommonTest/Transaction_Test.cs)  
    
- [MySql-DbFunction](test/Vitorm.MySql.MsTest/CustomTest/DbFunction_Test.cs)  
- [SqlServer-DbFunction](test/Vitorm.SqlServer.MsTest/CustomTest/DbFunction_Test.cs)  
- [Sqlite-DbFunction](test/Vitorm.Sqlite.MsTest/CustomTest/DbFunction_Test.cs)  

