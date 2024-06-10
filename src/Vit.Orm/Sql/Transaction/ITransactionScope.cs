using System;
using System.Data;

namespace Vit.Orm.Sql.Transaction
{
    public interface ITransactionScope : IDisposable
    {
        IDbTransaction BeginTransaction();
        IDbTransaction GetCurrentTransaction();
    }
}
