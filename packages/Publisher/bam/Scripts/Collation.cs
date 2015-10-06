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
using Bam.Core;
namespace Publisher
{
    public abstract class Collation :
        Bam.Core.Module
    {
        private ICollationPolicy Policy = null;
        public static Bam.Core.FileKey PackageRoot = Bam.Core.FileKey.Generate("Package Root");

        public enum EPublishingType
        {
            ConsoleApplication,
            WindowedApplication
        }

        protected Collation()
        {
            this.RegisterGeneratedFile(PackageRoot, Bam.Core.TokenizedString.Create("$(buildroot)/$(modulename)-$(config)", this));
        }

        private string
        PublishingPath(
            Bam.Core.Module module,
            EPublishingType type)
        {
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX) &&
                (EPublishingType.WindowedApplication == type))
            {
                return Bam.Core.TokenizedString.Create("$(OutputName).app/Contents/MacOS", module).Parse();
            }
            return null;
        }

        private CollatedFile
        CreateCollatedFile(
            CollatedFile reference = null)
        {
            // TODO: 'this' passed as the Parent - used later
            var copyFileModule = Bam.Core.Module.Create<CollatedFile>(this);
            this.Requires(copyFileModule);
            if (null != reference)
            {
                copyFileModule.Reference = reference;
            }
            return copyFileModule;
        }

        public CollatedFile
        Include<DependentModule>(
            Bam.Core.FileKey key,
            EPublishingType type,
            string subdir = null) where DependentModule : Bam.Core.Module, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return null;
            }

            var path = this.PublishingPath(dependent, type);
            string destSubDir;
            if (null == path)
            {
                destSubDir = subdir;
            }
            else
            {
                if (null != subdir)
                {
                    destSubDir = System.IO.Path.Combine(path, subdir);
                }
                else
                {
                    destSubDir = path;
                }
            }

            var copyFileModule = this.CreateCollatedFile();
            copyFileModule.SubDirectory = destSubDir;
            copyFileModule.SourceModule = dependent;
            copyFileModule.SourcePath = dependent.GeneratedPaths[key];

            return copyFileModule;
        }

        public void
        Include<DependentModule>(
            Bam.Core.FileKey key,
            string subdir,
            CollatedFile reference) where DependentModule : Bam.Core.Module, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }

            var copyFileModule = this.CreateCollatedFile(reference);
            copyFileModule.SubDirectory = subdir;
            copyFileModule.SourceModule = dependent;
            copyFileModule.SourcePath = dependent.GeneratedPaths[key];
        }

        public void
        IncludeFiles<DependentModule>(
            string parameterizedFilePath,
            string subdir,
            CollatedFile reference) where DependentModule : Bam.Core.Module, new()
        {
            var dependent = Bam.Core.Graph.Instance.FindReferencedModule<DependentModule>();
            if (null == dependent)
            {
                return;
            }

            var copyFileModule = this.CreateCollatedFile(reference);
            copyFileModule.SubDirectory = subdir;
            copyFileModule.SourceModule = dependent;
            copyFileModule.SourcePath = Bam.Core.TokenizedString.Create(parameterizedFilePath, dependent);
        }

        public void
        IncludeFile(
            string parameterizedFilePath,
            string subdir,
            CollatedFile reference)
        {
            var tokenString = Bam.Core.TokenizedString.Create(parameterizedFilePath, this);
            this.IncludeFile(tokenString, subdir, reference);
        }

        public void
        IncludeFile(
            Bam.Core.TokenizedString parameterizedFilePath,
            string subdir,
            CollatedFile reference)
        {
            var copyFileModule = this.CreateCollatedFile(reference);
            copyFileModule.SubDirectory = subdir;
            copyFileModule.SourcePath = parameterizedFilePath;
        }

        public override void
        Evaluate()
        {
            // TODO
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            if (null == this.Policy)
            {
                return;
            }
            this.Policy.Collate(this, context);
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            switch (mode)
            {
                case "MakeFile":
                    {
                        var className = "Publisher." + mode + "Collation";
                        this.Policy = Bam.Core.ExecutionPolicyUtilities<ICollationPolicy>.Create(className);
                    }
                    break;
            }
        }
    }
}
