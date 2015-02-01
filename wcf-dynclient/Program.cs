using System;
using System.ServiceModel;
using wcf_dynclient_lib;

namespace wcf_dynclient
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var host = new ServiceHost(typeof(DynClient)))
            {
                host.Open();

                Console.WriteLine("The service is ready.");
                Console.WriteLine("Press <Enter> to stop the service.");
                Console.ReadLine();

                host.Close();
            }
        }
    }
}
