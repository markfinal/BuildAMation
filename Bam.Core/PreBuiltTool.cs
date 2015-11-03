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
namespace Bam.Core
{
    /// <summary>
    /// A tool is a module in the usual sense, so that it can be added into the dependency tree
    /// </summary>
    public abstract class PreBuiltTool :
        Module,
        ICommandLineTool
    {
        protected PreBuiltTool()
            : base()
        {
            this.EnvironmentVariables = new System.Collections.Generic.Dictionary<string, TokenizedStringArray>();
            this.InheritedEnvironmentVariables = new System.Collections.Generic.List<string>();
        }

            #if false
        // TODO: Might move the Name into the Module?
        public string Name
        {
            get;
            protected set;
        }
            #endif

        public abstract Settings CreateDefaultSettings<T>(T module) where T : Module;

        public System.Collections.Generic.Dictionary<string, TokenizedStringArray> EnvironmentVariables
        {
            get;
            private set;
        }

        public System.Collections.Generic.List<string> InheritedEnvironmentVariables
        {
            get;
            private set;
        }

        public abstract TokenizedString Executable
        {
            get;
        }

        public virtual TokenizedStringArray InitialArguments
        {
            get
            {
                return null;
            }
        }

        protected override void
        ExecuteInternal(
            ExecutionContext context)
        {
            // by default, a PreBuiltTool's execution does nothing as it's on disk
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            // by default, the execution policy of a PreBuiltTool is to do nothing as it's on disk
        }

        public override void
        Evaluate()
        {
            this.ReasonToExecute = null;
            var exists = System.IO.File.Exists(this.Executable.Parse());
            if (!exists)
            {
                this.ReasonToExecute = ExecuteReasoning.FileDoesNotExist(this.Executable);
            }
        }
    }
}
