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
using System.Linq;
namespace C
{
    /// <summary>
    /// Utility class for accessing toolchain registration
    /// </summary>
    static class DefaultToolchain
    {
        private static readonly Options.DefaultToolchainCommand SelectDefaultToolChainCommand = new Options.DefaultToolchainCommand();
        private static readonly System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray> C_Compilers = new System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray>();
        private static readonly System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray> Cxx_Compilers = new System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray>();
        private static readonly System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray> Archivers = new System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray>();
        private static readonly System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray> C_Linkers = new System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray>();
        private static readonly System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray> Cxx_Linkers = new System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray>();
        private static readonly System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray> ObjectiveC_Compilers = new System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray>();
        private static readonly System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray> ObjectiveCxx_Compilers = new System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray>();
        private static readonly System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray> WinResourceCompilers = new System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray>();
        private static readonly System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray> Assemblers = new System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray>();
        private static readonly System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray> Preprocessors = new System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray>();
        private static readonly string UserToolchainOverride = null;

        // name of the toolchain to use, after disambiguation
        private static readonly System.Collections.Generic.Dictionary<EBit, string> DisambiguousToolchainToUse = new System.Collections.Generic.Dictionary<EBit, string>();

        // cache of the tool modules for a particular toolchain
        private sealed class ToolModules
        {
            public CompilerTool            c_compiler = null;
            public CompilerTool            cxx_compiler = null;
            public LibrarianTool           librarian = null;
            public LinkerTool              c_linker = null;
            public LinkerTool              cxx_linker = null;
            public CompilerTool            objc_compiler = null;
            public CompilerTool            objcxx_compiler = null;
            public WinResourceCompilerTool winres_compiler = null;
            public AssemblerTool           assembler = null;
            public PreprocessorTool        preprocessor = null;
        };
        private static readonly  System.Collections.Generic.Dictionary<EBit, ToolModules> Default = new System.Collections.Generic.Dictionary<EBit, ToolModules>();

