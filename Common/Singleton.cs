using System;

namespace ExcelConvertLib
{
    public abstract class Singleton<T> where T : class, new()
    {
        private static T instance = null;

        public Singleton() { }

        public static T Inst
        {
            get
            {
                if (instance == null)
                    instance = new T();
                return instance;
            }
        }
    }


}
