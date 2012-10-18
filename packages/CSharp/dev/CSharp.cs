// <copyright file="CSharp.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CSharp package</summary>
// <author>Mark Final</author>
[assembly: Opus.Core.MapToolChainClassTypes("CSharp", "dotnet", "ClassCsc", typeof(CSharp.Csc), typeof(CSharp.OptionCollection))]

[assembly: CSharp.RegisterToolchain("dotnet", typeof(CSharp.ToolsetInfo))]

namespace CSharp
{
    public sealed class ToolsetInfo : Opus.Core.IToolsetInfo
    {
        #region IToolsetInfo Members

        string Opus.Core.IToolsetInfo.BinPath(Opus.Core.Target target)
        {
            throw new System.NotImplementedException();
        }

        Opus.Core.StringArray Opus.Core.IToolsetInfo.Environment
        {
            get { throw new System.NotImplementedException(); }
        }

        string Opus.Core.IToolsetInfo.InstallPath(Opus.Core.Target target)
        {
            throw new System.NotImplementedException();
        }

        string Opus.Core.IToolsetInfo.Version(Opus.Core.Target target)
        {
            return "dev";
        }

        #endregion
    }
}
