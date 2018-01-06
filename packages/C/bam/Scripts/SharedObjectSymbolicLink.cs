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
namespace C
{
    public class SharedObjectSymbolicLink :
        Bam.Core.Module
    {
        static public Bam.Core.PathKey Key = Bam.Core.PathKey.Generate("SharedObjectSymbolicLink");

        private ISharedObjectSymbolicLinkPolicy SymlinkPolicy;
        private SharedObjectSymbolicLinkTool SymlinkTool;
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

            this.IsPrebuilt = (this.GetType().GetCustomAttributes(typeof(PrebuiltAttribute), true).Length > 0);

            this.RegisterGeneratedFile(Key,
                                       this.CreateTokenizedString("@dir($(0))/$(1)",
                                                                  this.SharedObject.GeneratedPaths[ConsoleApplication.Key],
                                                                  this.SharedObject.Macros[this.Macros["SymlinkUsage"].ToString()]));
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
                this.Macros["OutputName"] = value.Macros["OutputName"];
                this.DependsOn(value);
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
            this.SymlinkPolicy.Symlink(this, context, this.SymlinkTool, this.SharedObject);
        }

        public override void
        Evaluate()
        {
#if __MonoCS__
            if (this.IsPrebuilt)
            {
                return;
            }
            this.ReasonToExecute = null;
            var symlinkPath = this.GeneratedPaths[Key].ToString();
            var symlinkInfo = new Mono.Unix.UnixSymbolicLinkInfo(symlinkPath);
            if (!symlinkInfo.Exists)
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[Key]);
                return;
            }
            var targetPath = symlinkInfo.ContentsPath;
            if (targetPath != System.IO.Path.GetFileName(this.SharedObject.GeneratedPaths[ConsoleApplication.Key].ToString()))
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.InputFileNewer(this.GeneratedPaths[Key], this.SharedObject.Macros[this.Macros["SymlinkUsage"].ToString()]);
                return;
            }
#else
            throw new System.NotSupportedException("Symbolic links not supported for shared objects on this platform");
#endif
        }

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
    }
}
