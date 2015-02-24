using System;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;

namespace knyu.WcfSamples.DynClient.Lib
{
    public class DynClient : IDynClientLib
    {
        public HashInfo GetHash(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new FaultException<ArgumentException>(new ArgumentException("value"),
                    new FaultReason("Value should not be empty"), new FaultCode("Sender"));

            try
            {
                using (MD5 hashAlgorithm = new MD5Cng())
                {
                    byte[] hash = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(value));
                    return new HashInfo
                    {
                        HashAlgorithm = "MD5",
                        Hash = hash
                    };
                }
            }
            catch (InvalidOperationException invalidOperation)
            {
                throw new FaultException<InvalidOperationException>(invalidOperation,
                    new FaultReason(invalidOperation.Message), new FaultCode("Server"));
            }
            catch (ArgumentNullException argumentNull)
            {
                throw new FaultException<ArgumentNullException>(argumentNull,
                    new FaultReason(argumentNull.Message), new FaultCode("Server"));
            }
            catch (Exception exception)
            {
                throw new FaultException<Exception>(exception,
                    new FaultReason(exception.Message), new FaultCode("Server"));                
            }
        }
    }
}