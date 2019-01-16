#region License
// Copyright (c) 2010-2019, Mark Final
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
        private static readonly string Wow6432NodeString = @"Wow6432Node";

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
                    keyPath.Append($@"Software\{Wow6432NodeString}\{path}");
                }
                else
                {
                    keyPath.Append($@"Software\{path}");
                }
            }
            else
            {
                keyPath.Append($@"Software\{path}");
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
            string path) => DoesSoftwareKeyExist(path, Microsoft.Win32.Registry.CurrentUser, false);

        /// <summary>
        /// Does the 32-bit local machine software key at the provided path exists?
        /// </summary>
        /// <returns><c>true</c>, if bit LM software key exist was does32ed, <c>false</c> otherwise.</returns>
        /// <param name="path">Path.</param>
        public static bool
        Does32BitLMSoftwareKeyExist(
            string path) => DoesSoftwareKeyExist(path, Microsoft.Win32.Registry.LocalMachine, true);

        /// <summary>
        /// Wrapper around Microsoft.Win32.RegistryKey to avoid a public dependency on a NuGet package.
        /// </summary>
        public class RegKey :
            System.IDisposable
        {
            private Microsoft.Win32.RegistryKey key;

            /// <summary>
            /// Create an instance wrapping the specified RegistryKey.
            /// An exception is thrown if the source key is null.
            /// </summary>
            /// <param name="sourceKey">Source registry key.</param>
            public RegKey(
                Microsoft.Win32.RegistryKey sourceKey)
            {
                this.key = sourceKey ?? throw new Exception(
                        "Registry key cannot be null"
                    );
            }

            void
            System.IDisposable.Dispose() => this.key.Dispose();

            /// <summary>
            /// Name of the registry key represented.
            /// </summary>
            public string Name => this.key.Name;

            /// <summary>
            /// Get the string behind the named value on the registry key.
            /// Exceptions can be thrown if the name is not valid, or the
            /// value that name represents is not a string.
            /// </summary>
            /// <param name="name">Name of the value to get as a string.</param>
            /// <returns>String value</returns>
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

            /// <summary>
            /// Similar to GetStringValue, but will not throw an exception if the name is invalid.
            /// </summary>
            /// <param name="name">Name of the value to get as a string.</param>
            /// <returns>String value, or null if the name is invalid.</returns>
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

            /// <summary>
            /// Enumerate across all subkeys of the wrapped key.
            /// </summary>
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
                Log.DebugMessage($"Registry key '{keyPath}' on {registryArea.Name} not found");
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
            string path) => OpenSoftwareKey(path, Microsoft.Win32.Registry.LocalMachine, true);

        /// <summary>
        /// Open the current user software key specified by the path.
        /// </summary>
        /// <returns>The CU software key.</returns>
        /// <param name="path">Path.</param>
        public static RegKey
        OpenCUSoftwareKey(
            string path) => OpenSoftwareKey(path, Microsoft.Win32.Registry.CurrentUser, false);

        /// <summary>
        /// Does the local machine software key exist at the path?
        /// </summary>
        /// <returns><c>true</c>, if LM software key exist was doesed, <c>false</c> otherwise.</returns>
        /// <param name="path">Path.</param>
        public static bool
        DoesLMSoftwareKeyExist(
            string path) => DoesSoftwareKeyExist(path, Microsoft.Win32.Registry.LocalMachine, false);

        /// <summary>
        /// Open the local machine software key at the path.
        /// </summary>
        /// <returns>The LM software key.</returns>
        /// <param name="path">Path.</param>
        public static RegKey
        OpenLMSoftwareKey(
            string path) => OpenSoftwareKey(path, Microsoft.Win32.Registry.LocalMachine, false);
    }
}
