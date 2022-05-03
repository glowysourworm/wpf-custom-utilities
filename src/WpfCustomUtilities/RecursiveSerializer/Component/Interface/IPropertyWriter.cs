namespace WpfCustomUtilities.RecursiveSerializer.Component.Interface
{
    public interface IPropertyWriter
    {
        void Write<T>(string propertyName, T property);
    }
}
