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
namespace MakeFileBuilder
{
    public sealed class MakeFileRule
    {
        public
        MakeFileRule(
            Bam.Core.BaseModule moduleToBuild,
            Bam.Core.LocationKey primaryOutputLocationKey,
            string target,
            Bam.Core.LocationArray directoriesToCreate,
            MakeFileVariableDictionary inputVariables,
            Bam.Core.StringArray inputFiles,
            Bam.Core.StringArray recipes)
        {
            this.ModuleToBuild = moduleToBuild;
            this.PrimaryOutputLocationKey = primaryOutputLocationKey;
            this.Target = target;
            this.DirectoriesToCreate = directoriesToCreate;
            this.InputVariables = inputVariables;
            this.InputFiles = inputFiles;
            this.Recipes = recipes;
            this.ExportTarget = true;
            this.ExportVariable = true;
            this.TargetIsPhony = false;
        }

        public Bam.Core.BaseModule ModuleToBuild
        {
            get;
            private set;
        }

        public Bam.Core.LocationKey PrimaryOutputLocationKey
        {
            get;
            private set;
        }

        public string Target
        {
            get;
            private set;
        }

        public Bam.Core.LocationArray DirectoriesToCreate
        {
            get;
            private set;
        }

        public Bam.Core.StringArray InputFiles
        {
            get;
            private set;
        }

        public MakeFileVariableDictionary InputVariables
        {
            get;
            private set;
        }

        public Bam.Core.StringArray Recipes
        {
            get;
            private set;
        }

        public bool ExportVariable
        {
            get;
            set;
        }

        public bool ExportTarget
        {
            get;
            set;
        }

        public bool TargetIsPhony
        {
            get;
            set;
        }

        public Bam.Core.Array<Bam.Core.LocationKey> OutputLocationKeys
        {
            get;
            set;
        }
    }
}
