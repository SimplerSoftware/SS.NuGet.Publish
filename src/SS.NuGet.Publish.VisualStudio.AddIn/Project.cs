using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Properties;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Flavor;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SS.NuGet.Publish.VisualStudio.AddIn
{
    public class Project
    {
        private readonly EnvDTE.Project _vsProject;

        public Project(EnvDTE.Project project)
        {
            this._vsProject = project;
        }

        public string GetRootDirectory()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return Path.GetDirectoryName(this._vsProject.FileName);
        }

        public void AddFile(string filename, string type)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var file = this._vsProject.ProjectItems.AddFromFile(filename);
            file.Properties.Item("ItemType").Value = type;
        }

        public void Save()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            this._vsProject.Save();
        }

        public async Task<ProjectType> GetProjectTypeAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            var solution = await ServiceLocator.GetGlobalServiceAsync<SVsSolution, IVsSolution>(cancellationToken);
            if (solution == null) return ProjectType.Other;
            var _Configuration = "Debug";
            var _Platform = "Any CPU";

            solution.GetProjectOfUniqueName(this._vsProject.UniqueName, out IVsHierarchy hierarchy);
            if (IsCpsProject(hierarchy))
            {
                /*
                //Microsoft.VisualStudio.ProjectSystem.VS.Implementation.Package.Automation.OAProject
                UnconfiguredProject unconfiguredProject = GetUnconfiguredProject(this._vsProject); // obtained from above
                //var configuredProject = await unconfiguredProject.GetSuggestedConfiguredProjectAsync();
                //var pCfg = unconfiguredProject.LoadedConfiguredProjects.Where(c => c.ProjectConfiguration.Name == "Release|AnyCPU").SingleOrDefault().ProjectConfiguration;
                var d = System.Collections.Immutable.ImmutableDictionary<string, string>.Empty;
                d = d.Add("Configuration", _Configuration);
                d = d.Add("Platform", _Platform);
                var configuredProject = await unconfiguredProject.LoadConfiguredProjectAsync($"{_Configuration}|{_Platform}", d);
                //Debug|AnyCPU
                //unconfiguredProject.LoadedConfiguredProjects.FirstOrDefault().Services.ProjectService.Services.ProjectLockService;
                IProjectLockService projectLockService = unconfiguredProject.ProjectService.Services.ProjectLockService;
                using (var access = await projectLockService.WriteLockAsync(cancellationToken))
                {
                    //configuredProject.Services.
                    var project = await access.GetProjectAsync(configuredProject, cancellationToken);

                    // Use the msbuild project, respecting the type of lock acquired.

                    // If you're going to change the project in any way, 
                    // check it out from SCC first:
                    await access.CheckoutAsync(configuredProject.UnconfiguredProject.FullPath);
                    $"Imports: {project.Imports.Count}".Ouptut();
                    $"Targets: {project.Targets.Count}".Ouptut();
                    $"Properties: {project.Properties.Count}".Ouptut();
                    foreach (var prop in project.Properties.Where(p => p.Name.StartsWith("NuGetPublish")))
                    {
                        $"{prop.Name}: [{prop.UnevaluatedValue}] = [{prop.EvaluatedValue}]".Ouptut();
                        $"\tIsEnvironmentProperty: [{prop.IsEnvironmentProperty}]".Ouptut();
                        $"\tIsGlobalProperty: [{prop.IsGlobalProperty}]".Ouptut();
                        $"\tIsImported: [{prop.IsImported}]".Ouptut();
                        $"\tIsReservedProperty: [{prop.IsReservedProperty}]".Ouptut();
                        $"\tPredecessor: [{prop.Predecessor == null}]".Ouptut();
                    }
                    //project.SetProperty($"NuGetPublishLocation", @"D:\References\Packages\");
                }
                    VsHierarchy hierarchy1 = null;
                    var sol = Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution;
                    sol.GetProjectOfUniqueName(project.UniqueName, out hierarchy1);

                    IVsSolutionBuildManager bm = Package.GetGlobalService(typeof(IVsSolutionBuildManager)) as IVsSolutionBuildManager;

                    IVsProjectCfg[] cfgs = new IVsProjectCfg[1];
                    bm.FindActiveProjectCfg(IntPtr.Zero, IntPtr.Zero, hierarchy1, cfgs);

                    IVsCfg cfg = cfgs[0] as IVsCfg;
                 */
                return ProjectType.CPS;
            }
            //var proStor = hierarchy as IVsBuildPropertyStorage;
            //var result1 = proStor.GetPropertyValue("NuGetPublishVersion", $"{_Configuration}|{_Platform}", (int)_PersistStorageType.PST_PROJECT_FILE, out string propValue);
            //$"result1: {result1}".Ouptut();
            //$"propValue: {propValue}".Ouptut();
            //var result2 = proStor.SetPropertyValue("NuGetPublishLocation", $"{_Configuration}|{_Platform}", (int)_PersistStorageType.PST_PROJECT_FILE, @"D:\References\Packages\");
            //$"result2: {result2}".Ouptut();

            if (hierarchy == null) return ProjectType.Other;

            var ap = hierarchy as IVsAggregatableProjectCorrected;
            if (ap == null) return ProjectType.Other;

            string projectTypeGuids;
            ap.GetAggregateProjectTypeGuids(out projectTypeGuids);
            if (string.IsNullOrEmpty(projectTypeGuids)) return ProjectType.Other;

            // check if UWP project
            //if (projectTypeGuids.Contains("{A5A43C5B-DE2A-4C0C-9213-0A381AF9435A}")) return ProjectType.UWP;
            // check if android project
            //if (projectTypeGuids.Contains("{EFBA0AD7-5A72-4C68-AF49-83D382785DCF}")) return ProjectType.XamarinAndroid;
            // check if iOS project
            //if (projectTypeGuids.Contains("{FEACFBD2-3405-455C-9665-78FE426C6842}")) return ProjectType.XamariniOS;

            return ProjectType.Other;
        }

        internal bool IsCpsProject(IVsHierarchy hierarchy)
        {
            Microsoft.Requires.NotNull(hierarchy, nameof(hierarchy));
            return hierarchy.IsCapabilityMatch("CPS");
        }
        private UnconfiguredProject GetUnconfiguredProject(IVsProject project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IVsBrowseObjectContext context = project as IVsBrowseObjectContext;
            if (context == null)
            {
                // VC implements this on their DTE.Project.Object
                if (project is IVsHierarchy hierarchy)
                {
                    if (ErrorHandler.Succeeded(hierarchy.GetProperty((uint)VSConstants.VSITEMID.Root, (int)__VSHPROPID.VSHPROPID_ExtObject, out object extObject)))
                    {
                        if (extObject is EnvDTE.Project dteProject)
                        {
                            context = dteProject.Object as IVsBrowseObjectContext;
                        }
                    }
                }
            }

            return context?.UnconfiguredProject;
        }

        private UnconfiguredProject GetUnconfiguredProject(EnvDTE.Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IVsBrowseObjectContext context = project as IVsBrowseObjectContext;
            if (context == null && project != null)
            { 
                // VC implements this on their DTE.Project.Object
                context = project.Object as IVsBrowseObjectContext;
            }

            return context?.UnconfiguredProject;
        }

        public static async Task<Project> GetActiveProjectAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var dteService = await ServiceLocator.GetGlobalServiceAsync<DTE, DTE>(cancellationToken);
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            var vsProject = ((object[])dteService.ActiveSolutionProjects)
                .Select(x => {
                    ThreadHelper.ThrowIfNotOnUIThread(nameof(GetActiveProjectAsync));
                    return ((EnvDTE.Project)x);
                })
                .FirstOrDefault();

            return vsProject == null ? null : new Project(vsProject);
        }
    }

}
