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
namespace MakeFileBuilder
{
    public sealed class Target
    {
        private static object UniqueCounterGuard = new object();
        private static int UniqueCounter = 0; // TODO: this is probably not the best way of creating a unique name for all unreferenced modules

        public static bool
        IsPrerequisiteOfAll(
            Bam.Core.Module module)
        {
            return Bam.Core.Graph.Instance.IsReferencedModule(module);
        }

        public static string
        GetUnReferencedVariableName(
            Bam.Core.Module module,
            string keyName)
        {
            var encapsulating = module.GetEncapsulatingReferencedModule();
            System.Diagnostics.Debug.Assert(encapsulating != module);
            lock (UniqueCounterGuard)
            {
                return System.String.Format(
                    "{0}_{1}_{2}_{3}_{4}",
                    encapsulating.GetType().Name,
                    module.GetType().Name,
                    keyName,
                    module.BuildEnvironment.Configuration.ToString(),
                    UniqueCounter++
                );
            }
        }

        public Target(
            Bam.Core.TokenizedString nameOrOutput,
            bool isPhony,
            string variableName,
            Bam.Core.Module module,
            int ruleIndex,
            string keyName,
            bool isDependencyOfAll)
        {
            this.Path = nameOrOutput;
            this.IsPhony = isPhony;
            this.IsPrerequisiteofAll =
                isDependencyOfAll ||
                (module != null && Bam.Core.Graph.Instance.IsReferencedModule(module)) ||
                !System.String.IsNullOrEmpty(variableName);
            if (isPhony)
            {
                return;
            }
            if (ruleIndex > 0)
            {
                return;
            }
            if (!this.IsPrerequisiteofAll)
            {
                return;
            }

            // make the target names unique across configurations
            var varName = new System.Text.StringBuilder();
            if (!System.String.IsNullOrEmpty(variableName))
            {
                varName.Append(variableName);
            }
            else
            {
                if (Bam.Core.Graph.Instance.IsReferencedModule(module))
                {
                    varName.Append(module.ToString());
                }
                else
                {
                    var encapsulated = module.GetEncapsulatingReferencedModule();
                    varName.Append(encapsulated.ToString());
                }
                if (null != keyName)
                {
                    varName.AppendFormat("_{0}", keyName);
                }
            }
            varName.AppendFormat("_{0}", module.BuildEnvironment.Configuration.ToString());
            var expanded_variable = varName.ToString();
            MakeFileMeta.MakeVariableNameUnique(ref expanded_variable);
            this.VariableName = expanded_variable;
        }

        public bool
        IsPrerequisiteofAll
        {
            get;
            private set;
        }

        public Bam.Core.TokenizedString Path
        {
            get;
            private set;
        }

        public bool IsPhony
        {
            get;
            private set;
        }

        public string VariableName
        {
            get;
            private set;
        }
    }
}
