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
using Bam.Core.V2; // for EPlatform.PlatformExtensions
using System.Linq;
namespace C
{
namespace V2
{
namespace DefaultSettings
{
    public static partial class DefaultSettingsExtensions
    {
        public static void Defaults(this C.V2.ICommonCompilerOptions settings, Bam.Core.V2.Module module)
        {
            settings.Bits = (module as CModule).BitDepth;
            settings.DebugSymbols = module.BuildEnvironment.Configuration == Bam.Core.EConfiguration.Debug;
            settings.OmitFramePointer = module.BuildEnvironment.Configuration != Bam.Core.EConfiguration.Debug;
            settings.Optimization = module.BuildEnvironment.Configuration == Bam.Core.EConfiguration.Debug ? EOptimization.Off : EOptimization.Speed;
            settings.OutputType = ECompilerOutput.CompileOnly;
            settings.PreprocessorDefines.Add(System.String.Format("D_BAM_CONFIGURATION_{0}", module.BuildEnvironment.Configuration.ToString().ToUpper()));
            if (module.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
            {
                settings.PreprocessorDefines.Add("D_BAM_PLATFORM_WINDOWS");
            }
            else if (module.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Unix))
            {
                settings.PreprocessorDefines.Add("D_BAM_PLATFORM_UNIX");
            }
            else if (module.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
            {
                settings.PreprocessorDefines.Add("D_BAM_PLATFORM_OSX");
            }
            else
            {
                throw new Bam.Core.Exception("Unknown platform");
            }
            {
                var is64bit = Bam.Core.OSUtilities.Is64Bit(module.BuildEnvironment.Platform);
                var bits = (is64bit) ? 64 : 32;
                settings.PreprocessorDefines.Add("D_BAM_PLATFORM_BITS", bits.ToString());
            }
            {
                var isLittleEndian = Bam.Core.State.IsLittleEndian;
                if (isLittleEndian)
                {
                    settings.PreprocessorDefines.Add("D_BAM_PLATFORM_LITTLEENDIAN");
                }
                else
                {
                    settings.PreprocessorDefines.Add("D_BAM_PLATFORM_BIGENDIAN");
                }
            }
            settings.TargetLanguage = ETargetLanguage.C;
            settings.WarningsAsErrors = true;
        }
        public static void Empty(this C.V2.ICommonCompilerOptions settings)
        {
            settings.DisableWarnings = new Bam.Core.StringArray();
            settings.IncludePaths = new Bam.Core.Array<Bam.Core.V2.TokenizedString>();
            settings.PreprocessorDefines = new PreprocessorDefinitions();
            settings.PreprocessorUndefines = new Bam.Core.StringArray();
            settings.SystemIncludePaths = new Bam.Core.Array<Bam.Core.V2.TokenizedString>();
        }
        public static void
        SharedSettings(
            this C.V2.ICommonCompilerOptions shared,
            C.V2.ICommonCompilerOptions lhs,
            C.V2.ICommonCompilerOptions rhs)
        {
            shared.Bits = (lhs.Bits == rhs.Bits) ? lhs.Bits : null;
            shared.PreprocessorDefines = new PreprocessorDefinitions(lhs.PreprocessorDefines.Intersect(rhs.PreprocessorDefines));
            shared.IncludePaths = lhs.IncludePaths.Intersect(rhs.IncludePaths);
            shared.SystemIncludePaths = lhs.SystemIncludePaths.Intersect(rhs.SystemIncludePaths);
            shared.OutputType = (lhs.OutputType == rhs.OutputType) ? lhs.OutputType : null;
            shared.DebugSymbols = (lhs.DebugSymbols == rhs.DebugSymbols) ? lhs.DebugSymbols : null;
            shared.WarningsAsErrors = (lhs.WarningsAsErrors == rhs.WarningsAsErrors) ? lhs.WarningsAsErrors : null;
            shared.Optimization = (lhs.Optimization == rhs.Optimization) ? lhs.Optimization : null;
            shared.TargetLanguage = (lhs.TargetLanguage == rhs.TargetLanguage) ? lhs.TargetLanguage : null;
            shared.OmitFramePointer = (lhs.OmitFramePointer == rhs.OmitFramePointer) ? lhs.OmitFramePointer : null;
            shared.DisableWarnings = new Bam.Core.StringArray(lhs.DisableWarnings.Intersect(rhs.DisableWarnings));
            shared.PreprocessorUndefines = new Bam.Core.StringArray(lhs.PreprocessorUndefines.Intersect(rhs.PreprocessorUndefines));
        }
        public static void
        Delta(
            this C.V2.ICommonCompilerOptions delta,
            C.V2.ICommonCompilerOptions lhs,
            C.V2.ICommonCompilerOptions rhs)
        {
            delta.Bits = (lhs.Bits != rhs.Bits) ? lhs.Bits : null;
            delta.PreprocessorDefines = new PreprocessorDefinitions(lhs.PreprocessorDefines.Except(rhs.PreprocessorDefines));
            delta.IncludePaths = new Bam.Core.Array<TokenizedString>(lhs.IncludePaths.Except(rhs.IncludePaths));
            delta.SystemIncludePaths = new Bam.Core.Array<TokenizedString>(lhs.SystemIncludePaths.Except(rhs.SystemIncludePaths));
            delta.OutputType = (lhs.OutputType != rhs.OutputType) ? lhs.OutputType : null;
            delta.DebugSymbols = (lhs.DebugSymbols != rhs.DebugSymbols) ? lhs.DebugSymbols : null;
            delta.WarningsAsErrors = (lhs.WarningsAsErrors != rhs.WarningsAsErrors) ? lhs.WarningsAsErrors : null;
            delta.Optimization = (lhs.Optimization != rhs.Optimization) ? lhs.Optimization : null;
            delta.TargetLanguage = (lhs.TargetLanguage != rhs.TargetLanguage) ? lhs.TargetLanguage : null;
            delta.OmitFramePointer = (lhs.OmitFramePointer != rhs.OmitFramePointer) ? lhs.OmitFramePointer : null;
            delta.DisableWarnings = new Bam.Core.StringArray(lhs.DisableWarnings.Except(rhs.DisableWarnings));
            delta.PreprocessorUndefines = new Bam.Core.StringArray(lhs.PreprocessorUndefines.Except(rhs.PreprocessorUndefines));
        }
        public static void
        Clone(
            this C.V2.ICommonCompilerOptions settings,
            C.V2.ICommonCompilerOptions other)
        {
            settings.Bits = other.Bits;
            foreach (var define in other.PreprocessorDefines)
            {
                settings.PreprocessorDefines.Add(define.Key, define.Value);
            }
            foreach (var path in other.IncludePaths)
            {
                settings.IncludePaths.AddUnique(path);
            }
            foreach (var path in other.SystemIncludePaths)
            {
                settings.SystemIncludePaths.AddUnique(path);
            }
            settings.OutputType = other.OutputType;
            settings.DebugSymbols = other.DebugSymbols;
            settings.WarningsAsErrors = other.WarningsAsErrors;
            settings.Optimization = other.Optimization;
            settings.TargetLanguage = other.TargetLanguage;
            settings.OmitFramePointer = other.OmitFramePointer;
            foreach (var path in other.DisableWarnings)
            {
                settings.DisableWarnings.AddUnique(path);
            }
            foreach (var path in other.PreprocessorUndefines)
            {
                settings.PreprocessorUndefines.AddUnique(path);
            }
        }
    }
}
    public abstract class SettingsBase :
        Bam.Core.V2.Settings
    {
        public SettingsBase
        CreateDeltaSettings(
            Bam.Core.V2.Settings sharedSettings,
            Module module)
        {
            var settingsType = module.Settings.GetType();
            var moduleSpecificSettings = System.Activator.CreateInstance(settingsType, module, false) as SettingsBase;

            var attributeType = typeof(SettingsExtensionsAttribute);

            var interfaces = settingsType.GetInterfaces().Where(item => (item != typeof(ISettingsBase)) && typeof(ISettingsBase).IsAssignableFrom(item));
            var sharedInterfaces = sharedSettings.GetType().GetInterfaces().Where(item => (item != typeof(ISettingsBase)) && typeof(ISettingsBase).IsAssignableFrom(item));
            foreach (var i in interfaces)
            {
                var attributeArray = i.GetCustomAttributes(attributeType, false);
                if (0 == attributeArray.Length)
                {
                    throw new Bam.Core.Exception("Settings interface {0} is missing attribute {1}", i.ToString(), attributeType.ToString());
                }

                var attribute = attributeArray[0] as Bam.Core.V2.SettingsExtensionsAttribute;

                if (sharedInterfaces.Any(item => item == i))
                {
                    var deltaMethod = attribute.GetMethod("Delta", new[] { i, i, i });
                    if (null != deltaMethod)
                    {
                        Bam.Core.Log.DebugMessage("Executing {0}", deltaMethod.Name);
                        deltaMethod.Invoke(null, new[] { moduleSpecificSettings, this, sharedSettings });
                    }
                    else
                    {
                        throw new Bam.Core.Exception("Unable to find method {0}.Delta(this {1}, {1}, {1)", attribute.ClassType.ToString(), i.ToString());
                    }
                }
                else
                {
                    var cloneMethod = attribute.GetMethod("Clone", new[] { i, i });
                    if (null != cloneMethod)
                    {
                        Bam.Core.Log.DebugMessage("Executing {0}", cloneMethod.Name);
                        cloneMethod.Invoke(null, new[] { moduleSpecificSettings, this });
                    }
                    else
                    {
                        throw new Bam.Core.Exception("Unable to find method {0}.Clone(this {1}, {1})", attribute.ClassType.ToString(), i.ToString());
                    }
                }
            }

            return moduleSpecificSettings;
        }

        private static Bam.Core.TypeArray
        SharedInterfaces(
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> objectFiles)
        {
            System.Collections.Generic.IEnumerable<System.Type> sharedInterfaces = null;
            foreach (var input in objectFiles)
            {
                var interfaces = input.Settings.GetType().GetInterfaces().Where(item => (item != typeof(ISettingsBase)) && typeof(ISettingsBase).IsAssignableFrom(item));;
                if (null == sharedInterfaces)
                {
                    sharedInterfaces = interfaces;
                }
                else
                {
                    sharedInterfaces = sharedInterfaces.Intersect(interfaces);
                }
            }
            return new Bam.Core.TypeArray(sharedInterfaces);
        }

        public static SettingsBase
        SharedSettings(
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> objectFiles,
            System.Type convertExtensionClassType,
            System.Type conversionInterfaceType,
            Bam.Core.TypeArray convertParameterTypes)
        {
            var sharedInterfaces = SharedInterfaces(objectFiles);
            var implementedInterfaces = new Bam.Core.TypeArray(sharedInterfaces);
            implementedInterfaces.Add(conversionInterfaceType);

            // define a new type, that contains just the shared interfaces between all object files
            // (any interface not shared, must be cloned later)
            var typeSignature = "IDESharedSettings";
            var assemblyName = new System.Reflection.AssemblyName(typeSignature);
            var assemblyBuilder = System.AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, System.Reflection.Emit.AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule", true);
            var sharedSettingsTypeDefn = moduleBuilder.DefineType(typeSignature,
                System.Reflection.TypeAttributes.Public |
                System.Reflection.TypeAttributes.Class |
                System.Reflection.TypeAttributes.AutoClass |
                System.Reflection.TypeAttributes.AnsiClass |
                System.Reflection.TypeAttributes.BeforeFieldInit |
                System.Reflection.TypeAttributes.AutoLayout,
                typeof(C.V2.SettingsBase),
                implementedInterfaces.ToArray());

                // TODO: is this necessary?
#if false
            sharedSettingsTypeDefn.DefineDefaultConstructor(
                System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.SpecialName | System.Reflection.MethodAttributes.RTSpecialName);
#endif

            // implement 'automatic property' setter and getters for each property in each interface
            foreach (var i in sharedInterfaces)
            {
                var properties = i.GetProperties();
                foreach (var prop in properties)
                {
                    var dynamicProperty = sharedSettingsTypeDefn.DefineProperty(prop.Name,
                        System.Reflection.PropertyAttributes.None,
                        prop.PropertyType,
                        System.Type.EmptyTypes);
                    var field = sharedSettingsTypeDefn.DefineField("m" + prop.Name,
                        prop.PropertyType,
                        System.Reflection.FieldAttributes.Private);
                    var methodAttrs = System.Reflection.MethodAttributes.Public |
                        System.Reflection.MethodAttributes.HideBySig |
                        System.Reflection.MethodAttributes.Virtual;
                    if (prop.IsSpecialName)
                    {
                        methodAttrs |= System.Reflection.MethodAttributes.SpecialName;
                    }
                    var getter = sharedSettingsTypeDefn.DefineMethod("get_" + prop.Name,
                        methodAttrs,
                        prop.PropertyType,
                        System.Type.EmptyTypes);
                    var setter = sharedSettingsTypeDefn.DefineMethod("set_" + prop.Name,
                        methodAttrs,
                        null,
                        new[] { prop.PropertyType });
                    var getIL = getter.GetILGenerator();
                    getIL.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
                    getIL.Emit(System.Reflection.Emit.OpCodes.Ldfld, field);
                    getIL.Emit(System.Reflection.Emit.OpCodes.Ret);
                    dynamicProperty.SetGetMethod(getter);
                    var setIL = setter.GetILGenerator();
                    setIL.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
                    setIL.Emit(System.Reflection.Emit.OpCodes.Ldarg_1);
                    setIL.Emit(System.Reflection.Emit.OpCodes.Stfld, field);
                    setIL.Emit(System.Reflection.Emit.OpCodes.Ret);
                    dynamicProperty.SetSetMethod(setter);
                }
            }

            var vsProjectSettingsConvertMethod = sharedSettingsTypeDefn.DefineMethod("Convert",
                System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.Final | System.Reflection.MethodAttributes.HideBySig | System.Reflection.MethodAttributes.NewSlot | System.Reflection.MethodAttributes.Virtual,
                null,
                convertParameterTypes.ToArray());
            var convertIL = vsProjectSettingsConvertMethod.GetILGenerator();
            foreach (var i in sharedInterfaces)
            {
                var extConvertParameterTypes = new Bam.Core.TypeArray(i);
                extConvertParameterTypes.AddRange(convertParameterTypes);
                var methInfo = convertExtensionClassType.GetMethod("Convert", extConvertParameterTypes.ToArray());
                if (null == methInfo)
                {
                        throw new Bam.Core.Exception("Unable to locate the function {0}.{1}(this {2})", convertExtensionClassType.FullName, "Convert", i.Name);
                }
                // TODO: can this be simplified, using the ldarg opcode? a simple loop would suffice
                convertIL.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
                if (extConvertParameterTypes.Count > 1)
                {
                    convertIL.Emit(System.Reflection.Emit.OpCodes.Ldarg_1);
                }
                if (extConvertParameterTypes.Count > 2)
                {
                    convertIL.Emit(System.Reflection.Emit.OpCodes.Ldarg_2);
                }
                if (extConvertParameterTypes.Count > 3)
                {
                    convertIL.Emit(System.Reflection.Emit.OpCodes.Ldarg_3);
                }
                convertIL.Emit(System.Reflection.Emit.OpCodes.Call, methInfo);
            }
            convertIL.Emit(System.Reflection.Emit.OpCodes.Ret);

            var sharedSettingsType = sharedSettingsTypeDefn.CreateType();
            var attributeType = typeof(SettingsExtensionsAttribute);

            // now that we have an instance of the shared settings type, calculate the values of the individual settings across all object files
            // for all shared interfaces
            var commonSettings = System.Activator.CreateInstance(sharedSettingsType) as SettingsBase;
            foreach (var i in sharedInterfaces)
            {
                var attributeArray = i.GetCustomAttributes(attributeType, false);
                if (0 == attributeArray.Length)
                {
                    throw new Bam.Core.Exception("Settings interface {0} is missing attribute {1}", i.ToString(), attributeType.ToString());
                }

                var attribute = attributeArray[0] as Bam.Core.V2.SettingsExtensionsAttribute;

                var method = attribute.GetMethod("SharedSettings", new[] { i, i, i });
                if (null != method)
                {
                    Bam.Core.Log.DebugMessage("Executing {0}", method.Name);

                    var objectFileCount = objectFiles.Count;
                    for (int objIndex = 0; objIndex < objectFileCount - 1; ++objIndex)
                    {
                        method.Invoke(null, new[] { commonSettings, objectFiles[objIndex].Settings, objectFiles[objIndex + 1].Settings });
                    }
                }
                else
                {
                    throw new Bam.Core.Exception("Unable to find extension method {0}.SharedSettings(this {1}, {1}, {1})", attribute.ClassType.ToString(), i.ToString());
                }
            }
            return commonSettings;
        }
    }

