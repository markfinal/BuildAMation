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
namespace C
{
    /// <summary>
    /// Common macro names used for Modules.
    /// </summary>
    public static class ModuleMacroNames
    {
        /// <summary>
        /// Major version number of the Module.
        /// </summary>
        public const string MajorVersion = "MajorVersion";

        /// <summary>
        /// Minor version number of the Module.
        /// </summary>
        public const string MinorVersion = "MinorVersion";

        /// <summary>
        /// Patch version number of the Module.
        /// </summary>
        public const string PatchVersion = "PatchVersion";

        /// <summary>
        /// File extension used for object files
        /// </summary>
        public const string ObjectFileExtension = "objext";

        /// <summary>
        /// Prefix to apply before a library's basename
        /// </summary>
        public const string LibraryPrefix = "libprefix";

        /// <summary>
        /// File extension used for library files
        /// </summary>
        public const string LibraryFileExtension = "libext";

        /// <summary>
        /// File extension used for executable (linked) files.
        /// </summary>
        public const string ExecutableFileExtension = "exeext";

        /// <summary>
        /// Prefix to apply before a dynamic library's basename.
        /// </summary>
        public const string DynamicLibraryPrefix = "dynamicprefix";

        /// <summary>
        /// File extension used for dynamic library (linked) files.
        /// Note that this may contain versioning information on some platforms.
        /// This will at least be similar to DynamicLibraryUnversionedFileExtension
        /// </summary>
        public const string DynamicLibraryFileExtension = "dynamicext";

        /// <summary>
        /// The un-versioned file extension used for dynamic library (linked) files.
        /// </summary>
        public const string DynamicLibraryUnversionedFileExtension = "dynamicextonly";

        /// <summary>
        /// Prefix to apply before a plugins's basename.
        /// </summary>
        public const string PluginPrefix = "pluginprefix";

        /// <summary>
        /// File extension used for plugin (linked) files.
        /// </summary>
        public const string PluginFileExtension = "pluginext";

        /// <summary>
        /// Shared object linkername file extension for symbolic links.
        /// </summary>
        public const string SharedObjectLinkerNameFileExtension = "linkernameext";

        /// <summary>
        /// Shared object SOname file extension for symbolic links.
        /// </summary>
        public const string SharedObjectSONameFileExtension = "sonameext";
    }
}
