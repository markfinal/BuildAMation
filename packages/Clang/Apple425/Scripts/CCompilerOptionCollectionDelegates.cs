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

namespace Clang
{
    public partial class CCompilerOptionCollection
    {
        #region C.ICCompilerOptionsOSX Option delegates
        private static void
        FrameworkSearchDirectoriesCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var switchPrefix = "-F";
            var frameworkIncludePathsOption = option as Bam.Core.ReferenceTypeOption<Bam.Core.DirectoryCollection>;
            // TODO: convert to 'var'
            foreach (string includePath in frameworkIncludePathsOption.Value)
            {
                if (includePath.Contains(" "))
                {
                    commandLineBuilder.Add(System.String.Format("{0}\"{1}\"", switchPrefix, includePath));
                }
                else
                {
                    commandLineBuilder.Add(System.String.Format("{0}{1}", switchPrefix, includePath));
                }
            }
        }
        private static void
        FrameworkSearchDirectoriesXcodeProjectProcessor(
             object sender,
             XcodeBuilder.PBXProject project,
             XcodeBuilder.XcodeNodeData currentObject,
             XcodeBuilder.XCBuildConfiguration configuration,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var frameworkPathsOption = option as Bam.Core.ReferenceTypeOption<Bam.Core.DirectoryCollection>;
            configuration.Options["FRAMEWORK_SEARCH_PATHS"].AddRangeUnique(frameworkPathsOption.Value.ToStringArray());
        }
        #endregion
        protected override void
        SetDelegates(
            Bam.Core.DependencyNode node)
        {
            base.SetDelegates(node);
            this["FrameworkSearchDirectories"].PrivateData = new ClangCommon.PrivateData(FrameworkSearchDirectoriesCommandLineProcessor,FrameworkSearchDirectoriesXcodeProjectProcessor);
        }
    }
}
