using System.Collections.Generic;
using System.Collections.ObjectModel;

using WpfCustomUtilities.Extensions.ObservableCollection.Interface;

namespace WpfCustomUtilities.Extensions.ObservableCollection
{
    /// <summary>
    /// Observable collection that checks for specific ISelectableViewModel interface and allows just a single
    /// item to be selected at once.
    /// </summary>
    public class SelectableObservableCollection<T> : ObservableCollection<T> where T : ISelectableViewModel
    {
        bool _isUpdating = false;

        public SelectableObservableCollection()
        {

        }

        public SelectableObservableCollection(IEnumerable<T> collection) : base(collection)
        {
            Hook();
        }

        public SelectableObservableCollection(List<T> list) : base(list)
        {
            Hook();
        }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);

            item.IsSelectedChanged += OnSelectedChanged;
        }

        protected override void RemoveItem(int index)
        {
            this[index].IsSelectedChanged -= OnSelectedChanged;

            base.RemoveItem(index);
        }

        protected override void ClearItems()
        {
            Unhook();

            base.ClearItems();
        }

        private void Unhook()
        {
            foreach (var item in this)
                item.IsSelectedChanged -= OnSelectedChanged;
        }

        private void Hook()
        {
            foreach (var item in this)
                item.IsSelectedChanged += OnSelectedChanged;
        }

        private void OnSelectedChanged(object sender)
        {
            if (_isUpdating)
                return;

            _isUpdating = true;

            foreach (var item in this)
                item.IsSelected = ReferenceEquals(item, sender);

            _isUpdating = false;
        }
    }
}
