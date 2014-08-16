// <copyright file="PreExecute.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed partial class VSSolutionBuilder :
        Bam.Core.IBuilderPreExecute
    {
        #region IBuilderPreExecute Members

        void
        Bam.Core.IBuilderPreExecute.PreExecute()
        {
            Bam.Core.Log.DebugMessage("PreExecute for VSSolutionBuilder");

            var mainPackage = Bam.Core.State.PackageInfo[0];
            var solutionPathName = System.IO.Path.Combine(mainPackage.BuildDirectory, mainPackage.FullName + ".sln");
            Bam.Core.Log.DebugMessage("Solution pathname is '{0}'", solutionPathName);

            this.solutionFile = new SolutionFile(solutionPathName);
        }

        #endregion
    }
}
