#region License
// Copyright (c) 2010-2017, Mark Final
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
    /// A prebuilt tool is a module that has been built outside of Bam, but can still be included
    /// as a dependency in the build.
    /// </summary>
    public abstract class PreBuiltTool :
        Module,
        ICommandLineTool
    {
        /// <summary>
        /// Create an instance
        /// </summary>
        protected PreBuiltTool()
            : base()
        {
            this.EnvironmentVariables = new System.Collections.Generic.Dictionary<string, TokenizedStringArray>();
            this.InheritedEnvironmentVariables = new StringArray();
        }

            #if false
        // TODO: Might move the Name into the Module?
        public string Name
        {
            get;
            protected set;
        }
            #endif

        /// <summary>
        /// Create an instance of the default settings for the tool associated with the module.
        /// </summary>
        /// <returns>The default settings.</returns>
        /// <param name="module">Module.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public abstract Settings
        CreateDefaultSettings<T>(
            T module) where T : Module;

        /// <summary>
        /// Get the environment variables to be injected into the run of the tool.
        /// </summary>
        /// <value>The environment variables.</value>
        public System.Collections.Generic.Dictionary<string, TokenizedStringArray> EnvironmentVariables
        {
            get;
            private set;
        }

        /// <summary>
        /// Get the array of environment variable names to inherit from the parent environment.
        /// </summary>
        /// <value>The inherited environment variables.</value>
        public StringArray InheritedEnvironmentVariables
        {
            get;
            private set;
        }

        /// <summary>
        /// Get the executable name for the prebuilt tool.
        /// </summary>
        /// <value>The executable.</value>
        public abstract TokenizedString Executable
        {
            get;
        }

        /// <summary>
        /// Default to no extra initial arguments. Virtual to allow overriding.
        /// </summary>
        /// <value>The initial arguments.</value>
        public virtual TokenizedStringArray InitialArguments
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Default to no terminating arguments. Virtual to allow overriding.
        /// </summary>
        /// <value>The terminating arguments to a command line.</value>
        public virtual TokenizedStringArray TerminatingArguments
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Prebuilt tool does not support response files by default. Virtual to allow overiding.
        /// </summary>
        /// <value>The use response file option.</value>
        public virtual string UseResponseFileOption
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// No execution needed to update the prebuilt tool.
        /// </summary>
        /// <param name="context">Context.</param>
        protected override void
        ExecuteInternal(
            ExecutionContext context)
        {
            // by default, a PreBuiltTool's execution does nothing as it's on disk
        }

        /// <summary>
        /// No execution policy required for a prebuilt tool.
        /// </summary>
        /// <param name="mode">Mode.</param>
        protected override void
        GetExecutionPolicy(
            string mode)
        {
            // by default, the execution policy of a PreBuiltTool is to do nothing as it's on disk
        }

        /// <summary>
        /// Confirm that the prebuilt executable exists.
        /// </summary>
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
