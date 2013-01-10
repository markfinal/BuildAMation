// <copyright file="Toolset.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>DotNetFramework package</summary>
// <author>Mark Final</author>
namespace DotNetFramework
{
    public sealed class Toolset : Opus.Core.IToolset
    {
        private System.Collections.Generic.Dictionary<System.Type, Opus.Core.ITool> toolMap = new System.Collections.Generic.Dictionary<System.Type, Opus.Core.ITool>();
        private System.Collections.Generic.Dictionary<System.Type, System.Type> toolOptionsMap = new System.Collections.Generic.Dictionary<System.Type, System.Type>();

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

            this.toolMap[typeof(CSharp.ICSharpCompilerTool)] = new Csc(this);
            this.toolOptionsMap[typeof(CSharp.ICSharpCompilerTool)] = typeof(CSharp.OptionCollection);
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
                using (Microsoft.Win32.RegistryKey key = Opus.Core.Win32RegistryUtilities.OpenLMSoftwareKey(@"Microsoft\MSBuild\ToolsVersions\3.5"))
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

        Opus.Core.ITool Opus.Core.IToolset.Tool(System.Type toolType)
        {
            if (!this.toolMap.ContainsKey(toolType))
            {
                throw new Opus.Core.Exception("Tool '{0}' was not registered with toolset '{1}'", toolType.ToString(), this.ToString());
            }

            return this.toolMap[toolType];
        }

        System.Type Opus.Core.IToolset.ToolOptionType(System.Type toolType)
        {
            if (!this.toolOptionsMap.ContainsKey(toolType))
            {
                // if there is no tool then there will be no optionset
                if (!this.toolMap.ContainsKey(toolType))
                {
                    return null;
                }

                throw new Opus.Core.Exception("Tool '{0}' has no option type registered with toolset '{1}'", toolType.ToString(), this.ToString());
            }

            return this.toolOptionsMap[toolType];
        }

        string Opus.Core.IToolset.Version(Opus.Core.BaseTarget baseTarget)
        {
            return "3.5";
        }

        #endregion
    }
}
