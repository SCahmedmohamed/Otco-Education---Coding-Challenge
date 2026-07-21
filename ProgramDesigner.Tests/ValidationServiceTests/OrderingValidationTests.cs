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
    public class OrderingValidationTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IGenericRepository<ProgramEntity>> _programRepoMock;
        private readonly ValidationService _sut;

        public OrderingValidationTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _programRepoMock = new Mock<IGenericRepository<ProgramEntity>>();
            _unitOfWorkMock.Setup(u => u.GetRepository<ProgramEntity>()).Returns(_programRepoMock.Object);
            _sut = new ValidationService(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task ValidateProgramAsync_InOrderGroup_WhenNodeDependsOnLaterSibling_ShouldFailValidation()
        {
            // Arrange
            Guid step1Id = Guid.NewGuid();
            Guid step2Id = Guid.NewGuid();

            var step1 = new StepBuilder()
                .WithId(step1Id)
                .WithName("Programming")
                .WithPrerequisiteId(step2Id)
                .Build();

            var step2 = new StepBuilder()
                .WithId(step2Id)
                .WithName("Algorithms")
                .WithPrerequisiteId(null)
                .Build();

            var rootGroup = new GroupBuilder()
                .WithName("Root")
                .AsInOrder()
                .AddChild(step1)
                .AddChild(step2)
                .Build();

            var program = new ProgramBuilder()
                .WithName("Ordered Program")
                .WithRootGroup(rootGroup)
                .Build();

            _programRepoMock.Setup(r => r.GetAsync(program.Id)).ReturnsAsync(program);

            // Act
            var result = await _sut.ValidateProgramAsync(program.Id);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Should().Be("Impossible prerequisite: 'Programming' depends on 'Algorithms' which comes after it in InOrder group 'Root'.");
        }

        [Fact]
        public async Task ValidateProgramAsync_InOrderGroup_WhenNodeDependsOnEarlierSibling_ShouldBeValid()
        {
            // Arrange
            Guid step1Id = Guid.NewGuid();
            Guid step2Id = Guid.NewGuid();

            var step1 = new StepBuilder()
                .WithId(step1Id)
                .WithName("Programming")
                .WithPrerequisiteId(null)
                .Build();

            var step2 = new StepBuilder()
                .WithId(step2Id)
                .WithName("Algorithms")
                .WithPrerequisiteId(step1Id)
                .Build();

            var rootGroup = new GroupBuilder()
                .WithName("Root")
                .AsInOrder()
                .AddChild(step1)
                .AddChild(step2)
                .Build();

            var program = new ProgramBuilder()
                .WithName("Ordered Program")
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
        public async Task ValidateProgramAsync_InOrderGroup_TransitiveDependencyOnLaterSibling_ShouldFailValidation()
        {
            // Arrange (Step 1 -> Step 2 -> Step 3, where Step 1 depends on Step 2, Step 2 depends on Step 3)
            Guid step1Id = Guid.NewGuid();
            Guid step2Id = Guid.NewGuid();
            Guid step3Id = Guid.NewGuid();

            var step1 = new StepBuilder().WithId(step1Id).WithName("Step 1").WithPrerequisiteId(step2Id).Build();
            var step2 = new StepBuilder().WithId(step2Id).WithName("Step 2").WithPrerequisiteId(step3Id).Build();
            var step3 = new StepBuilder().WithId(step3Id).WithName("Step 3").WithPrerequisiteId(null).Build();

            var rootGroup = new GroupBuilder()
                .WithName("Root")
                .AsInOrder()
                .AddChild(step1)
                .AddChild(step2)
                .AddChild(step3)
                .Build();

            var program = new ProgramBuilder()
                .WithName("Transitive InOrder Failure")
                .WithRootGroup(rootGroup)
                .Build();

            _programRepoMock.Setup(r => r.GetAsync(program.Id)).ReturnsAsync(program);

            // Act
            var result = await _sut.ValidateProgramAsync(program.Id);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.Contains("Impossible prerequisite"));
        }
    }
}
