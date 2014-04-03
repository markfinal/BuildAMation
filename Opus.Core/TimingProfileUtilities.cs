// <copyright file="TimingProfileUtilities.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public static class TimingProfileUtilities
    {
        public static void DumpProfiles()
        {
            var additionalDetails = false;

            var profileHeader = "Profile";
            var minutesHeader = "Minutes";
            var secondsHeader = "Seconds";
            var millisecondsHeader = "Milliseconds";
            var startTimeHeader = "Start";
            var stopTimeHeader = "Stop";

            var profiles = System.Enum.GetValues(typeof(ETimingProfiles));
            int maxNameLength = profileHeader.Length;
            int maxMinuteLength = minutesHeader.Length;
            int maxSecondLength = secondsHeader.Length;
            int maxMillisecondLength = millisecondsHeader.Length;
            int maxTimeLength = 9;
            foreach (var profile in profiles)
            {
                int nameLength = profile.ToString().Length;
                if (nameLength > maxNameLength)
                {
                    maxNameLength = nameLength;
                }

                var profileTime = State.TimingProfiles[(int)profile];
                if (null == profileTime)
                {
                    continue;
                }

                int minuteLength = profileTime.Elapsed.Minutes.ToString().Length;
                if (minuteLength > maxMinuteLength)
                {
                    maxMinuteLength = minuteLength;
                }

                int secondLength = profileTime.Elapsed.Seconds.ToString().Length;
                if (secondLength > maxSecondLength)
                {
                    maxSecondLength = secondLength;
                }

                int millisecondLength = profileTime.Elapsed.Milliseconds.ToString().Length;
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
            Core.Log.Info(horizontalRule);
            Core.Log.Info(header);
            Core.Log.Info(horizontalRule);
            var cumulativeTime = new System.TimeSpan();
            foreach (ETimingProfiles profile in profiles)
            {
                int intProfile = (int)profile;
                var profileTime = State.TimingProfiles[intProfile];
                var elapsedTime = (null != profileTime) ? profileTime.Elapsed : new System.TimeSpan(0);
                if (ETimingProfiles.TimedTotal != profile)
                {
                    cumulativeTime = cumulativeTime.Add(elapsedTime);
                }

                string diffString = null;
                if (additionalDetails && (intProfile > 0) && (ETimingProfiles.TimedTotal != profile))
                {
                    var diff = profileTime.Start - State.TimingProfiles[intProfile - 1].Stop;
                    diffString = diff.Milliseconds.ToString();
                }

                var minuteString = elapsedTime.Minutes.ToString();
                var secondString = elapsedTime.Seconds.ToString();
                var millisecondString = elapsedTime.Milliseconds.ToString();
                var startTimeString = (null != profileTime) ? profileTime.Start.ToString(TimeProfile.DateTimeFormat) : "0";
                var stopTimeString = (null != profileTime) ? profileTime.Stop.ToString(TimeProfile.DateTimeFormat) : "0";

                if (ETimingProfiles.TimedTotal == profile)
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
                         profile.ToString(),
                         new string(' ', maxNameLength - profile.ToString().Length),
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