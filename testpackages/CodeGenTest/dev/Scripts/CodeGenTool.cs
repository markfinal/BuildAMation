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
            var module = Opus.Core.ModuleUtilities.GetModule(typeof(CodeGenTest.CodeGeneratorTool), baseTarget);

            if (null == module)
            {
                throw new Opus.Core.Exception("Unable to locate CodeGeneratorTool module in Graph for basetarget '{0}", baseTarget.ToString());
            }

#if true
            var outputLoc = (module as Opus.Core.BaseModule).Locations[C.Application.OutputFile];
            return outputLoc.GetSinglePath();
#else
            C.LinkerOptionCollection options = module.Options as C.LinkerOptionCollection;
            if (null == options)
            {
                throw new Opus.Core.Exception("CodeGeneratorTool options are not derived from C.LinkerOptionCollection");
            }

            string exe = options.OutputFilePath;
            return exe;
#endif
        }

        Opus.Core.Array<Opus.Core.LocationKey> Opus.Core.ITool.OutputLocationKeys
        {
            get
            {
                var array = new Opus.Core.Array<Opus.Core.LocationKey>(
                    CodeGenModule.OutputFile,
                    CodeGenModule.OutputDir
                    );
                return array;
            }
        }

        #endregion
    }
}
