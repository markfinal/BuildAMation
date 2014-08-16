﻿// <copyright file="INestedDependents.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Bam.Core
{
    public interface INestedDependents
    {
        ModuleCollection
        GetNestedDependents(
            Target target);
    }
}
