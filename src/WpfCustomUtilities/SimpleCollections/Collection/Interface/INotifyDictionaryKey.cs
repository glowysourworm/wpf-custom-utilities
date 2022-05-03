namespace WpfCustomUtilities.SimpleCollections.Collection.Interface
{
    /// <summary>
    /// Interface used to specify the change of hash code for a NotifyDictionary item
    /// </summary>
    public interface INotifyDictionaryKey
    {
        /// <summary>
        /// Event to notify listeners about change of hash code
        /// </summary>
        event NotifyHashCodeChanged HashCodeChangedEvent;
    }
}
