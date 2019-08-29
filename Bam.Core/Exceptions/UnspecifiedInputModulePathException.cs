#region License
// Copyright (c) 2010-2019, Mark Final
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
    /// Exception representing unspecified pathkeys for an input modules.
    /// </summary>
    public sealed class UnspecifiedInputModulePathException :
        Exception
    {
        /// <summary>
        /// Construct an instance
        /// </summary>
        /// <param name="module">The Module needing to be built</param>
        /// <param name="dependents">Enumeration of the Modules requiring pathkeys</param>
        public UnspecifiedInputModulePathException(
            Module module,
            System.Collections.Generic.IEnumerable<Module> dependents) :
            base(CreateMessage(module, dependents))
        {
        }

        private static string
        CreateMessage(
            Module module,
            System.Collections.Generic.IEnumerable<Module> dependents)
        {
            var message = new System.Text.StringBuilder();
            message.AppendLine($"The following dependents need pathkeys specified to indicate the input paths to Module '{module.ToString()}':");
            foreach (var dependent in dependents)
            {
                message.AppendLine($"\t{dependent.ToString()}");
            }
            message.AppendLine($"Ensure that the InputModulePaths property has been overridden in {module.ToString()}");
            return message.ToString();
        }
    }
}
