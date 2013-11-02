// <copyright file="DefineCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public sealed class DefineCollection : System.ICloneable, Opus.Core.ISetOperations<DefineCollection>
    {
        private Opus.Core.StringArray defines = new Opus.Core.StringArray();

        public void Add(object toAdd)
        {
            var define = toAdd as string;
            lock (this.defines)
            {
                if (!this.defines.Contains(define))
                {
                    this.defines.Add(define);
                }
                else
                {
                    Opus.Core.Log.DebugMessage("The define '{0}' is already present", define);
                }
            }
        }

        public int Count
        {
            get
            {
                return this.defines.Count;
            }
        }

        public System.Collections.Generic.IEnumerator<string> GetEnumerator()
        {
            return this.defines.GetEnumerator();
        }

        public object Clone()
        {
            var clonedObject = new DefineCollection();
            foreach (var define in this.defines)
            {
                clonedObject.Add(define.Clone() as string);
            }
            return clonedObject;
        }

        public Opus.Core.StringArray ToStringArray()
        {
            return this.defines;
        }

        public override bool Equals(object obj)
        {
            var otherCollection = obj as DefineCollection;
            return (this.defines.Equals(otherCollection.defines));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        DefineCollection Opus.Core.ISetOperations<DefineCollection>.Complement(DefineCollection other)
        {
            var complementDefines = this.defines.Complement(other.defines);
            if (0 == complementDefines.Count)
            {
                throw new Opus.Core.Exception("DefineCollection complement is empty");
            }

            var complementDefinesCollection = new DefineCollection();
            complementDefinesCollection.defines.AddRange(complementDefines);
            return complementDefinesCollection;
        }

        DefineCollection Opus.Core.ISetOperations<DefineCollection>.Intersect(DefineCollection other)
        {
            var intersectDefines = this.defines.Intersect(other.defines);
            var intersectDefinesCollection = new DefineCollection();
            intersectDefinesCollection.defines.AddRange(intersectDefines);
            return intersectDefinesCollection;
        }
    }
}
