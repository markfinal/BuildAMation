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

namespace QtCommon
{
    public partial class MocOptionCollection
    {
        #region IMocOptions Option delegates
        private static void
        IncludePathsCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var directoryCollectionOption = option as Bam.Core.ReferenceTypeOption<Bam.Core.DirectoryCollection>;
            // TODO: convert to var
            foreach (string directory in directoryCollectionOption.Value)
            {
                if (directory.Contains(" "))
                {
                    commandLineBuilder.Add(System.String.Format("-I\"{0}\"", directory));
                }
                else
                {
                    commandLineBuilder.Add(System.String.Format("-I{0}", directory));
                }
            }
        }
        private static void
        DefinesCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var definesCollectionOption = option as Bam.Core.ReferenceTypeOption<C.DefineCollection>;
            foreach (var directory in definesCollectionOption.Value)
            {
                commandLineBuilder.Add(System.String.Format("-D{0}", directory));
            }
        }
        private static void
        DoNotGenerateIncludeStatementCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var boolOption = option as Bam.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-i");
            }
        }
        private static void
        DoNotDisplayWarningsCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var boolOption = option as Bam.Core.ValueTypeOption<bool>;
            if (boolOption.Value)
            {
                commandLineBuilder.Add("-nw");
            }
        }
        private static void
        PathPrefixCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            var stringOption = option as Bam.Core.ReferenceTypeOption<string>;
            if (stringOption.Value != null)
            {
                commandLineBuilder.Add(System.String.Format("-p {0}", stringOption.Value));
            }
        }
        #endregion
        protected override void
        SetDelegates(
            Bam.Core.DependencyNode node)
        {
            this["IncludePaths"].PrivateData = new MocPrivateData(IncludePathsCommandLineProcessor);
            this["Defines"].PrivateData = new MocPrivateData(DefinesCommandLineProcessor);
            this["DoNotGenerateIncludeStatement"].PrivateData = new MocPrivateData(DoNotGenerateIncludeStatementCommandLineProcessor);
            this["DoNotDisplayWarnings"].PrivateData = new MocPrivateData(DoNotDisplayWarningsCommandLineProcessor);
            this["PathPrefix"].PrivateData = new MocPrivateData(PathPrefixCommandLineProcessor);
        }
    }
}
