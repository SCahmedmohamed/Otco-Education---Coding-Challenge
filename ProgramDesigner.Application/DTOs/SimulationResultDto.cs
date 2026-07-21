using System.Collections.Generic;

namespace ProgramDesigner.Application.DTOs
{
    public class SimulationResultDto
    {

        public List<NodeSimulationDto> Nodes { get; set; } = new List<NodeSimulationDto>();
    }
}
