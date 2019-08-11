#region License
// Copyright (c) 2010-2019, Mark Final
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
namespace VisualCCommon
{
    /// <summary>
    /// Warning levels for the compiler
    /// </summary>
    public enum EWarningLevel
    {
        Level0 = 0, //<! Warning level 0
        Level1,     //<! Warning level 1
        Level2,     //<! Warning level 2
        Level3,     //<! Warning level 3
        Level4      //<! Warning level 4
    }

    /// <summary>
    /// Warning levels for the assembler
    /// </summary>
    public enum EAssemblerWarningLevel
    {
        Level0 = 0, //<! Warning level 0
        Level1,     //<! Warning level 1
        Level2,     //<! Warning level 2
        Level3      //<! Warning level 3
    }

    /// <summary>
    /// Debug types
    /// </summary>
    public enum EDebugType
    {
        Embedded = 1,                       //<! Embed symbols into object files
        ProgramDatabase = 3,                //<! Symbols into pdbs
        ProgramDatabaseEditAndContinue = 4  //<! Symbols into pdbs and allow edit and continue
    }

    /// <summary>
    /// Browse information types
    /// </summary>
    public enum EBrowseInformation
    {
        None = 0,           //<! No browse information
        Full = 1,           //<! Full browse information
        NoLocalSymbols = 2  //<! Browse information except for local symbols
    }

    /// <summary>
    /// Managed compilation types
    /// </summary>
    public enum EManagedCompilation
    {
        NoCLR = 0,          //<! No CLR
        CLR = 1,            //<! CLR
        PureCLR = 2,        //<! Pure CLR
        SafeCLR = 3,        //<! Safe CLR
        OldSyntaxCLR = 4    //<! Old syntax CLR
    }

    /// <summary>
    /// Basic runtime checks types
    /// </summary>
    public enum EBasicRuntimeChecks
    {
        None = 0,                               //<! No checks
        StackFrame = 1,                         //<! Check for stack frames
        UninitializedVariables = 2,             //<! Check for uninitialised variables
        StackFrameAndUninitializedVariables = 3 //<! Check stack frame and uninitialised variables
    }

    /// <summary>
    /// Inline function expansion types
    /// </summary>
    public enum EInlineFunctionExpansion
    {
        None = 0,       //<! No inline function expansion
        OnlyInline = 1, //<! Only those functions marked inline are inlined
        AnySuitable = 2 //<! Compiler will determine which are suitable for inling
    }

    /// <summary>
    /// Types of optimisation
    /// </summary>
    public enum EOptimization
    {
        Full    //<! Full optimisations
    }
}
