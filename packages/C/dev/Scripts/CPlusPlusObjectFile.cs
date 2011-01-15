// <copyright file="CPlusPlusObjectFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C.CPlusPlus
{
    /// <summary>
    /// C++ object file
    /// </summary>
    [Opus.Core.AssignToolForModule(typeof(Compiler),
                                   typeof(ExportCompilerOptionsDelegateAttribute),
                                   typeof(LocalCompilerOptionsDelegateAttribute),
                                   C.ClassNames.CPlusPlusCompilerToolOptions)]
    public class ObjectFile : C.ObjectFile
    {
        public ObjectFile(params string[] fileParts)
            : base(fileParts)
        {
        }
    }
}