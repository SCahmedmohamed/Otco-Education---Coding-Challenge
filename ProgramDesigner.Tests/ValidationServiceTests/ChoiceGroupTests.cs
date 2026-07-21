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
    public class ChoiceGroupTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IGenericRepository<ProgramEntity>> _programRepoMock;
        private readonly ValidationService _sut;

        public ChoiceGroupTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _programRepoMock = new Mock<IGenericRepository<ProgramEntity>>();
            _unitOfWorkMock.Setup(u => u.GetRepository<ProgramEntity>()).Returns(_programRepoMock.Object);
            _sut = new ValidationService(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task ValidateProgramAsync_ChoiceGroup_WithIndependentChildren_ShouldBeValid()
        {
            // Arrange (Choice PickCount 1, 3 independent courses)
            var physics = new StepBuilder().WithName("Physics").WithPrerequisiteId(null).Build();
            var chemistry = new StepBuilder().WithName("Chemistry").WithPrerequisiteId(null).Build();
            var biology = new StepBuilder().WithName("Biology").WithPrerequisiteId(null).Build();

            var rootGroup = new GroupBuilder()
                .WithName("Electives")
                .AsChoice(pickCount: 1)
                .AddChild(physics)
                .AddChild(chemistry)
                .AddChild(biology)
                .Build();

            var program = new ProgramBuilder()
                .WithName("Choose One")
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
        public async Task ValidateProgramAsync_ChoiceGroup_WhenChildDependsOnSiblingInSameChoiceGroup_ShouldFailValidation()
        {
            // Arrange (Choice PickCount 1, Physics depends on Chemistry in same Choice group)
            Guid chemistryId = Guid.NewGuid();

            var physics = new StepBuilder()
                .WithName("Physics")
                .WithPrerequisiteId(chemistryId)
                .Build();

            var chemistry = new StepBuilder()
                .WithId(chemistryId)
                .WithName("Chemistry")
                .WithPrerequisiteId(null)
                .Build();

            var rootGroup = new GroupBuilder()
                .WithName("Electives")
                .AsChoice(pickCount: 1)
                .AddChild(physics)
                .AddChild(chemistry)
                .Build();

            var program = new ProgramBuilder()
                .WithName("Choose One Violation")
                .WithRootGroup(rootGroup)
                .Build();

            _programRepoMock.Setup(r => r.GetAsync(program.Id)).ReturnsAsync(program);

            // Act
            var result = await _sut.ValidateProgramAsync(program.Id);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Should().Be("Choice group validation failed: 'Physics' execution requires 2 items from its Choice group 'Electives', exceeding the PickCount of 1.");
        }

        [Fact]
        public async Task ValidateProgramAsync_ChoiceGroup_PickCount2_WhenChildRequires2Items_ShouldBeValid()
        {
            // Arrange (Choice PickCount 2, Physics depends on Chemistry)
            Guid chemistryId = Guid.NewGuid();

            var physics = new StepBuilder()
                .WithName("Physics")
                .WithPrerequisiteId(chemistryId)
                .Build();

            var chemistry = new StepBuilder()
                .WithId(chemistryId)
                .WithName("Chemistry")
                .WithPrerequisiteId(null)
                .Build();

            var rootGroup = new GroupBuilder()
                .WithName("Electives")
                .AsChoice(pickCount: 2)
                .AddChild(physics)
                .AddChild(chemistry)
                .Build();

            var program = new ProgramBuilder()
                .WithName("Choose Two Valid")
                .WithRootGroup(rootGroup)
                .Build();

            _programRepoMock.Setup(r => r.GetAsync(program.Id)).ReturnsAsync(program);

            // Act
            var result = await _sut.ValidateProgramAsync(program.Id);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public async Task ValidateProgramAsync_ChoiceGroup_PickCount2_WhenChildRequires3Items_ShouldFailValidation()
        {
            // Arrange (Choice PickCount 2, Physics depends on Chemistry, Chemistry depends on Biology)
            Guid chemistryId = Guid.NewGuid();
            Guid biologyId = Guid.NewGuid();

            var physics = new StepBuilder().WithName("Physics").WithPrerequisiteId(chemistryId).Build();
            var chemistry = new StepBuilder().WithId(chemistryId).WithName("Chemistry").WithPrerequisiteId(biologyId).Build();
            var biology = new StepBuilder().WithId(biologyId).WithName("Biology").WithPrerequisiteId(null).Build();

            var rootGroup = new GroupBuilder()
                .WithName("Electives")
                .AsChoice(pickCount: 2)
                .AddChild(physics)
                .AddChild(chemistry)
                .AddChild(biology)
                .Build();

            var program = new ProgramBuilder()
                .WithName("Choose Two Invalid")
                .WithRootGroup(rootGroup)
                .Build();

            _programRepoMock.Setup(r => r.GetAsync(program.Id)).ReturnsAsync(program);

            // Act
            var result = await _sut.ValidateProgramAsync(program.Id);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Should().Be("Choice group validation failed: 'Physics' execution requires 3 items from its Choice group 'Electives', exceeding the PickCount of 2.");
        }
    }
}
