using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramDesigner.Application.Serices.Abstractions
{
    public interface ISerivceManager
    {
        IProgramService ProgramService { get; }
        IValidationSerivce ValdationService { get; }
        ISimulationService SimulationService { get; }

    }
}
