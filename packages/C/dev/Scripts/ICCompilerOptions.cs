// <copyright file="ICCompilerOptions.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public interface ICCompilerOptions
    {
        C.ToolchainOptionCollection ToolchainOptionCollection
        {
            get;
            set;
        }

        C.DefineCollection Defines
        {
            get;
            set;
        }

        Opus.Core.DirectoryCollection IncludePaths
        {
            get;
            set;
        }

        Opus.Core.DirectoryCollection SystemIncludePaths
        {
            get;
            set;
        }

        C.ECompilerOutput OutputType
        {
            get;
            set;
        }

        bool DebugSymbols
        {
            get;
            set;
        }

        bool WarningsAsErrors
        {
            get;
            set;
        }

        bool IgnoreStandardIncludePaths
        {
            get;
            set;
        }

        C.EOptimization Optimization
        {
            get;
            set;
        }

        string CustomOptimization
        {
            get;
            set;
        }

        C.ETargetLanguage TargetLanguage
        {
            get;
            set;
        }

        bool ShowIncludes
        {
            get;
            set;
        }
    }
}