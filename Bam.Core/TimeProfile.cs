#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
namespace Bam.Core
{
    /// <summary>
    /// Store data for each timing profile.
    /// </summary>
    public class TimeProfile
    {
        /// <summary>
        /// Format for the time representation.
        /// </summary>
        public static readonly string DateTimeFormat = "m:s.fff";

        /// <summary>
        /// Construct a new instanced based on the enumeration.
        /// </summary>
        /// <param name="profile">Profile.</param>
        public
        TimeProfile(
            ETimingProfiles profile)
        {
            this.Profile = profile;
        }

        /// <summary>
        /// Start timing.
        /// </summary>
        public void
        StartProfile()
        {
            var time = System.DateTime.Now;
            this.Start = time;
            Log.DebugMessage("Profile '{0}': start {1}", this.Profile.ToString(), time.ToString(DateTimeFormat));
        }

        /// <summary>
        /// Stop timing.
        /// </summary>
        public void
        StopProfile()
        {
            var time = System.DateTime.Now;
            this.Stop = time;
            this.Elapsed = time - this.Start;
            TimingProfileUtilities.RegisterProfile(this);
            Log.DebugMessage("Profile '{0}': stop {1}", this.Profile.ToString(), time.ToString(DateTimeFormat));
        }

        /// <summary>
        /// Which timing profile is this?
        /// </summary>
        /// <value>The profile.</value>
        public ETimingProfiles Profile
        {
            get;
            private set;
        }

        /// <summary>
        /// Get the start time for the profile.
        /// </summary>
        /// <value>The start.</value>
        public System.DateTime Start
        {
            get;
            private set;
        }

        /// <summary>
        /// Get the stop time for the profile.
        /// </summary>
        /// <value>The stop.</value>
        public System.DateTime Stop
        {
            get;
            private set;
        }

        /// <summary>
        /// Get the elapsed time for the profile.
        /// </summary>
        /// <value>The elapsed.</value>
        public System.TimeSpan Elapsed
        {
            get;
            private set;
        }
    }
}
