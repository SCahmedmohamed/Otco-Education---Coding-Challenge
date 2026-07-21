using Microsoft.EntityFrameworkCore;
using ProgramDesigner.Domain.Contracts;
using ProgramDesigner.Domain.Entities;
using ProgramDesigner.Infrastructure.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hospital.Infrastructure.Repositories
{
    public class GenericRepository<TEntity>(ApplicationDbContext _context) : IGenericRepository<TEntity> where TEntity : BaseEntity
    {
        public async Task AddAsync(TEntity entity)
        {
           await _context.Set<TEntity>().AddAsync(entity);
        }


        public async Task<TEntity> GetAsync(Guid id)
        {
            if (typeof(TEntity) == typeof(ProgramEntity))
            {
                await _context.Nodes.Where(n => n.ProgramId == id).ToListAsync();
                var program = await _context.Programs
                    .Include(p => p.RootGroup)
                    .FirstOrDefaultAsync(p => p.Id == id);
                return program as TEntity;
            }
            return await _context.Set<TEntity>().FindAsync(id);
        }

    }
}
