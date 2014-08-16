[assembly: Bam.Core.RegisterToolset("CodeGenTest2", typeof(CodeGenTest2.Toolset))]

namespace CodeGenTest2
{
    public class CodeGenTool :
        ICodeGenTool
    {
        public CodeGenTool(
            Bam.Core.IToolset toolset)
        {}

        #region ITool Members

        string
        Bam.Core.ITool.Executable(
            Bam.Core.BaseTarget baseTarget)
        {
            var module = Bam.Core.ModuleUtilities.GetModule(typeof(CodeGenTest2.CodeGeneratorTool), baseTarget);
            if (null == module)
            {
                throw new Bam.Core.Exception("Unable to locate CodeGeneratorTool module in Graph for basetarget '{0}", baseTarget.ToString());
            }

            var outputLoc = (module as Bam.Core.BaseModule).Locations[CSharp.Assembly.OutputFile];
            return outputLoc.GetSinglePath();
        }

        Bam.Core.Array<Bam.Core.LocationKey>
        Bam.Core.ITool.OutputLocationKeys(
            Bam.Core.BaseModule module)
        {
            var array = new Bam.Core.Array<Bam.Core.LocationKey>(
                CodeGenModule.OutputFile,
                CodeGenModule.OutputDir
                );
            return array;
        }

        #endregion
    }
}
