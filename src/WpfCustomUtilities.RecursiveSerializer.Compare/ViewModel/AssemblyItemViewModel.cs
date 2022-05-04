using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WpfCustomUtilities.Extensions;

namespace WpfCustomUtilities.RecursiveSerializer.Compare.ViewModel
{
    public class AssemblyItemViewModel : ViewModelBase
    {
        string _assemblyName;
        bool _isIncluded;

        public string AssemblyName
        {
            get { return _assemblyName; }
            set { this.RaiseAndSetIfChanged(ref _assemblyName, value); }
        }
        public bool IsIncluded
        {
            get { return _isIncluded; }
            set { this.RaiseAndSetIfChanged(ref _isIncluded, value); }
        }

        public AssemblyItemViewModel()
        {

        }
    }
}
