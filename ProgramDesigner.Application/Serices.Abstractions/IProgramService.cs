using ProgramDesigner.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramDesigner.Application.Serices.Abstractions
{
    public interface IProgramService
    {
        Task<ProgramDto> CreateProgramAsync(CreateProgramDto dto);
        Task<ProgramDto> GetProgramAsync(Guid id);
    }
}
