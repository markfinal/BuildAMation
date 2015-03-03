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
namespace ClangCommon
{
    public sealed class PrivateData :
        CommandLineProcessor.ICommandLineDelegate,
        XcodeProjectProcessor.IXcodeProjectDelegate
    {
        // this constructor is for the non-Xcode Gcc code paths
        public
        PrivateData(
            CommandLineProcessor.Delegate commandLineDelegate)
        {
            this.CommandLineDelegate = commandLineDelegate;
            this.XcodeProjectDelegate = null;
        }

        public
        PrivateData(
            CommandLineProcessor.Delegate commandLineDelegate,
            XcodeProjectProcessor.Delegate xcodeProjectDelegate)
        {
            this.CommandLineDelegate = commandLineDelegate;
            this.XcodeProjectDelegate = xcodeProjectDelegate;
        }

        public CommandLineProcessor.Delegate CommandLineDelegate
        {
            get;
            set;
        }

        public XcodeProjectProcessor.Delegate XcodeProjectDelegate
        {
            get;
            set;
        }
    }
}
