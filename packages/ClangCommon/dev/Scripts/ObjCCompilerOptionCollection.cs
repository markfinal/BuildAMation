// <copyright file="ObjCCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>ClangCommon package</summary>
// <author>Mark Final</author>
namespace ClangCommon
{
    public class ObjCCompilerOptionCollection : CCompilerOptionCollection
    {
        public static void ExportedDefaults(Opus.Core.BaseOptionCollection options, Opus.Core.DependencyNode node)
        {
            var cInterfaceOptions = options as C.ICCompilerOptions;
            cInterfaceOptions.TargetLanguage = C.ETargetLanguage.ObjectiveC;
        }

        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            base.InitializeDefaults(node);
            ExportedDefaults(this, node);
        }

        public ObjCCompilerOptionCollection()
            : base()
        {
        }

        public ObjCCompilerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }
    }
}