    public enum EBit
    {
        ThirtyTwo = 32,
        SixtyFour = 64
    }

    [Bam.Core.V2.SettingsExtensions(typeof(C.V2.DefaultSettings.DefaultSettingsExtensions))]
    public interface ICommonCompilerOptions : Bam.Core.V2.ISettingsBase
    {
        EBit? Bits
        {
            get;
            set;
        }

        C.V2.PreprocessorDefinitions PreprocessorDefines
        {
            get;
            set;
        }

        Bam.Core.Array<Bam.Core.V2.TokenizedString> IncludePaths
        {
            get;
            set;
        }

        Bam.Core.Array<Bam.Core.V2.TokenizedString> SystemIncludePaths
        {
            get;
            set;
        }

        C.ECompilerOutput? OutputType
        {
            get;
            set;
        }

        bool? DebugSymbols
        {
            get;
            set;
        }

        bool? WarningsAsErrors
        {
            get;
            set;
        }

        C.EOptimization? Optimization
        {
            get;
            set;
        }

        C.ETargetLanguage? TargetLanguage
        {
            get;
            set;
        }

        bool? OmitFramePointer
        {
            get;
            set;
        }

        Bam.Core.StringArray DisableWarnings
        {
            get;
            set;
        }

        Bam.Core.StringArray PreprocessorUndefines
        {
            get;
            set;
        }
    }

