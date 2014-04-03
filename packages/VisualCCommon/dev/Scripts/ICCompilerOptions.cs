// <copyright file="ICCompilerOptions.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    // TODO: extend with runtime library option
    public interface ICCompilerOptions
    {
        bool NoLogo
        {
            get;
            set;
        }

        bool MinimalRebuild
        {
            get;
            set;
        }

        VisualCCommon.EWarningLevel WarningLevel
        {
            get;
            set;
        }

        VisualCCommon.EDebugType DebugType
        {
            get;
            set;
        }

        VisualCCommon.EBrowseInformation BrowseInformation
        {
            get;
            set;
        }

        bool StringPooling
        {
            get;
            set;
        }

        bool DisableLanguageExtensions
        {
            get;
            set;
        }

        bool ForceConformanceInForLoopScope
        {
            get;
            set;
        }

        bool UseFullPaths
        {
            get;
            set;
        }

        EManagedCompilation CompileAsManaged
        {
            get;
            set;
        }

        EBasicRuntimeChecks BasicRuntimeChecks
        {
            get;
            set;
        }

        bool SmallerTypeConversionRuntimeCheck
        {
            get;
            set;
        }

        EInlineFunctionExpansion InlineFunctionExpansion
        {
            get;
            set;
        }

        bool EnableIntrinsicFunctions
        {
            get;
            set;
        }

        ERuntimeLibrary RuntimeLibrary
        {
            get;
            set;
        }
    }
}
