using FrameworkThree.Domain;
using FrameworkThree.Repository.Interface;
using System;

namespace FrameworkThree.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private FrameworkThreeContext _context;

        public UnitOfWork(FrameworkThreeContext context)
        {
            this._context = context;
        }

        public FrameworkThreeContext DbContext
        {
            get
            {
                return this._context;
            }
        }

        public int Save()
        {
            return this._context.SaveChanges();
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this._context != null)
                {
                    this._context.Dispose();
                    this._context = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
