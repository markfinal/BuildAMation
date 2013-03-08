// <copyright file="ObjCxxCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
{
    public partial class ObjCxxCompilerOptionCollection : ObjCCompilerOptionCollection, C.ICxxCompilerOptions
    {
        public static new void ExportedDefaults<T>(T options, Opus.Core.DependencyNode node) where T : CCompilerOptionCollection, C.ICxxCompilerOptions
        {
            C.ICCompilerOptions cInterfaceOptions = options as C.ICCompilerOptions;
            C.ICxxCompilerOptions cxxInterfaceOptions = options as C.ICxxCompilerOptions;

            cInterfaceOptions.TargetLanguage = C.ETargetLanguage.ObjectiveCxx;
            cxxInterfaceOptions.ExceptionHandler = C.Cxx.EExceptionHandler.Disabled;
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);
            ExportedDefaults(this, node);
        }

        public ObjCxxCompilerOptionCollection()
            : base()
        {
        }

        public ObjCxxCompilerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }
    }
}
