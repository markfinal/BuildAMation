[assembly: Opus.Core.RegisterTargetToolChain("codegentool", "CodeGenTest.CodeGeneratorTool.VersionString")]

namespace CodeGenTest
{
    public class CodeGenTool : Opus.Core.ITool
    {
        public Opus.Core.StringArray EnvironmentPaths(Opus.Core.Target target)
        {
            return new Opus.Core.StringArray();
        }

        public string Executable(Opus.Core.Target target)
        {
            string toolchainImplementation = Opus.Core.ModuleUtilities.GetToolchainImplementation(typeof(CodeGenTest.CodeGeneratorTool));
            Opus.Core.Target targetToUse = new Opus.Core.Target(target, toolchainImplementation);

            Opus.Core.IModule module = Opus.Core.ModuleUtilities.GetModule(typeof(CodeGenTest.CodeGeneratorTool), targetToUse);
            if (null == module)
            {
                throw new Opus.Core.Exception(System.String.Format("Unable to locate CodeGeneratorTool module in Graph for target '{0}", targetToUse.ToString()), false);
            }

            C.LinkerOptionCollection options = module.Options as C.LinkerOptionCollection;
            if (null == options)
            {
                throw new Opus.Core.Exception("CodeGeneratorTool options are not derived from C.LinkerOptionCollection", false);
            }

            string exe = options.OutputFilePath;
            return exe; 
        }

        public Opus.Core.StringArray RequiredEnvironmentVariables
        {
            get
            {
                return new Opus.Core.StringArray();
            }
        }
    }
}