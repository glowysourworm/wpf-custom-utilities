using System;
using System.Windows.Controls;
using System.Windows.Media;

using WpfCustomUtilities.RecursiveSerializer.Shared.Data;

namespace WpfCustomUtilities.RecursiveSerializer.Compare.View
{
    public class DataNodeTextBlock : TextBlock
    {
        readonly static Brush _nullBackground;
        readonly static Brush _nullPrimitiveBackground;
        readonly static Brush _primitiveBackground;
        readonly static Brush _objectBackground;
        readonly static Brush _referenceBackground;
        readonly static Brush _collectionBackground;

        static DataNodeTextBlock()
        {
            _nullBackground = new SolidColorBrush(Color.FromScRgb(0.3f, Colors.Gray.ScR, Colors.Gray.ScG, Colors.Gray.ScB));
            _nullPrimitiveBackground = new SolidColorBrush(Color.FromScRgb(0.3f, Colors.LightGray.ScR, Colors.LightGray.ScG, Colors.LightGray.ScB));
            _primitiveBackground = new SolidColorBrush(Color.FromScRgb(0.3f, Colors.LightCoral.ScR, Colors.LightCoral.ScG, Colors.LightCoral.ScB));
            _objectBackground = new SolidColorBrush(Color.FromScRgb(0.3f, Colors.Blue.ScR, Colors.Blue.ScG, Colors.Blue.ScB));
            _referenceBackground = new SolidColorBrush(Color.FromScRgb(0.3f, Colors.Magenta.ScR, Colors.Magenta.ScG, Colors.Magenta.ScB));
            _collectionBackground = new SolidColorBrush(Color.FromScRgb(0.3f, Colors.Maroon.ScR, Colors.Maroon.ScG, Colors.Maroon.ScB));
        }

        public DataNodeTextBlock()
        {
            this.DataContextChanged += (sender, e) =>
            {
                if (e.NewValue != null &&
                    e.NewValue is SerializedNode)
                {
                    var viewModel = (SerializedNode)e.NewValue;
                    var header = "";
                    var dataText = "";

                    // Marked Internal (see enum specification)
                    switch (viewModel.NodeType)
                    {
                        case 0:
                            header = "Null Node";
                            dataText = "(null)";

                            this.Background = _nullBackground;
                            break;
                        case 1:
                            header = "Null Primitive";
                            dataText = "(null primitive)";

                            this.Background = _nullPrimitiveBackground;
                            break;
                        case 2:
                            header = "Primitive";
                            dataText = string.Format("Value={0}", viewModel.PrimitiveValue.ToString());

                            this.Background = _primitiveBackground;
                            break;
                        case 3:
                            header = "Object";
                            dataText = string.Format("TypeId={0} Id={1}", viewModel.TypeHashCode.ToString(), viewModel.ObjectId.ToString());

                            this.Background = _objectBackground;
                            break;
                        case 4:
                            header = "Reference";
                            dataText = string.Format("TypeId={0} Id={1}", viewModel.TypeHashCode.ToString(), viewModel.ReferenceId.ToString());

                            this.Background = _referenceBackground;
                            break;
                        case 5:
                            header = "Collection";
                            dataText = string.Format("TypeId={0} Id={1} Count={2} ElementTypeId={3}",
                                                     viewModel.TypeHashCode.ToString(),
                                                     viewModel.ObjectId.ToString(),
                                                     viewModel.CollectionCount.ToString(),
                                                     viewModel.CollectionElementTypeHashCode.ToString());

                            this.Background = _collectionBackground;
                            break;
                        default:
                            throw new Exception("Unhandled node type:  DataNodeTextBlock.cs");
                    }

                    this.Text = string.Format("{0} ({1}) HashId={2} {3}", header, viewModel.Mode == 0 ? "Auto" : "User-Defined", viewModel.TypeHashCode, dataText);
                }
            };
        }
    }
}
