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
namespace C
{
    public class SharedObjectSymbolicLink :
        Bam.Core.Module
    {
#if BAM_V2
        public const string SOSymLinkKey = "Shared object symbolic link";
#else
        static public Bam.Core.PathKey Key = Bam.Core.PathKey.Generate("SharedObjectSymbolicLink");

        private ISharedObjectSymbolicLinkPolicy SymlinkPolicy;
        private SharedObjectSymbolicLinkTool SymlinkTool;
#endif
        private ConsoleApplication sharedObject;

        /// <summary>
        /// Query whether the module has the C.Prebuilt attribute assigned to it.
        /// </summary>
        public bool IsPrebuilt
        {
            get;
            private set;
        }

        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.Tool = Bam.Core.Graph.Instance.FindReferencedModule<SharedObjectSymbolicLinkTool>();
            this.IsPrebuilt = (this.GetType().GetCustomAttributes(typeof(PrebuiltAttribute), true).Length > 0);

#if BAM_V2
            this.RegisterGeneratedFile(SOSymLinkKey,
                                       this.CreateTokenizedString("@dir($(0))/$(1)",
                                                                  this.SharedObject.GeneratedPaths[ConsoleApplication.ExecutableKey],
                                                                  this.SharedObject.GeneratedPaths[this.Macros["SymlinkUsage"].ToString()]));
#else
            this.RegisterGeneratedFile(Key,
                                       this.CreateTokenizedString("@dir($(0))/$(1)",
                                                                  this.SharedObject.GeneratedPaths[ConsoleApplication.Key],
                                                                  this.SharedObject.Macros[this.Macros["SymlinkUsage"].ToString()]));
#endif
        }

        public ConsoleApplication
        SharedObject
        {
            get
            {
                return this.sharedObject;
            }
            set
            {
                this.sharedObject = value;
                this.DependsOn(value);

                // ensure that the symlink is called the same as what it is linking to
                this.Macros["OutputName"] = value.Macros["OutputName"];
            }
        }

        protected override void
        ExecuteInternal(
            Bam.Core.ExecutionContext context)
        {
            if (this.IsPrebuilt)
            {
                return;
            }
#if BAM_V2
            switch (Bam.Core.Graph.Instance.Mode)
            {
#if D_PACKAGE_NATIVEBUILDER
                case "Native":
                    NativeSupport.SymLink(this, context);
                    break;
#endif

#if D_PACKAGE_MAKEFILEBUILDER
                case "MakeFile":
                    MakeFileSupport.SymLink(this);
                    break;
#endif

                default:
                    throw new System.NotImplementedException();
            }
#else
            this.SymlinkPolicy.Symlink(this, context, this.SymlinkTool, this.SharedObject);
#endif
        }

        protected override void
        EvaluateInternal()
        {
#if __MonoCS__
            if (this.IsPrebuilt)
            {
                return;
            }
            this.ReasonToExecute = null;
            var symlinkPath = this.GeneratedPaths[SOSymLinkKey].ToString();
            var symlinkInfo = new Mono.Unix.UnixSymbolicLinkInfo(symlinkPath);
            if (!symlinkInfo.Exists)
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[SOSymLinkKey]);
                return;
            }
            var targetPath = symlinkInfo.ContentsPath;
            if (targetPath != System.IO.Path.GetFileName(this.SharedObject.GeneratedPaths[ConsoleApplication.ExecutableKey].ToString()))
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(
                    this.GeneratedPaths[SOSymLinkKey],
                    this.SharedObject.Macros[this.GeneratedPaths["SymlinkUsage"].ToString()]
                );
                return;
            }
#else
            throw new System.NotSupportedException("Symbolic links not supported for shared objects on this platform");
#endif
        }

#if BAM_V2
#else
        protected override void
        GetExecutionPolicy(
            string mode)
        {
            if (this.IsPrebuilt)
            {
                return;
            }
            var className = "C." + mode + "SharedObjectSymbolicLink";
            this.SymlinkPolicy = Bam.Core.ExecutionPolicyUtilities<ISharedObjectSymbolicLinkPolicy>.Create(className);
            this.SymlinkTool = Bam.Core.Graph.Instance.FindReferencedModule<SharedObjectSymbolicLinkTool>();
    }
#endif

        public override System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, Bam.Core.Module>> InputModules
        {
            get
            {
                System.Diagnostics.Debug.Assert(1 == this.Dependents.Count);
                yield return new System.Collections.Generic.KeyValuePair<string, Bam.Core.Module>(C.DynamicLibrary.ExecutableKey, this.Dependents[0]);
            }
        }

        public override Bam.Core.TokenizedString WorkingDirectory
        {
            get
            {
                return this.OutputDirectories.First();
            }
        }
    }
}