    [Bam.Core.V2.SettingsExtensions(typeof(C.V2.DefaultSettings.DefaultSettingsExtensions))]
    public interface ICOnlyCompilerOptions : Bam.Core.V2.ISettingsBase
    {
        C.ECLanguageStandard? LanguageStandard
        {
            get;
            set;
        }
    }

    [Bam.Core.V2.SettingsExtensions(typeof(C.ObjC.V2.DefaultSettings.DefaultSettingsExtensions))]
    public interface IObjectiveCOnlyCompilerOptions : Bam.Core.V2.ISettingsBase
    {
        string ConstantStringClass
        {
            get;
            set;
        }
    }

    [Bam.Core.V2.SettingsExtensions(typeof(C.V2.DefaultSettings.DefaultSettingsExtensions))]
    public interface IObjectiveCxxOnlyCompilerOptions : Bam.Core.V2.ISettingsBase
    {
    }
}
    public interface ICCompilerOptions
    {
        /// <summary>
        /// Preprocessor definitions
        /// </summary>
        C.DefineCollection Defines
        {
            get;
            set;
        }

        /// <summary>
        /// Preprocessor user header search paths: #includes of the style #include "..."
        /// </summary>
        Bam.Core.DirectoryCollection IncludePaths
        {
            get;
            set;
        }

