// <copyright file="PreExecute.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed partial class XcodeBuilder :
        Opus.Core.IBuilderPreExecute
    {
#region IBuilderPreExecute Members

        void
        Opus.Core.IBuilderPreExecute.PreExecute()
        {
            this.Workspace = new Workspace();
        }

#endregion
    }
}
