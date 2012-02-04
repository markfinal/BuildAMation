[assembly: Opus.Core.RegisterTargetToolChain("codegentool", "CodeGenTest2.CodeGeneratorTool.VersionString")]

namespace CodeGenTest2
{
    public class CodeGenTool : Opus.Core.ITool
    {
        public string Executable(Opus.Core.Target target)
        {
            string toolchainImplementation = Opus.Core.ModuleUtilities.GetToolchainImplementation(typeof(CodeGenTest2.CodeGeneratorTool));
            Opus.Core.Target targetToUse = Opus.Core.Target.CreateFullyFormedTarget(target, toolchainImplementation);

            Opus.Core.IModule module = Opus.Core.ModuleUtilities.GetModule(typeof(CodeGenTest2.CodeGeneratorTool), targetToUse);
            if (null == module)
            {
                throw new Opus.Core.Exception(System.String.Format("Unable to locate CodeGeneratorTool module in Graph for target '{0}", targetToUse.ToString()), false);
            }

            CSharp.OptionCollection options = module.Options as CSharp.OptionCollection;
            if (null == options)
            {
                throw new Opus.Core.Exception("CodeGeneratorTool options are not derived from CSharp.OptionCollection", false);
            }

            string exe = options.OutputFilePath;
            return exe;
        }
    }
}