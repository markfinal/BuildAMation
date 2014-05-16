// <copyright file="Linker.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    public sealed class Linker : C.ILinkerTool, C.IWinImportLibrary, Opus.Core.IToolSupportsResponseFile, Opus.Core.IToolForwardedEnvironmentVariables, Opus.Core.IToolEnvironmentVariables
    {
        private Opus.Core.IToolset toolset;
        private Opus.Core.StringArray requiredEnvironmentVariables = new Opus.Core.StringArray();

        public Linker(Opus.Core.IToolset toolset)
        {
            this.toolset = toolset;
            // temp environment variables avoid generation of _CL_<hex> temporary files in the current directory
            this.requiredEnvironmentVariables.Add("TEMP");
            this.requiredEnvironmentVariables.Add("TMP");
        }

        #region ILinkerTool Members

        string C.ILinkerTool.ExecutableSuffix
        {
            get
            {
                return ".exe";
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
                return string.Empty;
            }
        }

        string C.ILinkerTool.EndLibraryList
        {
            get
            {
                return string.Empty;
            }
        }

        string C.ILinkerTool.DynamicLibraryPrefix
        {
            get
            {
                return string.Empty;
            }
        }

        string C.ILinkerTool.DynamicLibrarySuffix
        {
            get
            {
                return ".dll";
            }
        }

        string C.ILinkerTool.BinaryOutputSubDirectory
        {
            get
            {
                return "bin";
            }
        }

        Opus.Core.StringArray C.ILinkerTool.LibPaths(Opus.Core.BaseTarget baseTarget)
        {
            if (baseTarget.HasPlatform(Opus.Core.EPlatform.Win64))
            {
                return (this.toolset as VisualCCommon.Toolset).lib64Folder;
            }
            else
            {
                return (this.toolset as VisualCCommon.Toolset).lib32Folder;
            }
        }

        #endregion

        #region C.IWinImportLibrary Members

        string C.IWinImportLibrary.ImportLibraryPrefix
        {
            get
            {
                return string.Empty;
            }
        }

        string C.IWinImportLibrary.ImportLibrarySuffix
        {
            get
            {
                return ".lib";
            }
        }

        string C.IWinImportLibrary.ImportLibrarySubDirectory
        {
            get
            {
                return "lib";
            }
        }

        #endregion

        #region ITool Members

        string Opus.Core.ITool.Executable(Opus.Core.BaseTarget baseTarget)
        {
            var binPath = this.toolset.BinPath(baseTarget);
            return System.IO.Path.Combine(binPath, "link.exe");
        }

        Opus.Core.Array<Opus.Core.LocationKey> Opus.Core.ITool.OutputLocationKeys
        {
            get
            {
                var array = new Opus.Core.Array<Opus.Core.LocationKey>(
                    C.Application.OutputFile,
                    C.Application.OutputDir,
                    C.DynamicLibrary.ImportLibraryDir,
                    C.DynamicLibrary.ImportLibraryFile
                    );
                return array;
            }
        }

        #endregion

        #region IToolSupportsResponseFile Members

        string Opus.Core.IToolSupportsResponseFile.Option
        {
            get
            {
                return "@";
            }
        }

        #endregion

        #region IToolForwardedEnvironmentVariables Members

        Opus.Core.StringArray Opus.Core.IToolForwardedEnvironmentVariables.VariableNames
        {
            get
            {
                return this.requiredEnvironmentVariables;
            }
        }

        #endregion

        #region IToolEnvironmentVariables Members

        System.Collections.Generic.Dictionary<string, Opus.Core.StringArray> Opus.Core.IToolEnvironmentVariables.Variables(Opus.Core.BaseTarget baseTarget)
        {
            var environmentVariables = new System.Collections.Generic.Dictionary<string, Opus.Core.StringArray>();
            environmentVariables["LIB"] = (this as C.ILinkerTool).LibPaths(baseTarget);
            environmentVariables["PATH"] = this.toolset.Environment;
            return environmentVariables;
        }

        #endregion
    }
}