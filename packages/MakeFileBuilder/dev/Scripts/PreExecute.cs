// <copyright file="PreExecute.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MakeFileBuilder package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder : Opus.Core.IBuilderPreExecute
    {
        private string topLevelMakeFilePath;

        #region IBuilderPreExecute Members

        void Opus.Core.IBuilderPreExecute.PreExecute()
        {
            Opus.Core.Log.DebugMessage("PreExecute for MakeFiles");

            Opus.Core.PackageInformation mainPackage = Opus.Core.State.PackageInfo[0];
            this.topLevelMakeFilePath = System.IO.Path.Combine(mainPackage.BuildDirectory, "Makefile");
        }

        #endregion
    }
}
