 Because count function will exclude rows with non value, that's not what we want, so will use inner query for all query
 ```
  select count(distinct userFatherId) from [User];

  select count(*) from (select distinct fatherid,motherId from "User" u) u;
 
 ```


# count
```
----------------
select count(id)
select count(*)
select count(*) from (select fatherid,motherId from "User" u) u;
----------------
// single table
[ ] users.Select(user => user.fatherId)
[ ] users.Select(user => new { user.fatherId })
[ ] users.Select(user => user)                                  // has key
[ ] users.Select(user => new { user })                          // has key
[ ] users.Select(user => new { user, user.fatherId })           // has key
[ ] users.Select(user => new { user.id, user.fatherId })        // has key

users.Select(user => user)                              // has no key
users.Select(user => new { user })                      // has no key
users.Select(user => new { user, user.fatherId })       // has no key
users.Select(user => new { user.id, user.fatherId })    // has no key


// multiple table
[ ] users.SelectMany(user => users.Where(father => father.id == user.fatherId), (user, father) => user.fatherId)
[ ] users.SelectMany(user => users.Where(father => father.id == user.fatherId), (user, father) => new { user.fatherId })

[ ] users.SelectMany(user => users.Where(father => father.id == user.fatherId), (user, father) => user)                     // has key
[ ] users.SelectMany(user => users.Where(father => father.id == user.fatherId), (user, father) => new { user })             // has key
[ ] users.SelectMany(user => users.Where(father => father.id == user.fatherId), (user, father) => new { user, user.id })    // has key

users.SelectMany(user => users.Where(father => father.id == user.fatherId), (user, father) => new { user, father })
users.SelectMany(user => users.Where(father => father.id == user.fatherId), (user, father) => user)                     // has no key
users.SelectMany(user => users.Where(father => father.id == user.fatherId), (user, father) => new { user })             // has no key
users.SelectMany(user => users.Where(father => father.id == user.fatherId), (user, father) => new { user, user.id })    // has no key


```


--------------------------------
# distinct count
```
----------------
select count(distinct id)
select count(*) from (select distinct fatherid,motherId from "User" u) u;

```


--------------------------------
# single column
```
select count(*) from [dbo].[User] as t0; 
select count(userFatherId) from [dbo].[User];
select count(distinct userFatherId) from [dbo].[User];
```