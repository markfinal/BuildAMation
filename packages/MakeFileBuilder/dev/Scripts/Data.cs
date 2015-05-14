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
    // Notes:
    // A rule is target + prerequisities + receipe
    // A recipe is a collection of commands
    using System.Linq;

    public sealed class MakeFileCommonMetaData
    {
        public MakeFileCommonMetaData()
        {
            this.Directories = new Bam.Core.StringArray();
        }

        public Bam.Core.StringArray Directories
        {
            get;
            private set;
        }
    }

    public sealed class MakeFileMeta
    {
        public MakeFileMeta(Bam.Core.V2.Module module)
        {
            this.Prequisities = new System.Collections.Generic.Dictionary<Bam.Core.V2.Module, Bam.Core.V2.FileKey>();
            this.Recipe = new System.Collections.Generic.List<string>();

            if (Bam.Core.V2.Graph.Instance.IsReferencedModule(module))
            {
                this.TargetVariable = module.GetType().Name;
            }

            module.MetaData = this;
            this.CommonMetaData = Bam.Core.V2.Graph.Instance.MetaData as MakeFileCommonMetaData;
        }

        public MakeFileCommonMetaData CommonMetaData
        {
            get;
            private set;
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

        public System.Collections.Generic.List<string> Recipe
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
            var graph = Bam.Core.V2.Graph.Instance;
            graph.MetaData = new MakeFileCommonMetaData();
        }

        public static void PostExecution()
        {
            var graph = Bam.Core.V2.Graph.Instance;

            var makeContents = new System.Text.StringBuilder();

            var commonMeta = graph.MetaData as MakeFileCommonMetaData;
            if (commonMeta.Directories.Count > 0)
            {
                makeContents.Append("DIRS:=");
                foreach (var dir in commonMeta.Directories)
                {
                    makeContents.AppendFormat("{0} ", dir);
                }
                makeContents.AppendLine();
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

                    if (metadata.TargetVariable != null)
                    {
                        // simply expanded variable
                        makeContents.AppendFormat("{0}:={1}", metadata.TargetVariable, metadata.Target);
                        makeContents.AppendLine();
                        makeContents.AppendFormat("$({0}):", metadata.TargetVariable);
                    }
                    else
                    {
                        makeContents.AppendFormat("{0}:", metadata.Target);
                    }
                    foreach (var pre in metadata.Prequisities)
                    {
                        makeContents.AppendFormat("{0} ", pre.Key.GeneratedPaths[pre.Value]);
                    }
                    makeContents.AppendFormat("| $(DIRS)");
                    makeContents.AppendLine();
                    foreach (var command in metadata.Recipe)
                    {
                        makeContents.AppendFormat("\t{0}", command);
                        makeContents.AppendLine();
                    }
                }
            }

            makeContents.Append("all:");
            foreach (var module in graph.TopLevelModules)
            {
                var metadata = module.MetaData as MakeFileMeta;
                if (null == metadata)
                {
                    throw new Bam.Core.Exception("Top level module did not have any Make metadata");
                }
                makeContents.AppendFormat("$({0}) ", metadata.TargetVariable);
            }
            makeContents.AppendLine();

            makeContents.AppendLine("$(DIRS):");
            makeContents.AppendLine("\tmkdir $@");

            Bam.Core.Log.DebugMessage(makeContents.ToString());

            var makeFilePath = Bam.Core.V2.TokenizedString.Create("$(buildroot)/Makefile", null);
            makeFilePath.Parse();

            using (var writer = new System.IO.StreamWriter(makeFilePath.ToString()))
            {
                writer.Write(makeContents.ToString());
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
