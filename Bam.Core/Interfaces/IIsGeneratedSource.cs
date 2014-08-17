// <copyright file="IIsGeneratedSource.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Bam.Core
{
    public interface IIsGeneratedSource
    {
        bool
        AutomaticallyHandledByBuilder(Target target);
    }
}
