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
namespace Installer
{
    public sealed class DiskImageSettings :
        Bam.Core.Settings
    {
    }

    public sealed class DiskImageCompiler :
        Bam.Core.PreBuiltTool
    {
        public override Bam.Core.Settings
        CreateDefaultSettings<T>(
            T module)
        {
            return new TarSettings();
        }

        public override Bam.Core.TokenizedString Executable
        {
            get
            {
                return Bam.Core.TokenizedString.CreateVerbatim("hdiutil");
            }
        }
    }

    [Bam.Core.PlatformFilter(Bam.Core.EPlatform.OSX)]
    public abstract class DiskImage :
        Bam.Core.Module
    {
        public static Bam.Core.FileKey Key = Bam.Core.FileKey.Generate("Installer");

        private Bam.Core.TokenizedString SourceFolderPath;
        private Bam.Core.PreBuiltTool Compiler;
        private IDiskImagePolicy Policy;

        public DiskImage()
        {
            this.RegisterGeneratedFile(Key, Bam.Core.TokenizedString.Create("$(buildroot)/installer.dmg", this));

            this.Compiler = Bam.Core.Graph.Instance.FindReferencedModule<DiskImageCompiler>();
            this.Requires(this.Compiler);
        }

        public void
        SourceFolder<DependentModule>(
            Bam.Core.FileKey key) where DependentModule : Bam.Core.Module, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            this.DependsOn(dependent);
            this.SourceFolderPath = dependent.GeneratedPaths[key];
        }

        public override void
        Evaluate()
        {
            // do nothing
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            if (null != this.Policy)
            {
                this.Policy.CreateDMG(this, context, this.Compiler, this.SourceFolderPath, this.GeneratedPaths[Key]);
            }
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            if (mode == "Native")
            {
                var className = "Installer." + mode + "DMG";
                this.Policy = Bam.Core.ExecutionPolicyUtilities<IDiskImagePolicy>.Create(className);
            }
        }
    }
}
