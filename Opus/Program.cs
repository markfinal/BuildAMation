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

                System.TimeSpan elapsedTime = stop - start;
                Core.Log.Info("\nElapsed time: {0} minutes {1} seconds {2} milliseconds",
                              elapsedTime.Minutes,
                              elapsedTime.Seconds,
                              elapsedTime.Milliseconds);
            }
            catch (Core.Exception exception)
            {
                Core.Log.ErrorMessage("Opus Exception: " + exception.Message);
                if (exception.InnerException != null)
                {
                    Core.Log.ErrorMessage("Inner exception: {0}, {1}", exception.InnerException.GetType().ToString(), exception.InnerException.Message);
                    if (exception.RequiresStackTrace)
                    {
                        Core.Log.ErrorMessage("\n" + exception.InnerException.StackTrace.ToString());
                    }
                }
                if (exception.RequiresStackTrace)
                {
                    Core.Log.ErrorMessage("\n" + exception.StackTrace.ToString());
                }
                System.Environment.ExitCode = -1;
            }
            catch (System.Reflection.TargetInvocationException exception)
            {
                Core.Log.ErrorMessage("*** Reflection Exception (type {0}) ***", exception.GetType().ToString());
                if (exception.InnerException != null)
                {
                    Core.Log.ErrorMessage("Inner exception: {0}, {1}", exception.InnerException.GetType().ToString(), exception.InnerException.Message);
                    Core.Log.ErrorMessage("\n" + exception.InnerException.StackTrace.ToString());
                }
                else
                {
                    Core.Log.ErrorMessage(exception.Message);
                    Core.Log.ErrorMessage("\n" + exception.StackTrace.ToString());
                }
                System.Environment.ExitCode = -2;
            }
            catch (System.Exception exception)
            {
                Core.Log.ErrorMessage("*** System Exception (type {0}) ***", exception.GetType().ToString());
                Core.Log.ErrorMessage(exception.Message);
                if (exception.InnerException != null)
                {
                    Core.Log.ErrorMessage("Inner exception: {0}, {1}", exception.InnerException.GetType().ToString(), exception.InnerException.Message);
                    if (exception.InnerException is Core.Exception && (exception.InnerException as Core.Exception).RequiresStackTrace)
                    {
                        Core.Log.ErrorMessage("\n" + exception.InnerException.StackTrace.ToString());
                    }
                }
                Core.Log.ErrorMessage("\n" + exception.StackTrace.ToString());
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