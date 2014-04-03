// <copyright file="OutputFileFlags.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CSharp package</summary>
// <author>Mark Final</author>
namespace CSharp
{
    [System.Flags]
    public enum OutputFileFlags
    {
        AssemblyFile = (1 << 0),
        ProgramDatabaseFile = (1 << 1)
    }
}
