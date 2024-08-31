
# Vitorm.EntityGenerate
Library to generate Entity Type from Database
> source address: [https://github.com/Vit-Orm/Vitorm](https://github.com/Vit-Orm/Vitorm "https://github.com/Vit-Orm/Vitorm/tree/master/src/Vitorm.EntityGenerate")        

![](https://img.shields.io/github/license/Vit-Orm/Vitorm.svg)  
![](https://img.shields.io/github/repo-size/Vit-Orm/Vitorm.svg)  ![](https://img.shields.io/github/last-commit/Vit-Orm/Vitorm.svg)  
 

| Build | NuGet |
| -------- | -------- |
|![](https://github.com/Vit-Orm/Vitorm/workflows/ki_devops3_build/badge.svg) | [![](https://img.shields.io/nuget/v/Vitorm.EntityGenerate.svg)](https://www.nuget.org/packages/Vitorm.EntityGenerate) ![](https://img.shields.io/nuget/dt/Vitorm.EntityGenerate.svg) |




# Vitorm.EntityGenerate Documentation    
 
## Installation    
Before using , install the necessary package:
``` bash
dotnet add package Vitorm.EntityGenerate
dotnet add package Vitorm.Sqlite
```

## Minimum viable demo
> The Entity for table GeneratedUser does not exist, Vitorm.EntityGenerate can help us generate an temporary one.     

``` csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vitorm.Sql;
namespace Vitorm.MsTest.Sqlite
{
    [TestClass]
    public partial class EntityGenerate_Test
    {
        [TestMethod]
        public void Test()
        {
            // #1 init
            var entityNamespace = "Vitorm.MsTest.Sqlite";
            var guid = Guid.NewGuid().ToString();
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{guid}.sqlite.db");
            var connectionString = $"data source={filePath}";

            using var dbContext = new SqlDbContext();
            dbContext.UseSqlite(connectionString);

            dbContext.Execute(@"
DROP TABLE if exists GeneratedUser;
CREATE TABLE GeneratedUser(id integer NOT NULL PRIMARY KEY,  name text DEFAULT NULL);
Insert into GeneratedUser(id,name) values(1,'u146');
");

            // #2 test
            var dbSet = dbContext.GenerateDbSet(entityNamespace: entityNamespace, tableName: "GeneratedUser");
            var entityType = dbSet.entityDescriptor.entityType;

            // GetEntity
            dynamic user = dbSet.Get(1);
            string name = user.name;
            Assert.AreEqual("u146", name);

            BaseTest.TestDbSet(dbSet);
        }
    }
}

```
 

[Test Example](https://github.com/Vit-Orm/Vitorm/tree/master/test/Vitorm.EntityGenerate.MsTest)    