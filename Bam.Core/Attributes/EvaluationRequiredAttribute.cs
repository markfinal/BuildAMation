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
    /// There are two use cases for this attribute:
    /// 1) for a class implementing Bam.Core.IBuildModeMetaData (i.e. a build mode), the attribute is used to determine
    /// whether the build mode requires module evaluation to be executed. The default behaviour is NOT to require evaluation.
    /// 2) for a module class, the attribute is used to identify individual modules that override the build mode evaluation behaviour.
    /// i.e. to force evaluation on modules that perform actions slightly differently to other modules for the build mode. For example,
    /// for project generation build modes, some modules may procedurally generate files outside of the project build.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public sealed class EvaluationRequiredAttribute :
        System.Attribute
    {
        /// <summary>
        /// Construct an attribute.
        /// </summary>
        /// <param name="enabled">If set to <c>true</c>, evaluation is used on all modules.</param>
        public EvaluationRequiredAttribute(
            bool enabled)
        {
            this.Enabled = enabled;
        }

        /// <summary>
        /// Return whether evaluation is required for modules.
        /// </summary>
        /// <value><c>true</c> if evaluation is used; <c>false</c> if not.</value>
        public bool Enabled
        {
            get;
            private set;
        }
    }
}
