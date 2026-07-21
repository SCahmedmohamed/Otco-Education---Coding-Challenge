using ProgramDesigner.Application.DTOs;
using ProgramDesigner.Application.Serices.Abstractions;
using ProgramDesigner.Domain.Contracts;
using ProgramDesigner.Domain.Entities;
using ProgramDesigner.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProgramDesigner.Application.Services
{

    public class SimulationService(IUnitOfWork _unitOfWork) : ISimulationService
    {

        public async Task<SimulationResultDto> SimulateAsync(Guid programId, SimulationRequestDto request)
        {
            var program = await LoadProgramAsync(programId);

            if (program is null)
                return null!; // Controller maps null → 404, matching ProgramService convention

            if (program.RootGroup is null)
                return new SimulationResultDto(); // Empty program → empty result

            var allNodes = FlattenTree(program.RootGroup);

            ValidateCompletedItems(request.CompletedItems, allNodes);
            ValidateSelectedChoices(request.SelectedChoices, allNodes);

            var completedSet = new HashSet<Guid>(request.CompletedItems);
            var skippedSet   = BuildSkippedSet(allNodes, request.SelectedChoices);

            var nodes = allNodes.Values
                .Select(node => EvaluateNode(node, completedSet, skippedSet, allNodes))
                .ToList();

            return new SimulationResultDto { Nodes = nodes };
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Step 1 — Load
        // ─────────────────────────────────────────────────────────────────────────

        private async Task<ProgramEntity?> LoadProgramAsync(Guid programId)
        {
            var repository = _unitOfWork.GetRepository<ProgramEntity>();
            return await repository.GetAsync(programId);
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Step 2 — Flatten the recursive tree into a flat lookup dictionary
        // ─────────────────────────────────────────────────────────────────────────


        private static Dictionary<Guid, ProgramNode> FlattenTree(ProgramNode root)
        {
            var result = new Dictionary<Guid, ProgramNode>();
            FlattenNode(root, result);
            return result;
        }

        private static void FlattenNode(ProgramNode node, Dictionary<Guid, ProgramNode> result)
        {
            if (node is null) return;

            result[node.Id] = node;

            if (node is Group group)
            {
                foreach (var child in group.Children)
                    FlattenNode(child, result);
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Step 3 — Input validation (fail fast with clear messages)
        // ─────────────────────────────────────────────────────────────────────────

        private static void ValidateCompletedItems(
            IEnumerable<Guid> completedItems,
            Dictionary<Guid, ProgramNode> allNodes)
        {
            foreach (var id in completedItems)
            {
                if (!allNodes.ContainsKey(id))
                    throw new ArgumentException(
                        $"Completed item '{id}' does not belong to this program.");
            }
        }

        private static void ValidateSelectedChoices(
            Dictionary<Guid, List<Guid>> selectedChoices,
            Dictionary<Guid, ProgramNode> allNodes)
        {
            foreach (var (groupId, selectedChildIds) in selectedChoices)
            {
                if (!allNodes.TryGetValue(groupId, out var node))
                    throw new ArgumentException(
                        $"Selected choice group '{groupId}' does not belong to this program.");

                if (node is not Group group || group.Rule != GroupRule.Choice)
                    throw new ArgumentException(
                        $"Node '{node.Name}' ('{groupId}') is not a Choice group.");

                foreach (var childId in selectedChildIds)
                {
                    if (group.Children.All(c => c.Id != childId))
                        throw new ArgumentException(
                            $"Selected child '{childId}' is not a direct child of " +
                            $"Choice group '{group.Name}'.");
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Step 4 — Pre-pass: build the set of all skipped node IDs
        // ─────────────────────────────────────────────────────────────────────────


        private static HashSet<Guid> BuildSkippedSet(
            Dictionary<Guid, ProgramNode> allNodes,
            Dictionary<Guid, List<Guid>> selectedChoices)
        {
            var skipped = new HashSet<Guid>();

            foreach (var node in allNodes.Values)
            {
                if (node is not Group group || group.Rule != GroupRule.Choice)
                    continue;

                // If the group has no entry at all → treat all children as unresolved (Skipped)
                var selectedChildIds = selectedChoices.TryGetValue(group.Id, out var selected)
                    ? new HashSet<Guid>(selected)
                    : new HashSet<Guid>();

                foreach (var child in group.Children)
                {
                    if (!selectedChildIds.Contains(child.Id))
                        MarkSubtreeAsSkipped(child, skipped);
                }
            }

            return skipped;
        }

        private static void MarkSubtreeAsSkipped(ProgramNode node, HashSet<Guid> skipped)
        {
            if (node is null) return;

            skipped.Add(node.Id);

            if (node is Group group)
            {
                foreach (var child in group.Children)
                    MarkSubtreeAsSkipped(child, skipped);
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Steps 5–7 — Evaluate each node
        // ─────────────────────────────────────────────────────────────────────────

        private static NodeSimulationDto EvaluateNode(
            ProgramNode node,
            HashSet<Guid> completedSet,
            HashSet<Guid> skippedSet,
            Dictionary<Guid, ProgramNode> allNodes)
        {
            var (status, reason) = DetermineStatus(node, completedSet, skippedSet, allNodes);

            return new NodeSimulationDto
            {
                Id       = node.Id,
                Name     = node.Name,
                NodeType = node is Group ? "Group" : "Step",
                Status   = status,
                Reason   = reason
            };
        }


        private static (NodeStatus Status, string? Reason) DetermineStatus(
            ProgramNode node,
            HashSet<Guid> completedSet,
            HashSet<Guid> skippedSet,
            Dictionary<Guid, ProgramNode> allNodes)
        {
            if (skippedSet.Contains(node.Id))
                return (NodeStatus.Skipped, "This branch was not selected.");

            if (completedSet.Contains(node.Id))
                return (NodeStatus.Completed, null);

            return EvaluatePrerequisite(node, completedSet, allNodes);
        }


        private static (NodeStatus Status, string? Reason) EvaluatePrerequisite(
            ProgramNode node,
            HashSet<Guid> completedSet,
            Dictionary<Guid, ProgramNode> allNodes)
        {
            if (!node.PrerequisiteId.HasValue || node.PrerequisiteId.Value == Guid.Empty)
                return (NodeStatus.Unlocked, null);

            if (!allNodes.TryGetValue(node.PrerequisiteId.Value, out var prerequisite))
                return (NodeStatus.Unlocked, null); // Prerequisite outside this program → satisfied

            if (completedSet.Contains(prerequisite.Id))
                return (NodeStatus.Unlocked, null);

            return (NodeStatus.Blocked, $"Prerequisite '{prerequisite.Name}' is not completed.");
        }
    }
}
