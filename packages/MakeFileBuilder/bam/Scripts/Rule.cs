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
namespace MakeFileBuilder
{
    public sealed class Rule
    {
        public Rule(
            Bam.Core.Module module,
            int count)
        {
            this.RuleCount = count;
            this.Module = module;
            this.Targets = new Bam.Core.Array<Target>();
            this.Prequisities = new System.Collections.Generic.Dictionary<Bam.Core.Module, Bam.Core.PathKey>();
            this.PrerequisiteTargets = new Bam.Core.Array<Target>();
            this.PrerequisitePaths = new Bam.Core.TokenizedStringArray();
            this.ShellCommands = new Bam.Core.StringArray();
            this.OrderOnlyDependencies = new Bam.Core.StringArray();
            this.OrderOnlyDependencies.Add("$(DIRS)");
        }

        public Target
        AddTarget(
            Bam.Core.TokenizedString targetNameOrOutput,
            bool isPhony = false,
            string variableName = null)
        {
            var target = new Target(targetNameOrOutput, isPhony, variableName, this.Module, this.RuleCount);
            this.Targets.Add(target);
            return target;
        }

        public void
        AddPrerequisite(
            Bam.Core.Module module,
            Bam.Core.PathKey key)
        {
            if (!this.Prequisities.ContainsKey(module))
            {
                this.Prequisities.Add(module, key);
            }
        }

        public void
        AddPrerequisite(
            Bam.Core.TokenizedString path)
        {
            this.PrerequisitePaths.Add(path);
        }

        public void
        AddPrerequisite(
            Target target)
        {
            this.PrerequisiteTargets.Add(target);
        }

        public void
        AddShellCommand(
            string command,
            bool ignoreErrors = false)
        {
            if (ignoreErrors)
            {
                this.ShellCommands.Add(System.String.Format("-{0}", command));
            }
            else
            {
                this.ShellCommands.Add(command);
            }
        }

        public bool
        IsFirstRule
        {
            get
            {
                return (this.RuleCount == 0);
            }
        }

        public void
        AppendTargetNames(
            Bam.Core.StringArray variableNames)
        {
            foreach (var target in this.Targets)
            {
                var name = target.VariableName;
                if (null != name)
                {
                    variableNames.AddUnique("$(" + name + ")");
                }
                else
                {
                    variableNames.AddUnique(target.Path.Parse());
                }
            }
        }

        public void
        AddOrderOnlyDependency(
            string ooDep)
        {
            this.OrderOnlyDependencies.AddUnique(ooDep);
        }

        public void
        WriteVariables(
            System.Text.StringBuilder variables)
        {
            foreach (var target in this.Targets)
            {
                var name = target.VariableName;
                if (null == name)
                {
                    continue;
                }

                if (target.IsPhony)
                {
                    variables.AppendFormat(".PHONY: {0}", name);
                    variables.AppendLine();
                }

                // simply expanded variable
                variables.AppendFormat("{0}:={1}", name, target.Path.Parse());
                variables.AppendLine();
            }
        }

        public void
        WriteRules(
            System.Text.StringBuilder rules)
        {
            foreach (var target in this.Targets)
            {
                var name = target.VariableName;
                if (null != name)
                {
                    rules.AppendFormat("$({0}):", name);
                }
                else
                {
                    if (target.IsPhony)
                    {
                        rules.AppendFormat(".PHONY: {0}", target.Path);
                        rules.AppendLine();
                    }
                    rules.AppendFormat("{0}:", target.Path);
                }
                foreach (var pre in this.Prequisities)
                {
                    rules.AppendFormat("{0} ", pre.Key.GeneratedPaths[pre.Value]);
                }
                foreach (var pre in this.PrerequisitePaths)
                {
                    rules.AppendFormat("{0} ", pre.ParseAndQuoteIfNecessary());
                }
                foreach (var pre in this.PrerequisiteTargets)
                {
                    var preName = pre.VariableName;
                    if (null == preName)
                    {
                        rules.AppendFormat("{0} ", pre.Path.Parse());
                    }
                    else
                    {
                        rules.AppendFormat("$({0}) ", preName);
                    }
                }
                if (this.OrderOnlyDependencies.Count > 0)
                {
                    rules.AppendFormat("| {0}", this.OrderOnlyDependencies.ToString(' '));
                }
                rules.AppendLine();
                foreach (var command in this.ShellCommands)
                {
                    // look for text like $ORIGIN, which needs a double $ prefix (and quotes) to avoid being interpreted as an environment variable by Make
                    var escapedCommand = System.Text.RegularExpressions.Regex.Replace(command, @"\$([A-Za-z0-9]+)", @"'$$$$$1'");
                    rules.AppendFormat("\t{0}", escapedCommand);
                    rules.AppendLine();
                }
            }
        }

        private int RuleCount
        {
            get;
            set;
        }

        private Bam.Core.Module Module
        {
            get;
            set;
        }

        public Bam.Core.Array<Target> Targets
        {
            get;
            private set;
        }

        private System.Collections.Generic.Dictionary<Bam.Core.Module, Bam.Core.PathKey> Prequisities
        {
            get;
            set;
        }

        private Bam.Core.Array<Target> PrerequisiteTargets
        {
            get;
            set;
        }

        private Bam.Core.TokenizedStringArray PrerequisitePaths
        {
            get;
            set;
        }

        private Bam.Core.StringArray ShellCommands
        {
            get;
            set;
        }

        private Bam.Core.StringArray OrderOnlyDependencies
        {
            get;
            set;
        }
    }
}
