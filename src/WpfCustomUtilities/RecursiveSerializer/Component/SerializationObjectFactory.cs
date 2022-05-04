using RecursiveSerializer.Formatter;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using WpfCustomUtilities.Extensions;
using WpfCustomUtilities.RecursiveSerializer.IO.Data;
using WpfCustomUtilities.RecursiveSerializer.Shared;
using WpfCustomUtilities.RecursiveSerializer.Target;

namespace WpfCustomUtilities.RecursiveSerializer.Component
{
    /// <summary>
    /// Component that creates wrappers for serialization objects while keeping track of references and
    /// avoiding CIRCULAR references.
    /// </summary>
    internal class SerializationObjectFactory
    {
        // ID Generator for all OBJECT references. Primitives are treated with hash codes
        ObjectIDGenerator _referenceGenerator;

        // Referenced by object.GetHashCode() + HashedType.GetHashCode()
        Dictionary<int, SerializedNodeBase> _primitiveReferences;

        // Referenced by ObjectIDGenerator.GetID(..)
        Dictionary<long, List<SerializedObjectNode>> _objectReferences;

        List<SerializedNodeBase> _allObjects;

        internal SerializationObjectFactory()
        {
            _referenceGenerator = new ObjectIDGenerator();
            _primitiveReferences = new Dictionary<int, SerializedNodeBase>();
            _objectReferences = new Dictionary<long, List<SerializedObjectNode>>();
            _allObjects = new List<SerializedNodeBase>();
        }

        internal IEnumerable<SerializedNodeBase> GetAllSerializedObjects()
        {
            return _allObjects;
        }

        internal IEnumerable<SerializedObjectNode> GetReferenceObjects()
        {
            return _objectReferences.Values
                                    .SelectMany(list => list)
                                    .ToList();
        }

        private int CreatePrimitiveReference(object theObject, ResolvedHashedType resolvedType)
        {
            if (ReferenceEquals(theObject, null))
            {
                return resolvedType.GetHashCode();
            }
            else
                return RecursiveSerializerHashGenerator.CreateSimpleHash(theObject, resolvedType);
        }

        /// <summary>
        /// HANDLES NULLS!  Wraps object for serialization - locating methods for ctor and get method for reproducing object. 
        /// The object type is referenced for creating a wrapper for a null ref object.
        /// </summary>
        internal SerializedNodeBase Create(object theObject, ResolvedHashedType resolvedType)
        {
            // PRIMITIVE TYPE:    Treated as primitive value type - MUST HAVE FORMATTER!
            //
            // ATTRIBUTE MARKED:  Treat as explicitly defined. Locate constructor and get method
            //
            // REFERENCE TYPE:    Treated as default 1) ctor and get method (or) 2) parameterless ctor
            //
            // VALUE TYPE:        Treated as default - Serializer will try locating a formatter. THROWS
            //                    EXCEPTION OTHERWISE!
            //
            // COLLECTION:        Validate for the types we're supporting:  List.
            //
            // NULL REFERENCE:    Wrapped by type
            //

            // FOR LEAF NODES -> RETURN EXISTING REFERENCES
            var primitiveReference = CreatePrimitiveReference(theObject, resolvedType);
            var isPrimitive = FormatterFactory.IsPrimitiveSupported(resolvedType.GetImplementingType());

            // PRIMITIVE NULL
            if (ReferenceEquals(theObject, null) && isPrimitive)
            {
                if (_primitiveReferences.ContainsKey(primitiveReference))
                    return _primitiveReferences[primitiveReference];

                return FinalizePrimitive(new SerializedLeafNode(SerializedNodeType.NullPrimitive, resolvedType));
            }

            // NULL
            if (ReferenceEquals(theObject, null))
            {
                if (_primitiveReferences.ContainsKey(primitiveReference))
                    return _primitiveReferences[primitiveReference];

                return FinalizePrimitive(new SerializedLeafNode(SerializedNodeType.Null, resolvedType));
            }

            // STRINGS IMPLEMENT IEnumerable!
            var isCollection = (theObject.ImplementsInterface<IList>() || theObject.GetType().IsArray) && !isPrimitive;

            // PRIMITIVE
            if (isPrimitive)
            {
                if (_primitiveReferences.ContainsKey(primitiveReference))
                    return _primitiveReferences[primitiveReference];

                return FinalizePrimitive(new SerializedLeafNode(SerializedNodeType.Primitive, resolvedType, theObject));
            }

            // SEARCH FOR REFERENCE 
            var referenceNode = FindReference(theObject);

            // REFERENCE (Is reference type AND is OLD REFERENCE)
            if (referenceNode != null)
                return new SerializedReferenceNode(referenceNode.Id, resolvedType, RecursiveSerializerStore.GetMemberInfo(resolvedType));

            // ***** THE REST GET ADDED TO THE REFERENCE DICT

            // COLLECTION (STRINGS IMPLEMENT IEnumerable!)
            if (isCollection)
                return FinalizeObject(CreateCollectionNode(theObject, resolvedType));

            // OBJECT
            else
                return FinalizeObject(new SerializedObjectNode(SerializedNodeType.Object, resolvedType, theObject, RecursiveSerializerStore.GetMemberInfo(resolvedType)));
        }

