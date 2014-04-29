// <copyright file="ObjCxxCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>ClangCommon package</summary>
// <author>Mark Final</author>
namespace ClangCommon
{
    public partial class ObjCxxCompilerOptionCollection : ObjCCompilerOptionCollection, C.ICxxCompilerOptions
    {
        public static new void ExportedDefaults(Opus.Core.BaseOptionCollection options, Opus.Core.DependencyNode node)
        {
            var cInterfaceOptions = options as C.ICCompilerOptions;
            cInterfaceOptions.TargetLanguage = C.ETargetLanguage.ObjectiveCxx;
            var cxxInterfaceOptions = options as C.ICxxCompilerOptions;
            cxxInterfaceOptions.ExceptionHandler = C.Cxx.EExceptionHandler.Disabled;
        }

        protected override void SetDefaultOptionValues(Opus.Core.DependencyNode node)
        {
            base.SetDefaultOptionValues(node);
            ExportedDefaults(this, node);
        }

        public ObjCxxCompilerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }
    }
}
