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
namespace Bam.Core
{
namespace V2
{
    public interface ICommandLineArgument
    {
        string ShortName
        {
            get;
        }

        string LongName
        {
            get;
        }
    }

    public interface IBooleanCommandLineArgument : ICommandLineArgument
    {
    }

    public interface IStringCommandLineArgument : ICommandLineArgument
    {
    }

    public sealed class BuilderName :
        IStringCommandLineArgument
    {
        string ICommandLineArgument.ShortName
        {
            get
            {
                return "-b";
            }
        }

        string ICommandLineArgument.LongName
        {
            get
            {
                return "--builder";
            }
        }
    }

    public sealed class MultiThreaded :
        IBooleanCommandLineArgument
    {
        string ICommandLineArgument.ShortName
        {
            get
            {
                return "-j";
            }
        }

        string ICommandLineArgument.LongName
        {
            get
            {
                return "--threaded";
            }
        }
    }

    public sealed class UseTests :
        IBooleanCommandLineArgument
    {
        string ICommandLineArgument.ShortName
        {
            get
            {
                return "-t";
            }
        }

        string ICommandLineArgument.LongName
        {
            get
            {
                return "--tests";
            }
        }
    }
}
    public interface IAction :
        System.ICloneable
    {
        string CommandLineSwitch
        {
            get;
        }

        string Description
        {
            get;
        }

        bool
        Execute();
    }
}
