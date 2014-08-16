// <copyright file="TimeProfile.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Bam.Core
{
    public class TimeProfile
    {
        public static readonly string DateTimeFormat = "m:s.fff";

        public
        TimeProfile(
            ETimingProfiles profile)
        {
            this.Profile = profile;
        }

        public void
        StartProfile()
        {
            var time = System.DateTime.Now;
            this.Start = time;
            Log.DebugMessage("Profile '{0}': start {1}", this.Profile.ToString(), time.ToString(DateTimeFormat));
        }

        public void
        StopProfile()
        {
            var time = System.DateTime.Now;
            this.Stop = time;
            this.Elapsed = time - this.Start;
            State.TimingProfiles[(int)this.Profile] = this;
            Log.DebugMessage("Profile '{0}': stop {1}", this.Profile.ToString(), time.ToString(DateTimeFormat));
        }

        public ETimingProfiles Profile
        {
            get;
            private set;
        }

        public System.DateTime Start
        {
            get;
            private set;
        }

        public System.DateTime Stop
        {
            get;
            private set;
        }

        public System.TimeSpan Elapsed
        {
            get;
            private set;
        }
    }
}
