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
            try
            {
                System.DateTime start = System.DateTime.Now;
                Application application = new Application(args);
                application.Run();
                System.DateTime stop = System.DateTime.Now;
                Core.State.TimingProfiles[(int)Core.ETimingProfiles.Total] = stop - start;

                // TODO: only show this on a build action
                Core.TimingProfiles.DumpProfiles();
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
                Core.Log.ErrorMessage("*** Reflection Exception (type {0})***", exception.GetType().ToString());
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
    }
}