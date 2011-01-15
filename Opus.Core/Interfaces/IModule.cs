﻿// <copyright file="IModule.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public delegate void UpdateOptionCollectionDelegate(IModule module, Core.Target target);

    public interface IModule
    {
        event Core.UpdateOptionCollectionDelegate UpdateOptions;

        BaseOptionCollection Options
        {
            get;
            set;
        }

        ModuleCollection GetNestedDependents(Core.Target target);
        
        void ExecuteOptionUpdate(Core.Target target);
    }
}