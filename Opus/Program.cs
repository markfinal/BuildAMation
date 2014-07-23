// <copyright file="Program.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>
namespace Opus
{
    /// <summary>
    /// Opus application.
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