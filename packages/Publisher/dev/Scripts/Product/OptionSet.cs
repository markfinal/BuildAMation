// <copyright file="ProductModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Publisher package</summary>
// <author>Mark Final</author>
namespace Publisher
{
    public sealed partial class OptionSet :
        Opus.Core.BaseOptionCollection,
        IPublishOptions
    {
        public OptionSet(Opus.Core.DependencyNode owningNode)
            : base(owningNode)
        {
        }

        protected override void SetDefaultOptionValues(Opus.Core.DependencyNode owningNode)
        {
            var options = this as IPublishOptions;
            options.OSXApplicationBundle = false;
        }

        public override void FinalizeOptions(Opus.Core.DependencyNode node)
        {
            var options = this as IPublishOptions;

            var locationMap = node.Module.Locations;
            var exeDirLoc = new Opus.Core.ScaffoldLocation(Opus.Core.ScaffoldLocation.ETypeHint.Directory);
            exeDirLoc.SetReference(locationMap[Opus.Core.State.ModuleBuildDirLocationKey]);
            if (node.Target.HasPlatform(Opus.Core.EPlatform.OSX) && options.OSXApplicationBundle)
            {
                // TODO: specifying the application bundle name needs a better solution
                var appBundle = exeDirLoc.SubDirectory(node.ModuleName + ".app");
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
