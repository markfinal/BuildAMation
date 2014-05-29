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
                    var executableModuleTypes = field.GetValue(moduleToBuild) as Opus.Core.TypeArray;
                    moduleTypes.AddRangeUnique(executableModuleTypes);
                }
            }

            return moduleTypes;
        }

        public static Opus.Core.DependencyNodeCollection GetExecutableModules(Publisher.ProductModule moduleToBuild)
        {
            var executableModules = new Opus.Core.DependencyNodeCollection();

            var dependents = moduleToBuild.OwningNode.ExternalDependents;
            if (dependents.Count == 0)
            {
                return executableModules;
            }

            var executableModuleTypes = GetModulesTypesWithAttribute(moduleToBuild, typeof(Publisher.ExecutablesAttribute));
            if (executableModuleTypes.Count == 0)
            {
                return executableModules;
            }

            foreach (var dep in dependents)
            {
                if (executableModuleTypes.Contains(dep.Module.GetType()))
                {
                    executableModules.Add(dep);
                }
            }

            return executableModules;
        }
    }
}
