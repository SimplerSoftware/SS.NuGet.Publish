using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VsServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace SS.NuGet.Publish.VisualStudio.AddIn
{
    internal static class ServiceLocator
    {
        public static void InitializePackageServiceProvider(IServiceProvider provider)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            PackageServiceProvider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public static IServiceProvider PackageServiceProvider { get; private set; }


        //public static TInterface GetGlobalService<TService, TInterface>() where TInterface : class
        //{
        //    return ThreadHelper.JoinableTaskFactory.Run<TInterface>(GetGlobalServiceAsync<TService, TInterface>);
        //}

        internal static async Task<TInterface> GetGlobalServiceAsync<TService, TInterface>(CancellationToken cancellationToken = default(CancellationToken)) where TInterface : class
        {
            // VS Threading Rule #1
            // Access to ServiceProvider and a lot of casts are performed in this method,
            // and so this method can RPC into main thread. Switch to main thread explicitly, since method has STA requirement
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            if (PackageServiceProvider != null)
            {
                var result = PackageServiceProvider.GetService(typeof(TService));
                if (result is TInterface service)
                    return service;
            }

            return Package.GetGlobalService(typeof(TService)) as TInterface;
        }

        private static async Task<TService> GetDTEServiceAsync<TService>(CancellationToken cancellationToken = default(CancellationToken)) where TService : class
        {
#if DEBUG
            var access = ThreadHelper.CheckAccess();
            Debug.Assert(access);
#endif
            var dte = await GetGlobalServiceAsync<SDTE, DTE>(cancellationToken);
            return dte != null ? QueryService(dte, typeof(TService)) as TService : null;
        }

        private static async Task<IServiceProvider> GetServiceProviderAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
#if DEBUG
            var access = ThreadHelper.CheckAccess();
            Debug.Assert(access);
#endif
            var dte = await GetGlobalServiceAsync<SDTE, DTE>(cancellationToken);
            return GetServiceProviderFromDTE(dte);
        }

        private static object QueryService(_DTE dte, Type serviceType)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
#if DEBUG
            var access = ThreadHelper.CheckAccess();
            Debug.Assert(access);
#endif
            Guid guidService = serviceType.GUID;
            Guid riid = guidService;
            var serviceProvider = dte as VsServiceProvider;

            IntPtr servicePtr;
            int hr = serviceProvider.QueryService(ref guidService, ref riid, out servicePtr);

            if (hr != 0/*NuGetVSConstants.S_OK*/)
            {
                // We didn't find the service so return null
                return null;
            }

            object service = null;

            if (servicePtr != IntPtr.Zero)
            {
                service = Marshal.GetObjectForIUnknown(servicePtr);
                Marshal.Release(servicePtr);
            }

            return service;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The caller is responsible for disposing this")]
        private static IServiceProvider GetServiceProviderFromDTE(_DTE dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
#if DEBUG
            var access = ThreadHelper.CheckAccess();
            Debug.Assert(access);
#endif
            IServiceProvider serviceProvider = new ServiceProvider(dte as VsServiceProvider);
            Debug.Assert(serviceProvider != null, "Service provider is null");
            return serviceProvider;
        }
    }
}
