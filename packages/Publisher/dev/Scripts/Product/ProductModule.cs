// <copyright file="ProductModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Publisher package</summary>
// <author>Mark Final</author>
namespace Publisher
{
    [Bam.Core.ModuleToolAssignment(typeof(IPublishProductTool))]
    public abstract class ProductModule :
        Bam.Core.BaseModule,
        Bam.Core.IIdentifyExternalDependencies
    {
        public static readonly Bam.Core.LocationKey OSXAppBundle = new Bam.Core.LocationKey("OSXPrimaryApplicationBundle", Bam.Core.ScaffoldLocation.ETypeHint.Directory);
        public static readonly Bam.Core.LocationKey OSXAppBundleContents = new Bam.Core.LocationKey("OSXPrimaryAppBundleContents", Bam.Core.ScaffoldLocation.ETypeHint.Directory);
        public static readonly Bam.Core.LocationKey OSXAppBundleMacOS = new Bam.Core.LocationKey("OSXPrimaryAppBundleMacOS", Bam.Core.ScaffoldLocation.ETypeHint.Directory);
        public static readonly Bam.Core.LocationKey PublishDir = new Bam.Core.LocationKey("PrimaryModuleDirectory", Bam.Core.ScaffoldLocation.ETypeHint.Directory);

        #region IIdentifyExternalDependencies Members

        Bam.Core.TypeArray
        Bam.Core.IIdentifyExternalDependencies.IdentifyExternalDependencies(
            Bam.Core.Target target)
        {
            var dependentModuleTypes = new Bam.Core.TypeArray();

            var flags = System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.NonPublic;
            var fields = this.GetType().GetFields(flags);
            foreach (var field in fields)
            {
                var attributes = field.GetCustomAttributes(true);
                if (attributes.Length != 1)
                {
                    throw new Bam.Core.Exception("Found {0} attributes on field {1} of module {2}. Should be just one",
                                                  attributes.Length, field.Name, this.OwningNode.ModuleName);
                }

                var currentAttr = attributes[0];
                if (currentAttr.GetType() == typeof(PrimaryTargetAttribute))
                {
                    var fieldValue = field.GetValue(this) as System.Type;
                    if (null == fieldValue)
                    {
                        throw new Bam.Core.Exception("PrimaryTarget attribute field was not of type System.Type");
                    }
                    dependentModuleTypes.AddUnique(fieldValue);
                }
                else if (currentAttr.GetType() == typeof(OSXInfoPListAttribute))
                {
                    var fieldValue = field.GetValue(this) as System.Type;
                    if (null == fieldValue)
                    {
                        throw new Bam.Core.Exception("OSXInfoPList attribute field was not of type System.Type");
                    }
                    dependentModuleTypes.AddUnique(fieldValue);
                }
                else if (currentAttr.GetType() == typeof(AdditionalDirectoriesAttribute))
                {
                    // do nothing
                }
                else
                {
                    throw new Bam.Core.Exception("Unrecognized attribute of type {0} on field {1} of module {2}",
                                                  currentAttr.GetType().ToString(), field.Name, this.OwningNode.ModuleName);
                }
            }

            return dependentModuleTypes;
        }

        #endregion
    }
}
