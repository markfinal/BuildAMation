// <copyright file="CxxCompilerOptionEnums.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C.Cxx
{
    // TOOD: rename enum as structured?
    public enum EExceptionHandler
    {
        Disabled = 0,
        Synchronous = 1,
        Asynchronous = 2,
        SyncWithCExternFunctions = 3
    }
}
