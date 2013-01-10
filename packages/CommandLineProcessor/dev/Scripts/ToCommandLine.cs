// <copyright file="ToCommandLine.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CommandLineProcessor package</summary>
// <author>Mark Final</author>
namespace CommandLineProcessor
{
    public static class ToCommandLine
    {
        public static void Execute(object sender, Opus.Core.StringArray commandLineBuilder, Opus.Core.Target target)
        {
            Opus.Core.BaseOptionCollection optionCollection = sender as Opus.Core.BaseOptionCollection;

            foreach (System.Collections.Generic.KeyValuePair<string, Opus.Core.Option> optionKeyValue in optionCollection)
            {
                string optionName = optionKeyValue.Key;
                Opus.Core.Option option = optionKeyValue.Value;

                if (null != option.PrivateData)
                {
                    ICommandLineDelegate data = option.PrivateData as ICommandLineDelegate;
                    if (null == data)
                    {
                        throw new Opus.Core.Exception("Option data for '{0}', of type '{1}', does not implement the interface '{2}' in '{3}'", optionName, option.PrivateData.GetType().ToString(), typeof(ICommandLineDelegate).ToString(), sender.GetType().ToString());
                    }

                    Delegate commandLineDelegate = data.CommandLineDelegate;
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
                else
                {
                    throw new Opus.Core.Exception("Option '{0}' is not configured for command line translation in '{1}'", optionName, sender.GetType().ToString());
                }
            }
        }
    }
}