[assembly: Opus.Core.RegisterTargetToolChain("codegentool", "CodeGenTest2.CodeGeneratorTool.VersionString")]

#if true
[assembly: Opus.Core.RegisterToolset("CodeGenTest2", typeof(CodeGenTest2.Toolset))]
#else
[assembly: CodeGenTest2.RegisterToolchain("CodeGenTest2", typeof(CodeGenTest2.ToolInfo))]
#endif

namespace CodeGenTest2
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
#if false
            string toolchainImplementation = Opus.Core.ModuleUtilities.GetToolchainImplementation(typeof(CodeGenTest2.CodeGeneratorTool));
#endif

            Opus.Core.BaseTarget baseTargetToUse = (Opus.Core.BaseTarget)target;

            Opus.Core.IModule module = Opus.Core.ModuleUtilities.GetModule(typeof(CodeGenTest2.CodeGeneratorTool), baseTargetToUse);
            if (null == module)
            {
                throw new Opus.Core.Exception(System.String.Format("Unable to locate CodeGeneratorTool module in Graph for basetarget '{0}", baseTargetToUse.ToString()), false);
            }

            CSharp.OptionCollection options = module.Options as CSharp.OptionCollection;
            if (null == options)
            {
                throw new Opus.Core.Exception("CodeGeneratorTool options are not derived from CSharp.OptionCollection", false);
            }

            string exe = options.OutputFilePath;
            return exe;
        }

        #endregion
    }
}