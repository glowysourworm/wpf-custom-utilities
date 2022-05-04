using System;
using System.Collections.Generic;
using System.Linq;

using WpfCustomUtilities.Extensions;
using WpfCustomUtilities.RecursiveSerializer.IO.Data;
using WpfCustomUtilities.RecursiveSerializer.Planning;
using WpfCustomUtilities.RecursiveSerializer.Shared;
using WpfCustomUtilities.RecursiveSerializer.Target;

namespace WpfCustomUtilities.RecursiveSerializer.Component
{
    internal class DeserializationResolver
    {
        // Collection of UNIQUE REFERENCE OBJECT that HAVE BEEN DESERIALIZED -> By Id artifact from serialization
        Dictionary<int, DeserializedObjectNode> _deserializedObjectcs;

        readonly RecursiveSerializerConfiguration _configuration;
        readonly ImplementingTypeResolver _implementingTypeResolver;
        readonly DeserializedHeader _header;


        internal DeserializationResolver(RecursiveSerializerConfiguration configuration,
                                         ImplementingTypeResolver implementingTypeResolver,
                                         DeserializedHeader header)
        {
            _configuration = configuration;
            _implementingTypeResolver = implementingTypeResolver;
            _header = header;
            _deserializedObjectcs = new Dictionary<int, DeserializedObjectNode>();
        }

        /// <summary>
        /// Attempts to resolve the node as the specified type
        /// </summary>
        internal T Resolve<T>(DeserializedObjectNode node)
        {
            ResolveImpl(node);

            return (T)node.Resolve();
        }


        // Recursively resolves the graph
        private void ResolveImpl(DeserializedObjectNode node)
        {
            // Procedure:  Recursively collect PropertyStorageInfo objects to use with reflection. At
            //             the end of the loop call SetValue on the current node's object. Primitives
            //             for leaves should then be ready for resolving the node.
            //
            //             After node is resolved - STORE REFERENCE AS DESERIALIZED OBJECT ONLY IF IT
            //             IS OF A REFERENCE TYPE:  
            //
            //             DeserializedCollectionNode
            //             DeserializedObjectNode
            //
            //             COLLECTION ELEMENTS:  Resolved as "sub-trees" using different method of locating
            //                                   the PropertySpecification, and resolving the types.

            // MISSING / MODIFIED SPECIFICATIONS:
            //
            // MISSING:  The entire node has no specification. This should halt recursion for 
            //           the rest of the branch. 
            //
            //           NOTE*** Missing object references are dealt with when resolving a 
            //                   DeserializedReferenceNode.
            //
            // MODIFIED: The modified PROPERTIES are dealt with during the sub-node loop in
            //           a similar fashion.
            //

            // MISSING SPECIFICATION
            if (_header.MissingSpecificationLookup.ContainsKey(node.ReferenceId))
            {
                if (!_configuration.IgnoreRemovedProperties)
                    throw GetMissingPropertyException(node.SerializedType);

                // Halt Recursion
                return;
            }

            // MEMBER INFO:  These can be resolved for types that aren't MISSING. Get the member info
            //               for the SERIAILZED TYPE (CAN NOW BE TREATED AS "ACTUAL")
            //
            var actualType = RecursiveSerializerTypeFactory.ResolveAsActual(node.SerializedType);
            var memberInfo = RecursiveSerializerStore.GetMemberInfo(actualType, node.Mode);

            // COLLECTION
            if (node is DeserializedCollectionNode)
            {
                var collectionNode = node as DeserializedCollectionNode;

                var collectionProperties = new List<PropertyResolvedInfo>();

                // Loop properties: Collect property data recursively -> Store back on node
                foreach (var subNode in collectionNode.SubNodes)
                {
                    // RESOLVE (AND / OR) RECURSE
                    var resolvedProperty = ResolveNodeRecurse(node, subNode, false);

                    if (resolvedProperty == null)
                        continue;

                    collectionProperties.Add(resolvedProperty);
                }

                // Set properties using DeserializationObjectBase
                collectionNode.Construct(memberInfo, collectionProperties);

                var resolvedChildNodes = new List<PropertyResolvedInfo>();

                // Iterate Elements -> Resolve recursively
                for (int index = 0; index < collectionNode.Count; index++)
                {
                    var childNode = collectionNode.CollectionNodes[index];

                    // RECURSE CHILD NODES (NOTE*** Child nodes are not properties)
                    var resolvedElement = ResolveNodeRecurse(collectionNode, childNode, true);

                    resolvedChildNodes.Add(resolvedElement);
                }

                // FINALIZE COLLECTION (MUST HAVE CALLED Construct())
                //                
                if (resolvedChildNodes.Any())
                    collectionNode.FinalizeCollection(memberInfo, resolvedChildNodes);

                // STORE REFERENCE (OBJECT IS READY!)
                _deserializedObjectcs.Add(collectionNode.ReferenceId, collectionNode);
            }

            // OBJECT
            else
            {
                // Properties to RESOLVE
                var properties = new List<PropertyResolvedInfo>();

                // Loop properties: Collect property data recursively -> Store back on node
                foreach (var subNode in node.SubNodes)
                {
                    // RESOLVE (AND / OR) RECURSE
                    var resolvedProperty = ResolveNodeRecurse(node, subNode, false);

                    if (resolvedProperty == null)
                        continue;

                    properties.Add(resolvedProperty);
                }

                // Construct() -> Set properties 
                node.Construct(memberInfo, properties);

                // STORE REFERENCE (OBJECT IS READY!)
                _deserializedObjectcs.Add(node.ReferenceId, node);
            }
        }

