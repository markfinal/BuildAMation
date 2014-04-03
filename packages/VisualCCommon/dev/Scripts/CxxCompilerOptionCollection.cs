// <copyright file="CxxCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    public abstract partial class CxxCompilerOptionCollection : CCompilerOptionCollection, C.ICxxCompilerOptions
    {
        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);

            C.ICCompilerOptions cInterfaceOptions = this as C.ICCompilerOptions;
            C.ICxxCompilerOptions cxxInterfaceOptions = this as C.ICxxCompilerOptions;

            cInterfaceOptions.TargetLanguage = C.ETargetLanguage.Cxx;
            cxxInterfaceOptions.ExceptionHandler = C.Cxx.EExceptionHandler.Disabled;
        }

        public CxxCompilerOptionCollection()
            : base()
        {
        }

        public CxxCompilerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }
    }
}
