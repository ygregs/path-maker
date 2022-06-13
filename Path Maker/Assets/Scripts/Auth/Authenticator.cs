using System.Collections.Generic;

namespace PathMaker.Auth
{

    public class AuthData : Observed<AuthData>
    {
        protected Dictionary<string, string> m_contents = new Dictionary<string, string>();

        public string GetContent(string key)
        {
            if (!m_contents.ContainsKey(key))
                m_contents.Add(key, null); // Not alerting observers via OnChanged until the value is actually present (especially since this could be called by an observer, which would be cyclical).
            return m_contents[key];
        }

        public void SetContent(string key, string value)
        {
            if (!m_contents.ContainsKey(key))
                m_contents.Add(key, value);
            else
                m_contents[key] = value;
            OnChanged(this);
        }

        public override void CopyObserved(AuthData oldObserved)
        {
            m_contents = oldObserved.m_contents;
        }
    }
    public interface IAuthenticator : IProvidable<IAuthenticator>
    {
        AuthData GetAuthData();
    }

    public class AuthenticatorNoop : IAuthenticator
    {
        public AuthData GetAuthData() { return null; }
        public void OnReProvided(IAuthenticator other) { }
    }

    public class Authenticator : IAuthenticator
    {
        protected AuthData m_authData = new AuthData();

        public AuthData GetAuthData()
        {
            return m_authData;
        }

        public void OnReProvided(IAuthenticator prev)
        {
            if (prev is Authenticator)
            {
                Authenticator prevAuthenticator = prev as Authenticator;
                m_authData = prevAuthenticator.m_authData;
            }
        }
    }
}