#region License
// Copyright 2010-2015 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
namespace MakeFileBuilder
{
namespace V2
{
    using System.Linq;
    public sealed class MakeFileMeta
    {
        public MakeFileMeta(Bam.Core.V2.Module module)
        {
            this.Prequisities = new System.Collections.Generic.Dictionary<Bam.Core.V2.Module, Bam.Core.V2.FileKey>();
            this.Rules = new System.Collections.Generic.List<string>();

            if (Bam.Core.V2.Graph.Instance.IsReferencedModule(module))
            {
                this.TargetVariable = module.GetType().Name;
            }

            module.MetaData = this;
        }

        public string Target
        {
            get;
            set;
        }

        public System.Collections.Generic.Dictionary<Bam.Core.V2.Module, Bam.Core.V2.FileKey> Prequisities
        {
            get;
            private set;
        }

        public System.Collections.Generic.List<string> Rules
        {
            get;
            private set;
        }

        public string TargetVariable
        {
            get;
            private set;
        }

        public static void PreExecution()
        {
        }

        public static void PostExecution()
        {
            var graph = Bam.Core.V2.Graph.Instance;

            var command = new System.Text.StringBuilder();

            command.Append("all:");
            foreach (var module in graph.TopLevelModules)
            {
                var metadata = module.MetaData as MakeFileMeta;
                if (null == metadata)
                {
                    throw new Bam.Core.Exception("Top level module did not have any Make metadata");
                }
                command.AppendFormat("$({0}) ", metadata.TargetVariable);
            }
            command.AppendLine();

            foreach (var rank in graph.Reverse())
            {
                foreach (var module in rank)
                {
                    var metadata = module.MetaData as MakeFileMeta;
                    if (null == metadata)
                    {
                        continue;
                    }

                    if (metadata.TargetVariable != null)
                    {
                        command.AppendFormat("{0}:={1}", metadata.TargetVariable, metadata.Target);
                        command.AppendLine();
                        command.AppendFormat("$({0}):", metadata.TargetVariable);
                    }
                    else
                    {
                        command.AppendFormat("{0}:", metadata.Target);
                    }
                    foreach (var pre in metadata.Prequisities)
                    {
                        command.AppendFormat("{0} ", pre.Key.GeneratedPaths[pre.Value]);
                    }
                    command.AppendLine();
                    foreach (var rule in metadata.Rules)
                    {
                        command.AppendFormat("\t{0}", rule);
                        command.AppendLine();
                    }
                }
            }
            Bam.Core.Log.DebugMessage(command.ToString());
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
