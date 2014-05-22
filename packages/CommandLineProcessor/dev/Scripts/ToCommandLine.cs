// <copyright file="ToCommandLine.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CommandLineProcessor package</summary>
// <author>Mark Final</author>
namespace CommandLineProcessor
{
    public static class ToCommandLine
    {
        public static void Execute(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Target target, Opus.Core.StringArray excludedOptionNames)
        {
            var optionCollection = sender as Opus.Core.BaseOptionCollection;
            var optionNames = optionCollection.OptionNames;
            if (null != excludedOptionNames)
            {
                // validate
                var unrecognized = new Opus.Core.StringArray(excludedOptionNames.Complement(optionNames));
                if (unrecognized.Count > 0)
                {
                    throw new Opus.Core.Exception("Unrecognized option names to exclude:\n\t{0}", unrecognized.ToString("\n\t"));
                }

                optionNames = new Opus.Core.StringArray(optionNames.Complement(excludedOptionNames));
            }

            foreach (var optionName in optionNames)
            {
                var option = optionCollection[optionName];
                if (null == option.PrivateData)
                {
                    continue;
                }

                var data = option.PrivateData as ICommandLineDelegate;
                if (null == data)
                {
                    throw new Opus.Core.Exception("Option data for '{0}', of type '{1}', does not implement the interface '{2}' in '{3}'", optionName, option.PrivateData.GetType().ToString(), typeof(ICommandLineDelegate).ToString(), sender.GetType().ToString());
                }

                var commandLineDelegate = data.CommandLineDelegate;
                if (null != commandLineDelegate)
                {
                    if (null != commandLineDelegate.Target)
                    {
                        // Not a requirement, but just a check
                        throw new Opus.Core.Exception("Delegate for '{0}' should be static in '{1}'", optionName, sender.GetType().ToString());
                    }
                    commandLineDelegate(sender, commandLineBuilder, option, target);
                }
            }
        }
    }
}