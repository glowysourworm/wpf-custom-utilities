namespace WpfCustomUtilities.RecursiveSerializer.Component.Interface
{
    public interface IPropertyReader
    {
        T Read<T>(string propertyName);
    }
}
