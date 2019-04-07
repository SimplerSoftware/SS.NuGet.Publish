using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SS.NuGet.Publish.VisualStudio
{
    class Program
    {
        static void Main(string[] args)
        {
            Register(typeof(PublishPackagePageProvider));
        }

        static void Register(Type type)
        {
            string ProgID = type.FullName;
            string Version = type.Assembly.GetName().Version.ToString();
            string GUIDstr = "{" + type.GUID.ToString() + "}";
            string keyPath = @"Software\Classes\";


            RegistryKey regularx86View = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32);

            RegistryKey regularx64View = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);

            RegistryKey[] keys = {regularx86View.OpenSubKey(keyPath, RegistryKeyPermissionCheck.ReadWriteSubTree, System.Security.AccessControl.RegistryRights.FullControl),
                    regularx64View.OpenSubKey(keyPath, RegistryKeyPermissionCheck.ReadWriteSubTree, System.Security.AccessControl.RegistryRights.FullControl)};


            ProgIdAttribute[] attributes = (ProgIdAttribute[])type.GetCustomAttributes(typeof(ProgIdAttribute), false);

            if (attributes.Length > 0)
                ProgID = attributes[0].Value;

            foreach (RegistryKey RootKey in keys)
            {
                //[HKEY_CURRENT_USER\Software\Classes\Prog.ID]
                //@="Namespace.Class"

                RegistryKey keyProgID = RootKey.CreateSubKey(ProgID);
                keyProgID.SetValue(null, type.FullName);

                //[HKEY_CURRENT_USER\Software\Classes\Prog.ID\CLSID]
                //@="{XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX}"
                keyProgID.CreateSubKey(@"CLSID").SetValue(null, GUIDstr);


                //[HKEY_CURRENT_USER\Software\Classes\CLSID\{XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX}]
                //@="Namespace.Class
                //
                RegistryKey keyCLSID = RootKey.OpenSubKey(@"CLSID", RegistryKeyPermissionCheck.ReadWriteSubTree, System.Security.AccessControl.RegistryRights.FullControl).CreateSubKey(GUIDstr);
                keyCLSID.SetValue(null, type.FullName);


                //[HKEY_CURRENT_USER\Software\Classes\CLSID\{XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX}\ProgId]
                //@="Prog.ID"
                keyCLSID.CreateSubKey("ProgId").SetValue(null, ProgID);


                //[HKEY_CURRENT_USER\Software\Classes\CLSID\{XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX}\InprocServer32]
                //@="mscoree.dll"
                //"ThreadingModel"="Both"
                //"Class"="Namespace.Class"
                //"Assembly"="AssemblyName, Version=1.0.0.0, Culture=neutral, PublicKeyToken=71c72075855a359a"
                //"RuntimeVersion"="v4.0.30319"
                //"CodeBase"="file:///Drive:/Full/Image/Path/file.dll"
                RegistryKey InprocServer32 = keyCLSID.CreateSubKey("InprocServer32");
                SetInprocServer(InprocServer32, type, false);

                //[HKEY_CURRENT_USER\Software\Classes\CLSID\{XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX}\InprocServer32\1.0.0.0]
                //"Class"="Namespace.Class"
                //"Assembly"="AssemblyName, Version=1.0.0.0, Culture=neutral, PublicKeyToken=71c72075855a359a"
                //"RuntimeVersion"="v4.0.30319"
                //"CodeBase"="file:///Drive:/Full/Image/Path/file.dll"
                SetInprocServer(InprocServer32.CreateSubKey("Version"), type, true);

                //[HKEY_CURRENT_USER\Software\Classes\CLSID\{XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX}\Implemented Categories\{62C8FE65-4EBB-45E7-B440-6E39B2CDBF29}]
                keyCLSID.CreateSubKey(@"Implemented Categories\{62C8FE65-4EBB-45E7-B440-6E39B2CDBF29}");

                keyCLSID.Close();
            }

        }

        static void SetInprocServer(RegistryKey key, Type type, bool versionNode)
        {

            if (!versionNode)
            {
                key.SetValue(null, "mscoree.dll");
                key.SetValue("ThreadingModel", "Both");
            }

            key.SetValue("Class", type.FullName);
            key.SetValue("Assembly", type.Assembly.FullName);
            key.SetValue("RuntimeVersion", type.Assembly.ImageRuntimeVersion);
            key.SetValue("CodeBase", type.Assembly.CodeBase);
        }
    }
}
