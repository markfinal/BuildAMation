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
namespace MakeFileBuilder
{
namespace V2
{
    // Notes:
    // A rule is target + prerequisities + receipe
    // A recipe is a collection of commands
    using System.Linq;

    public sealed class MakeFileCommonMetaData
    {
        public MakeFileCommonMetaData()
        {
            this.Directories = new Bam.Core.StringArray();
            this.Environment = new System.Collections.Generic.Dictionary<string, Bam.Core.StringArray>();
            if (Bam.Core.OSUtilities.IsUnixHosting)
            {
                // for system utilities, e.g. mkdir, cp, echo
                this.Environment.Add("PATH", new Bam.Core.StringArray("/bin"));
            }
        }

        public Bam.Core.StringArray Directories
        {
            get;
            private set;
        }

        public System.Collections.Generic.Dictionary<string, Bam.Core.StringArray> Environment
        {
            get;
            private set;
        }

        public void
        ExtendEnvironmentVariables(
            System.Collections.Generic.Dictionary<string, Bam.Core.V2.TokenizedStringArray> import)
        {
            foreach (var env in import)
            {
                if (!this.Environment.ContainsKey(env.Key))
                {
                    this.Environment.Add(env.Key, new Bam.Core.StringArray());
                }
                foreach (var path in env.Value)
                {
                    this.Environment[env.Key].AddUnique(path.ToString());
                }
            }
        }
    }

    public sealed class Target
    {
        public Target(
            Bam.Core.V2.TokenizedString nameOrOutput,
            bool isPhony,
            Bam.Core.V2.Module module,
            int count)
        {
            this.Path = nameOrOutput;
            this.IsPhony = isPhony;
            if (isPhony)
            {
                return;
            }
            if (count > 0)
            {
                return;
            }
            if (Bam.Core.V2.Graph.Instance.IsReferencedModule(module))
            {
                // make the target names unique across configurations
                this.VariableName = System.String.Format("{0}_{1}", module.GetType().Name, module.BuildEnvironment.Configuration.ToString());
            }
        }

        public Bam.Core.V2.TokenizedString Path
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

    public sealed class Rule
    {
        public Rule(
            Bam.Core.V2.Module module,
            int count)
        {
            this.RuleCount = count;
            this.Module = module;
            this.Targets = new Bam.Core.Array<Target>();
            this.Prequisities = new System.Collections.Generic.Dictionary<Bam.Core.V2.Module, Bam.Core.V2.FileKey>();
            this.PrerequisiteTargets = new Bam.Core.Array<Target>();
            this.PrerequisitePaths = new Bam.Core.Array<Bam.Core.V2.TokenizedString>();
            this.ShellCommands = new Bam.Core.StringArray();
            this.OrderOnlyDependencies = new Bam.Core.StringArray();
            this.OrderOnlyDependencies.Add("$(DIRS)");
        }

        public Target
        AddTarget(
            Bam.Core.V2.TokenizedString targetNameOrOutput,
            bool isPhony = false)
        {
            var target = new Target(targetNameOrOutput, isPhony, this.Module, this.RuleCount);
            this.Targets.Add(target);
            return target;
        }

        public void
        AddPrerequisite(
            Bam.Core.V2.Module module,
            Bam.Core.V2.FileKey key)
        {
            if (this.Prequisities.ContainsKey(module))
            {
                throw new Bam.Core.Exception("Module {0} is already a prerequisite", module.ToString());
            }

            this.Prequisities.Add(module, key);
        }

