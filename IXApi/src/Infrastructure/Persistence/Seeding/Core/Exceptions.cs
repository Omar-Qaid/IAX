using System;
using System.Collections.Generic;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Core
{
    public abstract class SeederValidationException : Exception
    {
        protected SeederValidationException(string message) : base(message) { }
    }

    public class DuplicateSeederIdException : SeederValidationException
    {
        public string DuplicateId { get; }
        public IReadOnlyCollection<string> ConflictingTypes { get; }

        public DuplicateSeederIdException(string duplicateId, IEnumerable<string> conflictingTypes)
            : base($"Duplicate seeder ID detected: '{duplicateId}'. Registered by types: {string.Join(", ", conflictingTypes)}.")
        {
            DuplicateId = duplicateId;
            ConflictingTypes = new List<string>(conflictingTypes);
        }
    }

    public class MissingDependencyException : SeederValidationException
    {
        public string SeederId { get; }
        public string MissingDependencyId { get; }

        public MissingDependencyException(string seederId, string missingDependencyId)
            : base($"Seeder '{seederId}' declares dependency on '{missingDependencyId}', but no such seeder is registered in the dependency graph.")
        {
            SeederId = seederId;
            MissingDependencyId = missingDependencyId;
        }
    }

    public class CyclicDependencyException : SeederValidationException
    {
        public IReadOnlyList<string> CyclePath { get; }

        public CyclicDependencyException(IEnumerable<string> cyclePath)
            : base($"Dependency cycle detected in seeding graph: {string.Join(" -> ", cyclePath)}.")
        {
            CyclePath = new List<string>(cyclePath);
        }
    }

    public class ExcludedDependencyException : SeederValidationException
    {
        public string DependentId { get; }
        public string ExcludedDependencyId { get; }
        public SeederCategory ExcludedCategory { get; }

        public ExcludedDependencyException(string dependentId, string excludedDependencyId, SeederCategory excludedCategory)
            : base($"Seeder '{dependentId}' depends on '{excludedDependencyId}' (Category: {excludedCategory}), which is excluded by current active configuration.")
        {
            DependentId = dependentId;
            ExcludedDependencyId = excludedDependencyId;
            ExcludedCategory = excludedCategory;
        }
    }
}
