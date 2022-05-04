using RecursiveSerializer.Formatter;

using System;

using WpfCustomUtilities.RecursiveSerializer.Component;
using WpfCustomUtilities.RecursiveSerializer.Planning;

namespace WpfCustomUtilities.RecursiveSerializer.Shared
{
    /// <summary>
    /// Class that takes a shallow copy of a target type. Utilizes Recursive Serializer data facilities to
    /// cache reflection data. Creates shallow copy of object - using public properties - complex objects
    /// are copied using DEEP copy. This is optional for switching methods of copying.
    /// </summary>
    public class RecursiveSerializerShallowCopier
    {
        // Stores resolved types by resolved hashed type
        ImplementingTypeResolver _typeResolver;

        public RecursiveSerializerShallowCopier()
        {
            _typeResolver = new ImplementingTypeResolver();
        }

        /// <summary>
        /// Performs a shallow copy of the object properties and has the option to skip / throw reference
        /// exceptions
        /// </summary>
        /// <typeparam name="T">The item type</typeparam>
        /// <param name="item">Item to copy</param>
        /// <param name="skipReferences">True, then the copier will skip reference types</param>
        /// <param name="throwReferences">If true, then the copier will throw an exception if it encounters a reference type</param>
        public T Copy<T>(T item, bool skipReferences, bool throwReferences)
        {
            if (item == null)
                throw new ArgumentNullException("Trying to copy a null reference:  RecusriveSerializerShallowCopier");

            // Treat this item as the root of a serialization tree - forces serialization pattern
            var resolvedType = _typeResolver.ResolveRoot(item);

            // TODO:  NOT SURE THAT THIS IS THE RIGHT USE FOR THIS METHOD
            var specification = RecursiveSerializerStore.GetDeclaringSpecification(resolvedType);

            // Get constructor from the data store
            var memberInfo = RecursiveSerializerStore.GetMemberInfo(resolvedType);

            if (memberInfo.ParameterlessConstructor == null)
                throw new Exception("Copied item must have a parameterless constructor:  " + resolvedType.GetImplementingType().Name);

            // Construct the new object
            var result = Construct<T>(memberInfo);

            // Iterate property definitions and make a shallow copy
            foreach (var definition in specification.ResolvedDefinitions)
            {
                var propertyInfo = definition.GetReflectedInfo();

                // COMPLEX TYPE
                if (!FormatterFactory.IsPrimitiveSupported(definition.PropertyType.GetImplementingType()))
                {
                    if (skipReferences)
                        continue;

                    else if (throwReferences)
                        throw new Exception("Trying to shallow copy complex type:  " + resolvedType.GetImplementingType().Name);
                }

                // Copy the value
                var copyValue = propertyInfo.GetValue(item);

                // Set the value
                propertyInfo.SetValue(result, copyValue);
            }

            return result;
        }

        /// <summary>
        /// Performs a shallow mapping of the source item onto a NEW item - checking for differences in the properties
        /// and having options to skip, or throw exceptions.
        /// </summary>
        /// <typeparam name="TSource">The SOURCE item type</typeparam>
        /// <typeparam name="TDest">The DESTINATION item type</typeparam>
        /// <param name="source">Item to copy values FroM</param>
        /// <param name="skipReferences">True, then the copier will skip reference types</param>
        /// <param name="throwReferences">If true, then the copier will throw an exception if it encounters a reference type</param>
        /// <param name="skipDifferences">True, then the copier will skip differences in type</param>
        /// <param name="throwDifferences">If true, then the copier will throw an exception if it encounters a difference in type</param>
        public TDest MapToNew<TSource, TDest>(TSource source, bool skipDifferences, bool throwDifferences, bool skipReferences, bool throwReferences)
        {
            if (source == null)
                throw new ArgumentNullException("Trying to map a null reference:  RecusriveSerializerShallowCopier");

            // Treat this item as the root of a serialization tree - forces serialization pattern
            var destType = RecursiveSerializerTypeFactory.BuildAndResolve(typeof(TDest));

            // Get constructor from the data store
            var destInfo = RecursiveSerializerStore.GetMemberInfo(destType);

            if (destInfo.ParameterlessConstructor == null)
                throw new Exception("Mapped item must have a parameterless constructor:  " + destType.GetImplementingType().Name);

            var destination = Construct<TDest>(destInfo);

            // Utilize Map method
            Map(source, destination, skipDifferences, throwDifferences, skipReferences, throwReferences);

            return destination;
        }

