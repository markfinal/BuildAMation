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
#endregion // License
namespace CommandLineProcessor
{
    public static class ToCommandLine
    {
        public static void
        ExecuteForOptionNames(
            Bam.Core.BaseOptionCollection optionCollection,
            Bam.Core.StringArray commandLineBuilder,
            Bam.Core.Target target,
            Bam.Core.StringArray optionNames)
        {
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
                    throw new Bam.Core.Exception("Option data for '{0}', of type '{1}', does not implement the interface '{2}' in '{3}'", optionName, option.PrivateData.GetType().ToString(), typeof(ICommandLineDelegate).ToString(), optionCollection.GetType().ToString());
                }

                var commandLineDelegate = data.CommandLineDelegate;
                if (null != commandLineDelegate)
                {
                    if (null != commandLineDelegate.Target)
                    {
                        // Not a requirement, but just a check
                        throw new Bam.Core.Exception("Delegate for '{0}' should be static in '{1}'", optionName, optionCollection.GetType().ToString());
                    }
                    commandLineDelegate(optionCollection, commandLineBuilder, option, target);
                }
            }
        }

        public static void
        Execute(
            object sender,
            Bam.Core.StringArray commandLineBuilder,
            Bam.Core.Target target,
            Bam.Core.StringArray excludedOptionNames)
        {
            var optionCollection = sender as Bam.Core.BaseOptionCollection;
            var optionNames = optionCollection.OptionNames;
            if (null != excludedOptionNames)
            {
                // validate
                var unrecognized = new Bam.Core.StringArray(excludedOptionNames.Complement(optionNames));
                if (unrecognized.Count > 0)
                {
                    throw new Bam.Core.Exception("Unrecognized option names to exclude:\n\t{0}", unrecognized.ToString("\n\t"));
                }

                optionNames = new Bam.Core.StringArray(optionNames.Complement(excludedOptionNames));
            }

            ExecuteForOptionNames(optionCollection, commandLineBuilder, target, optionNames);
        }
    }
}
