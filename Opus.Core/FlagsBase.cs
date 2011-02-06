// <copyright file="FlagsBase.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
#if false
namespace Opus.Core
{
    public class FlagsBase
    {
        public int InternalValue
        {
            get; 
            protected set;
        }

        private string name;

        private int GetNextPowerOfTwo()
        {
            int flagValue = 0;
            System.Reflection.FieldInfo[] fields = this.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            foreach (System.Reflection.FieldInfo field in fields)
            {
                var obj = field.GetValue(null);
                if (null != obj)
                {
                    int value = (obj as FlagsBase).InternalValue;
                    if (value > flagValue)
                    {
                        flagValue = value;
                    }
                }
            }
            if (0 == flagValue)
            {
                flagValue = 1;
            }
            else
            {
                if (System.Int16.MaxValue == flagValue)
                {
                    throw new Exception(System.String.Format("Maximum flag value reached for type '{0}'", this.GetType().Name), false);
                }

                flagValue *= 2;
            }

            Log.DebugMessage("Flag type '{0}': '{1}' = {2}", this.GetType().Name, name, flagValue);

            return flagValue;
        }

        protected FlagsBase(string name)
        {
            int flagValue = this.GetNextPowerOfTwo();
            this.name = name;
            this.InternalValue = flagValue;
        }

        protected FlagsBase(string name, int value)
        {
            this.name = name;
            this.InternalValue = value;
        }

        public override string ToString()
        {
            return name;
        }

        public bool Includes(FlagsBase flags)
        {
            bool includes = (this.InternalValue == (flags.InternalValue & this.InternalValue));
            return includes;
        }

        public static FlagsBase operator |(FlagsBase a, FlagsBase b)
        {
            string name = System.String.Format("{0}|{1}", a.name, b.name);
            FlagsBase orResult = new FlagsBase(name, a.InternalValue | b.InternalValue);
            return orResult;
        }
    }
}
#endif