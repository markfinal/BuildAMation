#region License
// Copyright 2010-2014 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
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
