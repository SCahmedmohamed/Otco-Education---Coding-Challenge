using FluentAssertions;
using Moq;
using ProgramDesigner.Application.Services;
using ProgramDesigner.Domain.Contracts;
using ProgramDesigner.Domain.Entities;
using ProgramDesigner.Tests.Builders;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ProgramDesigner.Tests.ValidationServiceTests
{
    public class HierarchyAndReferenceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IGenericRepository<ProgramEntity>> _programRepoMock;
        private readonly ValidationService _sut;

        public HierarchyAndReferenceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _programRepoMock = new Mock<IGenericRepository<ProgramEntity>>();
            _unitOfWorkMock.Setup(u => u.GetRepository<ProgramEntity>()).Returns(_programRepoMock.Object);
            _sut = new ValidationService(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task ValidateProgramAsync_WhenProgramDoesNotExist_ShouldReturnNotFoundError()
        {
            // Arrange
            Guid missingProgramId = Guid.NewGuid();
            _programRepoMock.Setup(r => r.GetAsync(missingProgramId)).ReturnsAsync((ProgramEntity?)null!);

            // Act
            var result = await _sut.ValidateProgramAsync(missingProgramId);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle().Which.Should().Be("Program not found.");
        }

        [Fact]
        public async Task ValidateProgramAsync_WhenPrerequisiteIdDoesNotExistInTree_ShouldHandleGracefullyAndBeValid()
        {
            // Arrange
            Guid nonexistentPrereqId = Guid.NewGuid();
            var step = new StepBuilder()
                .WithName("Step With Unmapped Prerequisite")
                .WithPrerequisiteId(nonexistentPrereqId)
                .Build();

            var rootGroup = new GroupBuilder()
                .WithName("Root")
                .AsInOrder()
                .AddChild(step)
                .Build();

            var program = new ProgramBuilder()
                .WithName("Unmapped Prereq Program")
                .WithRootGroup(rootGroup)
                .Build();

            _programRepoMock.Setup(r => r.GetAsync(program.Id)).ReturnsAsync(program);

            // Act
            var result = await _sut.ValidateProgramAsync(program.Id);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
            result.Warnings.Should().BeEmpty();
        }

        [Fact]
        public async Task ValidateProgramAsync_ComplexRecursiveStructure_MixedGroups_ShouldValidateSuccessfully()
        {
            // Arrange
            var math101 = new StepBuilder().WithName("Math 101").Build();
            var math102 = new StepBuilder().WithName("Math 102").WithPrerequisiteId(math101.Id).Build();

            var mathGroup = new GroupBuilder()
                .WithName("Mathematics")
                .AsInOrder()
                .AddChild(math101)
                .AddChild(math102)
                .Build();

            var cs101 = new StepBuilder().WithName("CS 101").Build();
            var cs102 = new StepBuilder().WithName("CS 102").WithPrerequisiteId(cs101.Id).Build();

            var csGroup = new GroupBuilder()
                .WithName("Computer Science")
                .AsInOrder()
                .AddChild(cs101)
                .AddChild(cs102)
                .Build();

            var coreGroup = new GroupBuilder()
                .WithName("Core Subjects")
                .AsInOrder()
                .AddChild(mathGroup)
                .AddChild(csGroup)
                .Build();

            var elective1 = new StepBuilder().WithName("Art 101").Build();
            var elective2 = new StepBuilder().WithName("Music 101").Build();

            var electivesGroup = new GroupBuilder()
                .WithName("Electives")
                .AsChoice(pickCount: 1)
                .AddChild(elective1)
                .AddChild(elective2)
                .Build();

            var rootGroup = new GroupBuilder()
                .WithName("Degree Program")
                .AsInOrder()
                .AddChild(coreGroup)
                .AddChild(electivesGroup)
                .Build();

            var program = new ProgramBuilder()
                .WithName("Complex Degree")
                .WithRootGroup(rootGroup)
                .Build();

            _programRepoMock.Setup(r => r.GetAsync(program.Id)).ReturnsAsync(program);

            // Act
            var result = await _sut.ValidateProgramAsync(program.Id);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
            result.Warnings.Should().BeEmpty();
        }
    }
}
