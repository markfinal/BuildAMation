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
    public class HeaderFileCollection :
        Bam.Core.V2.Module,
        Bam.Core.V2.IModuleGroup
    {
        private Bam.Core.Array<HeaderFile> children = new Bam.Core.Array<HeaderFile>();

        public HeaderFile
        AddFile(
            string path,
            Bam.Core.V2.Module macroModuleOverride = null,
            bool verbatim = false)
        {
            var child = Bam.Core.V2.Module.Create<HeaderFile>(this);
            var macroModule = (macroModuleOverride == null) ? this : macroModuleOverride;
            child.InputPath = Bam.Core.V2.TokenizedString.Create(path, macroModule, verbatim);
            (child as Bam.Core.V2.IChildModule).Parent = this;
            this.children.Add(child);
            this.DependsOn(child);
            return child;
        }

        public Bam.Core.Array<Bam.Core.V2.Module>
        AddFiles(
            string path,
            Bam.Core.V2.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var macroModule = (macroModuleOverride == null) ? this : macroModuleOverride;
            var wildcardPath = Bam.Core.V2.TokenizedString.Create(path, macroModule).Parse();

            var dir = System.IO.Path.GetDirectoryName(wildcardPath);
            var leafname = System.IO.Path.GetFileName(wildcardPath);
            var files = System.IO.Directory.GetFiles(dir, leafname, System.IO.SearchOption.TopDirectoryOnly);
            if (filter != null)
            {
                files = files.Where(pathname => filter.IsMatch(pathname)).ToArray();
            }
            if (0 == files.Length)
            {
                throw new Bam.Core.Exception("No files were found that matched the pattern '{0}'", wildcardPath);
            }
            var modulesCreated = new Bam.Core.Array<Bam.Core.V2.Module>();
            foreach (var filepath in files)
            {
                var fp = filepath;
                modulesCreated.Add(this.AddFile(fp, verbatim: true));
            }
            return modulesCreated;
        }

        public override void Evaluate()
        {
            foreach (var child in this.children)
            {
                if (!child.IsUpToDate)
                {
                    return;
                }
            }
            this.IsUpToDate = true;
        }

        protected override void ExecuteInternal(Bam.Core.V2.ExecutionContext context)
        {
            // do nothing
        }

        protected override void GetExecutionPolicy(string mode)
        {
            // do nothing
        }
    }

    public class CObjectFileCollection :
        BaseObjectFiles<ObjectFile>
    {
        protected override void
        Init(
            Bam.Core.V2.Module parent)
        {
            base.Init(parent);
            this.Tool = DefaultToolchain.C_Compiler(this.BitDepth);
        }
    }
}
    /// <summary>
    /// C object file collection
    /// </summary>
    [Bam.Core.ModuleToolAssignment(typeof(ICompilerTool))]
    public class ObjectFileCollection :
        ObjectFileCollectionBase
    {
        public void
        Add(ObjectFile objectFile)
        {
            this.list.Add(objectFile);
        }

        protected override System.Collections.Generic.List<Bam.Core.IModule>
        MakeChildModules(
            Bam.Core.LocationArray locationList)
        {
            var objectFileList = new System.Collections.Generic.List<Bam.Core.IModule>();

            foreach (var location in locationList)
            {
                var objectFile = new ObjectFile();
                objectFile.SourceFileLocation = location;
                objectFileList.Add(objectFile);
            }

            return objectFileList;
        }
    }
}
