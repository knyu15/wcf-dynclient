using System;
using System.Diagnostics;
using System.Dynamic;

namespace knyu.WcfSamples.DynClient.Proxy
{
    /// <summary>
    /// DynamicProxy represents dynamic represenatition of abstract WCF client instance.
    /// It using as a helper abstract representation of dynamically changing open interface
    /// </summary>
    internal class DynamicProxy : DynamicObject
    {
        public DynamicProxy(object wcfObjectInstance)
        {
            if (wcfObjectInstance == null) 
                throw new ArgumentNullException("wcfObjectInstance");

            m_wcfObjectInstance = wcfObjectInstance;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            Debug.Assert(m_wcfObjectInstance != null);
            
            result = null;

            var method = m_wcfObjectInstance.GetType().GetMethod(binder.Name);
            if (method == null)
                return false;

            result = method.Invoke(m_wcfObjectInstance, args);
            return true;
        }

        private readonly object m_wcfObjectInstance;
    }
}