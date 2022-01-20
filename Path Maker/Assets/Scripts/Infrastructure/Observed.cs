using System;

namespace PathMaker
{
    public abstract class Observed<T>
    {
        public Action<T> onChanged { get; set; }

        protected void OnChanged(T observed)
        {
            onChanged?.Invoke(observed);
        }
    }
}