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
    }
}