        /// <summary>
        /// Attempts to RESOLVE the property defined in the DeserializationNodeBase. This will RECURSE Resolve() to
        /// fill in the node property data. RETURNS NULL IF MODIFIED / MISSING PROPERTY EXISTS. 
        /// </summary>
        private PropertyResolvedInfo ResolveNodeRecurse(DeserializedObjectNode parentNode, DeserializedNodeBase subNode, bool isCollectionElement)
        {
            // MODIFIED SPECIFICATION:  Would be a specification that was modified by removing properties. These
            //                          would appear as definitions under the header collection for the MODIFIED
            //                          specification.
            //
            if (_header.ModifiedSpecificationLookup.ContainsKey(parentNode.ReferenceId))
            {
                var modifiedSpecification = _header.ModifiedSpecificationLookup[parentNode.ReferenceId];

                // MODIFIED DEFINITION (MISSING FROM TYPE)
                if (modifiedSpecification.ContainsHashedDefinition(subNode.Property.PropertyName, subNode.Property.PropertyType))
                {
                    if (!_configuration.IgnoreRemovedProperties)
                        throw GetMissingPropertyException(subNode.SerializedType);

                    // RETURN NULL FOR IGNORED PROPERTY
                    else
                        return null;
                }
            }

            // NULL LEAF (NULL REFERENCE, NULL PRIMITIVE)
            if (subNode is DeserializedNullLeafNode)
                return CreateResolvedNullLeafProperty(parentNode, subNode as DeserializedNullLeafNode, isCollectionElement);

            // LEAF (PRIMITIVE)
            else if (subNode is DeserializedLeafNode)
            {
                return CreateResolvedProperty(parentNode, subNode, isCollectionElement);
            }

            // REFERENCE - MUST HAVE PREVIOUSLY DESERIALIZED OBJECT TO RESOLVE!
            else if (subNode is DeserializedReferenceNode)
            {
                var referenceNode = subNode as DeserializedReferenceNode;

                // MISSING SPECIFICATION:  MUST CHECK FOR MODIFICATIONS
                if (_header.MissingSpecificationLookup.ContainsKey(referenceNode.ReferenceId))
                {
                    if (!_configuration.IgnoreRemovedProperties)
                        throw GetMissingPropertyException(referenceNode.SerializedType);

                    // RETURN NULL FOR IGNORED PROPERTY
                    else
                        return null;
                }

                // Check the node object - which contains the REFERENCE ID
                if (!_deserializedObjectcs.ContainsKey(referenceNode.ReferenceId))
                    throw new FormattedException("UN-RESOLVED REFERENCE:  Id={0}, Type={1}", referenceNode.ReferenceId, subNode.SerializedType.Declaring.Type);

                return CreateResolvedProperty(parentNode, referenceNode, isCollectionElement);
            }

            // ELSE -> RECURSE
            else
            {
                var objectNode = subNode as DeserializedObjectNode;

                // OBJECT, COLLECTION
                ResolveImpl(objectNode);

                if (!_deserializedObjectcs.ContainsKey(objectNode.ReferenceId))
                    throw new Exception("Unresolved subnode PropertyDeserializer.ResolveNodeRecurse");

                return CreateResolvedProperty(parentNode, subNode, isCollectionElement);
            }
        }

