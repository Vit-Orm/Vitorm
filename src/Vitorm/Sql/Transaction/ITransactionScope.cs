using System;
using System.Data;

namespace Vitorm.Sql.Transaction
{
    public interface ITransactionScope : IDisposable
    {
        IDbTransaction BeginTransaction();
        IDbTransaction GetCurrentTransaction();
    }
}
