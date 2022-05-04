using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using WpfCustomUtilities.Extensions;
using WpfCustomUtilities.RecursiveSerializer.IO.Data;
using WpfCustomUtilities.RecursiveSerializer.Target;

namespace WpfCustomUtilities.RecursiveSerializer.Shared
{
    /// <summary>
    /// Reflecting static component for the RecursiveSerializer. This stores caches of reflected data types
    /// and provides hashing algorithms that are public for IDENTIFYING THE TYPE AS A VALUE-TYPE HASH CODE. This
    /// implies that we're treating TYPE as a VAULE-TYPE. (See CalculateHashCode public API method)
    /// </summary>
    internal static class RecursiveSerializerTypeFactory
    {
        private static Dictionary<string, Assembly> AssemblyLookup;
        private static Dictionary<string, Dictionary<string, Type>> TypeLookup;

        private static Dictionary<Type, HashedTypeData> HashedTypeDataCache;

        static RecursiveSerializerTypeFactory()
        {
            AssemblyLookup = new Dictionary<string, Assembly>();
            TypeLookup = new Dictionary<string, Dictionary<string, Type>>();
            HashedTypeDataCache = new Dictionary<Type, HashedTypeData>();
        }

        /// <summary>
        /// Adds the assemblies to the internal lookup dictionaries of the type factory.
        /// </summary>
        private static void LoadCurrentAssemblies()
        {
            // Assemblies
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                // Assembly Lookup
                if (!AssemblyLookup.ContainsKey(assembly.FullName))
                    AssemblyLookup.Add(assembly.FullName, assembly);

                // Type Lookup
                if (!TypeLookup.ContainsKey(assembly.FullName))
                    TypeLookup.Add(assembly.FullName, new Dictionary<string, Type>());

                // -> GetTypes()
                foreach (var type in assembly.GetTypes())
                {
                    if (!TypeLookup[assembly.FullName].ContainsKey(type.FullName))
                        TypeLookup[assembly.FullName].Add(type.FullName, type);
                }
            }
        }

        /// <summary>
        /// Calculates a UNIQUE Hash Code based on the supplied VALUES of the declaring and implementing
        /// Type instances. (MSFT treats these as reference types.. so Type.GetHashCode() is non-unique)
        /// </summary>
        /// <param name="declaringType">Declaring type of the object</param>
        /// <param name="implementingType">Implementing type of the object</param>
        /// <returns>A unique hash code for the data inside of the two supplied System.Type instances</returns>
        internal static int CalculateHashCode(Type declaringType, Type implementingType)
        {
            var declaring = BuildImpl(declaringType);
            var implementing = BuildImpl(implementingType);

            return CalculateHashCodeImpl(declaring, implementing);
        }

        /// <summary>
        /// Checks to see whether assembly has been loaded for the specified type. SEE RecursiveSerializerTypeFactory.LoadAssemblies().
        /// </summary>
        /// <param name="objectType">Object to serialize / deserialize</param>
        internal static bool IsAssemblyLoaded(Type objectType)
        {
            if (!AssemblyLookup.ContainsKey(objectType.Assembly.FullName))
                LoadCurrentAssemblies();

            return AssemblyLookup.ContainsKey(objectType.Assembly.FullName) &&
                   TypeLookup.ContainsKey(objectType.Assembly.FullName) &&
                   TypeLookup[objectType.Assembly.FullName].ContainsKey(objectType.FullName);
        }

        internal static bool DoesTypeExist(HashedType type)
        {
            if (!AssemblyLookup.ContainsKey(type.Declaring.Assembly))
                LoadCurrentAssemblies();

            if (!RecursiveSerializerTypeFactory.TypeLookup.ContainsKey(type.Declaring.Assembly))
                return false;

            // MAKE SURE TO TRY USING THE ASSEMBLY TO RESOLVE THIS DECLARED TYPE
            if (!RecursiveSerializerTypeFactory.TypeLookup[type.Declaring.Assembly].ContainsKey(type.Declaring.Type))
            {
                // Try resolving using the assembly
                var resolvedType = RecursiveSerializerTypeFactory.AssemblyLookup[type.Declaring.Assembly].GetType(type.Declaring.Type);

                if (resolvedType == null)
                    return false;

                else
                    return true;
            }

            return true;
        }

