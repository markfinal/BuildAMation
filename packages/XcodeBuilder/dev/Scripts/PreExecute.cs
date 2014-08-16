// <copyright file="PreExecute.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed partial class XcodeBuilder :
        Bam.Core.IBuilderPreExecute
    {
#region IBuilderPreExecute Members

        void
        Bam.Core.IBuilderPreExecute.PreExecute()
        {
            this.Workspace = new Workspace();
        }

#endregion
    }
}
