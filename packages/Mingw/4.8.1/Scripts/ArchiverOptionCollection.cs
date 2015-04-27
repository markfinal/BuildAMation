#region License
// Copyright 2010-2015 Mark Final
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
namespace Mingw
{
namespace V2
{
    public interface IArchiverOptions
    {
        bool Ranlib
        {
            get;
            set;
        }
    }

    public class LibrarianSettings :
        Bam.Core.V2.Settings,
        C.V2.IArchiverOptions,
        IArchiverOptions
    {
        public LibrarianSettings()
        {
            this.Ranlib = true;
        }

        public bool Ranlib
        {
            get;
            set;
        }
    }

    public sealed class Librarian :
        C.V2.LibrarianTool
    {
        public Librarian()
        {
            this.InheritedEnvironmentVariables.Add("TEMP");
        }

        public override string Executable
        {
            get
            {
                return @"C:\MinGW\bin\ar.exe";
            }
        }

        public override Bam.Core.V2.Settings CreateDefaultSettings<T>(T module)
        {
            var settings = new LibrarianSettings();
            return settings;
        }
    }
}
    public class ArchiverOptionCollection :
        MingwCommon.ArchiverOptionCollection
    {
        public
        ArchiverOptionCollection(
            Bam.Core.DependencyNode node) : base(node)
        {}
    }
}
