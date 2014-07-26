// <copyright file="CCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
{
    // Not sealed since the C++ compiler inherits from it
    public partial class CCompilerOptionCollection :
        C.CompilerOptionCollection,
        C.ICCompilerOptions,
        ICCompilerOptions
    {
        protected override void
        SetDefaultOptionValues(
            Opus.Core.DependencyNode node)
        {
            var localCompilerOptions = this as ICCompilerOptions;
            localCompilerOptions.AllWarnings = true;
            localCompilerOptions.ExtraWarnings = true;

            base.SetDefaultOptionValues(node);

            var target = node.Target;
            localCompilerOptions.SixtyFourBit = Opus.Core.OSUtilities.Is64Bit(target);

            if (target.HasConfiguration(Opus.Core.EConfiguration.Debug))
            {
                localCompilerOptions.StrictAliasing = false;
                localCompilerOptions.InlineFunctions = false;
            }
            else
            {
                localCompilerOptions.StrictAliasing = true;
                localCompilerOptions.InlineFunctions = true;
            }

            localCompilerOptions.PositionIndependentCode = false;

            var toolset = target.Toolset;
            var compilerTool = toolset.Tool(typeof(C.ICompilerTool)) as C.ICompilerTool;
            var cCompilerOptions = this as C.ICCompilerOptions;
            cCompilerOptions.SystemIncludePaths.AddRange(compilerTool.IncludePaths((Opus.Core.BaseTarget)node.Target));

            cCompilerOptions.TargetLanguage = C.ETargetLanguage.C;

            localCompilerOptions.Pedantic = true;

            if (null != node.Children)
            {
                // we use gcc as the compile - if there is ObjectiveC code included, disable ignoring standard include paths as it gets complicated otherwise
                foreach (var child in node.Children)
                {
                    if (child.Module is C.ObjC.ObjectFile || child.Module is C.ObjC.ObjectFileCollection ||
                        child.Module is C.ObjCxx.ObjectFile || child.Module is C.ObjCxx.ObjectFileCollection)
                    {
                        cCompilerOptions.IgnoreStandardIncludePaths = false;
                        break;
                    }
                }
            }
        }

        public
        CCompilerOptionCollection(
            Opus.Core.DependencyNode node) : base(node)
        {}
    }
}
