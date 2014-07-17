// <copyright file="TextFileTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XmlUtilities package</summary>
// <author>Mark Final</author>
namespace XmlUtilities
{
    public class TextFileTool : ITextFileTool
    {
        #region ITool Members

        string Opus.Core.ITool.Executable(Opus.Core.BaseTarget baseTarget)
        {
            throw new System.NotImplementedException ();
        }

        Opus.Core.Array<Opus.Core.LocationKey>
        Opus.Core.ITool.OutputLocationKeys(
            Opus.Core.BaseModule module)
        {
            var array = new Opus.Core.Array<Opus.Core.LocationKey>(
                TextFileModule.OutputFile,
                TextFileModule.OutputDir
                );
            return array;
        }

        #endregion
    }
}
