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
#endregion
namespace XcodeBuilder
{
    public sealed class PBXSourcesBuildPhase :
        BuildPhase,
        IWriteableNode
    {
        public
        PBXSourcesBuildPhase(
            string name,
            string moduleName) : base(name, moduleName)
        {}

#region IWriteableNode implementation

        void
        IWriteableNode.Write(
            System.IO.TextWriter writer)
        {
            if (0 == this.Files.Count)
            {
                return;
            }

            writer.WriteLine("\t\t{0} /* {1} */ = {{", this.UUID, this.Name);
            writer.WriteLine("\t\t\tisa = PBXSourcesBuildPhase;");
            writer.WriteLine("\t\t\tbuildActionMask = 2147483647;");
            writer.WriteLine("\t\t\tfiles = (");
            foreach (var file in this.Files)
            {
                var buildFile = file as PBXBuildFile;
                writer.WriteLine("\t\t\t\t{0} /* {1} in {2} */,", file.UUID, System.IO.Path.GetFileName(buildFile.FileReference.ShortPath), buildFile.BuildPhase.Name);
            }
            writer.WriteLine("\t\t\t);");
            writer.WriteLine("\t\t\trunOnlyForDeploymentPostprocessing = 0;");
            writer.WriteLine("\t\t};");
        }

#endregion
    }
}
