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
namespace VisualStudioProcessor
{
    public static class ToVisualStudioAttributes
    {
        public static VisualStudioProcessor.ToolAttributeDictionary
        Execute(
            object sender,
            Bam.Core.Target target,
            EVisualStudioTarget vsTarget)
        {
            var optionCollection = sender as Bam.Core.BaseOptionCollection;

            var optionsDictionary = new VisualStudioProcessor.ToolAttributeDictionary();

            // TODO: can I use a var here? especially on Mono
            foreach (System.Collections.Generic.KeyValuePair<string, Bam.Core.Option> optionKeyValue in optionCollection)
            {
                var optionName = optionKeyValue.Key;
                var option = optionKeyValue.Value;

                if (null == option.PrivateData)
                {
                    // state only
                    continue;
                }

                var data = option.PrivateData as IVisualStudioDelegate;
                if (null == data)
                {
                    throw new Bam.Core.Exception("Option data for '{0}', of type '{1}', does not implement the interface '{2}'", optionName, option.PrivateData.GetType().ToString(), typeof(IVisualStudioDelegate).ToString());
                }

                var visualStudioDelegate = data.VisualStudioProjectDelegate;
                if (null != visualStudioDelegate)
                {
                    if (null != visualStudioDelegate.Target)
                    {
                        // Not a requirement, but just a check
                        throw new Bam.Core.Exception("Delegate for '{0}' should be static", optionName);
                    }

                    var dictionary = data.VisualStudioProjectDelegate(optionCollection, option, target, vsTarget);
                    if (null != dictionary)
                    {
                        optionsDictionary.Merge(dictionary);
                    }
                }
            }

            return optionsDictionary;
        }
    }
}
