using System;
using System.Collections.Generic;
using System.Linq;
using Vit.Orm.Entity;
using System.Reflection;
using Vit.Orm.Sql.SqlTranslate;

namespace Vit.Orm.Sql
{
    public class SqlDbSetConstructor
    {
        public static IDbSet CreateDbSet( SqlDbContext dbContext, Type entityType, IEntityDescriptor entityDescriptor)
        {
            return _CreateDbSet.MakeGenericMethod(entityType)
                     .Invoke(null, new object[] { dbContext, entityDescriptor }) as IDbSet;
        }
       
        static MethodInfo _CreateDbSet = new Func<SqlDbContext, IEntityDescriptor,IDbSet>(CreateDbSet<object>)
                   .Method.GetGenericMethodDefinition();
        public static IDbSet CreateDbSet<Entity>(SqlDbContext dbContext, IEntityDescriptor entityDescriptor)
        {
            return new SqlDbSet<Entity>(dbContext, entityDescriptor);
        }

    }

    public class SqlDbSet<Entity> : Vit.Orm.DbSet<Entity>
    {
        protected SqlDbContext dbContext;

        protected IEntityDescriptor _entityDescriptor;
        public override IEntityDescriptor entityDescriptor => _entityDescriptor;


        public virtual ISqlTranslateService sqlTranslator => dbContext.sqlTranslateService;

        public SqlDbSet(SqlDbContext dbContext, IEntityDescriptor entityDescriptor)
        {
            this.dbContext = dbContext;
            this._entityDescriptor = entityDescriptor;
        }

        public override void Create() => dbContext.Create<Entity>();



        public override Entity Add(Entity entity) => dbContext.Add(entity);
        public override void AddRange(IEnumerable<Entity> entitys) => dbContext.AddRange(entitys);




        public override Entity Get(object keyValue) => dbContext.Get<Entity>(keyValue);
        public override IQueryable<Entity> Query() => dbContext.Query<Entity>();



        public override int Update(Entity entity) => dbContext.Update<Entity>(entity);
        public override int UpdateRange(IEnumerable<Entity> entitys) => dbContext.UpdateRange<Entity>(entitys);



        public override int Delete(Entity entity) => dbContext.Delete<Entity>(entity);
        public override int DeleteRange(IEnumerable<Entity> entitys) => dbContext.DeleteRange<Entity>(entitys);
        public override int DeleteByKey(object keyValue) => dbContext.DeleteByKey<Entity>(keyValue);
        public override int DeleteByKeys<Key>(IEnumerable<Key> keys) => dbContext.DeleteByKeys<Entity, Key>(keys);

    }
}
