// <copyright file="ICompilerInfo.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public interface ICompilerInfo
    {
        string PreprocessedOutputSuffix
        {
            get;
        }

        string ObjectFileSuffix
        {
            get;
        }

        string ObjectFileOutputSubDirectory
        {
            get;
        }
    }
}