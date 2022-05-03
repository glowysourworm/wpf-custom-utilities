using System.Collections.Generic;

using WpfCustomUtilities.RecursiveSerializer.Interface;
using WpfCustomUtilities.RecursiveSerializer.Target;

namespace WpfCustomUtilities.RecursiveSerializer.Planning
{
    internal class SerializationPlan : ISerializationPlan
    {
        public SerializedNodeBase RootNode { get; private set; }

        public IEnumerable<SerializedObjectNode> ReferenceObjects { get; private set; }

        public Dictionary<int, ResolvedHashedType> ElementTypeDict { get; private set; }

        public IEnumerable<PropertySpecificationResolved> UniquePropertySpecifications { get; private set; }

        public Dictionary<PropertySpecificationResolved, List<SerializedNodeBase>> PropertySpecificationGroups { get; private set; }

        public SerializationPlan(IEnumerable<SerializedObjectNode> referenceObjects,
                                 Dictionary<int, ResolvedHashedType> elementTypeDict,
                                 IEnumerable<PropertySpecificationResolved> propertySpecifications,
                                 Dictionary<PropertySpecificationResolved, List<SerializedNodeBase>> propertySpecificationGroups,
                                 SerializedNodeBase rootNode)
        {
            this.ReferenceObjects = referenceObjects;
            this.UniquePropertySpecifications = propertySpecifications;
            this.ElementTypeDict = elementTypeDict;
            this.PropertySpecificationGroups = propertySpecificationGroups;
            this.RootNode = rootNode;
        }
    }
}
