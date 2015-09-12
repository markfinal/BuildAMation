#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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

        string ContextHelp
        {
            get;
        }
    }

    public interface ICommandLineArgumentDefault<T>
    {
        T Default
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

    public interface IRegExCommandLineArgument : ICommandLineArgument, ICustomHelpText
    {
    }

    public interface IIntegerCommandLineArgument : ICommandLineArgument, ICommandLineArgumentDefault<int>
    {
    }

    public interface ICustomHelpText
    {
        string OptionHelp
        {
            get;
        }
    }

    public sealed class BuildMode :
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
                return "--buildmode";
            }
        }

        string ICommandLineArgument.ContextHelp
        {
            get
            {
                return "Specify the build mode to use";
            }
        }
    }

    public sealed class MultiThreaded :
        IIntegerCommandLineArgument
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

        string ICommandLineArgument.ContextHelp
        {
            get
            {
                return "Define the number of thread to use to build (zero means to use the processor count)";
            }
        }

        int ICommandLineArgumentDefault<int>.Default
        {
            get
            {
                return 1; // single threaded by default
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

        string ICommandLineArgument.ContextHelp
        {
            get
            {
                return "Include modules in the tests nested namespace of the top level module";
            }
        }
    }

    public sealed class CreateDebugProject :
        IBooleanCommandLineArgument
    {
        string ICommandLineArgument.ShortName
        {
            get
            {
                return null;
            }
        }

        string ICommandLineArgument.LongName
        {
            get
            {
                return "--createdebugproject";
            }
        }

        string ICommandLineArgument.ContextHelp
        {
            get
            {
                return "Create a standalone VisualStudio project to debug a Bam package build";
            }
        }
    }

    public sealed class PrintVersion :
        IBooleanCommandLineArgument
    {
        string ICommandLineArgument.ShortName
        {
            get
            {
                return null;
            }
        }

        string ICommandLineArgument.LongName
        {
            get
            {
                return "--version";
            }
        }

        string ICommandLineArgument.ContextHelp
        {
            get
            {
                return "Print the version of Bam!";
            }
        }
    }

    public sealed class PrintHelp :
        IBooleanCommandLineArgument
    {
        string ICommandLineArgument.ShortName
        {
            get
            {
                return "-h";
            }
        }

        string ICommandLineArgument.LongName
        {
            get
            {
                return "--help";
            }
        }

        string ICommandLineArgument.ContextHelp
        {
            get
            {
                return "Print this help";
            }
        }
    }

    public sealed class PackageDefaultVersion :
        IRegExCommandLineArgument
    {
        string ICommandLineArgument.ShortName
        {
            get
            {
                return null;
            }
        }

        string ICommandLineArgument.LongName
        {
            get
            {
                return @"--([A-Za-z0-9]+)\.version=([A-Za-z0-9\.]+)";
            }
        }

        string ICommandLineArgument.ContextHelp
        {
            get
            {
                return "Define the default version of a package";
            }
        }

        string ICustomHelpText.OptionHelp
        {
            get
            {
                return "--<packagename>.version=<packageversion>";
            }
        }
    }

    public sealed class CleanFirst :
        IBooleanCommandLineArgument
    {
        string ICommandLineArgument.ShortName
        {
            get
            {
                return "-c";
            }
        }

        string ICommandLineArgument.LongName
        {
            get
            {
                return "--clean";
            }
        }

        string ICommandLineArgument.ContextHelp
        {
            get
            {
                return "Delete the build root directory before building";
            }
        }
    }

    public sealed class VerbosityLevel :
        IIntegerCommandLineArgument
    {
        int ICommandLineArgumentDefault<int>.Default
        {
            get
            {
                return (int)EVerboseLevel.Detail;
            }
        }

        string ICommandLineArgument.ShortName
        {
            get
            {
                return "-v";
            }
        }

        string ICommandLineArgument.LongName
        {
            get
            {
                return "--verbosity";
            }
        }

        string ICommandLineArgument.ContextHelp
        {
            get
            {
                return "Change the logging level of detail (0 for least, 3 for maximum).";
            }
        }
    }

    public sealed class BuildRoot :
        IStringCommandLineArgument,
        ICommandLineArgumentDefault<string>
    {
        string ICommandLineArgument.ShortName
        {
            get
            {
                return "-o";
            }
        }

        string ICommandLineArgument.LongName
        {
            get
            {
                return "--output";
            }
        }

        string ICommandLineArgument.ContextHelp
        {
            get
            {
                return "Set the output directory (build root) for all build artifacts.";
            }
        }

        string ICommandLineArgumentDefault<string>.Default
        {
            get
            {
                return "build";
            }
        }
    }

    public sealed class UseDebugSymbols :
        IBooleanCommandLineArgument
    {
        string ICommandLineArgument.ShortName
        {
            get
            {
                return "-d";
            }
        }

        string ICommandLineArgument.LongName
        {
            get
            {
                return "--debug";
            }
        }

        string ICommandLineArgument.ContextHelp
        {
            get
            {
                return "Enable debug symbols for the compiled package assembly (may help with exception stack traces).";
            }
        }
    }

    public sealed class BuildConfigurations :
        IRegExCommandLineArgument
    {
        string ICommandLineArgument.ShortName
        {
            get
            {
                return null;
            }
        }

        string ICommandLineArgument.LongName
        {
            get
            {
                return @"--config=([a-z]+)";
            }
        }

        string ICommandLineArgument.ContextHelp
        {
            get
            {
                return "Specify each configuration to build (can be specified multiple times).";
            }
        }

        string ICustomHelpText.OptionHelp
        {
            get
            {
                return "--config=<configuration>";
            }
        }
    }

    public sealed class ExplainBuildReason :
        IBooleanCommandLineArgument
    {
        string ICommandLineArgument.ShortName
        {
            get
            {
                return null;
            }
        }

        string ICommandLineArgument.LongName
        {
            get
            {
                return "--explain";
            }
        }

        string ICommandLineArgument.ContextHelp
        {
            get
            {
                return "For build modes that evaluate, explain why modules are building";
            }
        }
    }

    public sealed class MakePackage :
        IBooleanCommandLineArgument
    {
        string ICommandLineArgument.ShortName
        {
            get
            {
                return null;
            }
        }

        string ICommandLineArgument.LongName
        {
            get
            {
                return "--makepackage";
            }
        }

        string ICommandLineArgument.ContextHelp
        {
            get
            {
                return "Make the current working directory into a Bam package.";
            }
        }
    }

    public sealed class PackageName :
        IStringCommandLineArgument
    {
        string ICommandLineArgument.ShortName
        {
            get
            {
                return null;
            }
        }

        string ICommandLineArgument.LongName
        {
            get
            {
                return "--name";
            }
        }

        string ICommandLineArgument.ContextHelp
        {
            get
            {
                return "Define the package name to use in other operations";
            }
        }
    }

    public sealed class PackageVersion :
        IStringCommandLineArgument
    {
        string ICommandLineArgument.ShortName
        {
            get
            {
                return null;
            }
        }

        string ICommandLineArgument.LongName
        {
            get
            {
                return "--version";
            }
        }

        string ICommandLineArgument.ContextHelp
        {
            get
            {
                return "Define the package version to use in other operations";
            }
        }
    }

    public sealed class ForceDefinitionFileUpdate :
        IBooleanCommandLineArgument
    {
        string ICommandLineArgument.ShortName
        {
            get
            {
                return null;
            }
        }

        string ICommandLineArgument.LongName
        {
            get
            {
                return "--forceupdates";
            }
        }

        string ICommandLineArgument.ContextHelp
        {
            get
            {
                return "Force an update of all package definition files read";
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
