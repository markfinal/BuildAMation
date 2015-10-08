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
    public abstract class CModuleContainer<ChildModuleType> :
        CModule,
        Bam.Core.IModuleGroup,
        IAddFiles
        where ChildModuleType : Bam.Core.Module, Bam.Core.IInputPath, Bam.Core.IChildModule, new()
    {
        private System.Collections.Generic.List<ChildModuleType> children = new System.Collections.Generic.List<ChildModuleType>();

        public ChildModuleType
        AddFile(
            string path,
            Bam.Core.Module macroModuleOverride = null,
            bool verbatim = false)
        {
            // TODO: how can I distinguish between creating a child module that inherits it's parents settings
            // and from a standalone object of type ChildModuleType which should have it's own copy of the settings?
            var child = Bam.Core.Module.Create<ChildModuleType>(this);
            if (verbatim)
            {
                child.InputPath = Bam.Core.TokenizedString.CreateVerbatim(path);
            }
            else
            {
                var macroModule = (macroModuleOverride == null) ? this : macroModuleOverride;
                child.InputPath = macroModule.CreateTokenizedString(path);
            }
            (child as Bam.Core.IChildModule).Parent = this;
            this.children.Add(child);
            this.DependsOn(child);
            return child;
        }

        public Bam.Core.Array<Bam.Core.Module>
        AddFiles(
            string path,
            Bam.Core.Module macroModuleOverride = null,
            System.Text.RegularExpressions.Regex filter = null)
        {
            var macroModule = (macroModuleOverride == null) ? this : macroModuleOverride;
            var wildcardPath = macroModule.CreateTokenizedString(path).Parse();

            var dir = System.IO.Path.GetDirectoryName(wildcardPath);
            if (!System.IO.Directory.Exists(dir))
            {
                throw new Bam.Core.Exception("The directory {0} does not exist", dir);
            }
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
            var modulesCreated = new Bam.Core.Array<Bam.Core.Module>();
            foreach (var filepath in files)
            {
                var fp = filepath;
                modulesCreated.Add(this.AddFile(fp, verbatim: true));
            }
            return modulesCreated;
        }

        public ChildModuleType
        AddFile(
            Bam.Core.FileKey generatedFileKey,
            Bam.Core.Module module,
            Bam.Core.Module.ModulePreInitDelegate preInitDlg = null)
        {
            if (!module.GeneratedPaths.ContainsKey(generatedFileKey))
            {
                throw new System.Exception(System.String.Format("No generated path found with key '{0}'", generatedFileKey.Id));
            }
            var child = Bam.Core.Module.Create<ChildModuleType>(this, preInitCallback: preInitDlg);
            child.InputPath = module.GeneratedPaths[generatedFileKey];
            (child as Bam.Core.IChildModule).Parent = this;
            this.children.Add(child);
            this.DependsOn(child);
            child.DependsOn(module);
            return child;
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            // do nothing
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            // do nothing
            // TODO: might have to get the policy, for the sharing settings
        }

        public override void
        Evaluate()
        {
            this.ReasonToExecute = null;
            try
            {
                foreach (var child in this.children)
                {
                    if (null != child.EvaluationTask)
                    {
                        child.EvaluationTask.Wait();
                    }
                    if (null != child.ReasonToExecute)
                    {
                        this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(child.ReasonToExecute.OutputFilePath, child.ReasonToExecute.OutputFilePath);
                        return;
                    }
                }
            }
            catch (System.AggregateException exception)
            {
                throw new Bam.Core.Exception(exception, "Failed to evaluate modules");
            }
        }
    }
}
