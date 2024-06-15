
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
| MySql         |   √   | [MySql](src/develop/src/Vitorm.MySql)             |  [Vitorm.MySql](https://www.nuget.org/packages/Vitorm.MySql)   |
| SqlServer     |   √   | [SqlServer](src/develop/src/Vitorm.SqlServer)     |  [Vitorm.SqlServer](https://www.nuget.org/packages/Vitorm.SqlServer)   |
| Sqlite        |   √   | [Sqlite](src/develop/src/Vitorm.Sqlite)           |  [Vitorm.Sqlite](https://www.nuget.org/packages/Vitorm.Sqlite)   |
| ElasticSearch |   √   | [ElasticSearch](https://github.com/VitormLib/Vitorm.ElasticSearch)     |  [Vitorm.MySql](https://www.nuget.org/packages/Vitorm.ElasticSearch.MySql)   |
| ClickHouse    |   ×   |      |      |
| Oracle        |   ×   |      |      |





# Examples:  
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

