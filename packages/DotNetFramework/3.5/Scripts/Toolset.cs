// <copyright file="Toolset.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>DotNetFramework package</summary>
// <author>Mark Final</author>
namespace DotNetFramework
{
    public sealed class Toolset :
        Bam.Core.IToolset
    {
        private System.Collections.Generic.Dictionary<System.Type, Bam.Core.ToolAndOptionType> toolConfig = new System.Collections.Generic.Dictionary<System.Type, Bam.Core.ToolAndOptionType>();

        public
        Toolset()
        {
            if (!Bam.Core.State.HasCategory("VSSolutionBuilder"))
            {
                Bam.Core.State.AddCategory("VSSolutionBuilder");
            }

            if (!Bam.Core.State.Has("VSSolutionBuilder", "SolutionType"))
            {
                Bam.Core.State.Add<System.Type>("VSSolutionBuilder", "SolutionType", typeof(Solution));
            }

            this.toolConfig[typeof(CSharp.ICSharpCompilerTool)] = new Bam.Core.ToolAndOptionType(new Csc(this), typeof(CSharp.OptionCollection));
        }

        #region IToolset Members

        string
        Bam.Core.IToolset.BinPath(
            Bam.Core.BaseTarget baseTarget)
        {
            throw new System.NotImplementedException();
        }

        Bam.Core.StringArray Bam.Core.IToolset.Environment
        {
            get { throw new System.NotImplementedException(); }
        }

        string
        Bam.Core.IToolset.InstallPath(
            Bam.Core.BaseTarget baseTarget)
        {
            if (Bam.Core.OSUtilities.IsWindowsHosting)
            {
                string toolsPath = null;
                using (var key = Bam.Core.Win32RegistryUtilities.Open32BitLMSoftwareKey(@"Microsoft\MSBuild\ToolsVersions\3.5"))
                {
                    toolsPath = key.GetValue("MSBuildToolsPath") as string;
                }

                return toolsPath;
            }
            else if (Bam.Core.OSUtilities.IsUnixHosting || Bam.Core.OSUtilities.IsOSXHosting)
            {
                return "/usr/bin";
            }
            else
            {
                throw new Bam.Core.Exception("DotNetFramework not supported on the current platform");
            }
        }

        bool
        Bam.Core.IToolset.HasTool(
            System.Type toolType)
        {
            return this.toolConfig.ContainsKey(toolType);
        }

        Bam.Core.ITool
        Bam.Core.IToolset.Tool(
            System.Type toolType)
        {
            if (!(this as Bam.Core.IToolset).HasTool(toolType))
            {
                throw new Bam.Core.Exception("Tool '{0}' was not registered with toolset '{1}'", toolType.ToString(), this.ToString());
            }

            return this.toolConfig[toolType].Tool;
        }

        System.Type
        Bam.Core.IToolset.ToolOptionType(
            System.Type toolType)
        {
            if (!(this as Bam.Core.IToolset).HasTool(toolType))
            {
                throw new Bam.Core.Exception("Tool '{0}' has no option type registered with toolset '{1}'", toolType.ToString(), this.ToString());
            }

            return this.toolConfig[toolType].OptionsType;
        }

        string
        Bam.Core.IToolset.Version(
            Bam.Core.BaseTarget baseTarget)
        {
            return "3.5";
        }

        #endregion
    }
}
