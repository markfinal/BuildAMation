[assembly: Opus.Core.RegisterTargetToolChain("codegentool", "CodeGenTest2.CodeGeneratorTool.VersionString")]
[assembly: CodeGenTest2.RegisterToolchain("CodeGenTest2", typeof(CodeGenTest2.ToolInfo))]

namespace CodeGenTest2
{
    public class ToolInfo : Opus.Core.IToolsetInfo
    {
        #region IToolsetInfo Members

        string Opus.Core.IToolsetInfo.BinPath(Opus.Core.Target target)
        {
            throw new System.NotImplementedException();
        }

        Opus.Core.StringArray Opus.Core.IToolsetInfo.Environment
        {
            get { throw new System.NotImplementedException(); }
        }

        string Opus.Core.IToolsetInfo.InstallPath(Opus.Core.Target target)
        {
            throw new System.NotImplementedException();
        }

        string Opus.Core.IToolsetInfo.Version(Opus.Core.Target target)
        {
            return "dev";
        }

        #endregion
    }

    [Opus.Core.LocalAndExportTypesAttribute(typeof(LocalCodeGenOptionsDelegateAttribute),
                                            typeof(ExportCodeGenOptionsDelegateAttribute))]
    public class CodeGenTool : Opus.Core.ITool
    {
        public string Executable(Opus.Core.Target target)
        {
            // NEW STYLE
#if true
            string toolchainImplementation = Opus.Core.ModuleUtilities.GetToolchainForModule(typeof(CodeGenTest2.CodeGeneratorTool));
#else
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
    }
}