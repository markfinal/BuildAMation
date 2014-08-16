// <copyright file="TextFileTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XmlUtilities package</summary>
// <author>Mark Final</author>
namespace XmlUtilities
{
    public class TextFileTool :
        ITextFileTool
    {
        #region ITool Members

        string
        Bam.Core.ITool.Executable(
            Bam.Core.BaseTarget baseTarget)
        {
            throw new System.NotImplementedException ();
        }

        Bam.Core.Array<Bam.Core.LocationKey>
        Bam.Core.ITool.OutputLocationKeys(
            Bam.Core.BaseModule module)
        {
            var array = new Bam.Core.Array<Bam.Core.LocationKey>(
                TextFileModule.OutputFile,
                TextFileModule.OutputDir
                );
            return array;
        }

        #endregion
    }
}
