namespace WpfCustomUtilities.RecursiveSerializer.Shared.Data
{
    public struct PropertyInfo
    {
        public string PropertyName { get; set; }
        public bool IsUserDefined { get; set; }
        public TypeInfo Declaring { get; set; }
        public TypeInfo Implementing { get; set; }
    }
}
