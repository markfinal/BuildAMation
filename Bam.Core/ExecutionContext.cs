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
    /// Each module execution has a copy of an execution context, which holds useful state data about
    /// invoking the execution, and recording the output from the execution.
    /// </summary>
    public class ExecutionContext
    {
        /// <summary>
        /// Construct an instance of the context.
        /// </summary>
        /// <param name="useEvaluation">If set to <c>true</c>, the build mode requires module evaluation to occur.</param>
        /// <param name="explainRebuild">If set to <c>true</c>, any module (re)build reasons are logged to the console.</param>
        /// <param name="useImmediateOutput">If set to <c>true</c>, any output from the module execution is not cached, but displayed immediately.</param>
        public ExecutionContext(
            bool useEvaluation,
            bool explainRebuild,
            bool useImmediateOutput)
        {
            this.Evaluate = useEvaluation;
            this.ExplainLoggingLevel = explainRebuild ? Graph.Instance.VerbosityLevel : EVerboseLevel.Full;
            this.UseDeferredOutput = !useImmediateOutput;
            if (this.UseDeferredOutput)
            {
                this.OutputStringBuilder = new System.Text.StringBuilder();
                this.ErrorStringBuilder = new System.Text.StringBuilder();
            }
        }

        /// <summary>
        /// Determine if the module execution requires evaluation first.
        /// </summary>
        /// <value><c>true</c> if evaluate is to occur before execution; otherwise, <c>false</c>.</value>
        public bool Evaluate
        {
            get;
            private set;
        }

        /// <summary>
        /// Obtain the log verbosity level of any (re)build explanations.
        /// </summary>
        /// <value>The explain logging level.</value>
        public EVerboseLevel ExplainLoggingLevel
        {
            get;
            private set;
        }

        /// <summary>
        /// Does any output from module execution get cached for a deferred display?
        /// </summary>
        /// <value><c>true</c> if use deferred output; otherwise, <c>false</c>.</value>
        public bool UseDeferredOutput
        {
            get;
            private set;
        }

        /// <summary>
        /// If using deferred output, this is where stdout output is captured. Otherwise this is null.
        /// </summary>
        /// <value>The output string builder.</value>
        public System.Text.StringBuilder OutputStringBuilder
        {
            get;
            private set;
        }

        /// <summary>
        /// If using deferred output, this is where stderr output is captured. Otherwise this is null.
        /// </summary>
        /// <value>The error string builder.</value>
        public System.Text.StringBuilder ErrorStringBuilder
        {
            get;
            private set;
        }

        /// <summary>
        /// Delegate for asynchronous capturing of stdout data.
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Data received event arguments, containing stdout data.</param>
        public void
        OutputDataReceived(
            object sender,
            System.Diagnostics.DataReceivedEventArgs e)
        {
            if (System.String.IsNullOrEmpty(e.Data))
            {
                return;
            }
            if (this.UseDeferredOutput)
            {
                this.OutputStringBuilder.Append(e.Data + '\n');
            }
            else
            {
                Log.MessageAll("stdout: {0}", e.Data);
            }
        }

        /// <summary>
        /// Delegate for asynchronous capturing of stderr data.
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Data received event arguments, containing stderr data.</param>
        public void
        ErrorDataReceived(
            object sender,
            System.Diagnostics.DataReceivedEventArgs e)
        {
            if (System.String.IsNullOrEmpty(e.Data))
            {
                return;
            }
            if (this.UseDeferredOutput)
            {
                this.ErrorStringBuilder.Append(e.Data + '\n');
            }
            else
            {
                Log.ErrorMessage("stderr: {0}", e.Data);
            }
        }
    }
}
