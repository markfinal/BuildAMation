#region License
// Copyright 2010-2014 Mark Final
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
namespace XcodeBuilder
{
    public sealed class XCBuildConfiguration :
        XcodeNodeData,
        IWriteableNode
    {
        public
        XCBuildConfiguration(
            string name,
            string moduleName) : base(name)
        {
            this.ModuleName = moduleName;
            this.Options = new OptionsDictionary();
            // http://meidell.dk/2010/05/xcode-header-map-files/
            this.Options["USE_HEADERMAP"].AddUnique("NO");
            this.SourceFiles = new Bam.Core.Array<PBXBuildFile>();
        }

        public string ModuleName
        {
            get;
            private set;
        }

        public OptionsDictionary Options
        {
            get;
            private set;
        }

        public Bam.Core.Array<PBXBuildFile> SourceFiles
        {
            get;
            private set;
        }

#region IWriteableNode implementation

        void
        IWriteableNode.Write(
            System.IO.TextWriter writer)
        {
            writer.WriteLine("\t\t{0} /* {1} */ = {{", this.UUID, this.Name);
            writer.WriteLine("\t\t\tisa = XCBuildConfiguration;");
            writer.WriteLine("\t\t\tbuildSettings = {");
            foreach (var option in this.Options)
            {
                if (option.Value.Count == 1)
                {
                    writer.WriteLine("\t\t\t\t{0} = {1};", option.Key, OptionsDictionary.SafeOptionValue(option.Value[0]));
                }
                else if (option.Value.Count > 1)
                {
                    writer.WriteLine("\t\t\t\t{0} = (", option.Key);
                    foreach (var value in option.Value)
                    {
                        writer.WriteLine("\t\t\t\t\t{0},", OptionsDictionary.SafeOptionValue(value));
                    }
                    writer.WriteLine("\t\t\t\t);");
                }
            }
            writer.WriteLine("\t\t\t};");
            writer.WriteLine("\t\t\tname = {0};", this.Name);
            writer.WriteLine("\t\t};");
        }

#endregion
    }
}