        /// <summary>
        /// Performs a shallow mapping of the source item onto the destination item - checking for differences in the properties
        /// and having options to skip, or throw exceptions.
        /// </summary>
        /// <typeparam name="TSource">The SOURCE item type</typeparam>
        /// <typeparam name="TDest">The DESTINATION item type</typeparam>
        /// <param name="source">Item to copy values FroM</param>
        /// <param name="destination">Item to copy values TO</param>
        /// <param name="skipReferences">True, then the copier will skip reference types</param>
        /// <param name="throwReferences">If true, then the copier will throw an exception if it encounters a reference type</param>
        /// <param name="skipDifferences">True, then the copier will skip differences in type</param>
        /// <param name="throwDifferences">If true, then the copier will throw an exception if it encounters a difference in type</param>
        public void Map<TSource, TDest>(TSource source, TDest destination, bool skipDifferences, bool throwDifferences, bool skipReferences, bool throwReferences)
        {
            if (source == null || destination == null)
                throw new ArgumentNullException("Trying to map a null reference:  RecusriveSerializerShallowCopier.Map");

            // Treat this item as the root of a serialization tree - forces serialization pattern
            var sourceType = _typeResolver.ResolveRoot(source);
            var destType = _typeResolver.ResolveRoot(destination);

            // TODO:  NOT SURE THAT THIS IS THE RIGHT USE FOR THIS METHOD
            var sourceSpec = RecursiveSerializerStore.GetDeclaringSpecification(sourceType);
            var destSpec = RecursiveSerializerStore.GetDeclaringSpecification(destType);

            // Iterate property definitions and map values
            foreach (var sourceDefinition in sourceSpec.ResolvedDefinitions)
            {
                // MISSING DEFINITION
                if (!destSpec.ContainsResolvedDefinition(sourceDefinition.PropertyName, sourceDefinition.PropertyType))
                {
                    if (skipDifferences)
                        continue;

                    else if (throwDifferences)
                        throw new Exception("Difference in source and destination types. Either set to skip differences or check source / destination type:  " + sourceType.GetImplementingType().Name);
                }

                else
                {
                    // COMPLEX TYPE
                    if (!FormatterFactory.IsPrimitiveSupported(sourceDefinition.PropertyType.GetImplementingType()))
                    {
                        if (skipReferences)
                            continue;

                        else if (throwReferences)
                            throw new Exception("Trying to map complex type property or field:  " + sourceType.GetImplementingType().Name);
                    }

                    // Get the destination property info using the hash lookup
                    var destDefinition = destSpec.GetResolvedDefinition(sourceDefinition.PropertyName, sourceDefinition.PropertyType);

                    // Get reflection info from the stored data
                    var sourcePropertyInfo = sourceDefinition.GetReflectedInfo();
                    var destPropertyInfo = destDefinition.GetReflectedInfo();

                    // Copy the value
                    var copyValue = sourcePropertyInfo.GetValue(source);

                    // Set the value
                    destPropertyInfo.SetValue(destination, copyValue);
                }
            }
        }

        private T Construct<T>(RecursiveSerializerMemberInfo memberInfo)
        {
            try
            {
                return (T)memberInfo.ParameterlessConstructor.Invoke(new object[] { });
            }
            catch (Exception ex)
            {
                throw new Exception("There was an error constructing object of type:  " + typeof(T).Name, ex);
            }
        }
    }
}
