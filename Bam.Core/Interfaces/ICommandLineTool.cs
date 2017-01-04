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
    /// Interface that defines a tool run on the command line (as opposed to a tool that can run entirely in C#).
    /// </summary>
    public interface ICommandLineTool :
        ITool
    {
        /// <summary>
        /// Obtain the dictionary of custom environment variables (name, array of values) to set when the tool is executed.
        /// </summary>
        /// <value>The environment variables.</value>
        System.Collections.Generic.Dictionary<string, TokenizedStringArray> EnvironmentVariables
        {
            get;
        }

        /// <summary>
        /// Obtain the list of environment variable names to inherit from the current environment when the tool is executed.
        /// </summary>
        /// <value>The inherited environment variables.</value>
        StringArray InheritedEnvironmentVariables
        {
            get;
        }

        /// <summary>
        /// Obtain the path to the executable to run. This could be a literal string, or the output from a module build.
        /// </summary>
        /// <value>The executable.</value>
        TokenizedString Executable
        {
            get;
        }

        /// <summary>
        /// Define any arguments that must appear directly after the executable, e.g. on Windows, any DOS commands must use
        /// an executable of CMD, and the command itself comes after, followed by the arguments to that command.
        /// </summary>
        /// <value>The initial arguments.</value>
        TokenizedStringArray InitialArguments
        {
            get;
        }

        /// <summary>
        /// Define any arguments that must appear directly after all other arguments, e.g. on Linux, commands that
        /// expect wildcard expansion must use an executable of bash, and the command itself comes after (with a -c),
        /// followed by the arguments to that command. However, the command and it's own arguments, must be quoted.
        /// The TerminatingArguments can be used to add that last quote.
        /// </summary>
        /// <value>The terminating arguments to a command line.</value>
        TokenizedStringArray TerminatingArguments
        {
            get;
        }

        /// <summary>
        /// Get the option to use a response file, or null if this is not supported.
        /// </summary>
        /// <value>The use response file option.</value>
        string UseResponseFileOption
        {
            get;
        }
    }
}
