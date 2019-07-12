using System;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace SS.NuGet.Publish.VisualStudio.AddIn.Commands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class NuGetPublishCfg
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("7d30d4d2-a73f-4bc5-93d4-21111ae37694");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="NuGetPublishCfg"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private NuGetPublishCfg(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(Execute, null, OnBeforeQueryStatusDynamicItem, menuCommandID);
            //var menuItem = new DynamicItemMenuCommand(menuCommandID, IsValidDynamicItem, Execute, OnBeforeQueryStatusDynamicItem);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static NuGetPublishCfg Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in NuGetPublishCfg's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new NuGetPublishCfg(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        [SuppressMessage("Await.Warning", "CS4014:Await.Warning")]
        [SuppressMessage("Await.Warning", "VSTHRD110:Await.Warning")]
        private void Execute(object sender, EventArgs e)
        {
            ExecuteAsync();
        }

        private async Task ExecuteAsync()
        {
            var project = await Project.GetActiveProjectAsync();
            var projectType = await project.GetProjectTypeAsync();
            if (projectType == ProjectType.Other)
            {
                VsShellUtilities.ShowMessageBox(
                    package,
                    "Extension currently works for Xamarin.Android, Xamarin.iOS and UWP projects",
                    Vsix.Name,
                    OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                return;
            }

            string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            string title = "NuGet Publish Settings";

            // Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox(
                this.package,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        private void OnBeforeQueryStatusDynamicItem(object sender, EventArgs args)
        {
            var pType = ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                var p = await Project.GetActiveProjectAsync();
                return await p.GetProjectTypeAsync();
            });
            
            
            DynamicItemMenuCommand matchedCommand = (DynamicItemMenuCommand)sender;
            matchedCommand.Enabled = pType == ProjectType.CPS;
            //matchedCommand.Visible = pType == ProjectType.CPS;
            return;

            //// Find out whether the command ID is 0, which is the ID of the root item.
            //// If it is the root item, it matches the constructed DynamicItemMenuCommand,
            //// and IsValidDynamicItem won't be called.
            bool isRootItem = (matchedCommand.MatchedCommandId == 0);

            // The index is set to 1 rather than 0 because the Solution.Projects collection is 1-based.
            int indexForDisplay = (isRootItem ? 1 : (matchedCommand.MatchedCommandId - (int)NuGetPublishCfg.CommandId) + 1);

            //matchedCommand.Text = dte2.Solution.Projects.Item(indexForDisplay).Name;

            //Array startupProjects = (Array)dte2.Solution.SolutionBuild.StartupProjects;
            //string startupProject = System.IO.Path.GetFileNameWithoutExtension((string)startupProjects.GetValue(0));

            //// Check the command if it isn't checked already selected
            //matchedCommand.Checked = (matchedCommand.Text == startupProject);

            //// Clear the ID because we are done with this item.
            //matchedCommand.MatchedCommandId = 0;
        }

        private bool IsValidDynamicItem(int commandId)
        {
            // The match is valid if the command ID is >= the id of our root dynamic start item
            // and the command ID minus the ID of our root dynamic start item
            // is less than or equal to the number of projects in the solution.
            return (commandId >= (int)NuGetPublishCfg.CommandId);// && ((commandId - (int)NuGetPublishCfg.CommandId) < dte2.Solution.Projects.Count);
        }
    }
}
