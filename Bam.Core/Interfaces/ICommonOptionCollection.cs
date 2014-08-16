// <copyright file="ICommonOptionCollection.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Bam.Core
{
    public interface ICommonOptionCollection
    {
        BaseOptionCollection CommonOptionCollection
        {
            get;
            set;
        }
    }
}
