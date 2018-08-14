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
using System.Linq;
namespace MakeFileBuilder
{
    public sealed class Rule
    {
        public Rule(
            Bam.Core.Module module,
            int count)
        {
            this.RuleIndex = count;
            this.Module = module;
            this.Targets = new Bam.Core.Array<Target>();
#if BAM_V2
            this.Prequisities = new System.Collections.Generic.Dictionary<Bam.Core.Module, string>();
#else
            this.Prequisities = new System.Collections.Generic.Dictionary<Bam.Core.Module, Bam.Core.PathKey>();
#endif
            this.PrerequisiteTargets = new Bam.Core.Array<Target>();
#if BAM_V2
#else
            this.PrerequisitePaths = new Bam.Core.TokenizedStringArray();
#endif
            this.ShellCommands = new Bam.Core.StringArray();
            this.OrderOnlyDependencies = new Bam.Core.Array<Target>();
            this.AddOrderOnlyDependency(MakeFileCommonMetaData.DIRSTarget);
        }

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

        public void
        AddPrerequisite(
            Bam.Core.Module module,
#if BAM_V2
            string key)
#else
            Bam.Core.PathKey key)
#endif
        {
            if (!this.Prequisities.ContainsKey(module))
            {
                this.Prequisities.Add(module, key);
            }
        }

#if BAM_V2
#else
        public void
        AddPrerequisite(
            Bam.Core.TokenizedString path)
        {
            this.PrerequisitePaths.Add(path);
        }
#endif

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
                        variableNames.AddUnique("$(" + name + ")");
                    }
                    else
                    {
                        variableNames.AddUnique(target.Path.ToString());
                    }
                }
            }
        }

        public void
        AddOrderOnlyDependency(
            Target target)
        {
            lock (this.OrderOnlyDependencies)
            {
                this.OrderOnlyDependencies.AddUnique(target);
            }
        }

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
                        variables.AppendFormat(".PHONY: {0}", name);
                        variables.AppendLine();
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
                        variables.AppendFormat(
                            "{0} = {1}",
                            name,
                            commonMeta.UseMacrosInPath(target.Path.ToString())
                        );
                        variables.AppendLine();
                        variables.AppendLine();
                    }
                    else
                    {
                        variables.AppendFormat(
                            "{0}:={1}",
                            name,
                            commonMeta.UseMacrosInPath(target.Path.ToString())
                        );
                        variables.AppendLine();
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

                    variables.AppendFormat("{0}=", name);
                    foreach (var pre in this.PrerequisiteTargets)
                    {
                        var preName = pre.VariableName;
                        if (null == preName)
                        {
                            variables.AppendFormat(
                                "{0} ",
                                commonMeta.UseMacrosInPath(pre.Path.ToString())
                            );
                        }
                        else
                        {
                            variables.AppendFormat(
                                "$({0}) ",
                                commonMeta.UseMacrosInPath(preName)
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
                    rules.AppendFormat("$({0}):", name);
                }
                else
                {
                    if (target.IsPhony)
                    {
                        rules.AppendFormat(".PHONY: {0}", target.Path);
                        rules.AppendLine();
                    }
                    rules.AppendFormat("{0}:", commonMeta.UseMacrosInPath(target.Path.ToString()));
                }

                // non-first targets just require the first target to exist
                // see https://stackoverflow.com/questions/2973445/gnu-makefile-rule-generating-a-few-targets-from-a-single-source-file
                if (target != this.FirstTarget)
                {
                    var firstTargetname = this.FirstTarget.VariableName;
                    if (null != firstTargetname)
                    {
                        rules.AppendFormat("$({0})", firstTargetname);
                    }
                    else
                    {
                        rules.AppendFormat("{0}", commonMeta.UseMacrosInPath(this.FirstTarget.Path.ToString()));
                    }
                    rules.AppendLine();
                    continue;
                }

                foreach (var pre in this.Prequisities)
                {
                    rules.AppendFormat(
                        "{0} ",
                        commonMeta.UseMacrosInPath(pre.Key.GeneratedPaths[pre.Value].ToStringQuoteIfNecessary())
                    );
                }
#if BAM_V2
#else
                foreach (var pre in this.PrerequisitePaths)
                {
                    lock (pre)
                    {
                        if (!pre.IsParsed)
                        {
                            pre.Parse();
                        }
                    }
                    rules.AppendFormat(
                        "{0} ",
                        commonMeta.UseMacrosInPath(pre.ToStringQuoteIfNecessary())
                    );
                }
#endif
                foreach (var pre in this.PrerequisiteTargets)
                {
                    var preName = pre.VariableName;
                    if (null == preName)
                    {
                        rules.AppendFormat(
                            "{0} ",
                            commonMeta.UseMacrosInPath(pre.Path.ToString())
                        );
                    }
                    else
                    {
                        rules.AppendFormat(
                            "$({0}) ",
                            commonMeta.UseMacrosInPath(preName)
                        );
                    }
                }
                if (MakeFileCommonMetaData.IsNMAKE)
                {
                    // NMake offers no support for order only dependents
                }
                else
                {
                    if (this.OrderOnlyDependencies.Any())
                    {
                        rules.AppendFormat(
                            "| "
                        );
                    }
                    foreach (var ood in this.OrderOnlyDependencies)
                    {
                        var oodName = ood.VariableName;
                        if (null == oodName)
                        {
                            rules.AppendFormat(
                                "{0} ",
                                commonMeta.UseMacrosInPath(ood.Path.ToString())
                            );
                        }
                        else
                        {
                            rules.AppendFormat(
                                "$({0}) ",
                                commonMeta.UseMacrosInPath(oodName)
                            );
                        }
                    }
                }
                rules.AppendLine();
                foreach (var command in this.ShellCommands)
                {
                    if (!MakeFileCommonMetaData.IsNMAKE)
                    {
                        // look for text like $ORIGIN, which needs a double $ prefix (and quotes) to avoid being interpreted as an environment variable by Make
                        var escapedCommand = System.Text.RegularExpressions.Regex.Replace(command, @"\$([A-Za-z0-9]+)", @"'$$$$$1'");
                        // any parentheses that are not associated with MakeFile commands must be escaped
                        if (!System.Text.RegularExpressions.Regex.IsMatch(escapedCommand, @"\$\(.*\)"))
                        {
                            EscapeCharacter(ref escapedCommand, '(');
                            EscapeCharacter(ref escapedCommand, ')');
                        }
                        rules.AppendFormat("\t{0}", escapedCommand);
                        rules.AppendLine();
                    }
                    else
                    {
                        rules.AppendFormat("\t{0}", command);
                        rules.AppendLine();
                    }
                }
            }
        }

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

        public delegate void eachTargetDelegate(Target target);

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

        public bool
        AnyTargetUsesVariableName(
            string variableName)
        {
            lock (this.Targets)
            {
                foreach (var target in this.Targets)
                {
                    if (target.VariableName == variableName)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private int RuleIndex
        {
            get;
            set;
        }

        private Bam.Core.Module Module
        {
            get;
            set;
        }

        private Bam.Core.Array<Target> Targets
        {
            get;
            set;
        }

#if BAM_V2
        private System.Collections.Generic.Dictionary<Bam.Core.Module, string> Prequisities
#else
        private System.Collections.Generic.Dictionary<Bam.Core.Module, Bam.Core.PathKey> Prequisities
#endif
        {
            get;
            set;
        }

        private Bam.Core.Array<Target> PrerequisiteTargets
        {
            get;
            set;
        }

#if BAM_V2
#else
        private Bam.Core.TokenizedStringArray PrerequisitePaths
        {
            get;
            set;
        }
#endif

        private Bam.Core.StringArray ShellCommands
        {
            get;
            set;
        }

        private Bam.Core.Array<Target> OrderOnlyDependencies
        {
            get;
            set;
        }
    }
}
