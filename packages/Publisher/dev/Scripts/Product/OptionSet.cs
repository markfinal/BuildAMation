// <copyright file="ProductModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Publisher package</summary>
// <author>Mark Final</author>
namespace Publisher
{
    public sealed partial class OptionSet :
        Bam.Core.BaseOptionCollection,
        IPublishOptions
    {
        public
        OptionSet(
            Bam.Core.DependencyNode owningNode) : base(owningNode)
        {}

        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode owningNode)
        {
            var options = this as IPublishOptions;
            options.OSXApplicationBundle = false;
        }

        public override void
        FinalizeOptions(
            Bam.Core.DependencyNode node)
        {
            var options = this as IPublishOptions;

            var locationMap = node.Module.Locations;
            var exeDirLoc = new Bam.Core.ScaffoldLocation(Bam.Core.ScaffoldLocation.ETypeHint.Directory);
            exeDirLoc.SetReference(locationMap[Bam.Core.State.ModuleBuildDirLocationKey]);
            if (node.Target.HasPlatform(Bam.Core.EPlatform.OSX) && options.OSXApplicationBundle)
            {
                var dependent = node.ExternalDependents[0];

                // TODO: specifying the application bundle name needs a better solution
                var appBundle = exeDirLoc.SubDirectory(dependent.ModuleName + ".app");
                locationMap[ProductModule.OSXAppBundle] = appBundle;
                var contents = appBundle.SubDirectory("Contents");
                locationMap[ProductModule.OSXAppBundleContents] = contents;
                var macos = contents.SubDirectory("MacOS");
                locationMap[ProductModule.OSXAppBundleMacOS] = macos;
                locationMap[ProductModule.PublishDir] = macos;
            }
            else
            {
                locationMap[ProductModule.PublishDir] = exeDirLoc;
            }

            base.FinalizeOptions (node);
        }
    }
}
