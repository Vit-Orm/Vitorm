using System;

namespace Vitorm.Transaction
{
    public interface ITransactionManager : IDisposable
    {
        ITransaction BeginTransaction();
    }
}
