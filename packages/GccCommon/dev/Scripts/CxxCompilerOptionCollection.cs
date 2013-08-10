// <copyright file="CxxCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
{
    public abstract partial class CxxCompilerOptionCollection : CCompilerOptionCollection, C.ICxxCompilerOptions
    {
        public static void ExportedDefaults(Opus.Core.BaseOptionCollection options, Opus.Core.DependencyNode node)
        {
            var cInterfaceOptions = options as C.ICCompilerOptions;
            cInterfaceOptions.TargetLanguage = C.ETargetLanguage.Cxx;
            var cxxInterfaceOptions = options as C.ICxxCompilerOptions;
            cxxInterfaceOptions.ExceptionHandler = C.Cxx.EExceptionHandler.Disabled;
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);
            ExportedDefaults(this, node);
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
