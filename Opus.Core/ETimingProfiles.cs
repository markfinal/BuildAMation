// <copyright file="ETimingProfiles.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public enum ETimingProfiles
    {
        GatherSource = 0,
        AssemblyCompilation,
        GraphGeneration,
        GraphExecution,
        Total
    }

    public static class TimingProfiles
    {
        public static void DumpProfiles()
        {
            foreach (ETimingProfiles profile in System.Enum.GetValues(typeof(ETimingProfiles)))
            {
                System.TimeSpan elapsedTime = State.TimingProfiles[(int)profile];
                Log.Info("{0} time: {1} minutes {2} seconds {3} milliseconds",
                         profile.ToString(),
                         elapsedTime.Minutes,
                         elapsedTime.Seconds,
                         elapsedTime.Milliseconds);
            }
        }
    }
}