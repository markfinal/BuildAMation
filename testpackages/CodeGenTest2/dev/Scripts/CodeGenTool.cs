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

        string Opus.Core.ITool.Executable(Opus.Core.BaseTarget baseTarget)
        {
            var module = Opus.Core.ModuleUtilities.GetModule(typeof(CodeGenTest2.CodeGeneratorTool), baseTarget);
            if (null == module)
            {
                throw new Opus.Core.Exception("Unable to locate CodeGeneratorTool module in Graph for basetarget '{0}", baseTarget.ToString());
            }

#if true
            var outputLoc = (module as Opus.Core.BaseModule).Locations[CSharp.Assembly.OutputFile];
            return outputLoc.GetSinglePath();
#else
            var options = module.Options as CSharp.OptionCollection;
            if (null == options)
            {
                throw new Opus.Core.Exception("CodeGeneratorTool options are not derived from CSharp.OptionCollection");
            }

            return options.OutputFilePath;
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
