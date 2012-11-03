[assembly: Opus.Core.RegisterTargetToolChain("codegentool", "CodeGenTest.CodeGeneratorTool.VersionString")]

#if true
[assembly: Opus.Core.RegisterToolset("CodeGenTest", typeof(CodeGenTest.Toolset))]
#else
[assembly: CodeGenTest.RegisterToolchain("CodeGenTest", typeof(CodeGenTest.ToolInfo))]
#endif

namespace CodeGenTest
{
    [Opus.Core.LocalAndExportTypesAttribute(typeof(LocalCodeGenOptionsDelegateAttribute),
                                            typeof(ExportCodeGenOptionsDelegateAttribute))]
    public class CodeGenTool : ICodeGenTool
    {
        private Opus.Core.IToolset toolset;

        public CodeGenTool(Opus.Core.IToolset toolset)
        {
            this.toolset = toolset;
        }

        #region ITool Members

        string Opus.Core.ITool.Executable(Opus.Core.Target target)
        {
            // NEW STYLE
#if true
            string toolchainImplementation = Opus.Core.ModuleUtilities.GetToolchainForModule(typeof(CodeGenTest.CodeGeneratorTool));
#else
            string toolchainImplementation = Opus.Core.ModuleUtilities.GetToolchainImplementation(typeof(CodeGenTest.CodeGeneratorTool));
#endif

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

        #endregion
    }
}