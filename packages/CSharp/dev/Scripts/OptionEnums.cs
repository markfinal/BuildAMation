// <copyright file="OptionEnums.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CSharp package</summary>
// <author>Mark Final</author>
namespace CSharp
{
    public enum ETarget
    {
        Executable,
        WindowsExecutable,
        Library,
        Module
    }

    public enum EPlatform
    {
        X86,
        X64,
        Itanium,
        AnyCpu
    }

    public enum EDebugInformation
    {
        Disabled,
        ProgramDatabaseOnly,
        Full
    }

    public enum EWarningLevel
    {
        Level0 = 0,
        Level1 = 1,
        Level2 = 2,
        Level3 = 3,
        Level4 = 4
    }
}