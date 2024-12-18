using System;

using Vitorm.Transaction;

namespace Vitorm
{
    public partial class DbContext : IDbContext, IDisposable
    {
        #region Transaction

        public virtual ITransaction BeginTransaction()
        {
            throw new NotImplementedException();
        }

        #endregion
    }


}
