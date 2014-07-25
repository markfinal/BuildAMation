// <copyright file="Builder.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MakeFileBuilder package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder : Opus.Core.IBuilder
    {
        public static string GetMakeFilePathName(Opus.Core.DependencyNode node)
        {
            var makeFileDirLoc = node.GetModuleBuildDirectoryLocation().SubDirectory("Makefiles");
            var leafname = System.String.Format("{0}_{1}.mak", node.UniqueModuleName, node.Target);
            var makeFileLoc = Opus.Core.FileLocation.Get(makeFileDirLoc, leafname, Opus.Core.Location.EExists.WillExist);
            var makeFilePathName = makeFileLoc.GetSingleRawPath();
            Opus.Core.Log.DebugMessage("Makefile : '{0}'", makeFilePathName);
            return makeFilePathName;
        }
    }
}