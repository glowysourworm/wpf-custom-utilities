using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace WpfCustomUtilities.ClassMaker
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            var inputText = this.InputTB.Text;
            var outputText = "";

            if (TryParse(inputText, out outputText))
                this.OutputTB.Text = outputText;
        }

        private bool TryParse(string input, out string output)
        {
            output = "";

            try
            {
                var builder = new StringBuilder();
                var declarations = new List<string>();

                var lines = input.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        builder.AppendLine("\r\n");

                    var parts = line.Split(new char[] { ' ', ';', '_' }, StringSplitOptions.RemoveEmptyEntries);

                    // Implicit Format
                    if (this.ImplicitFormatRB.IsChecked.HasValue &&
                        this.ImplicitFormatRB.IsChecked.Value)
                    {
                        if (!TryCreateImplicitProperty(parts, builder))
                            continue;
                    }

                    // Explicit Format
                    else if (this.ExplicitFormatRB.IsChecked.HasValue &&
                             this.ExplicitFormatRB.IsChecked.Value)
                    {
                        if (!TryCreateExplicitProperty(parts, builder))
                            continue;
                    }

                    // Dependency Property Format
                    else
                    {
                        string declaration;
                        string className = string.IsNullOrEmpty(this.ClassNameTB.Text) ? "ClassName" : this.ClassNameTB.Text;

                        if (!TryCreateDependencyProperty(parts, className, builder, out declaration))
                            continue;

                        declarations.Add(declaration);
                    }

                }

                if (declarations.Count > 0)
                {
                    foreach (var declaration in declarations)
                        output += declaration + "\r\n\r\n";
                }

                output += builder.ToString();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool TryCreateImplicitProperty(string[] parts, StringBuilder builder)
        {
            try
            {
                var type = "";
                var lowerCaseName = "";

                // private string _foo;
                // string _foo;

                if (parts.Length == 3)
                {
                    type = parts[1].Trim();
                    lowerCaseName = parts[2].Trim();
                }
                else if (parts.Length == 2)
                {
                    type = parts[0].Trim();
                    lowerCaseName = parts[1].Trim();
                }
                else
                    return false;

                var firstLetter = lowerCaseName.Substring(0, 1).ToUpper();
                var name = string.Concat(firstLetter, lowerCaseName.Substring(1, lowerCaseName.Length - 1));

                builder.AppendLine("public " + type + " " + name + " { get; set; }");

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool TryCreateExplicitProperty(string[] parts, StringBuilder builder)
        {
            try
            {
                var type = "";
                var lowerCaseName = "";

                // private string _foo;
                // string _foo;

                if (parts.Length == 3)
                {
                    type = parts[1].Trim();
                    lowerCaseName = parts[2].Trim();
                }
                else if (parts.Length == 2)
                {
                    type = parts[0].Trim();
                    lowerCaseName = parts[1].Trim();
                }
                else
                    return false;

                var firstLetter = lowerCaseName.Substring(0, 1).ToUpper();
                var name = string.Concat(firstLetter, lowerCaseName.Substring(1, lowerCaseName.Length - 1));

                builder.AppendLine(string.Format("public {0} {1}", type, name));
                builder.AppendLine("{");
                builder.AppendLine("\tget { return _" + lowerCaseName + "; }");
                builder.AppendLine("\tset { this.RaiseAndSetIfChanged(ref _" + lowerCaseName + ", value); }");
                builder.AppendLine("}");

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool TryCreateDependencyProperty(string[] parts, string className, StringBuilder builder, out string declaration)
        {
            declaration = "";

            try
            {
                var type = "";
                var lowerCaseName = "";

                if (parts.Length == 3)
                {
                    type = parts[1].Trim();
                    lowerCaseName = parts[2].Trim();
                }
                else if (parts.Length == 2)
                {
                    type = parts[0].Trim();
                    lowerCaseName = parts[1].Trim();
                }
                else
                    return false;

                var firstLetter = lowerCaseName.Substring(0, 1).ToUpper();
                var name = string.Concat(firstLetter, lowerCaseName.Substring(1, lowerCaseName.Length - 1));

                declaration = string.Format("public static readonly DependencyProperty {0}Property = ", name);
                declaration += string.Format("\r\n\tDependencyProperty.Register(\"{0}\", typeof({1}), typeof({2}));", name, type, className);

                builder.AppendLine(string.Format("public {0} {1}", type, name));
                builder.AppendLine("{");
                builder.AppendLine("\tget { return (" + type + ")GetValue(" + name + "Property); }");
                builder.AppendLine("\tset { SetValue(" + name + "Property, value); }");
                builder.AppendLine("}");

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
