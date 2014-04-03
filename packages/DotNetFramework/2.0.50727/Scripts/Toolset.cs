// <copyright file="Toolset.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>DotNetFramework package</summary>
// <author>Mark Final</author>
namespace DotNetFramework
{
    public sealed class Toolset : Opus.Core.IToolset
    {
        private System.Collections.Generic.Dictionary<System.Type, Opus.Core.ToolAndOptionType> toolConfig = new System.Collections.Generic.Dictionary<System.Type, Opus.Core.ToolAndOptionType>();

        public Toolset()
        {
            if (!Opus.Core.State.HasCategory("VSSolutionBuilder"))
            {
                Opus.Core.State.AddCategory("VSSolutionBuilder");
            }

            if (!Opus.Core.State.Has("VSSolutionBuilder", "SolutionType"))
            {
                Opus.Core.State.Add<System.Type>("VSSolutionBuilder", "SolutionType", typeof(Solution));
            }

            this.toolConfig[typeof(CSharp.ICSharpCompilerTool)] = new Opus.Core.ToolAndOptionType(new Csc(this), typeof(CSharp.OptionCollection));
        }

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
            if (Opus.Core.OSUtilities.IsWindowsHosting)
            {
                string toolsPath = null;
                using (Microsoft.Win32.RegistryKey key = Opus.Core.Win32RegistryUtilities.Open32BitLMSoftwareKey(@"Microsoft\MSBuild\ToolsVersions\2.0"))
                {
                    toolsPath = key.GetValue("MSBuildToolsPath") as string;
                }

                return toolsPath;
            }
            else if (Opus.Core.OSUtilities.IsUnixHosting || Opus.Core.OSUtilities.IsOSXHosting)
            {
                return "/usr/bin";
            }
            else
            {
                throw new Opus.Core.Exception("DotNetFramework not supported on the current platform");
            }
        }

        bool Opus.Core.IToolset.HasTool(System.Type toolType)
        {
            return this.toolConfig.ContainsKey(toolType);
        }

        Opus.Core.ITool Opus.Core.IToolset.Tool(System.Type toolType)
        {
            if (!(this as Opus.Core.IToolset).HasTool(toolType))
            {
                throw new Opus.Core.Exception("Tool '{0}' was not registered with toolset '{1}'", toolType.ToString(), this.ToString());
            }

            return this.toolConfig[toolType].Tool;
        }

        System.Type Opus.Core.IToolset.ToolOptionType(System.Type toolType)
        {
            if (!(this as Opus.Core.IToolset).HasTool(toolType))
            {
                throw new Opus.Core.Exception("Tool '{0}' has no option type registered with toolset '{1}'", toolType.ToString(), this.ToString());
            }

            return this.toolConfig[toolType].OptionsType;
        }

        string Opus.Core.IToolset.Version(Opus.Core.BaseTarget baseTarget)
        {
            return "2.0.50727";
        }

        #endregion
    }
}
