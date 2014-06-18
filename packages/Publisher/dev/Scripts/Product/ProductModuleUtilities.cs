// <copyright file="ProductModuleUtilities.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Publisher package</summary>
// <author>Mark Final</author>
namespace Publisher
{
    public static class ProductModuleUtilities
    {
        // TODO: out of all the dependents, how do we determine the metadata that they have associated with them
        // from the Publisher.ProductModule module, that is beyond just the need for graph building?
        private static Opus.Core.TypeArray
        GetModulesTypesWithAttribute(
            Publisher.ProductModule moduleToBuild,
            System.Type attributeType)
        {
            var moduleTypes = new Opus.Core.TypeArray();

            var flags = System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.NonPublic;
            var fields = moduleToBuild.GetType().GetFields(flags);
            foreach (var field in fields)
            {
                var attributes = field.GetCustomAttributes(true);
                if (attributes.Length != 1)
                {
                    throw new Opus.Core.Exception("Found {0} attributes on field {1} of module {2}. Should be just one",
                                                  attributes.Length, field.Name, moduleToBuild.OwningNode.ModuleName);
                }

                var currentAttr = attributes[0];
                if (currentAttr.GetType() == attributeType)
                {
                    var primaryTargetType = field.GetValue(moduleToBuild) as System.Type;
                    moduleTypes.AddUnique(primaryTargetType);
                }
            }

            return moduleTypes;
        }

        public static Opus.Core.DependencyNode GetPrimaryNode(Publisher.ProductModule moduleToBuild)
        {
            Opus.Core.DependencyNode primaryNode = null;

            var dependents = moduleToBuild.OwningNode.ExternalDependents;
            if (dependents.Count == 0)
            {
                return primaryNode;
            }

            var primaryTargetTypes = GetModulesTypesWithAttribute(moduleToBuild, typeof(Publisher.PrimaryTargetAttribute));
            if (primaryTargetTypes.Count == 0)
            {
                return primaryNode;
            }

            primaryNode = Opus.Core.ModuleUtilities.GetNode(primaryTargetTypes[0], (Opus.Core.BaseTarget)moduleToBuild.OwningNode.Target);
            return primaryNode;
        }

        public static string
        GetPublishedKeyName(
            Opus.Core.BaseModule primaryModule,
            Opus.Core.BaseModule module,
            Opus.Core.LocationKey key)
        {
            var builder = new System.Text.StringBuilder();
            builder.Append(module.OwningNode.ModuleName);
            builder.Append(".");
            builder.Append(key.ToString());
            builder.Append(".PublishedFile");
            if (primaryModule != module)
            {
                builder.Append(".For.");
                builder.Append(primaryModule.OwningNode.ModuleName);
            }
            return builder.ToString();
        }

        public static string
        GenerateDestinationPath(
            string sourcePath,
            string destinationDirectory,
            Opus.Core.BaseModule module,
            Opus.Core.LocationKey key)
        {
            var filename = System.IO.Path.GetFileName(sourcePath);
            var destPath = System.IO.Path.Combine(destinationDirectory, filename);
            module.Locations[key] = Opus.Core.FileLocation.Get(destPath, Opus.Core.Location.EExists.WillExist);
            return destPath;
        }

        public static void
        CopyFileToLocation(
            Opus.Core.Location sourceFile,
            string destinationDirectory,
            Opus.Core.BaseModule module,
            Opus.Core.LocationKey key)
        {
            var sourcePath = sourceFile.GetSingleRawPath();
            var destPath = GenerateDestinationPath(sourcePath, destinationDirectory, module, key);
            Opus.Core.Log.Info("Copying {0} to {1}", sourcePath, destPath);
            // TODO: this currently copies targets of symlinks
            System.IO.File.Copy(sourcePath, destPath, true);
        }
    }
}
