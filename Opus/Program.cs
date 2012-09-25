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
        public static void Main(string[] args)
        {
            // take control of Ctrl+C
            System.Console.CancelKeyPress += new System.ConsoleCancelEventHandler(HandleCancellation);

            try
            {
                Core.TimeProfile profile = new Core.TimeProfile(Core.ETimingProfiles.TimedTotal);
                profile.StartProfile();

                Application application = new Application(args);
                application.Run();

                profile.StopProfile();

                if (Core.State.ShowTimingStatistics)
                {
                    Core.TimingProfileUtilities.DumpProfiles();
                }
            }
            catch (Core.Exception exception)
            {
                Core.Log.ErrorMessage("Opus Exception: " + exception.Message);
                System.Exception innerException = exception;
                while (innerException.InnerException != null)
                {
                    innerException = innerException.InnerException;
                    Core.Log.ErrorMessage("Inner exception: {0}, {1}", innerException.GetType().ToString(), innerException.Message);
                }
                if (exception.RequiresStackTrace)
                {
                    Core.Log.ErrorMessage("\n" + innerException.StackTrace.ToString());
                }
                System.Environment.ExitCode = -1;
            }
            catch (System.Reflection.TargetInvocationException exception)
            {
                Core.Log.ErrorMessage("*** Reflection Exception (type {0}) ***", exception.GetType().ToString());
                Core.Log.ErrorMessage(exception.Message);
                System.Exception innerException = exception;
                while (innerException.InnerException != null)
                {
                    innerException = innerException.InnerException;
                    Core.Log.ErrorMessage("Inner exception: {0}, {1}", innerException.GetType().ToString(), innerException.Message);
                }
                Core.Log.ErrorMessage("\n" + innerException.StackTrace.ToString());
                System.Environment.ExitCode = -2;
            }
            catch (System.Exception exception)
            {
                Core.Log.ErrorMessage("*** System Exception (type {0}) ***", exception.GetType().ToString());
                Core.Log.ErrorMessage(exception.Message);
                System.Exception innerException = exception;
                while (innerException.InnerException != null)
                {
                    innerException = innerException.InnerException;
                    Core.Log.ErrorMessage("Inner exception: {0}, {1}", innerException.GetType().ToString(), innerException.Message);
                }
                Core.Log.ErrorMessage("\n" + innerException.StackTrace.ToString());
                System.Environment.ExitCode = -2;
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

        static void HandleCancellation(object sender, System.ConsoleCancelEventArgs e)
        {
            // allow the build to fail gracefully
            Core.BuildManager buildManager = Core.State.BuildManager;
            if (null != buildManager)
            {
                buildManager.Cancel();
                e.Cancel = true;
            }
        }
    }
}