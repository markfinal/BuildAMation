#region License
// Copyright (c) 2010-2016, Mark Final
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
    /// In some build modes, module execution only occurs after evaluation has determined that the module outputs
    /// require building. This class encapsulates the reasoning behind a (re)build of a module.
    /// </summary>
    public class ExecuteReasoning
    {
        /// <summary>
        /// Enumeration defining the different types of module (re)build.
        /// </summary>
        public enum EReason
        {
            /// <summary>
            /// A (re)build is required, but it is for some undefined reason.
            /// </summary>
            Undefined,

            /// <summary>
            /// A (re)build is required as the module output files do not exist.
            /// </summary>
            FileDoesNotExist,

            /// <summary>
            /// A (re)build is required as the sources to the modules are newer than the output files.
            /// </summary>
            InputFileIsNewer,

            /// <summary>
            /// Evaluation has been deferred because it cannot be evaluated before the build starts, e.g. console output of a built module is used as the contents of another file.
            /// </summary>
            DeferredEvaluation
        }

        private ExecuteReasoning(
            EReason reason,
            TokenizedString outputFilePath = null,
            TokenizedString inputFilePath = null)
        {
            this.Reason = reason;
            this.OutputFilePath = outputFilePath;
            this.InputFilePath = inputFilePath;
        }

        /// <summary>
        /// Utility method to create an instance of undefined (re)build.
        /// </summary>
        public static ExecuteReasoning
        Undefined()
        {
            return new ExecuteReasoning(EReason.Undefined);
        }

        /// <summary>
        /// Utility method to create an instance of file-does-not-exist (re)build, of the specified path.
        /// </summary>
        /// <returns>An instance of ExecuteReasoning.</returns>
        /// <param name="path">Module path that does not exist.</param>
        public static ExecuteReasoning
        FileDoesNotExist(
            TokenizedString path)
        {
            return new ExecuteReasoning(EReason.FileDoesNotExist, path);
        }

        /// <summary>
        /// Utility method to create an instance of source-file-newer (re)build, of the specified source and output paths.
        /// </summary>
        /// <returns>An instance of ExecuteReasoning.</returns>
        /// <param name="outputPath">Output path that is older.</param>
        /// <param name="inputPath">Source path that is newer.</param>
        public static ExecuteReasoning
        InputFileNewer(
            TokenizedString outputPath,
            TokenizedString inputPath)
        {
            return new ExecuteReasoning(EReason.InputFileIsNewer, outputPath, inputPath);
        }

        /// <summary>
        /// Utility method to create an instance of deferred-evaluation (re)build, of the specified output path.
        /// </summary>
        /// <returns>An instance of ExecuteReasoning.</returns>
        /// <param name="outputPath">Output path whose state must be deferred until the build.</param>
        /// <returns></returns>
        public static ExecuteReasoning
        DeferredUntilBuild(
            TokenizedString outputPath)
        {
            return new ExecuteReasoning(EReason.DeferredEvaluation, outputPath);
        }

        /// <summary>
        /// Convert the reasoning to a meaningful description. Used by the --explain command line option.
        /// </summary>
        /// <returns>A description of the (re)build reason.</returns>
        public override string
        ToString()
        {
            switch (this.Reason)
            {
                case EReason.Undefined:
                    return "of undefined reasons - therefore executing the module to err on the side of caution";

                case EReason.FileDoesNotExist:
                    return System.String.Format("{0} does not exist", this.OutputFilePath.Parse());

                case EReason.InputFileIsNewer:
                    {
                        if (this.InputFilePath == this.OutputFilePath)
                        {
                            return "member(s) of the module collection were updated";
                        }
                        return System.String.Format("{0} is newer than {1}", this.InputFilePath.Parse(), this.OutputFilePath.Parse());
                    }

                case EReason.DeferredEvaluation:
                    {
                        return System.String.Format("there is insufficient information during evaluation to determine the state of {0}",
                            this.OutputFilePath.Parse());
                    }

                default:
                    throw new Exception("Unknown execute reasoning, {0}", this.Reason.ToString());
            }
        }

        /// <summary>
        /// Obtain the (re)build enumeration.
        /// </summary>
        /// <value>The reason.</value>
        public EReason Reason
        {
            get;
            private set;
        }

        /// <summary>
        /// Obtain the (re)build output path, if it has been specified.
        /// </summary>
        /// <value>The output file path.</value>
        public TokenizedString OutputFilePath
        {
            get;
            private set;
        }

        /// <summary>
        /// Obtain the (re)build source path, if it has been specified.
        /// </summary>
        /// <value>The input file path.</value>
        public TokenizedString InputFilePath
        {
            get;
            private set;
        }
    }
}
