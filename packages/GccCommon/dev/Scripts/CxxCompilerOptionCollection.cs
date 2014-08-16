// <copyright file="CxxCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
{
    public abstract partial class CxxCompilerOptionCollection :
        CCompilerOptionCollection,
        C.ICxxCompilerOptions
    {
        public static void
        ExportedDefaults(
            Bam.Core.BaseOptionCollection options,
            Bam.Core.DependencyNode node)
        {
            var cInterfaceOptions = options as C.ICCompilerOptions;
            cInterfaceOptions.TargetLanguage = C.ETargetLanguage.Cxx;
            var cxxInterfaceOptions = options as C.ICxxCompilerOptions;
            cxxInterfaceOptions.ExceptionHandler = C.Cxx.EExceptionHandler.Disabled;
        }

        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode node)
        {
            base.SetDefaultOptionValues(node);
            ExportedDefaults(this, node);
        }

        public
        CxxCompilerOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}
    }
}
