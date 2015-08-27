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
using System.Linq;
namespace C
{
namespace V2
{
namespace DefaultSettings
{
    public static partial class DefaultSettingsExtensions
    {
        public static void
        Defaults(
            this C.V2.ICCompilerOptionsOSX settings,
            Bam.Core.V2.Module module)
        {
        }
        public static void
        Empty(
            this C.V2.ICCompilerOptionsOSX settings)
        {
            settings.FrameworkSearchDirectories = new Bam.Core.Array<Bam.Core.V2.TokenizedString>();
        }
        public static void
        SharedSettings(
            this C.V2.ICCompilerOptionsOSX shared,
            C.V2.ICCompilerOptionsOSX lhs,
            C.V2.ICCompilerOptionsOSX rhs)
        {
            shared.FrameworkSearchDirectories = new Bam.Core.Array<Bam.Core.V2.TokenizedString>(lhs.FrameworkSearchDirectories.Intersect(rhs.FrameworkSearchDirectories));
        }
        public static void
        Delta(
            this C.V2.ICCompilerOptionsOSX delta,
            C.V2.ICCompilerOptionsOSX lhs,
            C.V2.ICCompilerOptionsOSX rhs)
        {
            delta.FrameworkSearchDirectories = new Bam.Core.Array<Bam.Core.V2.TokenizedString>(lhs.FrameworkSearchDirectories.Except(rhs.FrameworkSearchDirectories));
        }
        public static void
        Clone(
            this C.V2.ICCompilerOptionsOSX settings,
            C.V2.ICCompilerOptionsOSX other)
        {
            foreach (var path in other.FrameworkSearchDirectories)
            {
                settings.FrameworkSearchDirectories.AddUnique(path);
            }
        }
    }
}
    [Bam.Core.V2.SettingsExtensions(typeof(C.V2.DefaultSettings.DefaultSettingsExtensions))]
    public interface ICCompilerOptionsOSX :
        Bam.Core.V2.ISettingsBase
    {
        Bam.Core.Array<Bam.Core.V2.TokenizedString> FrameworkSearchDirectories
        {
            get;
            set;
        }
    }
}
    public interface ICCompilerOptionsOSX
    {
        /// <summary>
        /// List of directories the compiler searches for Frameworks
        /// </summary>
        /// <value>The OSX frameworks.</value>
        Bam.Core.DirectoryCollection FrameworkSearchDirectories
        {
            get;
            set;
        }

        /// <summary>
        /// OSX SDK version used to compile against
        /// </summary>
        /// <value>The OSX SDK version.</value>
        string SDKVersion
        {
            get;
            set;
        }

        /// <summary>
        /// The minimum version of OSX required in order to run this code
        /// </summary>
        /// <value>The deployment target.</value>
        string DeploymentTarget
        {
            get;
            set;
        }

        /// <summary>
        /// OSX builds can target either MacOSX or iOS
        /// </summary>
        /// <value>The supported platform.</value>
        C.EOSXPlatform SupportedPlatform
        {
            get;
            set;
        }

        /// <summary>
        /// Name of the compiler, in reverse DNS form. Usually not required to be set.
        /// </summary>
        /// <value>The name of the compiler.</value>
        string CompilerName
        {
            get;
            set;
        }
    }
}
