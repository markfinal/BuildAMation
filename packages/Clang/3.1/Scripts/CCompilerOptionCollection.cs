// <copyright file="CCompilerOptionCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Clang package</summary>
// <author>Mark Final</author>
namespace Clang
{
    public class CCompilerOptionCollection : C.CompilerOptionCollection, C.ICCompilerOptions
    {
        public CCompilerOptionCollection()
        {
        }

        public CCompilerOptionCollection(Opus.Core.DependencyNode owningNode)
            : base(owningNode)
        {
        }

        public override Opus.Core.DirectoryCollection DirectoriesToCreate()
        {
            return null;
        }

        #region ICCompilerOptions Members

        C.ToolchainOptionCollection C.ICCompilerOptions.ToolchainOptionCollection
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        C.DefineCollection C.ICCompilerOptions.Defines
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        Opus.Core.DirectoryCollection C.ICCompilerOptions.IncludePaths
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        Opus.Core.DirectoryCollection C.ICCompilerOptions.SystemIncludePaths
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        C.ECompilerOutput C.ICCompilerOptions.OutputType
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        bool C.ICCompilerOptions.DebugSymbols
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        bool C.ICCompilerOptions.WarningsAsErrors
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        bool C.ICCompilerOptions.IgnoreStandardIncludePaths
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        C.EOptimization C.ICCompilerOptions.Optimization
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        string C.ICCompilerOptions.CustomOptimization
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        C.ETargetLanguage C.ICCompilerOptions.TargetLanguage
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        bool C.ICCompilerOptions.ShowIncludes
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        string C.ICCompilerOptions.AdditionalOptions
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        bool C.ICCompilerOptions.OmitFramePointer
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        Opus.Core.StringArray C.ICCompilerOptions.DisableWarnings
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        #endregion
    }
}
