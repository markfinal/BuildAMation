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
using System.Linq;
namespace MakeFileBuilder
{
    public sealed class Rule
    {
        /// <summary>
        /// Create an instance of a rule
        /// </summary>
        /// <param name="module">for this Module</param>
        /// <param name="count">and is this index</param>
        public Rule(
            Bam.Core.Module module,
            int count)
        {
            this.RuleIndex = count;
            this.Module = module;
            this.Targets = new Bam.Core.Array<Target>();
            this.Prequisities = new System.Collections.Generic.Dictionary<Bam.Core.Module, string>();
            this.PrerequisiteTargets = new Bam.Core.Array<Target>();
            this.ShellCommands = new Bam.Core.StringArray();
            this.OrderOnlyDependencies = new Bam.Core.Array<Target>();
            this.AddOrderOnlyDependency(MakeFileCommonMetaData.DIRSTarget);
        }

        /// <summary>
        /// Add a target to the rule
        /// </summary>
        /// <param name="targetNameOrOutput">Name or output path</param>
        /// <param name="isPhony">Is the target phony</param>
        /// <param name="variableName">Variable name to reference the target</param>
        /// <param name="keyName">Key name to reference target</param>
        /// <param name="isDependencyOfAll">Is the target a dependency of all</param>
        /// <returns></returns>
        public Target
        AddTarget(
            Bam.Core.TokenizedString targetNameOrOutput,
            bool isPhony = false,
            string variableName = null,
            string keyName = null,
            bool isDependencyOfAll = false)
        {
            if (!Target.IsPrerequisiteOfAll(this.Module) && isDependencyOfAll)
            {
                System.Diagnostics.Debug.Assert(null == variableName);
                variableName = Target.MakeUniqueVariableName(
                    this.Module,
                    keyName
                );
            }
            var target = new Target(
                targetNameOrOutput,
                isPhony,
                variableName,
                this.Module,
                this.RuleIndex,
                keyName,
                isDependencyOfAll
            );
            lock (this.Targets)
            {
                this.Targets.Add(target);
            }
            return target;
        }

        /// <summary>
        /// Add a prerequisite
        /// </summary>
        /// <param name="module">Module that is the prerequisite</param>
        /// <param name="key">Key name</param>
        public void
        AddPrerequisite(
            Bam.Core.Module module,
            string key)
        {
            if (!this.Prequisities.ContainsKey(module))
            {
                this.Prequisities.Add(module, key);
            }
        }

        /// <summary>
        /// Add a prerequisite target
        /// </summary>
        /// <param name="target">Target to add as prerequisite</param>
        public void
        AddPrerequisite(
            Target target)
        {
            this.PrerequisiteTargets.Add(target);
        }

        /// <summary>
        /// Add a shell command to the rul
        /// </summary>
        /// <param name="command">Single command to add</param>
        /// <param name="ignoreErrors">Optional, should errors be ignored? Default to false</param>
        public void
        AddShellCommand(
            string command,
            bool ignoreErrors = false)
        {
            if (ignoreErrors)
            {
                this.ShellCommands.Add($"-{command}");
            }
            else
            {
                this.ShellCommands.Add(command);
            }
        }

