// <copyright file="PreExecute.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MakeFileBuilder package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        private string topLevelMakeFilePath;

        public void PreExecute()
        {
            Opus.Core.Log.DebugMessage("PreExecute for MakeFiles");

            Opus.Core.PackageInformation mainPackage = Opus.Core.State.PackageInfo[0];
            this.topLevelMakeFilePath = System.IO.Path.Combine(mainPackage.BuildDirectory, "Makefile");
        }
    }
}
