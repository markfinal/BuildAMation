// <copyright file="Win32RegistryUtilities.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public static class Win32RegistryUtilities
    {
        private static string Wow6432NodeString = @"Wow6432Node";

        // TODO: what's the point of this? I can see no side-effects.
        // it could throw an exception if ever instantiated on any other platform if that was the intention?
        static
        Win32RegistryUtilities()
        {
            if (!OSUtilities.IsWindowsHosting)
            {
                return;
            }
        }

        private static string
        SoftwareKeyPath(
            string path,
            bool query32Bit)
        {
            var keyPath = new System.Text.StringBuilder();
            if (OSUtilities.Is64BitHosting)
            {
                if (query32Bit)
                {
                    keyPath.AppendFormat(@"Software\{0}\{1}", Wow6432NodeString, path);
                }
                else
                {
                    keyPath.AppendFormat(@"Software\{0}", path);
                }
            }
            else
            {
                keyPath.AppendFormat(@"Software\{0}", path);
            }

            return keyPath.ToString();
        }

        private static bool
        DoesSoftwareKeyExist(
            string path,
            Microsoft.Win32.RegistryKey registryArea,
            bool query32Bit)
        {
            if (!OSUtilities.IsWindowsHosting)
            {
                return false;
            }

            var exists = true;
            using (var key = registryArea.OpenSubKey(SoftwareKeyPath(path, query32Bit)))
            {
                if (null == key)
                {
                    exists = false;
                }
            }

            return exists;
        }

        public static bool
        DoesCUSoftwareKeyExist(
            string path)
        {
            var exists = DoesSoftwareKeyExist(path, Microsoft.Win32.Registry.CurrentUser, false);
            return exists;
        }

        public static bool
        Does32BitLMSoftwareKeyExist(
            string path)
        {
            var exists = DoesSoftwareKeyExist(path, Microsoft.Win32.Registry.LocalMachine, true);
            return exists;
        }

        private static Microsoft.Win32.RegistryKey
        OpenSoftwareKey(
            string path,
            Microsoft.Win32.RegistryKey registryArea,
            bool query32Bit)
        {
            if (!OSUtilities.IsWindowsHosting)
            {
                return null;
            }

            var keyPath = SoftwareKeyPath(path, query32Bit);
            var key = registryArea.OpenSubKey(keyPath);
            if (null == key)
            {
                Log.DebugMessage("Registry key '{0}' on {1} not found", keyPath, registryArea.Name);
            }
            return key;
        }

        public static Microsoft.Win32.RegistryKey
        Open32BitLMSoftwareKey(
            string path)
        {
            return OpenSoftwareKey(path, Microsoft.Win32.Registry.LocalMachine, true);
        }

        public static Microsoft.Win32.RegistryKey
        OpenCUSoftwareKey(
            string path)
        {
            return OpenSoftwareKey(path, Microsoft.Win32.Registry.CurrentUser, false);
        }

        public static bool
        DoesLMSoftwareKeyExist(
            string path)
        {
            var exists = DoesSoftwareKeyExist(path, Microsoft.Win32.Registry.LocalMachine, false);
            return exists;
        }

        public static Microsoft.Win32.RegistryKey
        OpenLMSoftwareKey(
            string path)
        {
            return OpenSoftwareKey(path, Microsoft.Win32.Registry.LocalMachine, false);
        }
    }
}
