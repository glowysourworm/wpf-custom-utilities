using System;

namespace WpfCustomUtilities.ClassMaker
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            var application = new App();

            application.Run();
        }
    }
}
