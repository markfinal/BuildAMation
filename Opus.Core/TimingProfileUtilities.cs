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
            bool additionalDetails = false;

            string profileHeader = "Profile";
            string minutesHeader = "Minutes";
            string secondsHeader = "Seconds";
            string millisecondsHeader = "Milliseconds";
            string startTimeHeader = "Start";
            string stopTimeHeader = "Stop";

            System.Array profiles = System.Enum.GetValues(typeof(ETimingProfiles));
            int maxNameLength = profileHeader.Length;
            int maxMinuteLength = minutesHeader.Length;
            int maxSecondLength = secondsHeader.Length;
            int maxMillisecondLength = millisecondsHeader.Length;
            int maxTimeLength = 9;
            foreach (ETimingProfiles profile in profiles)
            {
                int nameLength = profile.ToString().Length;
                if (nameLength > maxNameLength)
                {
                    maxNameLength = nameLength;
                }

                TimeProfile profileTime = State.TimingProfiles[(int)profile];

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

            string header =
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
            string horizontalRule = new string('-', header.Length);
            Core.Log.Info(horizontalRule);
            Core.Log.Info(header);
            Core.Log.Info(horizontalRule);
            System.TimeSpan cumulativeTime = new System.TimeSpan();
            foreach (ETimingProfiles profile in profiles)
            {
                int intProfile = (int)profile;
                TimeProfile profileTime = State.TimingProfiles[intProfile];
                System.TimeSpan elapsedTime = profileTime.Elapsed;
                if (ETimingProfiles.TimedTotal != profile)
                {
                    cumulativeTime = cumulativeTime.Add(elapsedTime);
                }

                string diffString = null;
                if (additionalDetails && (intProfile > 0) && (ETimingProfiles.TimedTotal != profile))
                {
                    System.TimeSpan diff = profileTime.Start - State.TimingProfiles[intProfile - 1].Stop;
                    diffString = diff.Milliseconds.ToString();
                }

                string minuteString = elapsedTime.Minutes.ToString();
                string secondString = elapsedTime.Seconds.ToString();
                string millisecondString = elapsedTime.Milliseconds.ToString();
                string startTimeString = profileTime.Start.ToString(TimeProfile.DateTimeFormat);
                string stopTimeString = profileTime.Stop.ToString(TimeProfile.DateTimeFormat);

                if (ETimingProfiles.TimedTotal == profile)
                {
                    Log.Info(horizontalRule);
                    string cumulativeString = "CumulativeTotal";
                    string cumulativeMinutesString = cumulativeTime.Minutes.ToString();
                    string cumulativeSecondsString = cumulativeTime.Seconds.ToString();
                    string cumulativeMillisecondsString = cumulativeTime.Milliseconds.ToString();

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