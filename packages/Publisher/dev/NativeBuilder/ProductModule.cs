// <copyright file="ProductModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Publisher package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object
        Build(
            Publisher.ProductModule moduleToBuild,
            out bool success)
        {
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Opus.Core.ScaffoldLocation.ETypeHint.Directory, Opus.Core.Location.EExists.WillExist);
            foreach (var dir in dirsToCreate)
            {
                var dirPath = dir.GetSinglePath();
                NativeBuilder.MakeDirectory(dirPath);
            }

            var primaryNode = Publisher.ProductModuleUtilities.GetPrimaryNode(moduleToBuild);
            var locationMap = moduleToBuild.Locations;
            var publishDirLoc = locationMap[Publisher.ProductModule.PublishDir];
            var publishDirPath = publishDirLoc.GetSingleRawPath();

            // TODO: the key here needs to be on an optionset or similar
            var sourceLoc = primaryNode.Module.Locations[C.Application.OutputFile];
            var publishedSourceKeyName = Publisher.ProductModuleUtilities.GetPublishedKeyName(primaryNode.Module, primaryNode.Module, C.Application.OutputFile);
            var publishedKey = new Opus.Core.LocationKey(publishedSourceKeyName, Opus.Core.ScaffoldLocation.ETypeHint.File);
            Publisher.ProductModuleUtilities.CopyFileToLocation(sourceLoc, publishDirPath, moduleToBuild, publishedKey);

            foreach (var dependency in primaryNode.ExternalDependents)
            {
                var module = dependency.Module;
                var moduleType = module.GetType();
                var flags = System.Reflection.BindingFlags.Instance |
                            System.Reflection.BindingFlags.NonPublic;
                var fields = moduleType.GetFields(flags);
                foreach (var field in fields)
                {
                    var candidates = field.GetCustomAttributes(typeof(Publisher.PublishModuleDependencyAttribute), false);
                    if (0 == candidates.Length)
                    {
                        continue;
                    }
                    if (candidates.Length > 1)
                    {
                        throw new Opus.Core.Exception("More than one publish module dependency found");
                    }
                    var candidateData = field.GetValue(module) as Opus.Core.Array<Opus.Core.LocationKey>;
                    foreach (var key in candidateData)
                    {
                        var loc = module.Locations[key];
                        var keyName = Publisher.ProductModuleUtilities.GetPublishedKeyName(primaryNode.Module, module, key);
                        var newKey = new Opus.Core.LocationKey(keyName, Opus.Core.ScaffoldLocation.ETypeHint.File);
                        Publisher.ProductModuleUtilities.CopyFileToLocation(loc, publishDirPath, moduleToBuild, newKey);
                    }
                }
            }

            success = true;
            return null;
        }
    }
}
