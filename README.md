
# Vitorm
Vitorm: an simple orm by Vit.Linq
>source address: [https://github.com/VitormLib/Vitorm](https://github.com/VitormLib/Vitorm "https://github.com/VitormLib/Vitorm")    

![](https://img.shields.io/github/license/VitormLib/Vitorm.svg)  
![](https://img.shields.io/github/repo-size/VitormLib/Vitorm.svg)  ![](https://img.shields.io/github/last-commit/VitormLib/Vitorm.svg)  
 

| Build | NuGet |
| -------- | -------- |
|![](https://github.com/VitormLib/Vitorm/workflows/ki_multibranch/badge.svg) | [![](https://img.shields.io/nuget/v/Vitorm.svg)](https://www.nuget.org/packages/Vitorm/) ![](https://img.shields.io/nuget/dt/Vitorm.svg) |



| Db | Supported | Code | nuget |
| -------- | -------- | -------- | -------- |
| MySql     |    √  | [MySql](src/develop/src/Vitorm.MySql)     |  [Vitorm.MySql](https://www.nuget.org/packages/Vitorm.MySql)   |
| SqlServer     |    √  | [SqlServer](src/develop/src/Vitorm.SqlServer)     |  [Vitorm.SqlServer](https://www.nuget.org/packages/Vitorm.SqlServer)   |
| Sqlite     |    √  | [Sqlite](src/develop/src/Vitorm.Sqlite)     |  [Vitorm.Sqlite](https://www.nuget.org/packages/Vitorm.Sqlite)   |
| ElasticSearch     |    ×  |      |      |
| ClickHouse     |    ×  |      |      |
| Oracle     |    ×  |      |      |



Async
TinyOrm




Examples:  
- [CRUD](test/Vitorm.Sqlite.MsTest/CommonTest/CRUD_Test.cs)    
- [Query](test/Vitorm.Sqlite.MsTest/CommonTest/Query_Test.cs)  

- [InnerJoin_BySelectMany](test/Vitorm.Sqlite.MsTest/CommonTest/Query_InnerJoin_BySelectMany_Test.cs)  
- [InnerJoin_ByJoin](test/Vitorm.Sqlite.MsTest/CommonTest/Query_InnerJoin_ByJoin_Test.cs)  
- [LeftJoin_BySelectMany](test/Vitorm.Sqlite.MsTest/CommonTest/Query_LeftJoin_BySelectMany_Test.cs)  
- [LeftJoin_ByGroupJoin](test/Vitorm.Sqlite.MsTest/CommonTest/Query_LeftJoin_ByGroupJoin_Test.cs)  

- [GroupBy](test/Vitorm.Sqlite.MsTest/CommonTest/Query_Group_Test.cs)  
- [Transaction](test/Vitorm.Sqlite.MsTest/CommonTest/Transaction_Test.cs)  
- [DbFunction](test/Vitorm.Sqlite.MsTest/CommonTest/DbFunction_Test.cs)  