        public void
        AddPrerequisite(
            Bam.Core.V2.TokenizedString path)
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
            string command)
        {
            this.ShellCommands.Add(command);
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
                variables.AppendFormat("{0}:={1}", name, target.Path);
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
                    rules.AppendFormat("{0} ", pre);
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
                    // look for text like $ORIGIN, which needs a double $ prefix to avoid being interpreted as an environment variable by Make
                    var escapedCommand = System.Text.RegularExpressions.Regex.Replace(command, @"\$([A-Za-z0-9]+)", @"$$$$$1");
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

        private Bam.Core.V2.Module Module
        {
            get;
            set;
        }

        public Bam.Core.Array<Target> Targets
        {
            get;
            private set;
        }

        private System.Collections.Generic.Dictionary<Bam.Core.V2.Module, Bam.Core.V2.FileKey> Prequisities
        {
            get;
            set;
        }

        private Bam.Core.Array<Target> PrerequisiteTargets
        {
            get;
            set;
        }

        private Bam.Core.Array<Bam.Core.V2.TokenizedString> PrerequisitePaths
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

    public sealed class MakeFileMeta
    {
        public MakeFileMeta(
            Bam.Core.V2.Module module)
        {
            this.Module = module;
            module.MetaData = this;
            this.CommonMetaData = Bam.Core.V2.Graph.Instance.MetaData as MakeFileCommonMetaData;
            this.Rules = new Bam.Core.Array<Rule>();
        }

        public MakeFileCommonMetaData CommonMetaData
        {
            get;
            private set;
        }

        public Rule
        AddRule()
        {
            var rule = new Rule(this.Module, this.Rules.Count);
            this.Rules.Add(rule);
            return rule;
        }

        public Bam.Core.Array<Rule> Rules
        {
            get;
            private set;
        }

        private Bam.Core.V2.Module Module
        {
            get;
            set;
        }

        public static void PreExecution()
        {
            var graph = Bam.Core.V2.Graph.Instance;
            graph.MetaData = new MakeFileCommonMetaData();
        }

        public static void PostExecution()
        {
            var graph = Bam.Core.V2.Graph.Instance;
            var commonMeta = graph.MetaData as MakeFileCommonMetaData;

            var makeEnvironment = new System.Text.StringBuilder();
            var makeVariables = new System.Text.StringBuilder();
            var makeRules = new System.Text.StringBuilder();

            // delete suffix rules
            makeEnvironment.AppendLine(".SUFFIXES:");
            foreach (var env in commonMeta.Environment)
            {
                makeEnvironment.AppendFormat("{0}:={1}", env.Key, env.Value.ToString(System.IO.Path.PathSeparator));
                makeEnvironment.AppendLine();
            }

            if (commonMeta.Directories.Count > 0)
            {
                makeVariables.Append("DIRS:=");
                foreach (var dir in commonMeta.Directories)
                {
                    makeVariables.AppendFormat("{0} ", dir);
                }
                makeVariables.AppendLine();
            }

            // prerequisites of target: all
            makeRules.Append("all:");
            var allPrerequisites = new Bam.Core.StringArray();
            foreach (var module in graph.TopLevelModules)
            {
                var metadata = module.MetaData as MakeFileMeta;
                if (null == metadata)
                {
                    throw new Bam.Core.Exception("Top level module, {0}, did not have any Make metadata", module.ToString());
                }
                foreach (var rule in metadata.Rules)
                {
                    // TODO: could just exit from the loop after the first iteration
                    if (!rule.IsFirstRule)
                    {
                        continue;
                    }
                    rule.AppendTargetNames(allPrerequisites);
                }
            }
            makeRules.AppendLine(allPrerequisites.ToString(' '));

            makeRules.AppendLine("$(DIRS):");
            if (Bam.Core.OSUtilities.IsWindowsHosting)
            {
                makeRules.AppendLine("\tmkdir $@");
            }
            else
            {
                makeRules.AppendLine("\tmkdir -pv $@");
            }

            foreach (var rank in graph.Reverse())
            {
                foreach (var module in rank)
                {
                    var metadata = module.MetaData as MakeFileMeta;
                    if (null == metadata)
                    {
                        continue;
                    }

                    foreach (var rule in metadata.Rules)
                    {
                        rule.WriteVariables(makeVariables);
                        rule.WriteRules(makeRules);
                    }
                }
            }

            Bam.Core.Log.DebugMessage("MAKEFILE CONTENTS: BEGIN");
            Bam.Core.Log.DebugMessage(makeEnvironment.ToString());
            Bam.Core.Log.DebugMessage(makeVariables.ToString());
            Bam.Core.Log.DebugMessage(makeRules.ToString());
            Bam.Core.Log.DebugMessage("MAKEFILE CONTENTS: END");

            var makeFilePath = Bam.Core.V2.TokenizedString.Create("$(buildroot)/Makefile", null);
            makeFilePath.Parse();

            using (var writer = new System.IO.StreamWriter(makeFilePath.ToString()))
            {
                writer.Write(makeEnvironment.ToString());
                writer.Write(makeVariables.ToString());
                writer.Write(makeRules.ToString());
            }
        }
    }
}
    public sealed class MakeFileData
    {
        public
        MakeFileData(
            string makeFilePath,
            MakeFileTargetDictionary targetDictionary,
            MakeFileVariableDictionary variableDictionary,
            System.Collections.Generic.Dictionary<string, Bam.Core.StringArray> environment)
        {
            this.MakeFilePath = makeFilePath;
            this.TargetDictionary = targetDictionary;
            this.VariableDictionary = variableDictionary;
            if (null != environment)
            {
                // TODO: better way to do a copy?
                this.Environment = new System.Collections.Generic.Dictionary<string, Bam.Core.StringArray>();
                foreach (var key in environment.Keys)
                {
                    this.Environment[key] = environment[key];
                }
            }
            else
            {
                this.Environment = null;
            }
        }

        public string MakeFilePath
        {
            get;
            private set;
        }

        public MakeFileTargetDictionary TargetDictionary
        {
            get;
            private set;
        }

        public MakeFileVariableDictionary VariableDictionary
        {
            get;
            private set;
        }

        public System.Collections.Generic.Dictionary<string, Bam.Core.StringArray> Environment
        {
            get;
            private set;
        }
    }
}
