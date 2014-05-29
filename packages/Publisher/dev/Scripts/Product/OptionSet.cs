// <copyright file="ProductModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Publisher package</summary>
// <author>Mark Final</author>
namespace Publisher
{
    public sealed class OptionSet : Opus.Core.BaseOptionCollection, IPublishOptions
    {
        public OptionSet(Opus.Core.DependencyNode owningNode)
            : base(owningNode)
        {
        }

        protected override void SetDefaultOptionValues(Opus.Core.DependencyNode owningNode)
        {
        }

        protected override void SetDelegates(Opus.Core.DependencyNode owningNode)
        {
        }

        protected override void SetNodeSpecificData(Opus.Core.DependencyNode node)
        {
            var locationMap = node.Module.Locations;
            var exeDirLoc = new Opus.Core.ScaffoldLocation(Opus.Core.ScaffoldLocation.ETypeHint.Directory);
            exeDirLoc.SetReference(locationMap[Opus.Core.State.ModuleBuildDirLocationKey]);
            locationMap[ProductModule.ExeDir] = exeDirLoc;

            base.SetNodeSpecificData(node);
        }
    }
}
