#region License
// Copyright (c) 2010-2017, Mark Final
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
namespace Bam.Core
{
    /// <summary>
    /// Utility class for evaluating command line options, implementing the ICommandLineArgument
    /// interface.
    /// </summary>
    public static class CommandLineProcessor
    {
        private static StringArray Arguments;

        static CommandLineProcessor()
        {
            Arguments = new StringArray(System.Environment.GetCommandLineArgs());
        }

        private static bool
        UsesName(
            ICommandLineArgument arg,
            string[] splitArgs)
        {
            var uses = ((splitArgs[0].StartsWith("--") && splitArgs[0].EndsWith(arg.LongName)) ||
                ((arg.ShortName != null) && (splitArgs[0].StartsWith("-") && splitArgs[0].EndsWith(arg.ShortName))));
            return uses;
        }

        /// <summary>
        /// Evaluate an option instance implementing the IBooleanCommandLineArgument interface.
        /// </summary>
        /// <param name="realArg">Instance of the option class.</param>
        /// <returns>The boolean value from the option.</returns>
        public static bool
        Evaluate(
            IBooleanCommandLineArgument realArg)
        {
            foreach (var arg in Arguments)
            {
                var splitArg = arg.Split('=');
                if (splitArg.Length != 1)
                {
                    continue;
                }

                if (UsesName(realArg, splitArg))
                {
                    return true;
                }
            }
            return (realArg is ICommandLineArgumentDefault<bool>) ? (realArg as ICommandLineArgumentDefault<bool>).Default : false;
        }

        /// <summary>
        /// Evaluate an option instance implementing the IStringCommandLineArgument interface.
        /// </summary>
        /// <param name="realArg">Instance of the option class.</param>
        /// <returns>The string value from the option.</returns>
        public static string
        Evaluate(
            IStringCommandLineArgument realArg)
        {
            foreach (var arg in Arguments)
            {
                var splitArg = arg.Split('=');
                if (splitArg.Length != 2)
                {
                    continue;
                }

                if (UsesName(realArg, splitArg))
                {
                    return splitArg[1];
                }
            }
            return (realArg is ICommandLineArgumentDefault<string>) ? (realArg as ICommandLineArgumentDefault<string>).Default : null;
        }

        /// <summary>
        /// Evaluate an option instance implementing the IIntegerCommandLineArgument interface.
        /// </summary>
        /// <param name="realArg">Instance of the option class.</param>
        /// <returns>The integer value from the option. An exception is thrown if the option value does not parse as an integer.</returns>
        public static int
        Evaluate(
            IIntegerCommandLineArgument realArg)
        {
            foreach (var arg in Arguments)
            {
                var splitArg = arg.Split('=');
                if (splitArg.Length != 2)
                {
                    continue;
                }

                if (UsesName(realArg, splitArg))
                {
                    try
                    {
                        return System.Convert.ToInt32(splitArg[1]);
                    }
                    catch (System.FormatException exception)
                    {
                        throw new Exception(exception, "Unable to parse value, '{0}', as an integer", splitArg[1]);
                    }
                }
            }
            return realArg.Default;
        }

        /// <summary>
        /// Evaluate an option instance implementing the IRegExCommandLineArgument interface.
        /// </summary>
        /// <param name="realArg">Instance of the option class.</param>
        /// <returns>The string array from the option, an element per match of the regular expression.</returns>
        public static Array<StringArray>
        Evaluate(
            IRegExCommandLineArgument realArg)
        {
            if (null != realArg.ShortName)
            {
                throw new Exception("The command line argument '{0}' does not support short names", realArg.GetType().ToString());
            }
            var reg = new System.Text.RegularExpressions.Regex(realArg.LongName);
            var results = new Array<StringArray>();
            foreach (var arg in Arguments)
            {
                var matches = reg.Match(arg);
                if (!matches.Success)
                {
                    continue;
                }

                var thisResult = new StringArray();
                foreach (var group in matches.Groups)
                {
                    if (group.ToString() == arg)
                    {
                        continue;
                    }
                    thisResult.Add(group.ToString());
                }
                results.Add(thisResult);
            }
            return results;
        }
    }
}
