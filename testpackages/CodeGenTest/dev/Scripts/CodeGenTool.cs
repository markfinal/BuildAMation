[assembly: Opus.Core.RegisterTargetToolChain("codegentool", "CodeGenTest.CodeGeneratorTool.VersionString")]
[assembly: CodeGenTest.RegisterToolchain("CodeGenTest", typeof(CodeGenTest.ToolInfo))]

namespace CodeGenTest
{
    public class ToolInfo : Opus.Core.IToolset
    {
        #region IToolset Members

        string Opus.Core.IToolset.BinPath(Opus.Core.BaseTarget baseTarget)
        {
            throw new System.NotImplementedException();
        }

        Opus.Core.StringArray Opus.Core.IToolset.Environment
        {
            get { throw new System.NotImplementedException(); }
        }

        string Opus.Core.IToolset.InstallPath(Opus.Core.BaseTarget baseTarget)
        {
            throw new System.NotImplementedException();
        }

        string Opus.Core.IToolset.Version(Opus.Core.BaseTarget baseTarget)
        {
            return "dev";
        }

        Opus.Core.ITool Opus.Core.IToolset.Tool(System.Type toolType)
        {
            return null;
        }

        System.Type Opus.Core.IToolset.ToolOptionType(System.Type toolType)
        {
            return null;
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
    }
}