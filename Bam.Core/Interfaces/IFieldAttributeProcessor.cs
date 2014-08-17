// <copyright file="IFieldAttributeProcessor.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Bam.Core
{
    public interface IFieldAttributeProcessor
    {
        void
        Execute(
            object sender,
            IModule module,
            Target target);
    }
}
