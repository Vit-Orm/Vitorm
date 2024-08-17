using Microsoft.VisualStudio.TestTools.UnitTesting;

using Vit.Core.Module.Serialization;
using Vit.Linq;
using Vit.Linq.FilterRules.ComponentModel;

namespace Vitorm.MsTest
{
    public class BaseTest
    {
        public static void TestDbSet(IDbSet dbSet)
        {
            // #1 Create :  Add AddRange
            dbSet.Add(new { id = 2, name = "u246" });
            dbSet.AddRange(new[]{
                new {id=3,name="u356" },
                new {id=4,name="u400" },
                new {id=5,name="u500" },
                new {id=6,name="u600" }
            });


            // #2 Retrieve : Get Query
            #region Get
            {
                dynamic user = dbSet.Get(2);
                string name = user.name;
                Assert.AreEqual("u246", name);
            }
            #endregion
            #region Query
            {
                var query = dbSet.Query();

                var strFilter = "{'field':'id',  'operator': '=',  'value': 3 }".Replace("'", "\"");
                var filter = Json.Deserialize<FilterRule>(strFilter);
                var users = query.IQueryable_Where(filter).IQueryable_ToList();
                dynamic user = users[0];

                string name = user.name;
                Assert.AreEqual("u356", name);
            }
            #endregion

            #region #3 Update: Update UpdateRange
            {
                dbSet.Update(new { id = 2, name = "u246_" });
                dbSet.UpdateRange(new[]{
                    new {id=3,name="u356_" },
                    new {id=4,name="u400_" }
                });

                dynamic user = dbSet.Get(2);
                Assert.AreEqual("u246_", (string)user.name);
            }
            #endregion

            #region #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
            {
                dbSet.Delete(new { id = 2, name = "u246_" });
                dbSet.DeleteRange(new[]{
                    new {id=3,name="u356_" },
                    new {id=4,name="u400_" }
                });

                dbSet.DeleteByKey(1);
                dbSet.DeleteByKeys(new[] { 5, 6 });

                var query = dbSet.Query();
                var count = query.IQueryable_Count();
                Assert.AreEqual(0, count);
            }
            #endregion

        }

    }
}
