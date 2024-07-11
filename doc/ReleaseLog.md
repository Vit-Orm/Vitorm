# Vitorm ReleaseLog

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

-----------------------
# 1.1.0

- fix Count issue (work for skip take and distinct)
- Data and Vitorm ReadOnly feature
- move extensions to root namespace Vitorm
- support Drop
- fix the issue when column name from database differ from property name from entity