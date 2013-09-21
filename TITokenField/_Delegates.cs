using System;

namespace TokenField
{
    public delegate void ProtocolHandler<T,K>(T sender, K token);
    public delegate bool CancellableProtocolHandler<T,K>(T sender, K token);

    public static class ProtocolHandler_Extensions
    {
        public static bool Raise<T,K>(this CancellableProtocolHandler<T,K> handler, T sender, K arg)
        {
            if (handler != null)
            {
                return handler(sender, arg);
            }
            return true;
        }
        public static void Raise<T,K>(this ProtocolHandler<T,K> handler, T sender, K arg)
        {
            if (handler != null)
            {
                handler(sender, arg);
            }
        }
        public static void Raise(this EventHandler handler, object sender, EventArgs arg)
        {
            if (handler != null)
            {
                handler(sender, arg);
            }
        }

    }
}

