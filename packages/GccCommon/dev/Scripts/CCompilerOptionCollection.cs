// <copyright file="CCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
{
    // Not sealed since the C++ compiler inherits from it
    public partial class CCompilerOptionCollection : C.CompilerOptionCollection, C.ICCompilerOptions, ICCompilerOptions
    {
        protected override void SetDefaultOptionValues(Opus.Core.DependencyNode node)
        {
            ICCompilerOptions compilerInterface = this as ICCompilerOptions;
            compilerInterface.AllWarnings = true;
            compilerInterface.ExtraWarnings = true;

            base.SetDefaultOptionValues(node);

            Opus.Core.Target target = node.Target;
            compilerInterface.SixtyFourBit = Opus.Core.OSUtilities.Is64Bit(target);

            if (target.HasConfiguration(Opus.Core.EConfiguration.Debug))
            {
                compilerInterface.StrictAliasing = false;
                compilerInterface.InlineFunctions = false;
            }
            else
            {
                compilerInterface.StrictAliasing = true;
                compilerInterface.InlineFunctions = true;
            }

            compilerInterface.PositionIndependentCode = false;

            Opus.Core.IToolset toolset = target.Toolset;
            C.ICompilerTool compilerTool = toolset.Tool(typeof(C.ICompilerTool)) as C.ICompilerTool;
            (this as C.ICCompilerOptions).SystemIncludePaths.AddRange(compilerTool.IncludePaths((Opus.Core.BaseTarget)node.Target));

            (this as C.ICCompilerOptions).TargetLanguage = C.ETargetLanguage.C;

            compilerInterface.Pedantic = true;

            if (null != node.Children)
            {
                // we use gcc as the compile - if there is ObjectiveC code included, disable ignoring standard include paths as it gets complicated otherwise
                foreach (Opus.Core.DependencyNode child in node.Children)
                {
                    if (child.Module is C.ObjC.ObjectFile || child.Module is C.ObjC.ObjectFileCollection ||
                        child.Module is C.ObjCxx.ObjectFile || child.Module is C.ObjCxx.ObjectFileCollection)
                    {
                        (this as C.ICCompilerOptions).IgnoreStandardIncludePaths = false;
                        break;
                    }
                }
            }
        }

        public CCompilerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        public override Opus.Core.DirectoryCollection DirectoriesToCreate()
        {
            Opus.Core.DirectoryCollection directoriesToCreate = new Opus.Core.DirectoryCollection();

            string objPathName = this.ObjectFilePath;
            if (null != objPathName)
            {
                directoriesToCreate.Add(System.IO.Path.GetDirectoryName(objPathName));
            }

            return directoriesToCreate;
        }
    }
}
