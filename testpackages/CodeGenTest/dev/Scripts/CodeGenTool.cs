[assembly: Opus.Core.RegisterTargetToolChain("codegentool", "CodeGenTest.CodeGeneratorTool.VersionString")]
[assembly: CodeGenTest.RegisterToolchain("CodeGenTest")]

namespace CodeGenTest
{
    [Opus.Core.LocalAndExportTypesAttribute(typeof(LocalCodeGenOptionsDelegateAttribute),
                                            typeof(ExportCodeGenOptionsDelegateAttribute))]
    public class CodeGenTool : Opus.Core.ITool
    {
        public string Executable(Opus.Core.Target target)
        {
            string toolchainImplementation = Opus.Core.ModuleUtilities.GetToolchainImplementation(typeof(CodeGenTest.CodeGeneratorTool));
            Opus.Core.BaseTarget baseTargetToUse = (Opus.Core.BaseTarget)target;

            Opus.Core.IModule module = Opus.Core.ModuleUtilities.GetModule(typeof(CodeGenTest.CodeGeneratorTool), baseTargetToUse);

            if (null == module)
            {
                throw new Opus.Core.Exception(System.String.Format("Unable to locate CodeGeneratorTool module in Graph for basetarget '{0}", baseTargetToUse.ToString()), false);
            }

            C.LinkerOptionCollection options = module.Options as C.LinkerOptionCollection;
            if (null == options)
            {
                throw new Opus.Core.Exception("CodeGeneratorTool options are not derived from C.LinkerOptionCollection", false);
            }

            string exe = options.OutputFilePath;
            return exe; 
        }
    }
}