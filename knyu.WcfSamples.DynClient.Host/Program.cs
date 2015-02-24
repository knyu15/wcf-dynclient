using System;
using System.ServiceModel;

namespace knyu.WcfSamples.DynClient
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var host = new ServiceHost(typeof(Lib.DynClient)))
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
