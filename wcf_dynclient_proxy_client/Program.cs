using System;
using System.Collections;
using System.Dynamic;
using System.Security.Cryptography;
using System.Text;
using wcf_dynclient_proxy;

namespace wcf_dynclient_proxy_client
{
    class Program
    {
        private static readonly Proxy Proxy = new Proxy("http://localhost:8100/mex", "IDynClientLib");
        private static dynamic Instance;

        static void Main(string[] args)
        {
            const string testString = "test string";            
            Demo(testString);
        }

        private static void Demo(string testString)
        {
            if (Instance == null)
                Instance = Proxy.CreateNewInstance();

            if (Proxy.IsUpdateNeeded())
                Instance = Proxy.CreateNewInstance();

            var serverHash = Instance.GetHash(testString);
            var serverHashBytes = serverHash.Hash;
            var clientHashBytes = GetHash(testString);

            if (serverHash.HashAlgorithm != "MD5")
                throw new InvalidOperationException("Wrong hash algorithm");

            Console.WriteLine(serverHash.HashAlgorithm);

            if (CompareHash(serverHashBytes, clientHashBytes) == false)
                throw new InvalidOperationException("Hash does not equals");
            else
                Console.WriteLine("Hash equals!");
        }

        static byte[] GetHash(string value)
        {
            using (MD5 hashAlgorithm = new MD5Cng())
            {
                return hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(value));
            }
        }

        static bool CompareHash(byte[] hash1, byte[] hash2)
        {
            return StructuralComparisons.StructuralEqualityComparer.Equals(hash1, hash2);
        }
    }
}
