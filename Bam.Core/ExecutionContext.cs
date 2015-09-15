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
    public class ExecutionContext
    {
        public ExecutionContext(
            bool useEvaluation,
            bool explainRebuild)
        {
            this.Evaluate = useEvaluation;
            this.ExplainLoggingLevel = explainRebuild ? State.VerbosityLevel : EVerboseLevel.Full;
            this.OutputStringBuilder = new System.Text.StringBuilder();
            this.ErrorStringBuilder = new System.Text.StringBuilder();
        }

        public bool Evaluate
        {
            get;
            private set;
        }

        public EVerboseLevel ExplainLoggingLevel
        {
            get;
            private set;
        }

        public System.Text.StringBuilder OutputStringBuilder
        {
            get;
            private set;
        }

        public System.Text.StringBuilder ErrorStringBuilder
        {
            get;
            private set;
        }

        public void
        OutputDataReceived(
            object sender,
            System.Diagnostics.DataReceivedEventArgs e)
        {
            if (System.String.IsNullOrEmpty(e.Data))
            {
                return;
            }
            //System.Diagnostics.Process process = sender as System.Diagnostics.Process;
            this.OutputStringBuilder.Append(e.Data + '\n');
        }

        public void
        ErrorDataReceived(
            object sender,
            System.Diagnostics.DataReceivedEventArgs e)
        {
            if (System.String.IsNullOrEmpty(e.Data))
            {
                return;
            }
            //System.Diagnostics.Process process = sender as System.Diagnostics.Process;
            this.ErrorStringBuilder.Append(e.Data + '\n');
        }
    }
}
