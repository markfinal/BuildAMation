// <copyright file="Win32RegistryUtilities.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public static class Win32RegistryUtilities
    {
        private static string Wow6432NodeString = @"SOFTWARE\Wow6432Node";

        static Win32RegistryUtilities()
        {
            if (!OSUtilities.IsWindowsHosting)
            {
                return;
            }
        }

        public static bool Does32BitLMSoftwareKeyExist(string path)
        {
            if (!OSUtilities.IsWindowsHosting)
            {
                return false;
            }

            string keyPath;
            if (OSUtilities.Is64BitHosting)
            {
                keyPath = System.String.Format(@"{0}\{1}", Wow6432NodeString, path);
            }
            else
            {
                keyPath = System.String.Format(@"SOFTWARE\{0}", path);
            }

            bool exists = true;
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath);
            if (null == key)
            {
                exists = false;
            }
            else
            {
                key.Close();
            }

            return exists;
        }

        public static Microsoft.Win32.RegistryKey Open32BitLMSoftwareKey(string path)
        {
            if (!OSUtilities.IsWindowsHosting)
            {
                return null;
            }

            string keyPath;
            if (OSUtilities.Is64BitHosting)
            {
                keyPath = System.String.Format(@"{0}\{1}", Wow6432NodeString, path);
            }
            else
            {
                keyPath = System.String.Format(@"SOFTWARE\{0}", path);
            }
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath);
            if (null == key)
            {
                Log.DebugMessage("Registry key '{0}' on LocalMachine not found", keyPath);
            }
            return key;
        }

        public static bool DoesLMSoftwareKeyExist(string path)
        {
            if (!OSUtilities.IsWindowsHosting)
            {
                return false;
            }

            if (!OSUtilities.Is64BitHosting)
            {
                return false;
            }

            string keyPath = System.String.Format(@"SOFTWARE\{0}", path);

            bool exists = true;
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath);
            if (null == key)
            {
                exists = false;
            }
            else
            {
                key.Close();
            }

            return exists;
        }

        public static Microsoft.Win32.RegistryKey OpenLMSoftwareKey(string path)
        {
            if (!OSUtilities.IsWindowsHosting)
            {
                return null;
            }

            string keyPath = System.String.Format(@"SOFTWARE\{0}", path);
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath);
            if (null == key)
            {
                Log.DebugMessage("Registry key '{0}' on LocalMachine not found", keyPath);
                if (OSUtilities.Is64BitHosting)
                {
                    keyPath = System.String.Format(@"{0}\{1}", Wow6432NodeString, path);
                    key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyPath);
                    if (null == key)
                    {
                        Log.DebugMessage("On a 64-bit OS, registry key '{0}' on LocalMachine also not found", keyPath);
                    }
                }
            }
            return key;
        }
    }
}