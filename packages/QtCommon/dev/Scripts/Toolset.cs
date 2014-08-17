#region License
// Copyright 2010-2014 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion
namespace QtCommon
{
    public abstract class Toolset :
        Bam.Core.IToolset
    {
        protected abstract string GetInstallPath(Bam.Core.BaseTarget baseTarget);
        protected abstract string GetVersionNumber();
        protected virtual string
        GetBinPath(
            Bam.Core.BaseTarget baseTarget)
        {
            var installPath = this.GetInstallPath(baseTarget);
            if (!baseTarget.HasPlatform(Bam.Core.EPlatform.OSX))
            {
                var binPath = System.IO.Path.Combine(installPath, "bin");
                return binPath;
            }
            else
            {
                return installPath;
            }
        }

        public virtual string
        GetIncludePath(
            Bam.Core.BaseTarget baseTarget)
        {
            if (!baseTarget.HasPlatform(Bam.Core.EPlatform.OSX))
            {
                var installPath = this.GetInstallPath(baseTarget);
                var includePath = System.IO.Path.Combine(installPath, "include");
                return includePath;
            }
            else
            {
                // frameworks are used on OSX
                return string.Empty;
            }
        }

        public virtual string
        GetLibraryPath(
            Bam.Core.BaseTarget baseTarget)
        {
            if (!baseTarget.HasPlatform(Bam.Core.EPlatform.OSX))
            {
                var installPath = this.GetInstallPath(baseTarget);
                var libPath = System.IO.Path.Combine(installPath, "lib");
                return libPath;
            }
            else
            {
                // frameworks are used on OSX
                return string.Empty;
            }
        }

        public virtual bool IncludePathIncludesQtModuleName
        {
            get
            {
                return false;
            }
        }

        protected System.Collections.Generic.Dictionary<System.Type, Bam.Core.ToolAndOptionType> toolConfig = new System.Collections.Generic.Dictionary<System.Type, Bam.Core.ToolAndOptionType>();

        public
        Toolset()
        {
            this.toolConfig[typeof(IMocTool)] = new Bam.Core.ToolAndOptionType(new MocTool(this), typeof(MocOptionCollection));
        }

        #region IToolset Members

        string
        Bam.Core.IToolset.BinPath(
            Bam.Core.BaseTarget baseTarget)
        {
            return this.GetBinPath(baseTarget);
        }

        Bam.Core.StringArray Bam.Core.IToolset.Environment
        {
            get { throw new System.NotImplementedException(); }
        }

        string
        Bam.Core.IToolset.InstallPath(
            Bam.Core.BaseTarget baseTarget)
        {
            var installPath = this.GetInstallPath(baseTarget);
            return installPath;
        }

        string
        Bam.Core.IToolset.Version(
            Bam.Core.BaseTarget baseTarget)
        {
            return this.GetVersionNumber();
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

        #endregion
    }
}
