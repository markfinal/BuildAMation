// <copyright file="ProductModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Publisher package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        private static string
        GetPublishedKeyName(
            Opus.Core.BaseModule module,
            Opus.Core.LocationKey key)
        {
            var builder = new System.Text.StringBuilder();
            builder.Append(module.OwningNode.ModuleName);
            builder.Append(".");
            builder.Append(key.ToString());
            builder.Append(".PublishedFile");
            return builder.ToString();
        }

        private static void
        CopyFileToLocation(
            Opus.Core.Location sourceFile,
            string destinationDirectory,
            Opus.Core.BaseModule module,
            Opus.Core.LocationKey key)
        {
            var sourcePath = sourceFile.GetSingleRawPath();
            var filename = System.IO.Path.GetFileName(sourcePath);
            var destPath = System.IO.Path.Combine(destinationDirectory, filename);
            Opus.Core.Log.Info("Copying {0} to {1}", sourcePath, destPath);
            System.IO.File.Copy(sourcePath, destPath, true);

            module.Locations[key] = Opus.Core.FileLocation.Get(destPath, Opus.Core.Location.EExists.WillExist);
        }

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
            var exeDirLoc = locationMap[Publisher.ProductModule.PublishDir];
            var exeDirPath = exeDirLoc.GetSingleRawPath();

            // TODO: the key here needs to be on an optionset or similar
            var sourceLoc = primaryNode.Module.Locations[C.Application.OutputFile];
            var publishedSourceKeyName = GetPublishedKeyName(primaryNode.Module, C.Application.OutputFile);
            var publishedKey = new Opus.Core.LocationKey(publishedSourceKeyName, Opus.Core.ScaffoldLocation.ETypeHint.File);
            CopyFileToLocation(sourceLoc, exeDirPath, moduleToBuild, publishedKey);

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
                        var keyName = System.String.Format("{0}.{1}.PublishedFile", module.OwningNode.ModuleName, key.ToString());
                        var newKey = new Opus.Core.LocationKey(keyName, Opus.Core.ScaffoldLocation.ETypeHint.File);
                        CopyFileToLocation(loc, exeDirPath, moduleToBuild, newKey);
                    }
                }
            }

            success = true;
            return null;
        }
    }
}
