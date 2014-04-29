// <copyright file="ThirdPartyOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public class ThirdPartyOptionCollection : Opus.Core.BaseOptionCollection
    {
        public ThirdPartyOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        protected override void SetDefaultOptionValues(Opus.Core.DependencyNode owningNode)
        {
            // no defaults, as there are no options
        }

        protected override void SetDelegates(Opus.Core.DependencyNode owningNode)
        {
            // no delegates, as there are no options
        }

        // TODO: this needs to be updated for Location
        public override void FinalizeOptions(Opus.Core.DependencyNode node)
        {
            var thirdPartyModule = node.Module as C.ThirdPartyModule;
            if (null != thirdPartyModule)
            {
                var packagePath = node.Package.Identifier.Path;
                var proxyPath = (node.Module as Opus.Core.BaseModule).ProxyPath;
                if (null != proxyPath)
                {
                    packagePath = proxyPath.Combine(node.Package.Identifier.Location).AbsolutePath;
                }

                thirdPartyModule.RegisterOutputFiles(this, node.Target, packagePath);
            }

            base.FinalizeOptions(node);
        }
    }
}
