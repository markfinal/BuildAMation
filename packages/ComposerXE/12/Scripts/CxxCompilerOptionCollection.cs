// <copyright file="CxxCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>ComposerXE package</summary>
// <author>Mark Final</author>
namespace ComposerXE
{
    // this implementation is here because the specific version of the ComposerXE compiler exposes a new interface
    // and because C# cannot derive from a generic type, this C++ option collection must derive from the specific
    // C option collection
    public sealed partial class CxxCompilerOptionCollection :
        CCompilerOptionCollection,
        C.ICxxCompilerOptions
    {
        protected override void
        SetDefaultOptionValues(
            Opus.Core.DependencyNode node)
        {
            base.SetDefaultOptionValues(node);
            ComposerXECommon.CxxCompilerOptionCollection.ExportedDefaults(this, node);
        }

        public
        CxxCompilerOptionCollection(
            Opus.Core.DependencyNode node) : base(node)
        {}
    }
}
