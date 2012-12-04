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
        protected override void InitializeDefaults(Opus.Core.DependencyNode node)
        {
            ICCompilerOptions compilerInterface = this as ICCompilerOptions;
            compilerInterface.AllWarnings = true;
            compilerInterface.ExtraWarnings = true;

            base.InitializeDefaults(node);

            Opus.Core.Target target = node.Target;
            this["64bit"] = new Opus.Core.ValueTypeOption<bool>(Opus.Core.OSUtilities.Is64Bit(target));

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
            (this as C.ICCompilerOptions).SystemIncludePaths.AddRange(compilerTool.IncludePaths(node.Target));

            (this as C.ICCompilerOptions).TargetLanguage = C.ETargetLanguage.C;

            compilerInterface.Pedantic = true;
        }

        public CCompilerOptionCollection()
            : base()
        {
        }

        public CCompilerOptionCollection(Opus.Core.DependencyNode node)
            : base(node)
        {
        }

        private static void SixtyFourBitCommandLine(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Option option, Opus.Core.Target target)
        {
            Opus.Core.ValueTypeOption<bool> sixtyFourBitOption = option as Opus.Core.ValueTypeOption<bool>;
            if (sixtyFourBitOption.Value)
            {
                commandLineBuilder.Add("-m64");
            }
            else
            {
                commandLineBuilder.Add("-m32");
            }
        }

        public override Opus.Core.DirectoryCollection DirectoriesToCreate()
        {
            Opus.Core.DirectoryCollection directoriesToCreate = new Opus.Core.DirectoryCollection();

            string objPathName = this.ObjectFilePath;
            if (null != objPathName)
            {
                directoriesToCreate.AddAbsoluteDirectory(System.IO.Path.GetDirectoryName(objPathName), false);
            }

            return directoriesToCreate;
        }
    }
}
