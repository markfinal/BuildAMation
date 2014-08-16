// <copyright file="ObjCCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>ClangCommon package</summary>
// <author>Mark Final</author>
namespace ClangCommon
{
    public class ObjCCompilerOptionCollection :
        CCompilerOptionCollection
    {
        public static void
        ExportedDefaults(
            Bam.Core.BaseOptionCollection options,
            Bam.Core.DependencyNode node)
        {
            var cInterfaceOptions = options as C.ICCompilerOptions;
            cInterfaceOptions.TargetLanguage = C.ETargetLanguage.ObjectiveC;
        }

        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode node)
        {
            base.SetDefaultOptionValues(node);
            ExportedDefaults(this, node);
        }

        public
        ObjCCompilerOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}
    }
}
