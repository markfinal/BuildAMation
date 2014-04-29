// <copyright file="CCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Mingw package</summary>
// <author>Mark Final</author>
namespace Mingw
{
    public partial class CCompilerOptionCollection : MingwCommon.CCompilerOptionCollection, ICCompilerOptions
    {
        public CCompilerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        protected override void SetDefaultOptionValues(Opus.Core.DependencyNode node)
        {
            base.SetDefaultOptionValues(node);

            // requires gcc 4.0, and only works on ELFs, but doesn't seem to do any harm
            (this as ICCompilerOptions).Visibility = EVisibility.Hidden;
        }
    }
}