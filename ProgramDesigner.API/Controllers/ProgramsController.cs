using Hospital.Domain.Exceptions.BadRequest;
using Hospital.Domain.Exceptions.NotFound;
using Microsoft.AspNetCore.Mvc;
using ProgramDesigner.Application.DTOs;
using ProgramDesigner.Application.Serices.Abstractions;
using System;
using System.Threading.Tasks;

namespace ProgramDesigner.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ProgramsController : ControllerBase
    {
        private readonly ISerivceManager _serviceManager;

        public ProgramsController(ISerivceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }


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


        [HttpPost("{id}/validate")]
        public async Task<IActionResult> ValidateProgram(Guid id)
        {
            var validationResult = await _serviceManager.ValdationService.ValidateProgramAsync(id);

            if (!validationResult.IsValid && validationResult.Errors.Contains("Program not found."))
            {
                return NotFound();
            }

            return Ok(validationResult);
        }


        [HttpPost("{id}/simulate")]
        public async Task<IActionResult> SimulateProgram(Guid id, [FromBody] SimulationRequestDto request)
        {
            if (request is null)
                throw new BadRequestException("Simulation request cannot be null.");

            try
            {
                var result = await _serviceManager.SimulationService.SimulateAsync(id, request);

                if (result is null)
                    throw new NotFoundException($"Program with ID {id} not found.");

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                throw new BadRequestException(ex.Message);
            }
        }
    }
}
