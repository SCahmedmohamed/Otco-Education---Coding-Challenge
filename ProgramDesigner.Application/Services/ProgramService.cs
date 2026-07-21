using AutoMapper;
using ProgramDesigner.Application.DTOs;
using ProgramDesigner.Application.Serices.Abstractions;
using ProgramDesigner.Domain.Contracts;
using ProgramDesigner.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace ProgramDesigner.Application.Services
{
    public class ProgramService(IUnitOfWork _unitOfWork , IMapper _mapper) : IProgramService
    {

        public async Task<ProgramDto> CreateProgramAsync(CreateProgramDto dto)
        {
            var program = _mapper.Map<ProgramEntity>(dto);
            
            // Detach root group temporarily to break EF Core circular dependency during insert
            var rootGroup = program.RootGroup;
            program.RootGroup = null;
            program.RootGroupId = null;

            var repository = _unitOfWork.GetRepository<ProgramEntity>();
            await repository.AddAsync(program);
            
            // Save just the ProgramEntity first
            await _unitOfWork.SaveChangesAsync();
            
            if (rootGroup != null)
            {
                FixTree(rootGroup, program.Id, null);
                
                // Re-attach the root group and update the ProgramEntity
                program.RootGroup = rootGroup;
                program.RootGroupId = rootGroup.Id;
                
                // Save the Group hierarchy and the update to ProgramEntity
                await _unitOfWork.SaveChangesAsync();
            }
            
            return _mapper.Map<ProgramDto>(program);
        }

        public async Task<ProgramDto> GetProgramAsync(Guid id)
        {
            var repository = _unitOfWork.GetRepository<ProgramEntity>();
            
            // GetAsync should be configured in the Infrastructure layer to include the 
            // entire Program hierarchy recursively.
            var program = await repository.GetAsync(id);
            
            if (program == null)
            {
                return null;
            }

            return _mapper.Map<ProgramDto>(program);
        }

        private void FixTree(ProgramNode node, Guid programId, Guid? parentGroupId)
        {
            if (node == null) return;
            
            node.ProgramId = programId;
            node.ParentGroupId = parentGroupId;
            
            if (node is Group group && group.Children != null)
            {
                foreach (var child in group.Children)
                {
                    FixTree(child, programId, group.Id);
                }
            }
        }
    }
}
