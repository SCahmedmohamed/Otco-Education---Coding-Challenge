using FluentAssertions;
using Moq;
using ProgramDesigner.Application.Services;
using ProgramDesigner.Domain.Contracts;
using ProgramDesigner.Domain.Entities;
using ProgramDesigner.Domain.Enums;
using ProgramDesigner.Tests.Builders;
using ProgramDesigner.Tests.Factories;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ProgramDesigner.Tests.ValidationServiceTests
{
    public class ValidProgramTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IGenericRepository<ProgramEntity>> _programRepoMock;
        private readonly ValidationService _sut;

        public ValidProgramTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _programRepoMock = new Mock<IGenericRepository<ProgramEntity>>();
            _unitOfWorkMock.Setup(u => u.GetRepository<ProgramEntity>()).Returns(_programRepoMock.Object);
            _sut = new ValidationService(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task OctoChallenge_ComputerScienceScenario_ValidatesSuccessfully()
        {
            // Arrange
            var program = TestDataFactory.CreateComputerScienceProgram();
            _programRepoMock.Setup(r => r.GetAsync(program.Id)).ReturnsAsync(program);

            // Act
            var result = await _sut.ValidateProgramAsync(program.Id);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
            result.Warnings.Should().BeEmpty();
        }

        [Fact]
        public async Task ValidateProgramAsync_WithEmptyRootGroup_ShouldBeValid()
        {
            // Arrange
            var emptyRootGroup = new GroupBuilder()
                .WithName("Empty Root")
                .AsInOrder()
                .Build();

            var program = new ProgramBuilder()
                .WithName("Empty Program")
                .WithRootGroup(emptyRootGroup)
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
        public async Task ValidateProgramAsync_WithSingleStepInRootGroup_ShouldBeValid()
        {
            // Arrange
            var step = new StepBuilder()
                .WithName("Single Step")
                .WithPrerequisiteId(null)
                .Build();

            var rootGroup = new GroupBuilder()
                .WithName("Root Group")
                .AsInOrder()
                .AddChild(step)
                .Build();

            var program = new ProgramBuilder()
                .WithName("Single Step Program")
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
        public async Task ValidateProgramAsync_WithNestedGroups_ShouldBeValid()
        {
            // Arrange
            var step1 = new StepBuilder().WithName("Intro Step").Build();
            var step2 = new StepBuilder().WithName("Advanced Step").WithPrerequisiteId(step1.Id).Build();

            var subGroup = new GroupBuilder()
                .WithName("Sub Group")
                .AsInOrder()
                .AddChild(step1)
                .AddChild(step2)
                .Build();

            var rootGroup = new GroupBuilder()
                .WithName("Root Group")
                .AsInOrder()
                .AddChild(subGroup)
                .Build();

            var program = new ProgramBuilder()
                .WithName("Nested Groups Program")
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
        public async Task ValidateProgramAsync_WithDeepNestedGroups_ShouldBeValid()
        {
            // Arrange
            var leafStep = new StepBuilder().WithName("Leaf Step").Build();

            var level3Group = new GroupBuilder().WithName("Level 3").AsInOrder().AddChild(leafStep).Build();
            var level2Group = new GroupBuilder().WithName("Level 2").AsInOrder().AddChild(level3Group).Build();
            var level1Group = new GroupBuilder().WithName("Level 1").AsInOrder().AddChild(level2Group).Build();
            var rootGroup = new GroupBuilder().WithName("Root").AsInOrder().AddChild(level1Group).Build();

            var program = new ProgramBuilder()
                .WithName("Deep Nested Program")
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
        public async Task ValidateProgramAsync_WithMultipleBranches_ShouldBeValid()
        {
            // Arrange
            var branch1Step = new StepBuilder().WithName("Branch 1 Step").Build();
            var branch2Step = new StepBuilder().WithName("Branch 2 Step").Build();

            var branch1Group = new GroupBuilder().WithName("Branch 1").AsInOrder().AddChild(branch1Step).Build();
            var branch2Group = new GroupBuilder().WithName("Branch 2").AsInOrder().AddChild(branch2Step).Build();

            var rootGroup = new GroupBuilder()
                .WithName("Root")
                .AsInOrder()
                .AddChild(branch1Group)
                .AddChild(branch2Group)
                .Build();

            var program = new ProgramBuilder()
                .WithName("Multi Branch Program")
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
        public async Task ValidateProgramAsync_WithLargeHierarchy_ShouldBeValid()
        {
            // Arrange
            var rootGroupBuilder = new GroupBuilder().WithName("Large Root").AsInOrder();
            ProgramNode? previousNode = null;

            for (int i = 0; i < 50; i++)
            {
                var stepBuilder = new StepBuilder().WithName($"Step {i}");
                if (previousNode != null)
                {
                    stepBuilder.WithPrerequisiteId(previousNode.Id);
                }
                var step = stepBuilder.Build();
                rootGroupBuilder.AddChild(step);
                previousNode = step;
            }

            var program = new ProgramBuilder()
                .WithName("Large Program")
                .WithRootGroup(rootGroupBuilder.Build())
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
