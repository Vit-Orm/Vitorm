# Vitorm ReleaseLog

-----------------------
# 1.2.0

- support multipe EntityReader ( EntityConstructor and CompiledLambda )
- fix String.Format not work for EntityConstructorReader issue
> userQuery.Select(user => new { name = $"{user.id}_{user.fatherId}_{user.motherId}" });

- fix treat String.Add as Numeric.Add issue

-----------------------
# 1.1.0

- fix Count issue (work for skip take and distinct)
- Data and Vitorm ReadOnly feature
- move extensions to root namespace Vitorm
- support Drop
- fix the issue when column name from database differ from property name from entity