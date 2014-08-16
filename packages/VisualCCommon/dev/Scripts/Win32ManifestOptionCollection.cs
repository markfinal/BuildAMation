// <copyright file="Win32ManifestOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    // TODO: this does not implement any options interface
    public sealed partial class Win32ManifestOptionCollection :
        C.Win32ManifestOptionCollection,
        VisualStudioProcessor.IVisualStudioSupport
    {
        public
        Win32ManifestOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}

        VisualStudioProcessor.ToolAttributeDictionary
        VisualStudioProcessor.IVisualStudioSupport.ToVisualStudioProjectAttributes(
            Bam.Core.Target target)
        {
            var vsTarget = (target.Toolset as VisualStudioProcessor.IVisualStudioTargetInfo).VisualStudioTarget;
            switch (vsTarget)
            {
            case VisualStudioProcessor.EVisualStudioTarget.VCPROJ:
            case VisualStudioProcessor.EVisualStudioTarget.MSBUILD:
                break;

            default:
                throw new Bam.Core.Exception("Unsupported VisualStudio target, '{0}'", vsTarget);
            }
            var dictionary = VisualStudioProcessor.ToVisualStudioAttributes.Execute(this, target, vsTarget);
            return dictionary;
        }
    }
}
