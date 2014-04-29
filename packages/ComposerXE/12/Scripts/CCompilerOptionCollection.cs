// <copyright file="CCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>ComposerXE package</summary>
// <author>Mark Final</author>
namespace ComposerXE
{
    // Not sealed since the C++ compiler inherits from it
    public partial class CCompilerOptionCollection : ComposerXECommon.CCompilerOptionCollection, ICCompilerOptions
    {
        public CCompilerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);

            (this as ICCompilerOptions).Visibility = EVisibility.Hidden;
        }
    }
}