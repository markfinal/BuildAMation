// <copyright file="CSharp.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CSharp package</summary>
// <author>Mark Final</author>
[assembly: Opus.Core.MapToolChainClassTypes("CSharp", "dotnet", "ClassCsc", typeof(CSharp.Csc), typeof(CSharp.OptionCollection))]

[assembly: CSharp.RegisterToolchain("dotnet", typeof(CSharp.Toolset))]

namespace CSharp
{
    public sealed class Toolset : Opus.Core.IToolset
    {
        #region IToolset Members

        string Opus.Core.IToolset.BinPath(Opus.Core.Target target)
        {
            throw new System.NotImplementedException();
        }

        Opus.Core.StringArray Opus.Core.IToolset.Environment
        {
            get { throw new System.NotImplementedException(); }
        }

        string Opus.Core.IToolset.InstallPath(Opus.Core.Target target)
        {
            throw new System.NotImplementedException();
        }

        string Opus.Core.IToolset.Version(Opus.Core.Target target)
        {
            return "dev";
        }

        #endregion
    }
}
