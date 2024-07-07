using Vitorm;

namespace App.OrmRunner
{
    public partial class Runner_Vitorm : IRunner
    {

        IQueryable<User> users = Data.Query<User>();
        public IQueryable<User> GetQueryable() => users;


        public void Run(RunConfig config)
        {
            for (int i = 0; i < config.repeatCount; i++)
            {
                if (config.queryJoin) QueryJoin(config.take);
                else Query(config.take);
            }
        }

        public void QueryJoin(int take)
        {
            var queryable = GetQueryable();
            var query =
                    from user in queryable
                    from father in queryable.Where(father => user.fatherId == father.id).DefaultIfEmpty()
                    from mother in queryable.Where(mother => user.motherId == mother.id).DefaultIfEmpty()
                    where user.id > 1
                    orderby user.id
                    select new
                    {
                        user,
                        father,
                        mother,
                        testId = user.id + 100,
                        hasFather = father.name != null ? true : false
                    }
                    ;

            query = query.Skip(1).Take(take);

            var userList = query.ToList();
            var rowCount = userList.Count();
            if (rowCount != take) throw new Exception($"query failed, expected row count : {take} , actual count: {rowCount} ");
        }

        public void Query(int take)
        {
            var userSet = Data.Query<User>();
            var query1 =
                    from user in userSet
                    where user.id > 1
                    orderby user.id
                    select user;

            var query = query1.Skip(1).Take(take);

            var userList = query.ToList();
            var rowCount = userList.Count();
            if (rowCount != take) throw new Exception($"query failed, expected row count : {take} , actual count: {rowCount} ");
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
            public int? motherId { get; set; }
        }

    }
}
