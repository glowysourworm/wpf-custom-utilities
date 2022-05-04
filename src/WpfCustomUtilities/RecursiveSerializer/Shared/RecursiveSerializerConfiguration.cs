namespace WpfCustomUtilities.RecursiveSerializer.Shared
{
    public class RecursiveSerializerConfiguration
    {
        /// <summary>
        /// This can occur when the type tree has changed in the code; and the serialized
        /// graph differs because a property has been REMOVED from the code tree.
        /// </summary>
        public bool IgnoreRemovedProperties { get; set; }

        /// <summary>
        /// This will prompt the user with changes to the type schema before proceeding.
        /// </summary>
        public bool PreviewRemovedProperties { get; set; }

        public RecursiveSerializerConfiguration()
        {
            this.IgnoreRemovedProperties = false;
            this.PreviewRemovedProperties = true;
        }
    }
}
