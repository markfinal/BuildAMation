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
using System.Linq;
namespace Installer
{
    public sealed class DiskImageCompiler :
        Bam.Core.PreBuiltTool
    {
        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module)
        {
            return new DiskImageSettings(module);
        }

        public override Bam.Core.TokenizedString Executable
        {
            get
            {
                return Bam.Core.TokenizedString.CreateVerbatim(Bam.Core.OSUtilities.GetInstallLocation("hdiutil").First());
            }
        }
    }

    /// <summary>
    /// Derive from this module to create an OSX disk image.
    /// </summary>
    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.OSX)]
    public abstract class DiskImage :
        Bam.Core.Module
    {
#if BAM_V2
        public const string DMGKey = "Disk Image Installer";
#else
        public static Bam.Core.PathKey Key = Bam.Core.PathKey.Generate("Installer");
#endif

#if BAM_V2
        private Bam.Core.Module SourceModule;
        private string SourcePathKey;
#else
        private Bam.Core.TokenizedString SourceFolderPath;
        private IDiskImagePolicy Policy;
#endif

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.RegisterGeneratedFile(
#if BAM_V2
                DMGKey,
#else
                Key,
#endif
                this.CreateTokenizedString("$(buildroot)/$(config)/$(OutputName).dmg")
            );

            this.Tool = Bam.Core.Graph.Instance.FindReferencedModule<DiskImageCompiler>();
        }

        /// <summary>
        /// Specify the source for the disk image. This could equally be a single file, but is usually a folder, such
        /// as the output from one of the publishing collation steps.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <typeparam name="DependentModule">The 1st type parameter.</typeparam>
        public void
        SourceFolder<DependentModule>(
#if BAM_V2
            string key
#else
            Bam.Core.PathKey key
#endif
        ) where DependentModule : Bam.Core.Module, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }
            this.DependsOn(dependent);
#if BAM_V2
            this.SourceModule = dependent;
            this.SourcePathKey = key;
#else
            this.SourceFolderPath = dependent.GeneratedPaths[key];
#endif
        }

        protected sealed override void
        EvaluateInternal()
        {
            // do nothing
        }

        protected sealed override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
#if BAM_V2
            switch (Bam.Core.Graph.Instance.Mode)
            {
#if D_PACKAGE_MAKEFILEBUILDER
                case "MakeFile":
                    MakeFileSupport.CreateDMG(this);
                    break;
#endif

#if D_PACKAGE_NATIVEBUILDER
                case "Native":
                    NativeSupport.CreateDMG(this, context);
                    break;
#endif

#if D_PACKAGE_XCODEBUILDER
                case "Xcode":
                    Bam.Core.Log.DebugMessage("DiskImage not supported on Xcode builds");
                    break;
#endif

                default:
                    throw new System.NotSupportedException();
            }
#else
            if (null != this.Policy)
            {
                this.Policy.CreateDMG(this, context, this.Tool as DiskImageCompiler, this.SourceFolderPath, this.GeneratedPaths[Key]);
            }
#endif
        }

        protected sealed override void
        GetExecutionPolicy(
            string mode)
        {
#if BAM_V2
#else
            if (mode == "Native")
            {
                var className = "Installer." + mode + "DMG";
                this.Policy = Bam.Core.ExecutionPolicyUtilities<IDiskImagePolicy>.Create(className);
            }
#endif
        }

        public override System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, Bam.Core.Module>> InputModules
        {
            get
            {
                yield return new System.Collections.Generic.KeyValuePair<string, Bam.Core.Module>(this.SourcePathKey, this.SourceModule);
            }
        }
    }
}
