// <copyright file="ProductModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Publisher package</summary>
// <author>Mark Final</author>
namespace Publisher
{
    [Opus.Core.ModuleToolAssignment(typeof(IPublishProductTool))]
    public abstract class ProductModule : Opus.Core.BaseModule, Opus.Core.IIdentifyExternalDependencies
    {
        public static readonly Opus.Core.LocationKey OSXAppBundle = new Opus.Core.LocationKey("OSXPrimaryApplicationBundle", Opus.Core.ScaffoldLocation.ETypeHint.Directory);
        public static readonly Opus.Core.LocationKey OSXAppBundleContents = new Opus.Core.LocationKey("OSXPrimaryAppBundleContents", Opus.Core.ScaffoldLocation.ETypeHint.Directory);
        public static readonly Opus.Core.LocationKey OSXAppBundleMacOS = new Opus.Core.LocationKey("OSXPrimaryAppBundleMacOS", Opus.Core.ScaffoldLocation.ETypeHint.Directory);
        public static readonly Opus.Core.LocationKey PublishDir = new Opus.Core.LocationKey("PrimaryModuleDirectory", Opus.Core.ScaffoldLocation.ETypeHint.Directory);

        #region IIdentifyExternalDependencies Members

        Opus.Core.TypeArray Opus.Core.IIdentifyExternalDependencies.IdentifyExternalDependencies(Opus.Core.Target target)
        {
            var dependentModuleTypes = new Opus.Core.TypeArray();

            var flags = System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.NonPublic;
            var fields = this.GetType().GetFields(flags);
            foreach (var field in fields)
            {
                var attributes = field.GetCustomAttributes(true);
                if (attributes.Length != 1)
                {
                    throw new Opus.Core.Exception("Found {0} attributes on field {1} of module {2}. Should be just one",
                                                  attributes.Length, field.Name, this.OwningNode.ModuleName);
                }

                var currentAttr = attributes[0];
                if (currentAttr.GetType() == typeof(PrimaryTargetAttribute))
                {
                    var fieldValue = field.GetValue(this) as System.Type;
                    if (null == fieldValue)
                    {
                        throw new Opus.Core.Exception("PrimaryTarget attribute field was not of type System.Type");
                    }
                    dependentModuleTypes.AddUnique(fieldValue);
                }
                else if (currentAttr.GetType() == typeof(OSXInfoPListAttribute))
                {
                    var fieldValue = field.GetValue(this) as PublishNodeData;
                    if (null == fieldValue)
                    {
                        throw new Opus.Core.Exception("OSXInfoPList attribute field was not of type PublishNodeData");
                    }
                    dependentModuleTypes.AddUnique(fieldValue.ModuleType);
                }
                else if (currentAttr.GetType() == typeof(AdditionalDirectoriesAttribute))
                {
                    // do nothing
                }
                else
                {
                    throw new Opus.Core.Exception("Unrecognized attribute of type {0} on field {1} of module {2}",
                                                  currentAttr.GetType().ToString(), field.Name, this.OwningNode.ModuleName);
                }
            }

            return dependentModuleTypes;
        }

        #endregion
    }
}
