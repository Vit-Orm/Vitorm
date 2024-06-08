namespace Vit.Orm.Entity
{
    public interface IEntityDescriptor
    {
        string tableName { get; }
        string keyName { get; }
        /// <summary>
        /// primary key
        /// </summary>
        public IColumnDescriptor key { get; }

        /// <summary>
        /// not include primary key
        /// </summary>
        public IColumnDescriptor[] columns { get; }

        public IColumnDescriptor[] allColumns { get; }
    }
}
