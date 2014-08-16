// <copyright file="PreExecute.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MakeFileBuilder package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder :
        Bam.Core.IBuilderPreExecute
    {
        private string topLevelMakeFilePath;

        #region IBuilderPreExecute Members

        void
        Bam.Core.IBuilderPreExecute.PreExecute()
        {
            Bam.Core.Log.DebugMessage("PreExecute for MakeFiles");
            this.topLevelMakeFilePath = System.IO.Path.Combine(Bam.Core.State.BuildRoot, "Makefile");
        }

        #endregion
    }
}
