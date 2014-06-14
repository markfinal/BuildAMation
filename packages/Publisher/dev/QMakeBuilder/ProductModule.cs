// <copyright file="ProductModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Publisher package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder
    {
        private static void
        InstallFile(
            Opus.Core.BaseModule moduleCopied,
            Opus.Core.Location fileToCopy,
            string destinationDirectory,
            QMakeData proData)
        {
            var customRules = new Opus.Core.StringArray();
            customRules.Add(System.String.Format("target.path={0}", destinationDirectory));
            customRules.Add(System.String.Format("INSTALL+=target"));
            proData.CustomRules = customRules;
        }

        public object
        Build(
            Publisher.ProductModule moduleToBuild,
            out bool success)
        {
            var primaryNode = Publisher.ProductModuleUtilities.GetPrimaryNode(moduleToBuild);
            var locationMap = moduleToBuild.Locations;
            var publishDirLoc = locationMap[Publisher.ProductModule.PublishDir];
            var publishDirPath = publishDirLoc.GetSingleRawPath();

            foreach (var dependency in primaryNode.ExternalDependents)
            {
                var proData = dependency.Data as QMakeData;
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

                        InstallFile(module, loc, publishDirPath, proData);
                    }
                }
            }

            success = true;
            return null;
        }
    }
}
