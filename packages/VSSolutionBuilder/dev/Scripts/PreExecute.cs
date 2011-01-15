// <copyright file="PreExecute.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed partial class VSSolutionBuilder
    {
        public void PreExecute()
        {
            Opus.Core.Log.DebugMessage("PreExecute for VSSolutionBuilder");

            Opus.Core.PackageInformation mainPackage = Opus.Core.State.PackageInfo[0];
            string solutionPathName = System.IO.Path.Combine(mainPackage.BuildDirectory, mainPackage.FullName + ".sln");
            Opus.Core.Log.DebugMessage("Solution pathname is '{0}'", solutionPathName);

            this.solutionFile = new SolutionFile(solutionPathName);
        }
    }
}