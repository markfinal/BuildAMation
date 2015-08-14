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
namespace C
{
namespace V2
{
namespace DefaultSettings
{
    public static partial class DefaultSettingsExtensions
    {
        public static void Defaults(this C.V2.ILinkerOptionsOSX settings, Bam.Core.V2.Module module)
        {
            settings.Frameworks = new Bam.Core.StringArray();
        }
    }
}
    [Bam.Core.V2.SettingsExtensions(typeof(C.V2.DefaultSettings.DefaultSettingsExtensions))]
    public interface ILinkerOptionsOSX : Bam.Core.V2.ISettingsBase
    {
        Bam.Core.StringArray Frameworks
        {
            get;
            set;
        }

        Bam.Core.V2.TokenizedString InstallName
        {
            get;
            set;
        }
    }
}
    public interface ILinkerOptionsOSX
    {
        /// <summary>
        /// List of names of OSX frameworks to include in the link step
        /// </summary>
        /// <value>The OSX frameworks.</value>
        Bam.Core.StringArray Frameworks
        {
            get;
            set;
        }

        /// <summary>
        /// List of directories the linker searches for Frameworks
        /// </summary>
        /// <value>The OSX frameworks.</value>
        Bam.Core.DirectoryCollection FrameworkSearchDirectories
        {
            get;
            set;
        }

        /// <summary>
        /// Suppress read only relocations
        /// </summary>
        /// <value><c>true</c> if read only relocations; otherwise, <c>false</c>.</value>
        bool SuppressReadOnlyRelocations
        {
            get;
            set;
        }
    }
}
