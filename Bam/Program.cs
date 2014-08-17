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
#endregion
namespace Bam
{
    /// <summary>
    /// Command line tool main entry point
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Application entry point.
        /// </summary>
        /// <param name="args">Argument array.</param>
        public static void
        Main(
            string[] args)
        {
            // take control of Ctrl+C
            System.Console.CancelKeyPress += new System.ConsoleCancelEventHandler(HandleCancellation);

            try
            {
                var profile = new Core.TimeProfile(Core.ETimingProfiles.TimedTotal);
                profile.StartProfile();

                var application = new Application(args);
                application.Run();

                profile.StopProfile();

                if (Core.State.ShowTimingStatistics)
                {
                    Core.TimingProfileUtilities.DumpProfiles();
                }
            }
            catch (Core.Exception exception)
            {
                Core.Exception.DisplayException(exception);
                System.Environment.ExitCode = -1;
            }
            catch (System.Reflection.TargetInvocationException exception)
            {
                Core.Exception.DisplayException(exception);
                System.Environment.ExitCode = -2;
            }
            catch (System.Exception exception)
            {
                Core.Exception.DisplayException(exception);
                System.Environment.ExitCode = -3;
            }
            finally
            {
                if (0 == System.Environment.ExitCode)
                {
                    Core.Log.Info("\nSucceeded");
                }
                else
                {
                    Core.Log.Info("\nFailed");
                }
                Core.Log.DebugMessage("Exit code is {0}", System.Environment.ExitCode);
            }
        }

        private static void
        HandleCancellation(
            object sender,
            System.ConsoleCancelEventArgs e)
        {
            // allow the build to fail gracefully
            var buildManager = Core.State.BuildManager;
            if (null != buildManager)
            {
                buildManager.Cancel();
                e.Cancel = true;
            }
        }
    }
}