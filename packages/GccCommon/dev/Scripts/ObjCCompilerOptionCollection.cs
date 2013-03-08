// <copyright file="ObjCCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
{
    public class ObjCCompilerOptionCollection : CCompilerOptionCollection
    {
        public static void ExportedDefaults<T>(T options, Opus.Core.DependencyNode node) where T : CCompilerOptionCollection
        {
            C.ICCompilerOptions cInterfaceOptions = options as C.ICCompilerOptions;

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
