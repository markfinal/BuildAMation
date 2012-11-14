// <copyright file="CxxCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Clang package</summary>
// <author>Mark Final</author>
namespace Clang
{
    public sealed partial class CxxCompilerOptionCollection : CCompilerOptionCollection, C.ICPlusPlusCompilerOptions
    {
        public CxxCompilerOptionCollection()
        {
        }

        public CxxCompilerOptionCollection(Opus.Core.DependencyNode owningNode)
            : base(owningNode)
        {
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);

            C.ICCompilerOptions cInterfaceOptions = this as C.ICCompilerOptions;
            cInterfaceOptions.TargetLanguage = C.ETargetLanguage.CPlusPlus;

            C.ICPlusPlusCompilerOptions cxxInterfaceOptions = this as C.ICPlusPlusCompilerOptions;
            cxxInterfaceOptions.ExceptionHandler = C.CPlusPlus.EExceptionHandler.Disabled;
        }
    }
}
