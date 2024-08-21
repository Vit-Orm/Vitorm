namespace Vitorm.Sql.SqlTranslate
{
    public enum EAddType
    {
        /// <summary>
        /// no key column
        /// </summary>
        noKeyColumn,
        /// <summary>
        /// keyValue is not empty
        /// </summary>
        keyWithValue,
        /// <summary>
        /// not Identity and keyValue is empty
        /// </summary>
        unexpectedEmptyKey,
        /// <summary>
        /// Identity and keyValue is empty
        /// </summary>
        identityKey,

        unexpectedType,
    }
}
