#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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
