// <copyright file="ProductModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Publisher package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object Build(Publisher.ProductModule moduleToBuild, out bool success)
        {
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Opus.Core.ScaffoldLocation.ETypeHint.Directory, Opus.Core.Location.EExists.WillExist);
            foreach (var dir in dirsToCreate)
            {
                var dirPath = dir.GetSinglePath();
                NativeBuilder.MakeDirectory(dirPath);
            }

            var executableModuleNodes = Publisher.ProductModuleUtilities.GetExecutableModules(moduleToBuild);
            var locationMap = moduleToBuild.Locations;
            var exeDirLoc = locationMap[Publisher.ProductModule.ExeDir];
            var exeDirPath = exeDirLoc.GetSingleRawPath();
            foreach (var exeNode in executableModuleNodes)
            {
                var sourceLoc = exeNode.Module.Locations[C.Application.OutputFile];
                var sourcePath = sourceLoc.GetSingleRawPath();
                var filename = System.IO.Path.GetFileName(sourcePath);
                var destPath = System.IO.Path.Combine(exeDirPath, filename);
                Opus.Core.Log.Info("Copying {0} to {1}", sourcePath, destPath);
                System.IO.File.Copy(sourcePath, destPath, true);
            }

            success = true;
            return null;
        }
    }
}
