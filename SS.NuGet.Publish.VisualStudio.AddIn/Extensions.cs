using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.NuGet.Publish.VisualStudio.AddIn
{
    static class Extensions
    {
        public static void Ouptut(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            var outputText = new StringBuilder();
            outputText.Append(text);

            if (!text.EndsWith(Environment.NewLine))
                outputText.Append(Environment.NewLine);

            if (!(Package.GetGlobalService(typeof(SVsOutputWindow)) is IVsOutputWindow outWindow))
                return;
            var generalPaneGuid = VSConstants.GUID_OutWindowDebugPane;
            outWindow.GetPane(ref generalPaneGuid, out IVsOutputWindowPane generalPane);
            generalPane?.OutputString($"{outputText}");
            generalPane?.Activate();
        }
        public static Guid? GetGuidProperty(this IVsHierarchy hierarchy, uint itemId, int propId)
        {
            if (hierarchy == null)
                return null;

            if (hierarchy.GetGuidProperty(itemId, propId, out Guid guid) != 0)
                return null;

            return guid;
        }


    }
}
