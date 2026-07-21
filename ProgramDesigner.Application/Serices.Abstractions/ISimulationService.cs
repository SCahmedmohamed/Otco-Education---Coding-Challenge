using ProgramDesigner.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramDesigner.Application.Serices.Abstractions
{
    /// <summary>
    /// Defines the contract for simulating a participant's progress through a Program tree.
    /// </summary>
    public interface ISimulationService
    {
        /// <summary>
        /// Simulates the state of every node in the specified program for a participant
        /// given their completed items and Choice Group selections.
        /// </summary>
        /// <param name="programId">The ID of the program to simulate.</param>
        /// <param name="request">The participant's completed items and Choice Group selections.</param>
        /// <returns>
        /// A <see cref="SimulationResultDto"/> with one entry per node, or null if the program does not exist.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="request"/> references node IDs or group IDs that do not belong to the program.
        /// </exception>
        Task<SimulationResultDto> SimulateAsync(Guid programId, SimulationRequestDto request);
    }
}