        /// <summary>
        /// Resolves the HashedType as BOTH the DECLARING type AND IMPLEMENTING TYPE. This is used before the object 
        /// implementation has been resolved.
        /// </summary>
        internal static ResolvedHashedType ResolveAsDeclaring(HashedType declaringType)
        {
            var declaringTypeResolved = ResolveImpl(declaringType.Declaring.Assembly, declaringType.Declaring.Type);

            return new ResolvedHashedType(declaringTypeResolved);
        }

        /// <summary>
        /// Treats the HashedType as properly set:  IMPLEMENTING + DECLARING
        /// </summary>
        internal static ResolvedHashedType ResolveAsActual(HashedType actualType)
        {
            var declaringTypeResolved = ResolveImpl(actualType.Declaring.Assembly, actualType.Declaring.Type);
            var implementingTypeResolved = ResolveImpl(actualType.Implementing.Assembly, actualType.Implementing.Type);

            return new ResolvedHashedType(declaringTypeResolved, implementingTypeResolved);
        }

        /// <summary>
        /// Tries to resolve the hashed type data against loaded assemblies using the ASSEMLY NAME + TYPE NAME
        /// </summary>
        internal static Type ResolveAssemblyType(HashedTypeData hashedTypeData)
        {
            return ResolveImpl(hashedTypeData.Assembly, hashedTypeData.Type);
        }

        internal static HashedType Build(Type declaringType)
        {
            return Build(declaringType, declaringType);
        }

        internal static HashedType Build(Type declaringType, Type implementingType)
        {
            var declaring = BuildImpl(declaringType);
            var implementing = BuildImpl(implementingType);

            return new HashedType(declaring, implementing);
        }

        internal static ResolvedHashedType BuildAndResolve(Type declaringType)
        {
            return ResolveAsDeclaring(Build(declaringType));
        }

        internal static HashedType BuildHashedType(ResolvedHashedType resolvedType)
        {
            return Build(resolvedType.GetDeclaringType(), resolvedType.GetImplementingType());
        }

        internal static int CalculateHashCode(HashedType hashedType)
        {
            return CalculateHashCodeImpl(hashedType.Declaring, hashedType.Implementing);
        }

        private static int CalculateHashCodeImpl(HashedTypeData declaring, HashedTypeData implementing)
        {
            return RecursiveSerializerHashGenerator.CreateSimpleHash(declaring, implementing);
        }

        private static HashedTypeData BuildImpl(Type type)
        {
            if (HashedTypeDataCache.ContainsKey(type))
                return HashedTypeDataCache[type];

            var hashedTypeData = new HashedTypeData(type.Assembly.FullName,
                                                    type.FullName,
                                                    type.IsGenericType,
                                                    type.IsEnum,
                                                    type.GetGenericArguments()
                                                        .Select(argument => BuildImpl(argument)).ToArray(),            // RECURSIVE
                                                    type.IsEnum ? BuildImpl(type.GetEnumUnderlyingType()) :            // RECURSIVE
                                                                  HashedTypeData.Empty);

            HashedTypeDataCache.Add(type, hashedTypeData);

            return hashedTypeData;
        }

        private static Type ResolveImpl(string assemblyFullName, string typeFullName)
        {
            if (!AssemblyLookup.ContainsKey(assemblyFullName))
                LoadCurrentAssemblies();

            if (!RecursiveSerializerTypeFactory.TypeLookup.ContainsKey(assemblyFullName))
                throw new Exception("No assembly found for type:  " + typeFullName);

            if (!RecursiveSerializerTypeFactory.TypeLookup[assemblyFullName].ContainsKey(typeFullName))
            {
                // Try resolving using the assembly (REQUIRED TO CALL ASSEMBLY.GETTYPE(...)) This will resolve
                // implementing polymorphic types!!! SOME OF THESE ARE NOT COMPILED!!
                var type = RecursiveSerializerTypeFactory.AssemblyLookup[assemblyFullName].GetType(typeFullName);

                if (type == null)
                    throw new FormattedException("No type {0} found in assembly {1}", typeFullName, assemblyFullName);

                // Cache resolved type
                RecursiveSerializerTypeFactory.TypeLookup[assemblyFullName].Add(typeFullName, type);
            }

            return RecursiveSerializerTypeFactory.TypeLookup[assemblyFullName][typeFullName];
        }
    }
}
