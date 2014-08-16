// <copyright file="ThirdPartyOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public class ThirdPartyOptionCollection :
        Bam.Core.BaseOptionCollection
    {
        public
        ThirdPartyOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}

        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode owningNode)
        {}

        protected override void
        SetDelegates(
            Bam.Core.DependencyNode owningNode)
        {}

        // TODO: this needs to be updated for Location
        public override void
        FinalizeOptions(
            Bam.Core.DependencyNode node)
        {
            var thirdPartyModule = node.Module as C.ThirdPartyModule;
            if (null != thirdPartyModule)
            {
                var packagePath = node.Package.Identifier.Path;
                var proxyPath = (node.Module as Bam.Core.BaseModule).ProxyPath;
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
