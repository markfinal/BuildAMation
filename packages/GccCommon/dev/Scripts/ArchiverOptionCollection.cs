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
namespace GccCommon
{
    public partial class ArchiverOptionCollection :
        C.ArchiverOptionCollection,
        C.IArchiverOptions,
        IArchiverOptions
    {
        protected override void
        SetDefaultOptionValues(
            Bam.Core.DependencyNode node)
        {
            base.SetDefaultOptionValues(node);

            var localArchiverOptions = this as IArchiverOptions;
            localArchiverOptions.Command = EArchiverCommand.Replace;
            localArchiverOptions.DoNotWarnIfLibraryCreated = true;

            var cArchiverOptions = this as C.IArchiverOptions;
            // this must be set last, as it appears last on the command line
            cArchiverOptions.OutputType = C.EArchiverOutput.StaticLibrary;
        }

        public
        ArchiverOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}
    }
}
