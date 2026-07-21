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
    public class ValidationService(IUnitOfWork _unitOfWork) : IValidationSerivce
    {

        public async Task<ValidationResult> ValidateProgramAsync(Guid programId)
        {
            var result = new ValidationResult { IsValid = true };
            
            var repository = _unitOfWork.GetRepository<ProgramEntity>();
            var program = await repository.GetAsync(programId);
            
            if (program == null)
            {
                result.Errors.Add("Program not found.");
                result.IsValid = false;
                return result;
            }

            var allNodes = new Dictionary<Guid, ProgramNode>();
            FlattenTree(program.RootGroup, allNodes);

            ValidateNodes(allNodes, result);
            
            if (result.Errors.Any())
            {
                result.IsValid = false;
            }

            return result;
        }

        private void FlattenTree(ProgramNode node, Dictionary<Guid, ProgramNode> allNodes)
        {
            if (node == null) return;

            allNodes[node.Id] = node;

            if (node is Group group && group.Children != null)
            {
                foreach (var child in group.Children)
                {
                    FlattenTree(child, allNodes);
                }
            }
        }

        private void ValidateNodes(Dictionary<Guid, ProgramNode> allNodes, ValidationResult result)
        {
            // Cache to avoid recalculating the active set for a node
            var activeSetCache = new Dictionary<Guid, HashSet<Guid>>();

            foreach (var node in allNodes.Values)
            {
                // 1. Self prerequisite
                if (node.PrerequisiteId != Guid.Empty && node.PrerequisiteId == node.Id)
                {
                    result.Errors.Add($"Node '{node.Name}' cannot depend on itself.");
                }

                // 2. Circular dependency
                if (HasCircularDependency(node, allNodes))
                {
                    result.Errors.Add($"Circular dependency detected for node '{node.Name}'.");
                    continue; // Skip further checks for this node to avoid infinite loops
                }

                // 3. Impossible prerequisite (InOrder group)
                CheckInOrderPrerequisite(node, allNodes, result);

                // 4. Choice group validation & 5. Reachability warning
                CheckReachabilityAndChoiceRules(node, allNodes, result, activeSetCache);
            }
        }

        private bool HasCircularDependency(ProgramNode node, Dictionary<Guid, ProgramNode> allNodes)
        {
            var visited = new HashSet<Guid>();
            var currentId = node.PrerequisiteId;

            while (currentId.HasValue && currentId.Value != Guid.Empty && allNodes.TryGetValue(currentId.Value, out var current))
            {
                if (currentId.Value == node.Id) return true;
                if (!visited.Add(currentId.Value)) break; // Reached a cycle, but not necessarily involving 'node'
                currentId = current.PrerequisiteId;
            }

            return false;
        }

        private void CheckInOrderPrerequisite(ProgramNode node, Dictionary<Guid, ProgramNode> allNodes, ValidationResult result)
        {
            if (!node.ParentGroupId.HasValue || !allNodes.TryGetValue(node.ParentGroupId.Value, out var parentNode)) return;
            if (parentNode is not Group parentGroup || parentGroup.Rule != GroupRule.InOrder) return;

            var childrenList = parentGroup.Children.ToList();
            int nodeIndex = childrenList.FindIndex(c => c.Id == node.Id);

            var currId = node.PrerequisiteId;
            var seen = new HashSet<Guid>();

            while (currId.HasValue && currId.Value != Guid.Empty && allNodes.TryGetValue(currId.Value, out var reqNode))
            {
                if (!seen.Add(currId.Value)) break;

                // Check if any prerequisite in the chain is a sibling that comes AFTER this node
                if (reqNode.ParentGroupId == node.ParentGroupId)
                {
                    int reqIndex = childrenList.FindIndex(c => c.Id == reqNode.Id);
                    if (reqIndex > nodeIndex)
                    {
                        result.Errors.Add($"Impossible prerequisite: '{node.Name}' depends on '{reqNode.Name}' which comes after it in InOrder group '{parentGroup.Name}'.");
                        break;
                    }
                }
                currId = reqNode.PrerequisiteId;
            }
        }

        private void CheckReachabilityAndChoiceRules(ProgramNode node, Dictionary<Guid, ProgramNode> allNodes, ValidationResult result, Dictionary<Guid, HashSet<Guid>> cache)
        {
            var visiting = new HashSet<Guid>();
            var activeSet = GetActiveSet(node.Id, visiting, cache, allNodes);

            if (activeSet == null) return; // Cycle detected, already handled by Rule 2

            // Group the active set by ParentGroupId to see how many children of each Choice group are required
            var childrenByParent = activeSet
                .Where(id => allNodes.TryGetValue(id, out var n) && n.ParentGroupId.HasValue)
                .GroupBy(id => allNodes[id].ParentGroupId.Value);

            foreach (var groupSet in childrenByParent)
            {
                if (allNodes.TryGetValue(groupSet.Key, out var parentNode) && parentNode is Group parentGroup)
                {
                    if (parentGroup.Rule == GroupRule.Choice && parentGroup.PickCount.HasValue)
                    {
                        if (groupSet.Count() > parentGroup.PickCount.Value)
                        {
                            // Rule 4: If the contradiction is directly in the node's parent Choice group, it's an Error
                            if (groupSet.Key == node.ParentGroupId)
                            {
                                result.Errors.Add($"Choice group validation failed: '{node.Name}' execution requires {groupSet.Count()} items from its Choice group '{parentGroup.Name}', exceeding the PickCount of {parentGroup.PickCount.Value}.");
                            }
                            // Rule 5: If the contradiction comes from a Choice group higher in the hierarchy or in another branch, it's a Warning
                            else
                            {
                                // Prevent duplicate warnings for the same node and parent group
                                string warningMsg = $"Reachability warning: '{node.Name}' is unreachable because it requires {groupSet.Count()} nodes from Choice group '{parentGroup.Name}' (PickCount: {parentGroup.PickCount.Value}).";
                                if (!result.Warnings.Contains(warningMsg))
                                {
                                    result.Warnings.Add(warningMsg);
                                }
                            }
                        }
                    }
                }
            }
        }

        private HashSet<Guid> GetActiveSet(Guid nodeId, HashSet<Guid> visiting, Dictionary<Guid, HashSet<Guid>> cache, Dictionary<Guid, ProgramNode> allNodes)
        {
            if (cache.TryGetValue(nodeId, out var cached)) return cached;
            if (visiting.Contains(nodeId)) return null; // Cycle detected

            visiting.Add(nodeId);
            var activeSet = new HashSet<Guid> { nodeId };

            if (!allNodes.TryGetValue(nodeId, out var node))
            {
                visiting.Remove(nodeId);
                return activeSet;
            }

            // A node requires its ParentGroup to be active
            if (node.ParentGroupId.HasValue && node.ParentGroupId.Value != Guid.Empty && allNodes.ContainsKey(node.ParentGroupId.Value))
            {
                var parentSet = GetActiveSet(node.ParentGroupId.Value, visiting, cache, allNodes);
                if (parentSet == null) return null; // Unreachable due to structural cycle (shouldn't happen in tree, but handled safely)
                activeSet.UnionWith(parentSet);
            }

            // A node requires its Prerequisite to be active
            if (node.PrerequisiteId.HasValue && node.PrerequisiteId.Value != Guid.Empty && allNodes.ContainsKey(node.PrerequisiteId.Value))
            {
                var prereqSet = GetActiveSet(node.PrerequisiteId.Value, visiting, cache, allNodes);
                if (prereqSet == null) return null; // Unreachable due to prerequisite cycle
                activeSet.UnionWith(prereqSet);
            }

            visiting.Remove(nodeId);
            cache[nodeId] = activeSet;
            return activeSet;
        }
    }
}