        private static System.Collections.Generic.IEnumerable<System.Tuple<System.Type,T>>
        GetToolsFromMetaData<T>()
            where T : ToolRegistrationAttribute
        {
            var discoverAllToolchains = Bam.Core.CommandLineProcessor.Evaluate(new Options.DiscoverAllToolchains());
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
                    if (!discoverAllToolchains && Bam.Core.OSUtilities.CurrentOS != tool.Platform)
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
            FindTools<RegisterCCompilerAttribute, CompilerTool>(C_Compilers);
            FindTools<RegisterCxxCompilerAttribute, CompilerTool>(Cxx_Compilers);
            FindTools<RegisterLibrarianAttribute, LibrarianTool>(Archivers);
            FindTools<RegisterCLinkerAttribute, LinkerTool>(C_Linkers);
            FindTools<RegisterCxxLinkerAttribute, LinkerTool>(Cxx_Linkers);
            FindTools<RegisterObjectiveCCompilerAttribute, CompilerTool>(ObjectiveC_Compilers);
            FindTools<RegisterObjectiveCxxCompilerAttribute, CompilerTool>(ObjectiveCxx_Compilers);
            FindTools<RegisterWinResourceCompilerAttribute, WinResourceCompilerTool>(WinResourceCompilers);
            FindTools<RegisterAssemblerAttribute, AssemblerTool>(Assemblers);
            FindTools<RegisterPreprocessorAttribute, PreprocessorTool>(Preprocessors);

            // disambiguate any bitdepths with multiple tool types
            // any bit depths that remain ambiguous have no entry in DisambiguousToolchainToUse
            // and exceptions are raised if and when they are used (not now)
            UserToolchainOverride = Bam.Core.CommandLineProcessor.Evaluate(SelectDefaultToolChainCommand);
            foreach (EBit bitDepth in System.Enum.GetValues(typeof(EBit)))
            {
                // always add an empty ToolModules for each bitdepth - the fields of which are filled out
                // if and when the specific tools are requested
                Default.Add(bitDepth, new ToolModules());

                if (!C_Compilers.ContainsKey(bitDepth) || !C_Compilers[bitDepth].Any())
                {
                    // all bets are off if there's not even a C compiler
                    continue;
                }

                var ambiguous_toolchain =
                    (C_Compilers.ContainsKey(bitDepth) && C_Compilers[bitDepth].Skip(1).Any()) ||
                    (Cxx_Compilers.ContainsKey(bitDepth) && Cxx_Compilers[bitDepth].Skip(1).Any()) ||
                    (Archivers.ContainsKey(bitDepth) && Archivers[bitDepth].Skip(1).Any()) ||
                    (C_Linkers.ContainsKey(bitDepth) && C_Linkers[bitDepth].Skip(1).Any()) ||
                    (Cxx_Linkers.ContainsKey(bitDepth) && Cxx_Linkers[bitDepth].Skip(1).Any()) ||
                    (ObjectiveC_Compilers.ContainsKey(bitDepth) && ObjectiveC_Compilers[bitDepth].Skip(1).Any()) ||
                    (ObjectiveCxx_Compilers.ContainsKey(bitDepth) && ObjectiveCxx_Compilers[bitDepth].Skip(1).Any()) ||
                    (WinResourceCompilers.ContainsKey(bitDepth) && WinResourceCompilers[bitDepth].Skip(1).Any()) ||
                    (Assemblers.ContainsKey(bitDepth) && Assemblers[bitDepth].Skip(1).Any()) ||
                    (Preprocessors.ContainsKey(bitDepth) && Preprocessors[bitDepth].Skip(1).Any());
                if (ambiguous_toolchain)
                {
                    if (UserToolchainOverride != null)
                    {
                        foreach (var toolTypeToUse in C_Compilers[bitDepth])
                        {
                            var attr = toolTypeToUse.GetCustomAttributes(false);
                            var toolToolSet = (attr[0] as ToolRegistrationAttribute).ToolsetName;
                            if (toolToolSet.Equals(UserToolchainOverride, System.StringComparison.Ordinal))
                            {
                                DisambiguousToolchainToUse.Add(bitDepth, toolToolSet);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    var toolTypeToUse = C_Compilers[bitDepth][0];
                    var attr = toolTypeToUse.GetCustomAttributes(false);
                    var toolToolSet = (attr[0] as ToolRegistrationAttribute).ToolsetName;
                    DisambiguousToolchainToUse.Add(bitDepth, toolToolSet);
                }
            }
        }

        private static ToolType
        GetTool<ToolType>(
            System.Collections.Generic.Dictionary<EBit, Bam.Core.TypeArray> collection,
            EBit bitDepth,
            string toolDescription,
            ref ToolType toolModule)
            where ToolType : Bam.Core.PreBuiltTool
        {
            if (null != toolModule)
            {
                return toolModule;
            }
            if (!collection.Any())
            {
                if (!DisambiguousToolchainToUse.Any())
                {
                    throw new Bam.Core.Exception(
                        "No toolchain packages were detected for this platform"
                    );
                }
                throw new Bam.Core.Exception(
                    $"Platform contains toolchains, but no registrations of a '{toolDescription}' were found"
                );
            }
            if (!DisambiguousToolchainToUse.ContainsKey(bitDepth))
            {
                var candidates = collection[bitDepth];
                var tooManyInstance = new System.Text.StringBuilder();
                tooManyInstance.AppendLine(
                    $"There are {candidates.Count} {toolDescription}s available for this platform in {(int)bitDepth}-bits. Resolve using the command line option {(SelectDefaultToolChainCommand as Bam.Core.ICommandLineArgument).LongName}=<choice>"
                );
                foreach (var tool in candidates)
                {
                    tooManyInstance.AppendLine($"\t{tool.ToString()}");
                }
                throw new Bam.Core.Exception(tooManyInstance.ToString());
            }
            var toolchainToUse = DisambiguousToolchainToUse[bitDepth];
            if (null == toolchainToUse)
            {
                throw new Bam.Core.Exception($"{toolDescription} tool is undefined in {bitDepth.ToString()}-bit architectures");
            }
            var toolTypeCollection = collection[bitDepth];
            var toolTypeToInstantiate = toolTypeCollection.FirstOrDefault(item => (item.GetCustomAttributes(false)[0] as ToolRegistrationAttribute).ToolsetName.Equals(toolchainToUse, System.StringComparison.Ordinal));
            if (null == toolTypeToInstantiate)
            {
                throw new Bam.Core.Exception(
                    $"Unable to identify {toolDescription} tool in {bitDepth.ToString()}-bit architectures for toolchain {toolchainToUse}"
                );
            }
            toolModule = Bam.Core.Graph.Instance.MakeModuleOfType<ToolType>(toolTypeToInstantiate);
            return toolModule;
        }

        /// <summary>
        /// Get a suitable C compiler
        /// </summary>
        /// <param name="bitDepth">Bit-depth required to compile for</param>
        /// <returns>The compiler</returns>
        public static CompilerTool
        C_Compiler(
            EBit bitDepth) => GetTool<CompilerTool>(C_Compilers, bitDepth, "C compiler", ref Default[bitDepth].c_compiler);

        /// <summary>
        /// Get a suitable C++ compiler
        /// </summary>
        /// <param name="bitDepth">Bit-depth required to compile for</param>
        /// <returns>The compiler</returns>
        public static CompilerTool
        Cxx_Compiler(
            EBit bitDepth) => GetTool<CompilerTool>(Cxx_Compilers, bitDepth, "C++ compiler", ref Default[bitDepth].cxx_compiler);

        /// <summary>
        /// Get a suitable librarian
        /// </summary>
        /// <param name="bitDepth">Bit-depth required to archive for</param>
        /// <returns>The librarian</returns>
        public static LibrarianTool
        Librarian(
            EBit bitDepth) => GetTool<LibrarianTool>(Archivers, bitDepth, "librarian", ref Default[bitDepth].librarian);

        /// <summary>
        /// Get a suitable C linker
        /// </summary>
        /// <param name="bitDepth">Bit-depth required to link for</param>
        /// <returns>The linker</returns>
        public static LinkerTool
        C_Linker(
            EBit bitDepth) => GetTool<LinkerTool>(C_Linkers, bitDepth, "C linker", ref Default[bitDepth].c_linker);

        /// <summary>
        /// Get a suitable C++ linker
        /// </summary>
        /// <param name="bitDepth">Bit-depth required to link for</param>
        /// <returns>The linker</returns>
        public static LinkerTool
        Cxx_Linker(
            EBit bitDepth) => GetTool<LinkerTool>(Cxx_Linkers, bitDepth, "C++ linker", ref Default[bitDepth].cxx_linker);

        /// <summary>
        /// Get a suitable Objective C compiler
        /// </summary>
        /// <param name="bitDepth">Bit-depth required to compile for</param>
        /// <returns>The compiler</returns>
        public static CompilerTool
        ObjectiveC_Compiler(
            EBit bitDepth) => GetTool<CompilerTool>(ObjectiveC_Compilers, bitDepth, "Objective C compiler", ref Default[bitDepth].objc_compiler);

        /// <summary>
        /// Get a suitable Objective C++ compiler
        /// </summary>
        /// <param name="bitDepth">Bit-depth required to compile for</param>
        /// <returns>The compiler</returns>
        public static CompilerTool
        ObjectiveCxx_Compiler(
            EBit bitDepth) => GetTool<CompilerTool>(ObjectiveCxx_Compilers, bitDepth, "Objective C++ compiler", ref Default[bitDepth].objcxx_compiler);

        /// <summary>
        /// Get a suitable Windows resource compiler
        /// </summary>
        /// <param name="bitDepth">Bit-depth required to compile for</param>
        /// <returns>The compiler</returns>
        public static WinResourceCompilerTool
        WinResource_Compiler(
            EBit bitDepth) => GetTool<WinResourceCompilerTool>(WinResourceCompilers, bitDepth, "Windows resource compiler", ref Default[bitDepth].winres_compiler);

        /// <summary>
        /// Get a suitable assembler
        /// </summary>
        /// <param name="bitDepth">Bit-depth required to assemble for</param>
        /// <returns>The assembler</returns>
        public static AssemblerTool
        Assembler(
            EBit bitDepth) => GetTool<AssemblerTool>(Assemblers, bitDepth, "Assembler", ref Default[bitDepth].assembler);

        /// <summary>
        /// Get a suitable preprocessor
        /// </summary>
        /// <param name="bitDepth">Bit-depth required to preprocess for</param>
        /// <returns>The preprocessor</returns>
        public static PreprocessorTool
        Preprocessor(
            EBit bitDepth) => GetTool<PreprocessorTool>(Preprocessors, bitDepth, "Preprocessor", ref Default[bitDepth].preprocessor);
    }
}
