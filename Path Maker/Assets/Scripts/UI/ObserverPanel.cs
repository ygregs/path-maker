namespace PathMaker.UI
{
    public abstract class ObserverPanel<T> : UIPanelBase where T : Observed<T>
    {
        public abstract void ObservedUpdated(T observed);
    }
}