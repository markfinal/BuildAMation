[assembly: Opus.Core.RegisterToolset("CodeGenTest", typeof(CodeGenTest.Toolset))]

namespace CodeGenTest
{
    public class CodeGenTool : ICodeGenTool
    {
        private Opus.Core.IToolset toolset;

        public CodeGenTool(Opus.Core.IToolset toolset)
        {
            this.toolset = toolset;
        }

        #region ITool Members

        string Opus.Core.ITool.Executable(Opus.Core.BaseTarget baseTarget)
        {
            Opus.Core.IModule module = Opus.Core.ModuleUtilities.GetModule(typeof(CodeGenTest.CodeGeneratorTool), baseTarget);

            if (null == module)
            {
                throw new Opus.Core.Exception("Unable to locate CodeGeneratorTool module in Graph for basetarget '{0}", baseTarget.ToString());
            }

            C.LinkerOptionCollection options = module.Options as C.LinkerOptionCollection;
            if (null == options)
            {
                throw new Opus.Core.Exception("CodeGeneratorTool options are not derived from C.LinkerOptionCollection");
            }

            string exe = options.OutputFilePath;
            return exe;
        }

        Opus.Core.Array<Opus.Core.LocationKey> Opus.Core.ITool.OutputLocationKeys
        {
            get
            {
                var array = new Opus.Core.Array<Opus.Core.LocationKey>(
                    CodeGenModule.GeneratedSourceFileLocationKey
                    );
                return array;
            }
        }

        #endregion
    }
}
