using Microsoft.AspNetCore.Mvc;
using ProgramDesigner.Application.DTOs;
using ProgramDesigner.Application.Serices.Abstractions;
using System;
using System.Threading.Tasks;

namespace ProgramDesigner.API.Controllers
{
    /// <summary>
    /// Controller for managing and validating Programs.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProgramsController : ControllerBase
    {
        private readonly ISerivceManager _serviceManager;

        public ProgramsController(ISerivceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        /// <summary>
        /// Creates a new Program tree.
        /// </summary>
        /// <param name="dto">The data transfer object containing the program details.</param>
        /// <returns>The created Program data with a location header.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateProgram([FromBody] CreateProgramDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Program data cannot be null.");
            }

            var createdProgram = await _serviceManager.ProgramService.CreateProgramAsync(dto);
            
            return CreatedAtAction(nameof(GetProgram), new { id = createdProgram.Id }, createdProgram);
        }

        /// <summary>
        /// Retrieves a Program hierarchy by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the Program.</param>
        /// <returns>The entire Program tree if found; otherwise, 404 Not Found.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProgram(Guid id)
        {
            var program = await _serviceManager.ProgramService.GetProgramAsync(id);
            
            if (program == null)
            {
                return NotFound();
            }

            return Ok(program);
        }

        /// <summary>
        /// Validates a Program against the specified business rules.
        /// </summary>
        /// <param name="id">The unique identifier of the Program to validate.</param>
        /// <returns>The validation result, or 404 Not Found if the program does not exist.</returns>
        [HttpPost("{id}/validate")]
        public async Task<IActionResult> ValidateProgram(Guid id)
        {
            var validationResult = await _serviceManager.ValdationService.ValidateProgramAsync(id);

            // Our validation service explicitly returns "Program not found." error if it's missing
            if (!validationResult.IsValid && validationResult.Errors.Contains("Program not found."))
            {
                return NotFound();
            }

            return Ok(validationResult);
        }
    }
}