        // Referenced by object.GetHashCode() -> ReferenceEquals( , )
        //
        private SerializedObjectNode FindReference(object theObject)
        {
            var firstReference = false;
            var referenceId = _referenceGenerator.GetId(theObject, out firstReference);

            // NARROW SEARCH BY HASH CODE
            if (_objectReferences.ContainsKey(referenceId))
            {
                // Iterate reference type nodes
                foreach (var objectNode in _objectReferences[referenceId])
                {
                    // MUST USE MSFT REFERENCE EQUALS!!!
                    if (ReferenceEquals(objectNode.GetObject(), theObject))
                        return objectNode;
                }
            }

            return null;
        }

        private SerializedCollectionNode CreateCollectionNode(object theObject, ResolvedHashedType resolvedType)
        {
            // Procedure
            //
            // 0) Determine SerializationMode by checking for constructor and get method
            // 1) Get element type
            // 2) Check that collection type is supported:  List<T>
            // 3) Create the result
            //

            // Validates type and SerializationMode
            var memberInfo = RecursiveSerializerStore.GetMemberInfo(resolvedType);

            if (!resolvedType.GetImplementingType().IsGenericType)
                throw new RecursiveSerializerException(resolvedType, "PropertySerializer only supports Arrays, and Generic Collections:  List<T>");

            // ARRAY
            else if (resolvedType.GetImplementingType().IsArray)
            {
                var argument = theObject.GetType().GetElementType();
                var array = theObject as Array;

                if (argument == null)
                    throw new Exception("Unable to reflect array element type for PropertySerializer: " + resolvedType.ToString());

                var elementDeclaringType = RecursiveSerializerTypeFactory.BuildAndResolve(argument);

                return new SerializedCollectionNode(resolvedType, memberInfo, array, array.Length, CollectionInterfaceType.Array, elementDeclaringType);
            }

            // LIST
            else if (theObject.ImplementsInterface<IList>())
            {
                var argument = (theObject as IList).GetType().GetGenericArguments()[0];

                if (argument == null)
                    throw new Exception("Invalid IList argument for PropertySerializer: " + resolvedType.ToString());

                var list = theObject as IList;
                var elementDeclaringType = RecursiveSerializerTypeFactory.BuildAndResolve(argument);

                return new SerializedCollectionNode(resolvedType, memberInfo, list, list.Count, CollectionInterfaceType.IList, elementDeclaringType);
            }
            else
                throw new RecursiveSerializerException(resolvedType, "PropertySerializer only supports Arrays, and Generic Collections:  List<T>");
        }

        // TRACK REFERENCES AND ALL OBJECTS -> RETURN
        private SerializedNodeBase FinalizePrimitive(SerializedLeafNode result)
        {
            _allObjects.Add(result);

            var primitiveReference = CreatePrimitiveReference(result.GetObject(), result.Type);

            if (!_primitiveReferences.ContainsKey(primitiveReference))
                _primitiveReferences.Add(primitiveReference, result);

            return result;
        }

        // TRACK REFERENCES AND ALL OBJECTS -> RETURN
        private SerializedNodeBase FinalizeObject(SerializedObjectNode result)
        {
            var firstReference = false;
            var referenceId = _referenceGenerator.GetId(result.GetObject(), out firstReference);

            _allObjects.Add(result);

            // Existing HASH CODE -> Add REFERENCE
            if (_objectReferences.ContainsKey(referenceId))
                _objectReferences[referenceId].Add(result);

            else
                _objectReferences.Add(referenceId, new List<SerializedObjectNode>() { result });

            return result;
        }
    }
}
