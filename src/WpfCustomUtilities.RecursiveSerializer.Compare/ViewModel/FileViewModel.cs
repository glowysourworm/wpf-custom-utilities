using System.Collections.ObjectModel;

using WpfCustomUtilities.Extensions;
using WpfCustomUtilities.RecursiveSerializer.Shared.Data;

namespace WpfCustomUtilities.RecursiveSerializer.Compare.ViewModel
{
    public class FileViewModel : ViewModelBase
    {
        TypeViewModel _rootType;

        public TypeViewModel RootType
        {
            get { return _rootType; }
            set { this.RaiseAndSetIfChanged(ref _rootType, value); }
        }

        public ObservableCollection<TypeViewModel> Types { get; set; }
        public ObservableCollection<SerializedNode> DataNodes { get; set; } 
        public ObservableCollection<AssemblyItemViewModel> LoadedAssemblies { get; set; }

        public FileViewModel()
        {
            this.Types = new ObservableCollection<TypeViewModel>();
            this.DataNodes = new ObservableCollection<SerializedNode>();
            this.LoadedAssemblies = new ObservableCollection<AssemblyItemViewModel>();
        }
    }
}
