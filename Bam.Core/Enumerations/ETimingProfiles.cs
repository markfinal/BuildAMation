// <copyright file="ETimingProfiles.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Bam.Core
{
    public enum ETimingProfiles
    {
        ProcessCommandLine = 0,
        PreambleCommandExecution,
        GatherSource,
        AssemblyCompilation,
        LoadAssembly,
        AdditionalArgumentProcessing,
        IdentifyBuildableModules,
        PopulateGraph,
        CreateOptionCollections,
        GraphExecution,
        TimedTotal
    }
}
