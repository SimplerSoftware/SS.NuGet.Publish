using Microsoft.VisualStudio.Editors.PropertyPages;
using Microsoft.VisualStudio.Shell;
using SS.NuGet.Publish.VisualStudio;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SS.NuGet.Publish
{
    [ComVisible(true)]
    [Guid("AA790F50-5208-41F5-89B2-D3733E12BE1D")]
    [ProvideObject(typeof(PublishPackagePageProvider))]
    public class PublishPackagePageProvider : PropPageBase
    {
        protected override Type ControlType
        {
            get
            {
                return typeof(PublishPackagePageControl);
            }
        }

        protected override string Title
        {
            get { return "Publish Package"; }
        }

        protected override Control CreateControl()
        {
            return new PublishPackagePageControl();
        }
    }
}
