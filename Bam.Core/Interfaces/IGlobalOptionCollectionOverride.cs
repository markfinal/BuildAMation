// <copyright file="IGlobalOptionCollectionOverride.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Bam.Core
{
    public interface IGlobalOptionCollectionOverride
    {
        void
        OverrideOptions(
            BaseOptionCollection optionCollection,
            Target target);
    }
}
