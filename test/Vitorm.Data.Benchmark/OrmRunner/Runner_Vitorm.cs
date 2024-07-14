using App.OrmRunner.VitormRunner;

using Vitorm;

namespace App.OrmRunner
{
    public partial class Runner_Vitorm : IRunner
    {
        public void Run(RunConfig config)
        {
            for (int i = 0; i < config.repeatCount; i++)
            {
                var userQuery = Data.Query<User>();
                if (config.queryJoin) QueryExecute.QueryJoin(userQuery, config);
                else QueryExecute.Query(userQuery, config);
            }
        }
    }
}


namespace App.OrmRunner.VitormRunner
{
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


    public class QueryExecute
    {
        public static void QueryJoin(IQueryable<User> userSet, RunConfig config)
        {
            var query =
                    from user in userSet
                    from father in userSet.Where(father => user.fatherId == father.id).DefaultIfEmpty()
                    from mother in userSet.Where(mother => user.motherId == mother.id).DefaultIfEmpty()
                    where user.id > 1 && user.id < 10000
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

            Execute(query, config);
        }

        public static void Query(IQueryable<User> userSet, RunConfig config)
        {
            var query =
                    from user in userSet
                    where user.id > 1 && user.id < 10000
                    orderby user.id
                    select user;

            Execute(query, config);
        }

        public static void Execute<Result>(IQueryable<Result> query, RunConfig config)
        {
            if (config.skip > 0) query = query.Skip(config.skip.Value);
            query = query.Take(config.take);

            if (config.executeQuery)
            {
                var userList = query.ToList();
                var rowCount = userList.Count();
                if (rowCount != config.take) throw new Exception($"query failed, expected row count : {config.take} , actual count: {rowCount} ");
            }
            else
            {
                var sql = query.ToExecuteString();
                if (string.IsNullOrEmpty(sql)) throw new Exception($"query failed, can not generated sql script");
            }
        }

    }
}

