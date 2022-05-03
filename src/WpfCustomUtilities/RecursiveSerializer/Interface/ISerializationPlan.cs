using System.Collections.Generic;

using WpfCustomUtilities.RecursiveSerializer.Planning;
using WpfCustomUtilities.RecursiveSerializer.Target;

namespace WpfCustomUtilities.RecursiveSerializer.Interface
{
    internal interface ISerializationPlan
    {
        /// <summary>
        /// Stores dictionary of unique references to reference-type objects
        /// </summary>
        public IEnumerable<SerializedObjectNode> ReferenceObjects { get; }

        /// <summary>
        /// Additional element DECLARING types built by the serialization planner
        /// </summary>
        Dictionary<int, ResolvedHashedType> ElementTypeDict { get; }

        /// <summary>
        /// Stores collection of unique property specifications
        /// </summary>
        IEnumerable<PropertySpecificationResolved> UniquePropertySpecifications { get; }

        /// <summary>
        /// Stores grouping of objects by property specification
        /// </summary>
        Dictionary<PropertySpecificationResolved, List<SerializedNodeBase>> PropertySpecificationGroups { get; }

        /// <summary>
        /// Root node for the object graph
        /// </summary>
        SerializedNodeBase RootNode { get; }
    }
}