        /// <summary>
        /// Preprocessor system header search paths: #includes of the stylr #include <...>
        /// </summary>
        Bam.Core.DirectoryCollection SystemIncludePaths
        {
            get;
            set;
        }

        /// <summary>
        /// Type of file output from the compiler
        /// </summary>
        C.ECompilerOutput OutputType
        {
            get;
            set;
        }

        /// <summary>
        /// Compiled objects contain debug symbols
        /// </summary>
        bool DebugSymbols
        {
            get;
            set;
        }

        /// <summary>
        /// Treat all warnings as errors
        /// </summary>
        bool WarningsAsErrors
        {
            get;
            set;
        }

        /// <summary>
        /// Compiler ignores all built in standard include paths. User must provide them.
        /// </summary>
        bool IgnoreStandardIncludePaths
        {
            get;
            set;
        }

        /// <summary>
        /// The level of optimization the compiler uses
        /// </summary>
        C.EOptimization Optimization
        {
            get;
            set;
        }

        /// <summary>
        /// Custom optimization settings not provided by the Optimization option.
        /// </summary>
        string CustomOptimization
        {
            get;
            set;
        }

        /// <summary>
        /// The target language of the compiled source code
        /// </summary>
        C.ETargetLanguage TargetLanguage
        {
            get;
            set;
        }

        /// <summary>
        /// Display the paths of the header files included on stdout
        /// </summary>
        bool ShowIncludes
        {
            get;
            set;
        }

        /// <summary>
        /// Explicit compiler options added to the compilation step
        /// </summary>
        string AdditionalOptions
        {
            get;
            set;
        }

        /// <summary>
        /// Omit the frame pointer from the compiled object code for each stack frame
        /// </summary>
        bool OmitFramePointer
        {
            get;
            set;
        }

        /// <summary>
        /// A list of warnings that are disabled
        /// </summary>
        Bam.Core.StringArray DisableWarnings
        {
            get;
            set;
        }

        /// <summary>
        /// The target character set in use by the compiled code
        /// </summary>
        C.ECharacterSet CharacterSet
        {
            get;
            set;
        }

        /// <summary>
        /// The language standard for C/C++ compilation
        /// </summary>
        C.ELanguageStandard LanguageStandard
        {
            get;
            set;
        }

        /// <summary>
        /// Preprocessor definitions to be undefined
        /// </summary>
        C.DefineCollection Undefines
        {
            get;
            set;
        }
    }
}