        // RESOLVE:  Definition, Object, Implementing Type (ALSO ELEMENT IMPLEMENTING TYPE)
        private PropertyResolvedInfo CreateResolvedProperty(DeserializedObjectNode parentNode,
                                                            DeserializedNodeBase subNode,
                                                            bool isCollectionElement)
        {
            // COLLECTION ELEMENT (NOT A PROPERTY)
            if (isCollectionElement)
            {
                var resolvedObject = (subNode is DeserializedLeafNode) ? subNode.Resolve() : _deserializedObjectcs[subNode.GetHashCode()].Resolve();
                var elementType = (parentNode as DeserializedCollectionNode).ElementType;
                var elementDeclaringType = RecursiveSerializerTypeFactory.ResolveAsDeclaring(elementType);
                var resolvedType = _implementingTypeResolver.Resolve(resolvedObject, elementDeclaringType);

                return new PropertyResolvedInfo(null)
                {
                    PropertyName = subNode.Property.PropertyName,
                    ResolvedObject = resolvedObject,
                    ResolvedType = resolvedType
                };
            }

            else
            {
                var isUserDefined = false;
                var resolvedDefinition = GetResolvedProperty(parentNode, subNode, out isUserDefined);
                var resolvedObject = (subNode is DeserializedLeafNode) ? subNode.Resolve() : _deserializedObjectcs[subNode.GetHashCode()].Resolve();
                var resolvedType = _implementingTypeResolver.Resolve(resolvedObject, resolvedDefinition.PropertyType);

                return new PropertyResolvedInfo(isUserDefined ? null : resolvedDefinition.GetReflectedInfo())
                {
                    PropertyName = subNode.Property.PropertyName,
                    ResolvedObject = resolvedObject,
                    ResolvedType = resolvedType
                };
            }
        }

        private PropertyResolvedInfo CreateResolvedNullLeafProperty(DeserializedObjectNode parentNode,
                                                                    DeserializedNullLeafNode nullLeafNode,
                                                                    bool isCollectionElement)
        {
            // COLLECTION ELEMENT (NOT A PROPERTY)
            if (isCollectionElement)
            {
                var resolvedType = ResolveNodeType(nullLeafNode);

                return new PropertyResolvedInfo(null)
                {
                    PropertyName = nullLeafNode.Property.PropertyName,
                    ResolvedObject = null,
                    ResolvedType = resolvedType
                };
            }
            else
            {
                var isUserDefined = false;
                var resolvedDefinition = GetResolvedProperty(parentNode, nullLeafNode, out isUserDefined);

                return new PropertyResolvedInfo(isUserDefined ? null : resolvedDefinition.GetReflectedInfo())
                {
                    PropertyName = nullLeafNode.Property.PropertyName,
                    ResolvedObject = null,
                    ResolvedType = RecursiveSerializerTypeFactory.ResolveAsDeclaring(nullLeafNode.SerializedType)
                };
            }
        }

        /// <summary>
        /// Gets the member info based on the RESOLVED property specification. It SIMULATES the ACTUAL IMPLEMENTING TYPE!
        /// </summary>
        private RecursiveSerializerMemberInfo GetMemberInfo(DeserializedObjectNode objectNode)
        {
            // VERIFY RAW HEADER DATA
            if (!_header.Data.PropertySpecificationLookup.ContainsKey(objectNode.ReferenceId))
                throw new Exception("MISSING PROPERTY SPECIFICATION for node: " + objectNode.ToString());

            var specification = _header.Data.PropertySpecificationLookup[objectNode.ReferenceId];

            // "ACTUAL" TYPE:  The serialized header specification contained the actual DECLARING + IMPLEMENTING
            //                 MSFT types from having been stored that way.
            //
            var actualType = specification.ObjectType;

            var resolvedType = RecursiveSerializerTypeFactory.ResolveAsActual(actualType);

            return RecursiveSerializerStore.GetMemberInfo(resolvedType, objectNode.Mode);
        }

