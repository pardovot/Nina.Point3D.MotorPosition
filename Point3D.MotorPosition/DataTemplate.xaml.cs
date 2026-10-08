using System.ComponentModel.Composition;
using System.Windows;

namespace NINA.Point3d.MotorPosition {

    [Export(typeof(ResourceDictionary))]
    public partial class DataTemplate : ResourceDictionary {
        public DataTemplate() {
            InitializeComponent();
        }
    }
}
