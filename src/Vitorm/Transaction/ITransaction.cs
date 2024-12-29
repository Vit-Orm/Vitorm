using System;

namespace Vitorm.Transaction
{
    public interface ITransaction : IDisposable
    {
        void Commit();
        void Rollback();
    }
}
