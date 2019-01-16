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
    public enum EWarningLevel
    {
        Level0 = 0,
        Level1,
        Level2,
        Level3,
        Level4
    }

    public enum EAssemblerWarningLevel
    {
        Level0 = 0,
        Level1,
        Level2,
        Level3
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

    public enum EOptimization
    {
        Full
    }
}
