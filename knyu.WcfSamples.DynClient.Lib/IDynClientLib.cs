using System.ServiceModel;

namespace wcf_dynclient_lib
{
    [ServiceContract]
    public interface IDynClientLib
    {
        [OperationContract]
        HashInfo GetHash(string value);
    }
}
