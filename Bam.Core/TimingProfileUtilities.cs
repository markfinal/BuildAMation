#region License
// Copyright (c) 2010-2019, Mark Final
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
using System.Linq;
namespace Bam.Core
{
    /// <summary>
    /// Static utility class for all timing profiles.
    /// </summary>
    public static class TimingProfileUtilities
    {
        private static Array<TimeProfile> Profiles = new Array<TimeProfile>();

        /// <summary>
        /// Register a new timing profile.
        /// </summary>
        /// <param name="profile">Profile.</param>
        public static void
        RegisterProfile(
            TimeProfile profile)
        {
            if (null == profile)
            {
                throw new Exception("Timing profile is invalid");
            }
            Profiles.Add(profile);
        }

        /// <summary>
        /// Dump all of the profiles to the console.
        /// </summary>
        public static void
        DumpProfiles()
        {
            var profileHeader = "Task";
            var minutesHeader = "Minutes";
            var secondsHeader = "Seconds";
            var millisecondsHeader = "Milliseconds";

            int maxNameLength = profileHeader.Length;
            int maxMinuteLength = minutesHeader.Length;
            int maxSecondLength = secondsHeader.Length;
            int maxMillisecondLength = millisecondsHeader.Length;
            foreach (var profile in Profiles)
            {
                int nameLength = profile.Profile.ToString().Length;
                if (nameLength > maxNameLength)
                {
                    maxNameLength = nameLength;
                }

                int minuteLength = profile.Elapsed.Minutes.ToString().Length;
                if (minuteLength > maxMinuteLength)
                {
                    maxMinuteLength = minuteLength;
                }

                int secondLength = profile.Elapsed.Seconds.ToString().Length;
                if (secondLength > maxSecondLength)
                {
                    maxSecondLength = secondLength;
                }

                int millisecondLength = profile.Elapsed.Milliseconds.ToString().Length;
                if (millisecondLength > maxMillisecondLength)
                {
                    maxMillisecondLength = millisecondLength;
                }
            }

            var pad0 = new string(' ', maxNameLength - profileHeader.Length);
            var pad1 = new string(' ', maxMinuteLength - minutesHeader.Length);
            var pad2 = new string(' ', maxSecondLength - secondsHeader.Length);
            var pad3 = new string(' ', maxMillisecondLength - millisecondsHeader.Length);
            var header = $"{profileHeader}{pad0} | {minutesHeader}{pad1} | {secondsHeader}{pad2} | {millisecondsHeader}{pad3}";
            var sectionRule = new string('=', header.Length);
            var profileRule = new string('-', header.Length);
            Log.Info("\nTask timing");
            Log.Info(sectionRule);
            Log.Info(header);
            Log.Info(sectionRule);
            var cumulativeTime = new System.TimeSpan();
            foreach (var profile in Profiles)
            {
                var requiresProfileRule = true;

                var elapsedTime = profile.Elapsed;
                if (ETimingProfiles.TimedTotal != profile.Profile)
                {
                    cumulativeTime = cumulativeTime.Add(elapsedTime);
                }

                var minuteString = elapsedTime.Minutes > 0 ? elapsedTime.Minutes.ToString() : string.Empty;
                var secondString = elapsedTime.Seconds > 0 ? elapsedTime.Seconds.ToString() : string.Empty;
                var millisecondString = elapsedTime.Milliseconds > 0 ? elapsedTime.Milliseconds.ToString() : string.Empty;

                if (ETimingProfiles.TimedTotal == profile.Profile)
                {
                    Log.Info(sectionRule);
                    Log.Info(sectionRule);
                    var cumulativeString = "CumulativeTotal";
                    var cumulativeMinutesString = cumulativeTime.Minutes > 0 ? cumulativeTime.Minutes.ToString() : string.Empty;
                    var cumulativeSecondsString = cumulativeTime.Seconds > 0 ? cumulativeTime.Seconds.ToString() : string.Empty;
                    var cumulativeMillisecondsString = cumulativeTime.Milliseconds > 0 ? cumulativeTime.Milliseconds.ToString() : string.Empty;

                    var pad01 = new string(' ', maxNameLength - cumulativeString.Length);
                    var pad11 = new string(' ', maxMinuteLength - cumulativeMinutesString.Length);
                    var pad21 = new string(' ', maxSecondLength - cumulativeSecondsString.Length);
                    var pad31 = new string(' ', maxMillisecondLength - cumulativeMillisecondsString.Length);
                    Log.Info($"{cumulativeString}{pad01} | {pad11}{cumulativeMinutesString} | {pad21}{cumulativeSecondsString} | {pad31}{cumulativeMillisecondsString}");
                    Log.Info(sectionRule);
                    requiresProfileRule = false;
                }
                else if (ETimingProfiles.GraphExecution == profile.Profile)
                {
                    requiresProfileRule = false;
                }

                var pad02 = new string(' ', maxNameLength - profile.Profile.ToString().Length);
                var pad12 = new string(' ', maxMinuteLength - minuteString.Length);
                var pad22 = new string(' ', maxSecondLength - secondString.Length);
                var pad32 = new string(' ', maxMillisecondLength - millisecondString.Length);
                Log.Info($"{profile.Profile.ToString()}{pad02} | {pad12}{minuteString} | {pad22}{secondString} | {pad32}{millisecondString}");
                if (requiresProfileRule)
                {
                    Log.Info(profileRule);
                }
            }
            Log.Info(sectionRule);
        }
    }
}
