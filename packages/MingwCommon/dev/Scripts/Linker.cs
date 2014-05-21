// <copyright file="Linker.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MingwCommon package</summary>
// <author>Mark Final</author>
namespace MingwCommon
{
    public abstract class Linker : C.ILinkerTool, C.IWinImportLibrary, Opus.Core.IToolForwardedEnvironmentVariables, Opus.Core.IToolEnvironmentVariables
    {
        private Opus.Core.IToolset toolset;
        private Opus.Core.StringArray requiredEnvironmentVariables = new Opus.Core.StringArray();

        protected Linker(Opus.Core.IToolset toolset)
        {
            this.toolset = toolset;
            this.requiredEnvironmentVariables.Add("TEMP");
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
                return "-Wl,--start-group";
            }
        }

        string C.ILinkerTool.EndLibraryList
        {
            get
            {
                return "-Wl,--end-group";
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
            throw new System.NotImplementedException();
        }

        #endregion

        #region C.IWinImportLibrary Members

        string C.IWinImportLibrary.ImportLibraryPrefix
        {
            get
            {
                return "lib";
            }
        }

        string C.IWinImportLibrary.ImportLibrarySuffix
        {
            get
            {
                return ".a";
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
            string installPath = this.toolset.BinPath(baseTarget);
            string executablePath = System.IO.Path.Combine(installPath, this.Filename);
            return executablePath;
        }

        Opus.Core.Array<Opus.Core.LocationKey>
        Opus.Core.ITool.OutputLocationKeys(
            Opus.Core.BaseModule module)
        {
            var array = new Opus.Core.Array<Opus.Core.LocationKey>(
                C.Application.OutputFile,
                C.Application.OutputDir,
                C.Application.MapFile,
                C.Application.MapFileDir);
            if (module is C.DynamicLibrary)
            {
                array.AddRange(new [] {
                    C.DynamicLibrary.ImportLibraryFile,
                    C.DynamicLibrary.ImportLibraryDir});
            }
            return array;
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
            System.Collections.Generic.Dictionary<string, Opus.Core.StringArray> dictionary = new System.Collections.Generic.Dictionary<string, Opus.Core.StringArray>();
            dictionary["PATH"] = this.toolset.Environment;
            return dictionary;
        }

        #endregion
    }
}