using ProgramDesigner.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramDesigner.Domain.Contracts
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<T> GetAsync(Guid id) ;
        Task AddAsync(T entity) ;
    }
}
