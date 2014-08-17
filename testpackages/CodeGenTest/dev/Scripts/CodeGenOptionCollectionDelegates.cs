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

namespace CodeGenTest
{
    public partial class CodeGenOptionCollection
    {
        #region ICodeGenOptions Option delegates
        private static void
        OutputSourceDirectoryCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            Bam.Core.ReferenceTypeOption<string> stringOption = option as Bam.Core.ReferenceTypeOption<string>;
            commandLineBuilder.Add(stringOption.Value);
        }
        private static void
        OutputNameCommandLineProcessor(
             object sender,
             Bam.Core.StringArray commandLineBuilder,
             Bam.Core.Option option,
             Bam.Core.Target target)
        {
            Bam.Core.ReferenceTypeOption<string> stringOption = option as Bam.Core.ReferenceTypeOption<string>;
            commandLineBuilder.Add(stringOption.Value);
        }
        #endregion
        protected override void
        SetDelegates(
            Bam.Core.DependencyNode node)
        {
            this["OutputSourceDirectory"].PrivateData = new PrivateData(OutputSourceDirectoryCommandLineProcessor);
            this["OutputName"].PrivateData = new PrivateData(OutputNameCommandLineProcessor);
        }
    }
}
