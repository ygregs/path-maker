using System;
using System.Collections.Generic;
using PathMaker.Auth;

namespace PathMaker
{
    public class Locator : LocatorBase
    {
        private static Locator s_instance;

        public static Locator Get
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new Locator();
                }
                return s_instance;
            }
        }

        protected override void OnFinishConstruction()
        {
            s_instance = this;
        }
    }

    public interface IProvidable<T>
    {
        void OnReProvided(T previousProvider);
    }

    public class LocatorBase
    {
        private Dictionary<Type, object> m_provided = new Dictionary<Type, object>();

        public LocatorBase()
        {
            Provide(new Messenger());
            Provide(new UpdateSlowNoop());
            Provide(new IdentityNoop());
            Provide(new AuthenticatorNoop());

            OnFinishConstruction();
        }

        protected virtual void OnFinishConstruction() { }

        private void ProvideAny<T>(T instance) where T : IProvidable<T>
        {
            Type type = typeof(T);
            if (m_provided.ContainsKey(type))
            {
                var previousProvider = (T)m_provided[type]; // Ici, on caste m_provided[type] -> le (T) renvoie une erreure si m_provided[type] n'est pas du type, ou derive de T.
                instance.OnReProvided(previousProvider);
                m_provided.Remove(type);
            }

            m_provided.Add(type, instance);
        }

        private T Locate<T>() where T : class
        {
            Type type = typeof(T);
            if (!m_provided.ContainsKey(type))
            {
                return null;
            }
            return m_provided[type] as T; // Ici le 'as' renvoie null si m_provider[type] n'est pas du type de T.
        }

        public IMessenger Messenger => Locate<IMessenger>();
        public void Provide(IMessenger messenger) { ProvideAny<IMessenger>(messenger); }

        public IUpdateSlow UpdateSlow => Locate<IUpdateSlow>();
        public void Provide(IUpdateSlow updateSlow) { ProvideAny(updateSlow); }

        public IIdentity Identity => Locate<IIdentity>();
        public void Provide(IIdentity identity) { ProvideAny(identity); }

        public IAuthenticator Authenticator => Locate<IAuthenticator>();
        public void Provide(IAuthenticator authenticator) { ProvideAny(authenticator); }
    }
}