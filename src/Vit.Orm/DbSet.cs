using System.Collections.Generic;
using System.Linq;

using Vit.Orm.Entity;

namespace Vit.Orm
{
    public interface IDbSet
    {
        IEntityDescriptor entityDescriptor { get; }
    }

    public abstract class DbSet<Entity> : IDbSet
    {
        public abstract IEntityDescriptor entityDescriptor { get; }


        public abstract void Create();


        public abstract Entity Add(Entity entity);
        public abstract void AddRange(IEnumerable<Entity> entitys);


        public abstract Entity Get(object keyValue);
        public abstract IQueryable<Entity> Query();



        public abstract int Update(Entity entity);
        public abstract int UpdateRange(IEnumerable<Entity> entitys);


        public abstract int Delete(Entity entity);
        public abstract int DeleteRange(IEnumerable<Entity> entitys);

        public abstract int DeleteByKey(object keyValue);
        public abstract int DeleteByKeys<Key>(IEnumerable<Key> keys);



    }
}
