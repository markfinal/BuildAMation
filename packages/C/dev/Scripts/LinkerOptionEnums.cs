// <copyright file="LinkerOptionEnums.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public enum ELinkerOutput
    {
        Executable,
        DynamicLibrary
    }

    public enum ESubsystem
    {
        NotSet = 0,
        Console = 1,
        Windows = 2
    }
}