        /// <summary>
        /// Append target names that are prerequisites of the all target
        /// </summary>
        /// <param name="variableNames">List of target variable names acquired</param>
        public void
        AppendAllPrerequisiteTargetNames(
            Bam.Core.StringArray variableNames)
        {
            lock (this.Targets)
            {
                foreach (var target in this.Targets)
                {
                    if (!target.IsPrerequisiteofAll)
                    {
                        continue;
                    }
                    var name = target.VariableName;
                    if (null != name)
                    {
                        variableNames.AddUnique($"$({name})");
                    }
                    else
                    {
                        variableNames.AddUnique(target.Path.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Add an order only dependency
        /// </summary>
        /// <param name="target">of this Target</param>
        public void
        AddOrderOnlyDependency(
            Target target)
        {
            lock (this.OrderOnlyDependencies)
            {
                this.OrderOnlyDependencies.AddUnique(target);
            }
        }

        /// <summary>
        /// Write the variables associated with this rule to a string builder
        /// </summary>
        /// <param name="variables">Where to write the rule variables to</param>
        /// <param name="commonMeta">Meta data to append to</param>
        public void
        WriteVariables(
            System.Text.StringBuilder variables,
            MakeFileCommonMetaData commonMeta)
        {
            bool hasShellCommands = this.ShellCommands.Any();
            if (hasShellCommands)
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
                        variables.AppendLine($".PHONY: {name}");
                    }

                    // simply expanded variable
                    lock (target.Path)
                    {
                        if (!target.Path.IsParsed)
                        {
                            // some sources may be generated after the string parsing phase
                            target.Path.Parse();
                        }
                    }
                    if (MakeFileCommonMetaData.IsNMAKE)
                    {
                        variables.AppendLine(
                            $"{name} = {commonMeta.UseMacrosInPath(target.Path.ToString())}"
                        );
                        variables.AppendLine();
                    }
                    else
                    {
                        variables.AppendLine(
                            $"{name}:={commonMeta.UseMacrosInPath(target.Path.ToString())}"
                        );
                    }
                }
            }
            else
            {
                foreach (var target in this.Targets)
                {
                    var name = target.VariableName;
                    if (null == name)
                    {
                        name = target.Path.ToString();
                    }

                    variables.Append($"{name}=");
                    foreach (var pre in this.PrerequisiteTargets)
                    {
                        var preName = pre.VariableName;
                        if (null == preName)
                        {
                            variables.Append(
                                $"{commonMeta.UseMacrosInPath(pre.Path.ToString())} "
                            );
                        }
                        else
                        {
                            variables.Append(
                                $"$({commonMeta.UseMacrosInPath(preName)}) "
                            );
                        }
                    }
                    variables.AppendLine();
                }
            }
        }

        private static void
        EscapeCharacter(
            ref string input,
            char toReplace)
        {
            var offset = 0;
            for (; ; )
            {
                var index = input.IndexOf(toReplace, offset);
                if (-1 == index)
                {
                    break;
                }
                var charBefore = input[index - 1];
                if ('$' == charBefore)
                {
                    offset = index + 1;
                    continue;
                }
                input = input.Substring(0, index) + '\\' + input.Substring(index);
                offset = index + 2;
            }
        }

        /// <summary>
        /// Write the rules to a string builder
        /// </summary>
        /// <param name="rules">Where to write to</param>
        /// <param name="commonMeta">Meta data to append</param>
        public void
        WriteRules(
            System.Text.StringBuilder rules,
            MakeFileCommonMetaData commonMeta)
        {
            bool hasShellCommands = this.ShellCommands.Any();
            if (!hasShellCommands)
            {
                return;
            }
            foreach (var target in this.Targets)
            {
                var name = target.VariableName;
                if (null != name)
                {
                    rules.Append($"$({name}):");
                }
                else
                {
                    if (target.IsPhony)
                    {
                        rules.AppendLine($".PHONY: {target.Path}");
                    }
                    rules.Append($"{commonMeta.UseMacrosInPath(target.Path.ToString())}:");
                }

                // non-first targets just require the first target to exist
                // see https://stackoverflow.com/questions/2973445/gnu-makefile-rule-generating-a-few-targets-from-a-single-source-file
                if (target != this.FirstTarget)
                {
                    var firstTargetname = this.FirstTarget.VariableName;
                    if (null != firstTargetname)
                    {
                        rules.AppendLine($"$({firstTargetname})");
                    }
                    else
                    {
                        rules.AppendLine($"{commonMeta.UseMacrosInPath(this.FirstTarget.Path.ToString())}");
                    }
                    continue;
                }

                foreach (var pre in this.Prequisities)
                {
                    if (!pre.Key.GeneratedPaths.ContainsKey(pre.Value))
                    {
                        continue;
                    }
                    rules.Append(
                        $"{commonMeta.UseMacrosInPath(pre.Key.GeneratedPaths[pre.Value].ToStringQuoteIfNecessary())} "
                    );
                }
                foreach (var pre in this.PrerequisiteTargets)
                {
                    var preName = pre.VariableName;
                    if (null == preName)
                    {
                        rules.Append(
                            $"{commonMeta.UseMacrosInPath(pre.Path.ToString())} "
                        );
                    }
                    else
                    {
                        rules.Append(
                            $"$({commonMeta.UseMacrosInPath(preName)}) "
                        );
                    }
                }
                if (MakeFileCommonMetaData.IsNMAKE)
                {
                    // NMake offers no support for order only dependents
                    // so must just list them as ordinary dependencies, and the recipe must filter appropriately
                    // for how it uses dependencies
                }
                else
                {
                    if (this.OrderOnlyDependencies.Any())
                    {
                        rules.Append("| ");
                    }
                }
                foreach (var ood in this.OrderOnlyDependencies)
                {
                    var oodName = ood.VariableName;
                    if (null == oodName)
                    {
                        rules.Append(
                            $"{commonMeta.UseMacrosInPath(ood.Path.ToString())} "
                        );
                    }
                    else
                    {
                        rules.Append(
                            $"$({commonMeta.UseMacrosInPath(oodName)}) "
                        );
                    }
                }
                rules.AppendLine();
                foreach (var command in this.ShellCommands)
                {
                    var macro_command = command.Replace(this.FirstTarget.Path.ToString(), "$@");
                    if (!MakeFileCommonMetaData.IsNMAKE)
                    {
                        // check paths first, as it's more likely to be a source file/object file etc
                        if (null != this.FirstPrerequisitePath)
                        {
                            macro_command = macro_command.Replace(this.FirstPrerequisitePath.ToString(), "$<");
                        }
                        else if (null != this.FirstPrerequisiteTarget)
                        {
                            macro_command = macro_command.Replace(this.FirstPrerequisiteTarget.Path.ToString(), "$<");
                        }
                        // look for text like $ORIGIN, which needs a double $ prefix (and quotes) to avoid being interpreted as an environment variable by Make
                        var escapedCommand = System.Text.RegularExpressions.Regex.Replace(macro_command, @"\$([A-Za-z0-9]+)", @"'$$$$$1'");
                        // any parentheses that are not associated with MakeFile commands must be escaped
                        if (!System.Text.RegularExpressions.Regex.IsMatch(escapedCommand, @"\$\(.*\)"))
                        {
                            EscapeCharacter(ref escapedCommand, '(');
                            EscapeCharacter(ref escapedCommand, ')');
                        }
                        // perform macro replacement after regex, otherwise it may match $(var)
                        escapedCommand = commonMeta.UseMacrosInPath(escapedCommand);
                        rules.AppendLine($"\t{escapedCommand}");
                    }
                    else
                    {
                        macro_command = commonMeta.UseMacrosInPath(macro_command);
                        rules.AppendLine($"\t{macro_command}");
                    }
                }
            }
        }

        /// <summary>
        /// Get the first target of this rule
        /// </summary>
        public Target
        FirstTarget
        {
            get
            {
                lock (this.Targets)
                {
                    return this.Targets.FirstOrDefault();
                }
            }
        }

        /// <summary>
        /// Get the first prerequisite target of this rule
        /// </summary>
        private Target
        FirstPrerequisiteTarget
        {
            get
            {
                lock (this.PrerequisiteTargets)
                {
                    return this.PrerequisiteTargets.FirstOrDefault();
                }
            }
        }

        /// <summary>
        /// Get the first prerequisite path of this rule
        /// </summary>
        private Bam.Core.TokenizedString
        FirstPrerequisitePath
        {
            get
            {
                lock (this.Prequisities)
                {
                    var first = this.Prequisities.FirstOrDefault();
                    if (first.Equals(default(System.Collections.Generic.KeyValuePair<Bam.Core.Module,string>)))
                    {
                        return null;
                    }
                    else
                    {
                        if (!first.Key.GeneratedPaths.ContainsKey(first.Value))
                        {
                            return null;
                        }
                        return first.Key.GeneratedPaths[first.Value];
                    }
                }
            }
        }

        /// <summary>
        /// Delegate for enumerating across targets
        /// </summary>
        /// <param name="target">Current target</param>
        public delegate void eachTargetDelegate(Target target);

        /// <summary>
        /// Utility function to enumerate across all targets in this rule
        /// </summary>
        /// <param name="dlg">Apply this delegate to each target</param>
        public void
        ForEachTarget(
            eachTargetDelegate dlg)
        {
            lock (this.Targets)
            {
                foreach (var target in this.Targets)
                {
                    dlg(target);
                }
            }
        }

        private int RuleIndex { get; set; }
        private Bam.Core.Module Module { get; set; }
        private Bam.Core.Array<Target> Targets { get; set; }
        private System.Collections.Generic.Dictionary<Bam.Core.Module, string> Prequisities { get; set; }
        private Bam.Core.Array<Target> PrerequisiteTargets { get; set; }
        private Bam.Core.StringArray ShellCommands { get; set; }
        private Bam.Core.Array<Target> OrderOnlyDependencies { get; set; }
    }
}
