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
namespace MakeFileBuilder
{
    public sealed class MakeFileData
    {
        public
        MakeFileData(
            string makeFilePath,
            MakeFileTargetDictionary targetDictionary,
            MakeFileVariableDictionary variableDictionary,
            System.Collections.Generic.Dictionary<string, Bam.Core.StringArray> environment)
        {
            this.MakeFilePath = makeFilePath;
            this.TargetDictionary = targetDictionary;
            this.VariableDictionary = variableDictionary;
            if (null != environment)
            {
                // TODO: better way to do a copy?
                this.Environment = new System.Collections.Generic.Dictionary<string, Bam.Core.StringArray>();
                foreach (var key in environment.Keys)
                {
                    this.Environment[key] = environment[key];
                }
            }
            else
            {
                this.Environment = null;
            }
        }

        public string MakeFilePath
        {
            get;
            private set;
        }

        public MakeFileTargetDictionary TargetDictionary
        {
            get;
            private set;
        }

        public MakeFileVariableDictionary VariableDictionary
        {
            get;
            private set;
        }

        public System.Collections.Generic.Dictionary<string, Bam.Core.StringArray> Environment
        {
            get;
            private set;
        }
    }
}
