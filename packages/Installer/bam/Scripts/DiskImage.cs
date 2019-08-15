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
using System.Linq;
namespace Installer
{
    /// <summary>
    /// Compiler tool for hdiutil
    /// </summary>
    public sealed class DiskImageCompiler :
        Bam.Core.PreBuiltTool
    {
        /// <summary>
        /// \copydoc Bam.Core.ITool.SettingsType
        /// </summary>
        public override System.Type SettingsType => typeof(DiskImageSettings);

        /// <summary>
        /// Executable path to the tool
        /// </summary>
        public override Bam.Core.TokenizedString Executable => Bam.Core.TokenizedString.CreateVerbatim(Bam.Core.OSUtilities.GetInstallLocation("hdiutil").First());
    }

    /// <summary>
    /// Derive from this module to create an OSX disk image.
    /// </summary>
    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.OSX)]
    public abstract class DiskImage :
        Bam.Core.Module
    {
        /// <summary>
        /// Path key to the generated disk image
        /// </summary>
        public const string DMGKey = "Disk Image Installer";

        private Bam.Core.Module SourceModule;
        private string SourcePathKey;

        protected override void
        Init()
        {
            base.Init();

            this.RegisterGeneratedFile(
                DMGKey,
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
            string key
        ) where DependentModule : Bam.Core.Module, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }
            this.DependsOn(dependent);
            this.SourceModule = dependent;
            this.SourcePathKey = key;
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
            switch (Bam.Core.Graph.Instance.Mode)
            {
#if D_PACKAGE_MAKEFILEBUILDER
                case "MakeFile":
                    MakeFileBuilder.Support.Add(this);
                    break;
#endif

#if D_PACKAGE_NATIVEBUILDER
                case "Native":
                    {
                        // hdiutil will fail on an incremental build if the DMG exists
                        // 'hdiutil: create failed - File exists'
                        var dmg_path = this.GeneratedPaths[DiskImage.DMGKey].ToString();
                        if (System.IO.File.Exists(dmg_path))
                        {
                            System.IO.File.Delete(dmg_path);
                        }
                        NativeBuilder.Support.RunCommandLineTool(this, context);
                    }
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
        }

        /// <summary>
        /// Enumerate across all inputs to this module
        /// </summary>
        public override System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, Bam.Core.Module>> InputModules
        {
            get
            {
                yield return new System.Collections.Generic.KeyValuePair<string, Bam.Core.Module>(this.SourcePathKey, this.SourceModule);
            }
        }
    }
}
