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
namespace XcodeProjectProcessor
{
    public static class ToXcodeProject
    {
        public static void
        Execute(
            object sender,
            XcodeBuilder.PBXProject project,
            XcodeBuilder.XcodeNodeData currentObject,
            XcodeBuilder.XCBuildConfiguration configuration,
            Bam.Core.Target target)
        {
            var optionCollection = sender as Bam.Core.BaseOptionCollection;
            var optionNames = optionCollection.OptionNames;

            foreach (var optionName in optionNames)
            {
                var option = optionCollection[optionName];
                if (null == option.PrivateData)
                {
                    continue;
                }

                var data = option.PrivateData as IXcodeProjectDelegate;
                if (null == data)
                {
                    throw new Bam.Core.Exception("Option data for '{0}', of type '{1}', does not implement the interface '{2}' in '{3}'", optionName, option.PrivateData.GetType().ToString(), typeof(IXcodeProjectDelegate).ToString(), sender.GetType().ToString());
                }

                var xcodeProjectDelegate = data.XcodeProjectDelegate;
                if (null != xcodeProjectDelegate)
                {
                    if (null != xcodeProjectDelegate.Target)
                    {
                        // Not a requirement, but just a check
                        throw new Bam.Core.Exception("Delegate for '{0}' should be static in '{1}'", optionName, sender.GetType().ToString());
                    }
                    xcodeProjectDelegate(sender, project, currentObject, configuration, option, target);
                }
            }
        }
    }
}
