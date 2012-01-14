// <copyright file="ProcessCompilerFlags.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder
    {
        private void ProcessCompilerFlags(Opus.Core.StringArray compilerFlags,
                                          System.Text.StringBuilder compilerStatement,
                                          System.Text.StringBuilder includesStatement,
                                          System.Text.StringBuilder definesStatement)
        {
            foreach (string cflag in compilerFlags)
            {
                if (cflag.StartsWith("-o") || cflag.StartsWith("/Fo"))
                {
                    // don't include any output path
                    continue;
                }

                string cflagModified = cflag;
                if (cflag.Contains("\""))
                {
                    int indexOfFirstQuote = cflag.IndexOf('"');
                    cflagModified = cflag.Substring(0, indexOfFirstQuote);
                    cflagModified += "$$quote(";
                    int indexOfLastQuote = cflag.IndexOf('"', indexOfFirstQuote + 1);
                    cflagModified += cflag.Substring(indexOfFirstQuote + 1, indexOfLastQuote - indexOfFirstQuote - 1);
                    cflagModified += ")";
                    cflagModified += cflag.Substring(indexOfLastQuote + 1);
                }

                if (cflagModified.StartsWith("-I") || cflagModified.StartsWith("-isystem") || cflagModified.StartsWith("/I"))
                {
                    // strip the include path command
                    if (cflagModified.StartsWith("-isystem"))
                    {
                        cflagModified = cflagModified.Remove(0, 8);
                    }
                    else
                    {
                        cflagModified = cflagModified.Remove(0, 2);
                    }
                    includesStatement.AppendFormat("\\\n\t{0}", cflagModified.Replace('\\', '/'));
                }
                else if (cflagModified.StartsWith("-D") || cflagModified.StartsWith("/D"))
                {
                    // strip the define command
                    cflagModified = cflagModified.Remove(0, 2);
                    definesStatement.AppendFormat("\\\n\t{0}", cflagModified.Replace('\\', '/'));
                }
                else
                {
                    compilerStatement.AppendFormat("\\\n\t{0}", cflagModified.Replace('\\', '/'));
                }
            }
        }
    }
}