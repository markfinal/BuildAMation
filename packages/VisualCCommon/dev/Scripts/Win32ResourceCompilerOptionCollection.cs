// <copyright file="Win32ResourceCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    // TODO: this does not implement any options interface
    public sealed partial class Win32ResourceCompilerOptionCollection :
        C.Win32ResourceCompilerOptionCollection,
        VisualStudioProcessor.IVisualStudioSupport
    {
        public
        Win32ResourceCompilerOptionCollection(
            Opus.Core.DependencyNode node) : base(node)
        {}

        VisualStudioProcessor.ToolAttributeDictionary
        VisualStudioProcessor.IVisualStudioSupport.ToVisualStudioProjectAttributes(
            Opus.Core.Target target)
        {
            var vsTarget = (target.Toolset as VisualStudioProcessor.IVisualStudioTargetInfo).VisualStudioTarget;
            switch (vsTarget)
            {
            case VisualStudioProcessor.EVisualStudioTarget.VCPROJ:
            case VisualStudioProcessor.EVisualStudioTarget.MSBUILD:
                break;

            default:
                throw new Opus.Core.Exception("Unsupported VisualStudio target, '{0}'", vsTarget);
            }
            var dictionary = VisualStudioProcessor.ToVisualStudioAttributes.Execute(this, target, vsTarget);
            return dictionary;
        }
    }
}
