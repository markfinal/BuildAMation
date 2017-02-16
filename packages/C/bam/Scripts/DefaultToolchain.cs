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
    public static class DefaultToolchain
    {
        private static Options.DefaultToolchainCommand SelectDefaultToolChainCommand = new Options.DefaultToolchainCommand();
        private static System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray> C_Compilers = new System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray>();
        private static System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray> Cxx_Compilers = new System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray>();
        private static System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray> Archivers = new System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray>();
        private static System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray> C_Linkers = new System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray>();
        private static System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray> Cxx_Linkers = new System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray>();
        private static System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray> ObjectiveC_Compilers = new System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray>();
        private static System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray> ObjectiveCxx_Compilers = new System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray>();
        private static System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray> WinResourceCompilers = new System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray>();
        private static System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray> Assemblers = new System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray>();
        private static string UserToolchainOverride = null;

        private static System.Collections.Generic.IEnumerable<System.Tuple<System.Type,T>>
        GetToolsFromMetaData<T>()
            where T : ToolRegistrationAttribute
        {
            var allTypes = Bam.Core.Graph.Instance.ScriptAssembly.GetTypes();
            foreach (var type in allTypes)
            {
                var tools = type.GetCustomAttributes(typeof(T), false) as T[];
                if (0 == tools.Length)
                {
                    continue;
                }
                foreach (var tool in tools)
                {
                    if (Bam.Core.OSUtilities.CurrentOS != tool.Platform)
                    {
                        continue;
                    }
                    yield return new System.Tuple<System.Type, T>(type, tool);
                }
            }
        }

        private static void
        FindTools<AttributeType, ToolType>(
            System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray> collection)
            where AttributeType : ToolRegistrationAttribute
            where ToolType : Bam.Core.PreBuiltTool
        {
            var graph = Bam.Core.Graph.Instance;
            foreach (var toolData in GetToolsFromMetaData<AttributeType>())
            {
                var bits = toolData.Item2.BitDepth;
                if (!collection.ContainsKey(bits))
                {
                    collection[bits] = new Bam.Core.TypeArray(toolData.Item1);
                }
                else
                {
                    collection[bits].AddUnique(toolData.Item1);
                }
            }
        }

        static DefaultToolchain()
        {
            UserToolchainOverride = Bam.Core.CommandLineProcessor.Evaluate(SelectDefaultToolChainCommand);
            FindTools<RegisterCCompilerAttribute, CompilerTool>(C_Compilers);
            FindTools<RegisterCxxCompilerAttribute, CompilerTool>(Cxx_Compilers);
            FindTools<RegisterLibrarianAttribute, LibrarianTool>(Archivers);
            FindTools<RegisterCLinkerAttribute, LinkerTool>(C_Linkers);
            FindTools<RegisterCxxLinkerAttribute, LinkerTool>(Cxx_Linkers);
            FindTools<RegisterObjectiveCCompilerAttribute, CompilerTool>(ObjectiveC_Compilers);
            FindTools<RegisterObjectiveCxxCompilerAttribute, CompilerTool>(ObjectiveCxx_Compilers);
            FindTools<RegisterWinResourceCompilerAttribute, WinResourceCompilerTool>(WinResourceCompilers);
            FindTools<RegisterAssemblerAttribute, AssemblerTool>(Assemblers);
        }

        private static ToolType
        GetTool<ToolType>(
            System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray> collection,
            EBit bitDepth,
            string toolDescription)
            where ToolType : Bam.Core.PreBuiltTool
        {
            if (!collection.ContainsKey(bitDepth) || 0 == collection[bitDepth].Count)
            {
                throw new Bam.Core.Exception("No default {0}s found for this platform in {1}-bits. Are all relevant packages present?", toolDescription, (int)bitDepth);
            }
            var candidates = collection[bitDepth];
            if (candidates.Count > 1)
            {
                if (null != UserToolchainOverride)
                {
                    foreach (var toolType in candidates)
                    {
                        var attr = toolType.GetCustomAttributes(false);
                        if ((attr[0] as ToolRegistrationAttribute).ToolsetName == UserToolchainOverride)
                        {
                            return Bam.Core.Graph.Instance.MakeModuleOfType<ToolType>(toolType);
                        }
                    }
                }

                var tooManyInstance = new System.Text.StringBuilder();
                tooManyInstance.AppendFormat("There are {0} {1}s available for this platform in {2}-bits. Resolve using the command line option {3}=<choice>",
                    candidates.Count,
                    toolDescription,
                    (int)bitDepth,
                    (SelectDefaultToolChainCommand as Bam.Core.ICommandLineArgument).LongName);
                tooManyInstance.AppendLine();
                foreach (var tool in candidates)
                {
                    tooManyInstance.AppendFormat("\t{0}", tool.ToString());
                    tooManyInstance.AppendLine();
                }
                throw new Bam.Core.Exception(tooManyInstance.ToString());
            }
            var toolTypeToUse = candidates[0];
            var toolToolSet = (toolTypeToUse.GetCustomAttributes(false)[0] as ToolRegistrationAttribute).ToolsetName;
            if ((null != UserToolchainOverride) && (toolToolSet != UserToolchainOverride))
            {
                var wrongToolchain = new System.Text.StringBuilder();
                wrongToolchain.AppendFormat("{0}-bit {1} identified is from the {2} toolchain, not {3} as requested. Resolve using the command line option {4}={2}",
                    (int)bitDepth,
                    toolDescription,
                    toolToolSet,
                    UserToolchainOverride,
                    (SelectDefaultToolChainCommand as Bam.Core.ICommandLineArgument).LongName);
                wrongToolchain.AppendLine();
                throw new Bam.Core.Exception(wrongToolchain.ToString());
            }
            return Bam.Core.Graph.Instance.MakeModuleOfType<ToolType>(toolTypeToUse);
        }

        public static CompilerTool
        C_Compiler(
            EBit bitDepth)
        {
            return GetTool<CompilerTool>(C_Compilers, bitDepth, "C compiler");
        }

        public static CompilerTool
        Cxx_Compiler(
            EBit bitDepth)
        {
            return GetTool<CompilerTool>(Cxx_Compilers, bitDepth, "C++ compiler");
        }

        public static LibrarianTool
        Librarian(
            EBit bitDepth)
        {
            return GetTool<LibrarianTool>(Archivers, bitDepth, "librarian");
        }

        public static LinkerTool
        C_Linker(
            EBit bitDepth)
        {
            return GetTool<LinkerTool>(C_Linkers, bitDepth, "C linker");
        }

        public static LinkerTool
        Cxx_Linker(
            EBit bitDepth)
        {
            return GetTool<LinkerTool>(Cxx_Linkers, bitDepth, "C++ linker");
        }

        public static CompilerTool
        ObjectiveC_Compiler(
            EBit bitDepth)
        {
            return GetTool<CompilerTool>(ObjectiveC_Compilers, bitDepth, "Objective C compiler");
        }

        public static CompilerTool
        ObjectiveCxx_Compiler(
            EBit bitDepth)
        {
            return GetTool<CompilerTool>(ObjectiveCxx_Compilers, bitDepth, "Objective C++ compiler");
        }

        public static WinResourceCompilerTool
        WinResource_Compiler(
            EBit bitDepth)
        {
            return GetTool<WinResourceCompilerTool>(WinResourceCompilers, bitDepth, "Windows resource compiler");
        }

        public static AssemblerTool
        Assembler(
            EBit bitDepth)
        {
            return GetTool<AssemblerTool>(Assemblers, bitDepth, "Assembler");
        }
    }
}
