// <copyright file="CCompilerOptionEnums.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    public enum EWarningLevel
    {
        Level0 = 0,
        Level1,
        Level2,
        Level3,
        Level4
    }

    public enum EDebugType
    {
        Embedded = 1,
        ProgramDatabase = 3,
        ProgramDatabaseEditAndContinue = 4
    }

    public enum EBrowseInformation
    {
        None = 0,
        Full = 1,
        NoLocalSymbols = 2
    }

    public enum EManagedCompilation
    {
        NoCLR = 0,
        CLR = 1,
        PureCLR = 2,
        SafeCLR = 3,
        OldSyntaxCLR = 4
    }

    public enum EBasicRuntimeChecks
    {
        None = 0,
        StackFrame = 1,
        UninitializedVariables = 2,
        StackFrameAndUninitializedVariables = 3
    }

    public enum EInlineFunctionExpansion
    {
        None = 0,
        OnlyInline = 1,
        AnySuitable = 2
    }
}