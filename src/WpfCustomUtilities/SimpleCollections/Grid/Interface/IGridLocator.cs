
using WpfCustomUtilities.RecursiveSerializer.Interface;

namespace WpfCustomUtilities.SimpleCollections.Grid.Interface
{
    /// <summary>
    /// Interface for related 2D array objects that have a location on a 2D array or a member that has
    /// some location specified by 2 indices.
    /// </summary>
    public interface IGridLocator : IRecursiveSerializable
    {
        int Column { get; }
        int Row { get; }

        /// <summary>
        /// Compares two locators for location equivalence
        /// </summary>
        bool IsLocationEquivalent(IGridLocator locator);
    }
}
