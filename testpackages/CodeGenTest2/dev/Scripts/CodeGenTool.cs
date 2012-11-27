[assembly: Opus.Core.RegisterToolset("CodeGenTest2", typeof(CodeGenTest2.Toolset))]

namespace CodeGenTest2
{
    public class CodeGenTool : ICodeGenTool
    {
        //private Opus.Core.IToolset toolset;

        public CodeGenTool(Opus.Core.IToolset toolset)
        {
#if false
            this.toolset = toolset;
#endif
        }

        #region ITool Members

        string Opus.Core.ITool.Executable(Opus.Core.Target target)
        {
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

            return options.OutputFilePath;
        }

        #endregion
    }
}