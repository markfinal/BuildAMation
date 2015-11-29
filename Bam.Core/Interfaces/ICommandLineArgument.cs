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
    /// <summary>
    /// Common interface for all command line options.
    /// </summary>
    public interface ICommandLineArgument
    {
        /// <summary>
        /// Retrieve the short (single dash) name of the option. Null if not supported.
        /// </summary>
        /// <value>The short name.</value>
        string ShortName
        {
            get;
        }

        /// <summary>
        /// Retrieve the long (double dash) name of the option. Must be non-null;
        /// </summary>
        /// <value>The long name.</value>
        string LongName
        {
            get;
        }

        /// <summary>
        /// Help text to display for the option.
        /// </summary>
        /// <value>The context help.</value>
        string ContextHelp
        {
            get;
        }
    }

    /// <summary>
    /// Interface to define a default value.
    /// </summary>
    public interface ICommandLineArgumentDefault<T>
    {
        /// <summary>
        /// Obtain the default value, of type T, for the option.
        /// </summary>
        /// <value>The default.</value>
        T Default
        {
            get;
        }
    }

    /// <summary>
    /// Interface to define custom help text, that the regular context help cannot achieve. This is a more dynamic
    /// approach to help text.
    /// </summary>
    public interface ICustomHelpText
    {
        /// <summary>
        /// Obtain the help for the option.
        /// </summary>
        /// <value>The option help.</value>
        string OptionHelp
        {
            get;
        }
    }

    /// <summary>
    /// Interface for an option that is <c>true</c> or <c>false</c>.
    /// </summary>
    public interface IBooleanCommandLineArgument : ICommandLineArgument
    {
    }

    /// <summary>
    /// Interface for an option that has a string value.
    /// </summary>
    public interface IStringCommandLineArgument : ICommandLineArgument
    {
    }

    /// <summary>
    /// Interface that is a regular expression to match a family of command line options.
    /// </summary>
    public interface IRegExCommandLineArgument : ICommandLineArgument, ICustomHelpText
    {
    }

    /// <summary>
    /// Interface for an option that has an integer value, and always defines a default.
    /// </summary>
    public interface IIntegerCommandLineArgument : ICommandLineArgument, ICommandLineArgumentDefault<int>
    {
    }
}
