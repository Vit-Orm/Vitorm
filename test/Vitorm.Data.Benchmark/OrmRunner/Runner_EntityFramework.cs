using Microsoft.EntityFrameworkCore;

using Vit.Core.Util.ConfigurationManager;

using Vitorm;


namespace App.OrmRunner
{
    public partial class Runner_EntityFramework : IRunner
    {
        RunConfig config;

        int? skip => config.skip;
        int take => config.take;
        bool executeQuery => config.executeQuery;



        IQueryable<User> userQuery;
        public IQueryable<User> GetQueryable() => userQuery;


        public void Run(RunConfig config)
        {
            this.config = config;

            for (int i = 0; i < config.repeatCount; i++)
            {
                using MyDbContext myDbContext = new MyDbContext();
                userQuery = myDbContext.users;

                if (config.queryJoin) QueryJoin();
                else Query();
            }
        }

        #region Executor
        int exceptUserId = 1;
        public void QueryJoin()
        {
            var userSet = GetQueryable();

            var minId = 1;
            var config = new { maxId = 10000 };
            var offsetId = 100;

            var query =
                    from user in userSet
                    from father in userSet.Where(father => user.fatherId == father.id).DefaultIfEmpty()
                    from mother in userSet.Where(mother => user.motherId == mother.id).DefaultIfEmpty()
                    where user.id > minId && user.id < config.maxId && user.id != exceptUserId
                    orderby user.id
                    select new
                    {
                        user,
                        father,
                        mother,
                        testId = user.id + offsetId,
                        hasFather = father.name != null ? true : false
                    }
                    ;

            Execute(query);
        }

        public void Query()
        {
            var userSet = GetQueryable();

            var minId = 1;
            var config = new { maxId = 10000 };

            var query =
                    from user in userSet
                    where user.id > minId && user.id < config.maxId && user.id != exceptUserId
                    orderby user.id
                    select user;

            Execute(query);
        }
        #endregion

        public void Execute<Result>(IQueryable<Result> query)
        {
            if (skip > 0) query = query.Skip(skip.Value);
            query = query.Take(take);

            if (executeQuery)
            {
                var userList = query.ToList();
                var rowCount = userList.Count();
                if (rowCount != take) throw new Exception($"query failed, expected row count : {take} , actual count: {rowCount} ");
            }
            else
            {
                var sql = query.ToQueryString();
                if (string.IsNullOrEmpty(sql)) throw new Exception($"query failed, can not generated sql script");
            }
        }


        public class MyDbContext : Microsoft.EntityFrameworkCore.DbContext
        {
            public Microsoft.EntityFrameworkCore.DbSet<User> users { get; set; }

            static string connectionString = Appsettings.json.GetStringByPath("Vitorm.Data[0].connectionString");
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlite(connectionString);
            }
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
