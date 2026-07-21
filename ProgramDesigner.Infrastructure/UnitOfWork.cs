using Hospital.Infrastructure.Repositories;
using ProgramDesigner.Domain.Contracts;
using ProgramDesigner.Domain.Entities;
using ProgramDesigner.Infrastructure.DbContexts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hospital.Infrastructure
{
    public class UnitOfWork(ApplicationDbContext _context) : IUnitOfWork
    {
        private ConcurrentDictionary<string, object> _repositories = new ConcurrentDictionary<string, object>();

        public IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseEntity
        {
            return (IGenericRepository<TEntity>)_repositories.GetOrAdd(typeof(TEntity).Name, (type) => new GenericRepository<TEntity>(_context));

        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
