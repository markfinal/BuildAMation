// <copyright file="ThirdPartyOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public class ThirdPartyOptionCollection : Opus.Core.BaseOptionCollection
    {
        public ThirdPartyOptionCollection()
            : base()
        {
        }

        public ThirdPartyOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode owningNode)
        {
            // no defaults, as there are no options
        }

        protected override void SetDelegates(Opus.Core.DependencyNode owningNode)
        {
            // no delegates, as there are no options
        }

        public override void FinalizeOptions(Opus.Core.DependencyNode node)
        {
            var thirdPartyModule = node.Module as C.ThirdPartyModule;
            if (null != thirdPartyModule)
            {
                string packagePath = node.Package.Identifier.Path;
                Opus.Core.ProxyModulePath proxyPath = (node.Module as Opus.Core.BaseModule).ProxyPath;
                if (null != proxyPath)
                {
                    packagePath = proxyPath.Combine(node.Package.Identifier);
                }

                thirdPartyModule.RegisterOutputFiles(this, node.Target, packagePath);
            }

            base.FinalizeOptions(node);
        }
    }
}
