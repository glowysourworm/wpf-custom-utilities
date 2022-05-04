using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WpfCustomUtilities.Extensions;

namespace WpfCustomUtilities.RecursiveSerializer.Compare.ViewModel
{
    public class PropertyViewModel : ViewModelBase
    {
        string _propertyName;
        bool _isUserDefined;
        bool _missing;
        bool _extra;
        TypeViewModel _propertyType;

        public string PropertyName
        {
            get { return _propertyName; }
            set { this.RaiseAndSetIfChanged(ref _propertyName, value); }
        }
        public bool IsUserDefined
        {
            get { return _isUserDefined; }
            set { this.RaiseAndSetIfChanged(ref _isUserDefined, value); }
        }
        public bool Missing
        {
            get { return _missing; }
            set { this.RaiseAndSetIfChanged(ref _missing, value); }
        }
        public bool Extra
        {
            get { return _extra; }
            set { this.RaiseAndSetIfChanged(ref _extra, value); }
        }
        public TypeViewModel PropertyType
        {
            get { return _propertyType; }
            set { this.RaiseAndSetIfChanged(ref _propertyType, value); }
        }

        public PropertyViewModel()
        {

        }
    }
}
