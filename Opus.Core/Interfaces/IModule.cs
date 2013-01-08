// <copyright file="IModule.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public delegate void UpdateOptionCollectionDelegate(IModule module, Target target);

    public interface IModule
    {
        event UpdateOptionCollectionDelegate UpdateOptions;

        BaseOptionCollection Options
        {
            get;
            set;
        }
    }
}