
using System.Collections.ObjectModel;

using WpfCustomUtilities.Extensions;

namespace WpfCustomUtilities.RecursiveSerializer.Compare.ViewModel
{
    public class TypeViewModel : ViewModelBase
    {
        string _name;
        string _assembly;
        bool _resolved;
        bool _missing;
        bool _modified;
        int _hashId;

        public string Name
        {
            get { return _name; }
            set { this.RaiseAndSetIfChanged(ref _name, value); }
        }
        public string Assembly
        {
            get { return _assembly; }
            set { this.RaiseAndSetIfChanged(ref _assembly, value); }
        }
        public bool Resolved
        {
            get { return _resolved; }
            set { this.RaiseAndSetIfChanged(ref _resolved, value); }
        }
        public bool Missing
        {
            get { return _missing; }
            set { this.RaiseAndSetIfChanged(ref _missing, value); }
        }
        public bool Modified
        {
            get { return _modified; }
            set { this.RaiseAndSetIfChanged(ref _modified, value); }
        }
        public int HashId
        {
            get { return _hashId; }
            set { this.RaiseAndSetIfChanged(ref _hashId, value); }
        }

        public ObservableCollection<PropertyViewModel> Properties { get; set; }

        public TypeViewModel()
        {
            this.Properties = new ObservableCollection<PropertyViewModel>();
        }
    }
}
