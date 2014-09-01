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
namespace BamOptionGenerator
{
    class Parameters
    {
        [System.Flags]
        public enum Mode
        {
            GenerateProperties = (1<<0),
            GenerateDelegates  = (1<<1)
        }

        public static System.Collections.Specialized.StringCollection excludedFlagsFromHeaders = new System.Collections.Specialized.StringCollection();

        static
        Parameters()
        {
            excludedFlagsFromHeaders.Add("-f");
            excludedFlagsFromHeaders.Add("-uh");
            excludedFlagsFromHeaders.Add("-ih");
            excludedFlagsFromHeaders.Add("-l");
        }

        public string[] args;
        public string outputPropertiesPathName;
        public string outputDelegatesPathName;
        public string[] inputPathNames;
        public string[] inputDelegates;
        public string outputNamespace;
        public string outputClassName;
        public string privateDataClassName;
        public string licenseFile;
        public Mode mode;
        public bool toStdOut;
        public bool forceWrite;
        public bool extendedDelegates;
        public bool isBaseClass;
        public bool updateHeader;
        public bool ignoreHeaderUpdates;
    }
}
