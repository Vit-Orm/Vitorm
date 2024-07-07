using Vitorm;

namespace App.Runner
{
    public class EnvSetup
    {
        public static void InitDb()
        {
            Data.Drop<User>();
            Data.Create<User>();

            var users = new List<User> {
                    new User { id=1, name="u146", fatherId=4, motherId=6 },
                    new User { id=2, name="u246", fatherId=4, motherId=6 },
                    new User { id=3, name="u356", fatherId=5, motherId=6 },
                    new User { id=4, name="u400" },
                    new User { id=5, name="u500" },
                    new User { id=6, name="u600" },
                };
            Data.AddRange(users);

            users = Enumerable.Range(7, 1000).Select(id => new User { id = id, name = "user" + id }).ToList();
            Data.AddRange(users);
        }

    }
}
