﻿using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;

namespace knyu.WcfSamples.DynClient.ProxyClient
{
    static class Program
    {
        // persistent proxy object
        private static readonly Proxy.Proxy m_proxy = new Proxy.Proxy("http://localhost:8100/mex", "IDynClientLib");
        
        // persistent dynamic client-side instance of the WCF service contract implementation
        private static dynamic Instance;

        static void Main()
        {
            const string testString = "test string";            
            
            // simple single-threaded demo
            Demo(testString);
        }

        private static void Demo(string testString)
        {
            // Check if we have an instance, create otherwise
            if (Instance == null)
                Instance = m_proxy.CreateNewInstance();

            // Check if update of contract needed. 
            // Please note, that if contract was updated (some methods added, some removed, etc.) the
            // existent code may be not valid!
            if (m_proxy.IsUpdateNeeded())
                Instance = m_proxy.CreateNewInstance();

            // Get response of the server (HashInfo instance)
            var serverHash = Instance.GetHash(testString);
            
            // Server-side hash bytes
            var serverHashBytes = serverHash.Hash;

            // Client-side hash bytes
            var clientHashBytes = GetHash(testString);

            // Check if server returns correct hash algorithm info
            if (serverHash.HashAlgorithm != "MD5")
                throw new InvalidOperationException("Wrong hash algorithm");

            // Dump algorithm info to console
            Console.WriteLine(serverHash.HashAlgorithm);

            // Check if hash valid
            if (CompareHash(serverHashBytes, clientHashBytes) == false)
                throw new InvalidOperationException("Hash does not equal");
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
