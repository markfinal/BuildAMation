#region License
// Copyright (c) 2010-2017, Mark Final
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
namespace C
{
    public static class PatchUtilities
    {
        private static void
        CheckIsCompilable<ChildModuleType>(
            Bam.Core.Module module)
            where ChildModuleType : Bam.Core.Module, Bam.Core.IInputPath, Bam.Core.IChildModule, new()
        {
            if (module is Bam.Core.IModuleGroup)
            {
                if (!(module is C.CCompilableModuleContainer<ChildModuleType>))
                {
                    throw new Bam.Core.Exception(
                        "Group module type, '{0}', is not a compilable of type '{1}'",
                        module.GetType().ToString(),
                        typeof(C.CCompilableModuleContainer<ChildModuleType>).ToString());
                }
            }
            else
            {
                if (!(module is ChildModuleType))
                {
                    throw new Bam.Core.Exception(
                        "Module type, '{0}', is not a compilable of type '{1}'",
                        module.GetType().ToString(),
                        typeof(ChildModuleType).ToString());
                }
            }
        }

        /// <summary>
        /// From a generic Bam.Core.Module for a compilable module, get the CompilerTool
        /// associated with it.
        /// </summary>
        /// <typeparam name="ChildModuleType">What kind of compilable module should be expected.</typeparam>
        /// <param name="module">The module. Usually obtained from settings.Module in a patch.</param>
        /// <returns>The compiler associated with the module. Or an exception is thrown if the module is unsuitable.</returns>
        public static CompilerTool
        GetCompiler<ChildModuleType>(
            Bam.Core.Module module)
            where ChildModuleType : Bam.Core.Module, Bam.Core.IInputPath, Bam.Core.IChildModule, new()
        {
            CheckIsCompilable<ChildModuleType>(module);
            var compilerUsed = (module is Bam.Core.IModuleGroup) ?
              (module as C.CCompilableModuleContainer<ChildModuleType>).Compiler :
              (module as ObjectFile).Compiler;
            return compilerUsed;
        }

        /// <summary>
        /// From a generic Bam.Core.Module for a compilable module, get the module's bit depth (32 or 64 bit).
        /// </summary>
        /// <typeparam name="ChildModuleType">What kind of compilable module should be expected.</typeparam>
        /// <param name="module">The module. Usually obtained from settings.Module in a patch.</param>
        /// <returns>The bit depth of the module. Or an exception is thrown if the module in unsuitable.</returns>
        public static C.EBit
        GetBitDepth<ChildModuleType>(
            Bam.Core.Module module)
            where ChildModuleType : Bam.Core.Module, Bam.Core.IInputPath, Bam.Core.IChildModule, new()
        {
            CheckIsCompilable<ChildModuleType>(module);
            var bitDepth = (module is Bam.Core.IModuleGroup) ?
              (module as C.CCompilableModuleContainer<ChildModuleType>).BitDepth :
              (module as ObjectFile).BitDepth;
            return bitDepth;
        }
    }
}
