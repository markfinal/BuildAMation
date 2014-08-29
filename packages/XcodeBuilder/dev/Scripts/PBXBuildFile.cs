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
    public sealed class PBXBuildFile :
        XcodeNodeData,
        IWriteableNode
    {
        public
        PBXBuildFile(
            string name,
            PBXFileReference fileRef,
            BuildPhase buildPhase) : base(name)
        {
            this.FileReference = fileRef;
            this.Settings = new OptionsDictionary();
            this.BuildPhase = buildPhase;
        }

        public PBXFileReference FileReference
        {
            get;
            private set;
        }

        public BuildPhase BuildPhase
        {
            get;
            private set;
        }

        public OptionsDictionary Settings
        {
            get;
            private set;
        }

#region IWriteableNode implementation

        void
        IWriteableNode.Write(
            System.IO.TextWriter writer)
        {
            if (this.FileReference == null)
            {
                throw new Bam.Core.Exception("File reference not set on this build file");
            }
            if (this.BuildPhase == null)
            {
                throw new Bam.Core.Exception("Build phase not set on this build file");
            }

            if (this.Settings.Count > 0)
            {
                writer.WriteLine("\t\t{0} /* {1} in {2} */ = {{isa = PBXBuildFile; fileRef = {3} /* {1} */; settings = {4} }};", this.UUID, this.FileReference.ShortPath, this.BuildPhase.Name, this.FileReference.UUID, this.Settings.ToString());
            }
            else
            {
                writer.WriteLine("\t\t{0} /* {1} in {2} */ = {{isa = PBXBuildFile; fileRef = {3} /* {1} */; }};", this.UUID, this.FileReference.ShortPath, this.BuildPhase.Name, this.FileReference.UUID);
            }
        }

#endregion
    }
}
