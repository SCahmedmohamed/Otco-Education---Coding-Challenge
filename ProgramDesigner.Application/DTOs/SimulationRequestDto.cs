using System;
using System.Collections.Generic;

namespace ProgramDesigner.Application.DTOs
{

    public class SimulationRequestDto
    {

        public List<Guid> CompletedItems { get; set; } = new List<Guid>();


        public Dictionary<Guid, List<Guid>> SelectedChoices { get; set; } = new Dictionary<Guid, List<Guid>>();
    }
}
