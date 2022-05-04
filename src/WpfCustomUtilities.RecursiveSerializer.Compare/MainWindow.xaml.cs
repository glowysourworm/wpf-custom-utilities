using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyModel.Resolution;
using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Shapes;

using WpfCustomUtilities.Extensions.ObservableCollection;
using WpfCustomUtilities.RecursiveSerializer.Compare.ViewModel;
using WpfCustomUtilities.RecursiveSerializer.Shared;

namespace WpfCustomUtilities.RecursiveSerializer.Compare
{
    public partial class MainWindow : Window
    {
        FileViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new FileViewModel();

            this.DataContext = _viewModel;

            ShowLoadedAssemblies();
        }

        private void OpenFile(string fileName)
        {
            // CLEAR OUT THE VIEW MODEL EXCEPT FOR ASSEMBLIES
            _viewModel.RootType = null;
            _viewModel.Types.Clear();
            _viewModel.DataNodes.Clear();

            var reader = new RecursiveSerializerDataReader(new RecursiveSerializerConfiguration()
            {
                IgnoreRemovedProperties = true,
                PreviewRemovedProperties = false
            });

            using (var stream = File.OpenRead(fileName))
            {
                // Stores data from the stream
                reader.Read(stream);

                _viewModel.RootType = new TypeViewModel()
                {
                    Name = reader.SerializedType.Name,
                    Assembly = reader.SerializedType.Assembly,
                    HashId = 0
                };

                // Data Nodes
                _viewModel.DataNodes.AddRange(reader.SerializedNodes);                     

                // Resolved Types
                foreach (var type in reader.ResolvedTypeTable)
                {
                    _viewModel.Types.Add(new TypeViewModel()
                    {
                        Assembly = type.Value.Assembly,
                        HashId = type.Key,
                        Missing = false,
                        Modified = false,
                        Name = type.Value.Name,
                        Resolved = true
                    });
                }

                // Missing Types
                foreach (var type in reader.MissingTypeTable)
                {
                    _viewModel.Types.Add(new TypeViewModel()
                    {
                        Assembly = type.Value.Assembly,
                        HashId = type.Key,
                        Missing = true,
                        Modified = false,
                        Name = type.Value.Name,
                        Resolved = false
                    });
                }

                // Type Specifications
                foreach (var type in _viewModel.Types)
                {
                    // Resolved
                    var resolved = reader.ResolvedSpecificationLookup.ContainsKey(type.HashId);
                    var missing = reader.MissingSpecificationLookup.ContainsKey(type.HashId);
                    var modified = reader.ModifiedSpecificationLookup.ContainsKey(type.HashId);

                    var spec = resolved ? reader.ResolvedSpecificationLookup[type.HashId] :
                               missing ? reader.MissingSpecificationLookup[type.HashId] :
                               modified ? reader.ModifiedSpecificationLookup[type.HashId] : null;

                    if (spec == null)
                        throw new Exception("Missing type specification!");

                    foreach (var property in spec.ResolvedDefinitions)
                    {
                        type.Properties.Add(new PropertyViewModel()
                        {
                            IsUserDefined = property.IsUserDefined,
                            Missing = false,
                            Extra = false,
                            PropertyName = property.PropertyName,

                            // TODO: TRACK HASH CODES TO THE VIEW MODEL
                            PropertyType = _viewModel.Types.FirstOrDefault(x => x.Assembly == property.Declaring.Assembly &&
                                                                                x.Name == property.Declaring.Name)
                        });
                    }

                    foreach (var property in spec.MissingDefinitions)
                    {
                        type.Properties.Add(new PropertyViewModel()
                        {
                            IsUserDefined = property.IsUserDefined,
                            Missing = true,
                            Extra = false,
                            PropertyName = property.PropertyName,

                            // TODO: TRACK HASH CODES TO THE VIEW MODEL
                            PropertyType = _viewModel.Types.FirstOrDefault(x => x.Assembly == property.Declaring.Assembly &&
                                                                                x.Name == property.Declaring.Name)
                        });
                    }

                    foreach (var property in spec.ExtraDefinitions)
                    {
                        type.Properties.Add(new PropertyViewModel()
                        {
                            IsUserDefined = property.IsUserDefined,
                            Missing = false,
                            Extra = true,
                            PropertyName = property.PropertyName,

                            // TODO: TRACK HASH CODES TO THE VIEW MODEL
                            PropertyType = _viewModel.Types.FirstOrDefault(x => x.Assembly == property.Declaring.Assembly &&
                                                                                x.Name == property.Declaring.Name)
                        });
                    }
                }
            }
        }

        private void ShowLoadedAssemblies()
        {
            _viewModel.LoadedAssemblies.Clear();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().OrderBy(x => x.GetName().FullName))
            {
                _viewModel.LoadedAssemblies.Add(new AssemblyItemViewModel()
                { 
                    AssemblyName = assembly.GetName().FullName,
                });
            }
        }

        private void OnOpenClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    OpenFile(openFileDialog.FileName);

                    this.StatusRun.Text = "File opened successfully!";
                }
                catch(Exception)
                {
                    this.StatusRun.Text = "Error opening file. Be sure to set directory to file assemblies!";
                }
            }
        }

        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void AssemblyDirButton_Click(object sender, RoutedEventArgs e)
        {
            var browser = new FolderBrowserDialog();

            if (browser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.AssemblyDirTB.Text = browser.SelectedPath;

                foreach (var fileName in Directory.EnumerateFiles(browser.SelectedPath))
                {
                    if (fileName.EndsWith(".exe") || fileName.EndsWith(".dll"))
                    {
                        try
                        {
                            // https://samcragg.wordpress.com/2017/06/30/resolving-assemblies-in-net-core/
                            //
                            var assemblyResolver = new AssemblyResolver(fileName);

                            foreach (var refAssembly in assemblyResolver.ReferencedAssemblies)
                            {
                                var refResolver = new AssemblyResolver(refAssembly.Location);
                            }

                            this.StatusRun.Text = "Assembly loaded:  " + assemblyResolver.Assembly?.FullName;
                        }
                        catch (Exception)
                        {
                            this.StatusRun.Text = "Error loading assembly:  " + fileName;
                        }
                    }
                }
            }

            ShowLoadedAssemblies();
        }
    }
}
