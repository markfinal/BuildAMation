// <copyright file="CCompilerOptionEnums.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public enum EOptimization
    {
        Off = 0,
        Size = 1,
        Speed = 2,
        Full = 3,
        Custom = 4 // TODO: confirm
    }

    public enum ECompilerOutput
    {
        CompileOnly = 0,
        Preprocess = 1
    }

    public enum ETargetLanguage
    {
        Default = 0,
        C = 1,
        Cxx = 2
    }
}