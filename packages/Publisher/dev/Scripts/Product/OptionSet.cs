#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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
