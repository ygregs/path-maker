using System;

namespace PathMaker
{
    public abstract class Observed<T>
    {

        // If you want to copy all the value, and only trgger OnChanged one.
        public abstract void CopyObserved(T oldObserved);

        public Action<T> onChanged { get; set; }
        public Action<T> onDestroyed { get; set; }

        protected void OnChanged(T observed)
        {
            onChanged?.Invoke(observed);
        }

        protected void OnDestroyed(T observed)
        {
            onDestroyed?.Invoke(observed);
        }
    }
}