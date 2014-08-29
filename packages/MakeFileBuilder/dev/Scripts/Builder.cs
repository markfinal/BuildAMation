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
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder :
        Bam.Core.IBuilder
    {
        public static string
        GetMakeFilePathName(
            Bam.Core.DependencyNode node)
        {
            var makeFileDirLoc = node.GetModuleBuildDirectoryLocation().SubDirectory("Makefiles");
            var leafname = System.String.Format("{0}_{1}.mak", node.UniqueModuleName, node.Target);
            var makeFileLoc = Bam.Core.FileLocation.Get(makeFileDirLoc, leafname, Bam.Core.Location.EExists.WillExist);
            var makeFilePathName = makeFileLoc.GetSingleRawPath();
            Bam.Core.Log.DebugMessage("Makefile : '{0}'", makeFilePathName);
            return makeFilePathName;
        }
    }
}
