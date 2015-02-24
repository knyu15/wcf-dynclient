using System.ServiceModel;

namespace knyu.WcfSamples.DynClient.Lib
{
    [ServiceContract]
    public interface IDynClientLib
    {
        [OperationContract]
        HashInfo GetHash(string value);
    }
}
