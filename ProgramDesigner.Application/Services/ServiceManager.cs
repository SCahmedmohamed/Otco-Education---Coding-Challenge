using AutoMapper;
using ProgramDesigner.Application.Serices.Abstractions;
using ProgramDesigner.Domain.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramDesigner.Application.Services
{
    public class ServiceManager(
        IUnitOfWork _unitOfWork,
        IMapper _mapper
        ) : ISerivceManager
    {
        public IProgramService ProgramService { get;  } = new ProgramService(_unitOfWork,_mapper);
        public IValidationSerivce ValdationService { get; } = new ValidationService(_unitOfWork);
        public ISimulationService SimulationService { get; } = new SimulationService(_unitOfWork);

    }
}
 