namespace WpfCustomUtilities.Extensions
{
    public class FormattedException : System.Exception
    {
        public FormattedException(string message) : base(message) { }

        public FormattedException(string message, params object[] args)
            : base(string.Format(message, args)) { }

        public FormattedException(string message, System.Exception innerException, params object[] args)
            : base(string.Format(message, args), innerException) { }
    }
}
