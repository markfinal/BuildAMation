#region License
// Copyright (c) 2010-2016, Mark Final
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
            Profiles.Add(profile);
        }

        /// <summary>
        /// Dump all of the profiles to the console.
        /// </summary>
        public static void
        DumpProfiles()
        {
            var additionalDetails = false;

            var profileHeader = "Profile";
            var minutesHeader = "Minutes";
            var secondsHeader = "Seconds";
            var millisecondsHeader = "Milliseconds";
            var startTimeHeader = "Start";
            var stopTimeHeader = "Stop";

            int maxNameLength = profileHeader.Length;
            int maxMinuteLength = minutesHeader.Length;
            int maxSecondLength = secondsHeader.Length;
            int maxMillisecondLength = millisecondsHeader.Length;
            int maxTimeLength = 9;
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

            var header =
                System.String.Format("{0}{1} | {2}{3} | {4}{5} | {6}{7} | {8}{9} | {10}{11}",
                                     profileHeader,
                                     new string(' ', maxNameLength - profileHeader.Length),
                                     minutesHeader,
                                     new string(' ', maxMinuteLength - minutesHeader.Length),
                                     secondsHeader,
                                     new string(' ', maxSecondLength - secondsHeader.Length),
                                     millisecondsHeader,
                                     new string(' ', maxMillisecondLength - millisecondsHeader.Length),
                                     new string(' ', maxTimeLength - startTimeHeader.Length),
                                     startTimeHeader,
                                     new string(' ', maxTimeLength - stopTimeHeader.Length),
                                     stopTimeHeader);
            var horizontalRule = new string('-', header.Length);
            Log.Info("\nTask timing");
            Log.Info(horizontalRule);
            Log.Info(header);
            Log.Info(horizontalRule);
            var cumulativeTime = new System.TimeSpan();
            foreach (var profile in Profiles)
            {
                var elapsedTime = (null != profile) ? profile.Elapsed : new System.TimeSpan(0);
                if (ETimingProfiles.TimedTotal != profile.Profile)
                {
                    cumulativeTime = cumulativeTime.Add(elapsedTime);
                }

                string diffString = null;
                if (additionalDetails && (ETimingProfiles.TimedTotal != profile.Profile))
                {
                    var diff = profile.Start - Profiles.First().Stop;
                    diffString = diff.Milliseconds.ToString();
                }

                var minuteString = elapsedTime.Minutes.ToString();
                var secondString = elapsedTime.Seconds.ToString();
                var millisecondString = elapsedTime.Milliseconds.ToString();
                var startTimeString = (null != profile) ? profile.Start.ToString(TimeProfile.DateTimeFormat) : "0";
                var stopTimeString = (null != profile) ? profile.Stop.ToString(TimeProfile.DateTimeFormat) : "0";

                if (ETimingProfiles.TimedTotal == profile.Profile)
                {
                    Log.Info(horizontalRule);
                    var cumulativeString = "CumulativeTotal";
                    var cumulativeMinutesString = cumulativeTime.Minutes.ToString();
                    var cumulativeSecondsString = cumulativeTime.Seconds.ToString();
                    var cumulativeMillisecondsString = cumulativeTime.Milliseconds.ToString();

                    Log.Info("{0}{1} | {2}{3} | {4}{5} | {6}{7}",
                             cumulativeString,
                             new string(' ', maxNameLength - cumulativeString.Length),
                             new string(' ', maxMinuteLength - cumulativeMinutesString.Length),
                             cumulativeMinutesString,
                             new string(' ', maxSecondLength - cumulativeSecondsString.Length),
                             cumulativeSecondsString,
                             new string(' ', maxMillisecondLength - cumulativeMillisecondsString.Length),
                         cumulativeMillisecondsString);
                    Log.Info(horizontalRule);
                }

                Log.Info("{0}{1} | {2}{3} | {4}{5} | {6}{7} | {8}{9} | {10}{11} {12}",
                         profile.Profile.ToString(),
                         new string(' ', maxNameLength - profile.Profile.ToString().Length),
                         new string(' ', maxMinuteLength - minuteString.Length),
                         minuteString,
                         new string(' ', maxSecondLength - secondString.Length),
                         secondString,
                         new string(' ', maxMillisecondLength - millisecondString.Length),
                         millisecondString,
                         new string(' ', maxTimeLength - startTimeString.Length),
                         startTimeString,
                         new string(' ', maxTimeLength - stopTimeString.Length),
                         stopTimeString,
                         diffString);
            }
            Log.Info(horizontalRule);
        }
    }
}
