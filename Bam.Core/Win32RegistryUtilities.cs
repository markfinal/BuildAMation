#region License
// Copyright (c) 2010-2018, Mark Final
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
    /// <summary>
    /// Static utility class for all Windows registry methods.
    /// </summary>
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

        /// <summary>
        /// Query if the current user software key at the provided path exists?
        /// </summary>
        /// <returns><c>true</c>, if CU software key exist was doesed, <c>false</c> otherwise.</returns>
        /// <param name="path">Path.</param>
        public static bool
        DoesCUSoftwareKeyExist(
            string path)
        {
            var exists = DoesSoftwareKeyExist(path, Microsoft.Win32.Registry.CurrentUser, false);
            return exists;
        }

        /// <summary>
        /// Does the 32-bit local machine software key at the provided path exists?
        /// </summary>
        /// <returns><c>true</c>, if bit LM software key exist was does32ed, <c>false</c> otherwise.</returns>
        /// <param name="path">Path.</param>
        public static bool
        Does32BitLMSoftwareKeyExist(
            string path)
        {
            var exists = DoesSoftwareKeyExist(path, Microsoft.Win32.Registry.LocalMachine, true);
            return exists;
        }

        public class RegKey :
            System.IDisposable
        {
            private Microsoft.Win32.RegistryKey key;

            public RegKey(
                Microsoft.Win32.RegistryKey sourceKey)
            {
                if (null == sourceKey)
                {
                    throw new Exception(
                        "Registry key cannot be null"
                    );
                }
                this.key = sourceKey;
            }

            void System.IDisposable.Dispose()
            {
                this.key.Dispose();
            }

            public string Name
            {
                get
                {
                    return this.key.Name;
                }
            }

            public string
            GetStringValue(
                string name)
            {
                var value = this.key.GetValue(name);
                if (null == value)
                {
                    throw new Exception(
                        "Value '{0}' does not exist for the key",
                        name
                    );
                }
                if (!(value is string))
                {
                    throw new Exception(
                        "Registry key value is not a string"
                    );
                }
                return value as string;
            }

            public string
            FindStringValue(
                string name)
            {
                var value = this.key.GetValue(name);
                if (null == value)
                {
                    return null;
                }
                if (!(value is string))
                {
                    throw new Exception(
                        "Registry key value is not a string"
                    );
                }
                return value as string;
            }

            public System.Collections.Generic.IEnumerable<RegKey> SubKeys
            {
                get
                {
                    foreach (var name in this.key.GetSubKeyNames())
                    {
                        using (var subkey = this.key.OpenSubKey(name))
                        {
                            yield return new RegKey(subkey);
                        }
                    }
                }
            }
        }

        private static RegKey
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
                return null;
            }
            return new RegKey(key);
        }

        /// <summary>
        /// Open the 32-bit local machine software key specified by the path.
        /// </summary>
        /// <returns>The bit LM software key.</returns>
        /// <param name="path">Path.</param>
        public static RegKey
        Open32BitLMSoftwareKey(
            string path)
        {
            return OpenSoftwareKey(path, Microsoft.Win32.Registry.LocalMachine, true);
        }

        /// <summary>
        /// Open the current user software key specified by the path.
        /// </summary>
        /// <returns>The CU software key.</returns>
        /// <param name="path">Path.</param>
        public static RegKey
        OpenCUSoftwareKey(
            string path)
        {
            return OpenSoftwareKey(path, Microsoft.Win32.Registry.CurrentUser, false);
        }

        /// <summary>
        /// Does the local machine software key exist at the path?
        /// </summary>
        /// <returns><c>true</c>, if LM software key exist was doesed, <c>false</c> otherwise.</returns>
        /// <param name="path">Path.</param>
        public static bool
        DoesLMSoftwareKeyExist(
            string path)
        {
            var exists = DoesSoftwareKeyExist(path, Microsoft.Win32.Registry.LocalMachine, false);
            return exists;
        }

        /// <summary>
        /// Open the local machine software key at the path.
        /// </summary>
        /// <returns>The LM software key.</returns>
        /// <param name="path">Path.</param>
        public static RegKey
        OpenLMSoftwareKey(
            string path)
        {
            return OpenSoftwareKey(path, Microsoft.Win32.Registry.LocalMachine, false);
        }
    }
}