        /// <summary>
        /// Retrieves the resolved property definition from the serialized property definition FOR THE SUB NODE. VALIDATES IMPLEMENTING TYPE!
        /// </summary>
        private PropertyDefinitionResolved GetResolvedProperty(DeserializedObjectNode parentNode,
                                                               DeserializedNodeBase subNode,
                                                               out bool isUserDefined)
        {
            // CHECK RAW HEADER DATA ONE MORE TIME (For parent specifications)
            if (!_header.Data.PropertySpecificationLookup.ContainsKey(parentNode.ReferenceId))
                throw new Exception("MISSING PROPERTY SPECIFICATION for node: " + parentNode.ToString());

            var resolvedType = ResolveNodeType(subNode);

            // RESOLVED (Parent specification -> Child Property Definition
            if (_header.ResolvedSpecificationLookup.ContainsKey(parentNode.ReferenceId) &&
                _header.ResolvedSpecificationLookup[parentNode.ReferenceId]
                       .ContainsResolvedDefinition(subNode.Property.PropertyName, resolvedType))
            {
                // IS USER DEFINED
                isUserDefined = _header.ResolvedSpecificationLookup[parentNode.ReferenceId].IsUserDefined;

                return _header.ResolvedSpecificationLookup[parentNode.ReferenceId]
                              .GetResolvedDefinition(subNode.Property.PropertyName, resolvedType);
            }


            // MODIFIED (MODIFIED specification would contain the MISSING property definition)
            else if (_header.ModifiedSpecificationLookup.ContainsKey(parentNode.ReferenceId) &&
                     _header.ModifiedSpecificationLookup[parentNode.ReferenceId]
                            .ContainsHashedDefinition(subNode.Property.PropertyName, subNode.Property.PropertyType))
            {
                throw new Exception("MISSED MODIFIED SPECIFICATION VALIDATION: " + subNode.ToString());
            }

            else
                throw GetMissingPropertyException(subNode.SerializedType);
        }

        private ResolvedHashedType ResolveNodeType(DeserializedNodeBase node)
        {
            // RESOLVE TYPES (Serialized type can be treated as "ACTUAL")
            var actualType = RecursiveSerializerTypeFactory.ResolveAsActual(node.SerializedType);

            ResolvedHashedType resolvedType = null;

            // SAFE TO CALL:  The object node has been fully constructed and resolved through the tree
            if (node is DeserializedCollectionNode ||
                node is DeserializedObjectNode ||
                node is DeserializedLeafNode)
            {
                resolvedType = _implementingTypeResolver.Resolve(node.Resolve(), actualType);
            }

            else if (node is DeserializedNullLeafNode)
                resolvedType = actualType;

            else if (node is DeserializedReferenceNode)
            {
                // Get referenced object
                var resolvedObject = _deserializedObjectcs[(node as DeserializedReferenceNode).ReferenceId].Resolve();

                // Resolve against implementation
                resolvedType = _implementingTypeResolver.Resolve(resolvedObject, actualType);
            }
            else
                throw new Exception("Unhandled node type:  DeserializationResolver.ResolveNodeType");

            // VALIDATE IMPLEMENTING TYPE
            if (resolvedType.GetHashCode() != actualType.GetHashCode())
            {
                if (!actualType.GetDeclaringType().IsAssignableFrom(resolvedType.GetImplementingType()))
                    throw new FormattedException("Invalid IMPLEMENTING TYPE:  Expected Type={0}, Resolved Type={1}", actualType, resolvedType);
            }

            return actualType;
        }

        private Exception GetMissingPropertyException(HashedType nodeType)
        {
            return new RecursiveSerializerException(nodeType, "Trying to Deserialize modified type file. To proceed please set " +
                                                              "the IgnoreRemovedProperties flag in the configuration");
        }
    }
}
