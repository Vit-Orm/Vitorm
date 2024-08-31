# Vitorm ReleaseLog

-----------------------
# 2.1.2
- [Vitorm.Data] add DataSource class, change methods to instance from static


-----------------------
# 2.1.0
- block query from different data source
- support execute event
- [Vitorm.Data] Ability to load the configuration from a specified file, such as appsettings.Development.json

-----------------------
# 2.0.6

- #4 support schema name of table for MySql
- #5 single bool value in where condition
- [Vitorm.Data] support name and multiple namespace
- #6 [Vitorm.Data] auto load entityLoader from appsettings.json

-----------------------
# 2.0.5
- support Async Methods
  - AddAsync AddRangeAsync
  - GetAsync
  - UpdateAsync UpdateRangeAsync
  - DeleteAsync DeleteRangeAsync DeleteByKeyAsync DeleteByKeysAsync

- support Async IQueryable Methods
  - ToListAsync
  - CountAsync TotalCountAsync
  - ToListAndTotalCountAsync
  - ExecuteDeleteAsync
  - ExecuteUpdateAsync
  - FirstOrDefaultAsync FirstAsync LastOrDefaultAsync LastAsync

- EntityLoader_FromAttribute
- Vitorm.Entity.Loader.DataAnnotations.EntityLoader (strictMode, default false)
> if strictMode is false: will get typeName as tableName if not specify TableAttribute, and will set property named Id (or tableName + "Id") as key

- support Guid
- support column attribute Require and MaxLength

-----------------------
# 2.0.4
- [Vitorm] support Truncate

-----------------------
# 2.0.3
- [Vitorm] StreamReader support custom methodCallConvertor and add test case

-----------------------
# 2.0.1

- support ToListAsync
- Extensions for IDbSet


-----------------------
# 1.2.0

- support multipe EntityReader ( EntityConstructor and CompiledLambda )
- fix String.Format not work for EntityConstructorReader issue
> userQuery.Select(user => new { name = $"{user.id}_{user.fatherId}_{user.motherId}" });

- fix treat String.Add as Numeric.Add issue
- support bool and bool? column mapping
- [Vitorm.SqlServer] fix bool type was not supported in database issue (especially in select sentence)
- [Vitorm.ClickHouse] fix String.Add null value and cast issue : ifNull(  cast( (userFatherId) as Nullable(String) ) , ''  )
- [Vitorm.Sqlite] fix String.Add null value and cast issue
- [Vitorm] new feature to change mapped table and change database for sharding
- [Vitorm] new feature to query TotalCount(Vit.Linq.Queryable_Extensions.TotalCount) 
           and query List and TotalCount(Vit.Linq.Queryable_Extensions.ToListAndTotalCount) at one request

- [Vitorm] rename PrepareCreate to PrepareTryCreateTable
- [Vitorm] rename PrepareDrop to PrepareTryDropTable
- [Vitorm] move Dictionary<string, object> sqlParam to SqlTranslateArgument or QueryTranslateArgument



-----------------------
# 1.1.0

- fix Count issue (work for skip take and distinct)
- Data and Vitorm ReadOnly feature
- move extensions to root namespace Vitorm
- support Drop
- fix the issue when column name from database differ from property name from entity