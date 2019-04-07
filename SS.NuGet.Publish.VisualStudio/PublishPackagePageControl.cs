using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.Editors.PropertyPages;

namespace SS.NuGet.Publish.VisualStudio
{
    public partial class PublishPackagePageControl :
#if DESIGN_TIME
        PropPageUserControlBase
#else
        UserControl
#endif
    {
        public PublishPackagePageControl()
        {
            InitializeComponent();
        }
    }
}
