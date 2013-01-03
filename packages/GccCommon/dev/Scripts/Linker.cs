// <copyright file="Linker.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
{
    public abstract class Linker : C.ILinkerTool, Opus.Core.IToolEnvironmentVariables
    {
        protected Opus.Core.IToolset toolset;
        private Opus.Core.StringArray environment;

        protected Linker(Opus.Core.IToolset toolset)
        {
            this.toolset = toolset;
            this.environment = new Opus.Core.StringArray("/usr/bin");
        }

        protected abstract string Filename
        {
            get;
        }

        #region ILinkerTool Members

        string C.ILinkerTool.ExecutableSuffix
        {
            get
            {
                return string.Empty;
            }
        }

        string C.ILinkerTool.MapFileSuffix
        {
            get
            {
                return ".map";
            }
        }

        string C.ILinkerTool.StartLibraryList
        {
            get
            {
                if (Opus.Core.OSUtilities.IsUnixHosting)
                {
                    return "-Wl,--start-group";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        string C.ILinkerTool.EndLibraryList
        {
            get
            {
                if (Opus.Core.OSUtilities.IsUnixHosting)
                {
                    return "-Wl,--end-group";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        string C.ILinkerTool.DynamicLibraryPrefix
        {
            get
            {
                return "lib";
            }
        }

        string C.ILinkerTool.DynamicLibrarySuffix
        {
            get
            {
                if (Opus.Core.OSUtilities.IsUnixHosting)
                {
                    return ".so";
                }
                else if (Opus.Core.OSUtilities.IsOSXHosting)
                {
                    return ".dylib";
                }
                else
                {
                    return null;
                }
            }
        }

        string C.ILinkerTool.BinaryOutputSubDirectory
        {
            get
            {
                return "bin";
            }
        }

        Opus.Core.StringArray C.ILinkerTool.LibPaths(Opus.Core.Target target)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region ITool Members

        string Opus.Core.ITool.Executable(Opus.Core.BaseTarget baseTarget)
        {
            string installPath = this.toolset.BinPath(baseTarget);
            string executablePath = System.IO.Path.Combine(installPath, this.Filename);
            return executablePath;
        }

        #endregion

        #region IToolEnvironmentVariables implementation
        System.Collections.Generic.Dictionary<string, Opus.Core.StringArray> Opus.Core.IToolEnvironmentVariables.Variables(Opus.Core.Target target)
        {
            System.Collections.Generic.Dictionary<string, Opus.Core.StringArray> dictionary = new System.Collections.Generic.Dictionary<string, Opus.Core.StringArray>();
            dictionary["PATH"] = this.environment;
            return dictionary;
        }
        #endregion
    }
}

