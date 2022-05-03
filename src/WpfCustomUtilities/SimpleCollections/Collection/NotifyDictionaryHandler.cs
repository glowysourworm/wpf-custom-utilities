using WpfCustomUtilities.SimpleCollections.Collection.Interface;

namespace WpfCustomUtilities.SimpleCollections.Collection
{
    /// <summary>
    /// Delegate to notify listener about change of hash code
    /// </summary>
    public delegate void NotifyHashCodeChanged(INotifyDictionaryKey sender, int oldHashCode, int newHashCode);
}
