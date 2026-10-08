using System.ComponentModel.Composition;
using System.Windows;

namespace NINA.Point3d.MotorPosition {

    [Export(typeof(ResourceDictionary))]
    public partial class Options : ResourceDictionary {
        public Options() {
            InitializeComponent();
        }
    }
}
