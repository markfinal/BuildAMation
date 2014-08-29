#region License
// Copyright 2010-2014 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
namespace Bam.Core
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
