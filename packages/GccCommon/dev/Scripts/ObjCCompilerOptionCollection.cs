// <copyright file="ObjCCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
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
            var gccInterfaceOptions = options as ICCompilerOptions;
            gccInterfaceOptions.StrictAliasing = false; // causes type-punning warnings with 'super' in 4.0
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
