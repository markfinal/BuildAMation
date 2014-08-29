#region License
// Copyright 2010-2014 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
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
