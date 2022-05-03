using WpfCustomUtilities.RecursiveSerializer.IO.Data;
using WpfCustomUtilities.RecursiveSerializer.Target;

namespace WpfCustomUtilities.RecursiveSerializer.Component
{
    /// <summary>
    /// Creates wrapped types with information for constructing / reading data from the stream to write
    /// to them in a default / specific constructor mode.
    /// </summary>
    internal class DeserializationObjectFactory
    {
        internal DeserializationObjectFactory()
        {
        }

        internal DeserializedNodeBase CreateCollection(PropertyDefinition definingProperty,
                                                       int referenceId, HashedType actualType,
                                                       CollectionInterfaceType interfaceType,
                                                       int childCount,
                                                       SerializationMode mode,
                                                       PropertySpecification specification,
                                                       HashedType elementType)
        {
            switch (interfaceType)
            {
                case CollectionInterfaceType.Array:
                case CollectionInterfaceType.IList:
                    return new DeserializedCollectionNode(definingProperty, actualType,
                                                          referenceId, specification,
                                                          elementType, childCount,
                                                          interfaceType, mode);
                default:
                    throw new System.Exception("Unhandled collection interface type:  DeserializationObjectFactory.cs");
            }
        }

        internal DeserializedNodeBase CreateNullReference(PropertyDefinition definingProperty, HashedType type, SerializationMode mode)
        {
            return new DeserializedNullLeafNode(definingProperty, type, mode);
        }

        internal DeserializedNodeBase CreateNullPrimitive(PropertyDefinition definingProperty, HashedType type, SerializationMode mode)
        {
            return new DeserializedNullLeafNode(definingProperty, type, mode);
        }

        internal DeserializedNodeBase CreatePrimitive(PropertyDefinition definingProperty, object theObject, HashedType type, SerializationMode mode)
        {
            return new DeserializedLeafNode(definingProperty, type, theObject, mode);
        }

        internal DeserializedNodeBase CreateReference(PropertyDefinition definingProperty, int referenceId, HashedType type, SerializationMode mode)
        {
            return new DeserializedReferenceNode(definingProperty, referenceId, type, mode);
        }

        internal DeserializedNodeBase CreateObject(PropertyDefinition definingProperty, int referenceId, HashedType type, SerializationMode mode, PropertySpecification specification)
        {
            return new DeserializedObjectNode(definingProperty, type, referenceId, specification, mode);
        }
    }
}
