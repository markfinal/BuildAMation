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
namespace VisualCCommon
{
    public enum EWarningLevel
    {
        Level0 = 0,
        Level1,
        Level2,
        Level3,
        Level4
    }

    public enum EDebugType
    {
        Embedded = 1,
        ProgramDatabase = 3,
        ProgramDatabaseEditAndContinue = 4
    }

    public enum EBrowseInformation
    {
        None = 0,
        Full = 1,
        NoLocalSymbols = 2
    }

    public enum EManagedCompilation
    {
        NoCLR = 0,
        CLR = 1,
        PureCLR = 2,
        SafeCLR = 3,
        OldSyntaxCLR = 4
    }

    public enum EBasicRuntimeChecks
    {
        None = 0,
        StackFrame = 1,
        UninitializedVariables = 2,
        StackFrameAndUninitializedVariables = 3
    }

    public enum EInlineFunctionExpansion
    {
        None = 0,
        OnlyInline = 1,
        AnySuitable = 2
    }
}
