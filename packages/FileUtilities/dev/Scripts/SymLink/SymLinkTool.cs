// <copyright file="SymLinkTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
[assembly: Opus.Core.RegisterTargetToolChain("symlinktool", "FileUtilities.SymLinkTool.Version")]
[assembly: FileUtilities.SymLinkRegisterToolchainAttribute(typeof(FileUtilities.SymLinkToolset))]

namespace FileUtilities
{
    public sealed class SymLinkToolset : Opus.Core.IToolset
    {
        #region IToolset Members

        string Opus.Core.IToolset.BinPath(Opus.Core.BaseTarget baseTarget)
        {
            throw new System.NotImplementedException();
        }

        Opus.Core.StringArray Opus.Core.IToolset.Environment
        {
            get { throw new System.NotImplementedException(); }
        }

        string Opus.Core.IToolset.InstallPath(Opus.Core.BaseTarget baseTarget)
        {
            throw new System.NotImplementedException();
        }

        string Opus.Core.IToolset.Version(Opus.Core.BaseTarget baseTarget)
        {
            return "dev";
        }

        Opus.Core.ITool Opus.Core.IToolset.Tool(System.Type toolType)
        {
            return null;
        }

        #endregion
    }

    [Opus.Core.LocalAndExportTypes(typeof(LocalOptionsDelegateAttribute),
                                   typeof(ExportOptionsDelegateAttribute))]
    public sealed class SymLinkTool : Opus.Core.ITool
    {
        public static string Version
        {
            get
            {
                return "dev";
            }
        }

        public string Executable(Opus.Core.Target target)
        {
            if (Opus.Core.OSUtilities.IsWindowsHosting)
            {
                return @"c:\Windows\System32\cmd.exe";
            }
            else if (Opus.Core.OSUtilities.IsUnixHosting || Opus.Core.OSUtilities.IsOSXHosting)
            {
                return "ln";
            }

            throw new Opus.Core.Exception("Unsupported platform for sym links", false);
        }
    }
}
