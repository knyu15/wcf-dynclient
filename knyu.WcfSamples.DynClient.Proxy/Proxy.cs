using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Xml;

namespace knyu.WcfSamples.DynClient.Proxy
{
    /// <summary>
    /// This class represents a proxy that allows a client to store a reference to WCF service client.
    /// This proxy dynamic instance can update itself dynamically when WCF service interface changed
    /// without recompiling the project.
    /// </summary>
    public class Proxy
    {
        /// <summary>
        /// Creates new instance of Proxy
        /// </summary>
        /// <param name="mexAddress">Metadata echange endpoint address</param>
        /// <param name="contractName">Contract name</param>
        public Proxy(string mexAddress, string contractName)
        {
            if (string.IsNullOrWhiteSpace(mexAddress))
                throw new ArgumentException("mexAddress");

            if (contractName == null)
                throw new ArgumentNullException("contractName");

            m_mexAddress = mexAddress;
            m_contractName = contractName;
        }

        /// <summary>
        /// Creates dynamic variable implemented current WCF service public interface
        /// </summary>
        /// <returns>Dynamic instance of WCF service client</returns>
        public dynamic CreateNewInstance()
        {
            Debug.Assert(string.IsNullOrWhiteSpace(m_contractName) == false);

            Update();
            return Compile();
        }

        /// <summary>
        /// Checks if update of dynamic WCF client instance needed
        /// </summary>
        /// <returns>True if update needed, false otherwise</returns>
        public bool IsUpdateNeeded()
        {
            if (m_metadataSet == null)
                return true;

            var newServerMetadata = GetMetadata();

            var newMetadataString = SerializeMetadataSetToString(newServerMetadata);
            var currentMetadataString = SerializeMetadataSetToString(m_metadataSet);

            return newMetadataString == currentMetadataString;
        }

        private string SerializeMetadataSetToString(MetadataSet metadataSet)
        {
            if (metadataSet == null)
                throw new ArgumentNullException("metadataSet");

            var stringBuilder = new StringBuilder();
            using (var textWriter = new StringWriter(stringBuilder))
            {
                using (var writer = new XmlTextWriter(textWriter))
                {
                    metadataSet.WriteTo(writer);
                }
            }

            return stringBuilder.ToString();
        }

        private MetadataSet GetMetadata()
        {
            Debug.Assert(string.IsNullOrWhiteSpace(m_mexAddress) == false);

            var mexUri = new Uri(m_mexAddress);
            var mexClient = new MetadataExchangeClient(mexUri, MetadataExchangeClientMode.MetadataExchange)
            {
                ResolveMetadataReferences = true
            };

            return mexClient.GetMetadata();
        }

        private void UpdateMetadata(MetadataSet metadataSet)
        {
            if (metadataSet == null)
                throw new ArgumentNullException("metadataSet");

            MetadataImporter metadataImporter = new WsdlImporter(metadataSet);
            m_contractDescriptions = metadataImporter.ImportAllContracts();
            m_serviceEndpoints = metadataImporter.ImportAllEndpoints();
        }

        private CompilerResults CompileMetadata()
        {
            Debug.Assert(string.IsNullOrWhiteSpace(m_contractName) == false);
            Debug.Assert(m_contractDescriptions != null);
            Debug.Assert(m_serviceEndpoints != null);

            var generator = new ServiceContractGenerator();

            m_serviceContractEndpoints.Clear();
            foreach (var contract in m_contractDescriptions)
            {
                generator.GenerateServiceContractType(contract);
                m_serviceContractEndpoints[contract.Name] = m_serviceEndpoints.Where(
                    se => se.Contract.Name == contract.Name).ToList();
            }

            if (generator.Errors.Count != 0)
                throw new InvalidOperationException("Compilation errors");

            var codeDomProvider = CodeDomProvider.CreateProvider("C#");
            var compilerParameters = new CompilerParameters(
                new[]
                {
                    "System.dll", "System.ServiceModel.dll",
                    "System.Runtime.Serialization.dll"
                }) {GenerateInMemory = true};


            var compilerResults = codeDomProvider.CompileAssemblyFromDom(compilerParameters,
                generator.TargetCompileUnit);

            if (compilerResults.Errors.Count > 0)
                throw new InvalidOperationException("Compilation errors");

            return compilerResults;
        }

        private ServiceEndpoint GetServiceEndpoint()
        {
            Debug.Assert(string.IsNullOrWhiteSpace(m_contractName) == false);

            return m_serviceContractEndpoints[m_contractName].FirstOrDefault();
        }

        private void Update()
        {
            Debug.Assert(string.IsNullOrWhiteSpace(m_contractName) == false);

            m_metadataSet = GetMetadata();
            UpdateMetadata(m_metadataSet);
        }

        private dynamic Compile()
        {
            var compilerResults = CompileMetadata();

            var wcfObjectInstance = CreateInstance(compilerResults);
            dynamic instance = new DynamicProxy(wcfObjectInstance);
            return instance;
        }

        private object CreateInstance(CompilerResults compilerResults)
        {
            ServiceEndpoint serviceEndpoint = GetServiceEndpoint();
            if (serviceEndpoint == null)
                throw new InvalidOperationException("ServiceEndpoint is not initialized");

            var clientProxyType = compilerResults.CompiledAssembly.GetTypes().First(
                t => t.IsClass &&
                     t.GetInterface(m_contractName) != null &&
                     t.GetInterface(typeof (ICommunicationObject).Name) != null);

            var instance = compilerResults.CompiledAssembly.CreateInstance(
                clientProxyType.Name,
                false,
                BindingFlags.CreateInstance,
                null,
                new object[] {serviceEndpoint.Binding, serviceEndpoint.Address},
                CultureInfo.CurrentCulture, null);

            return instance;
        }

        private readonly string m_mexAddress;
        private readonly string m_contractName;

        private MetadataSet m_metadataSet;
        private ServiceEndpointCollection m_serviceEndpoints;
        private Collection<ContractDescription> m_contractDescriptions;

        private readonly Dictionary<string, IEnumerable<ServiceEndpoint>> m_serviceContractEndpoints =
            new Dictionary<string, IEnumerable<ServiceEndpoint>>();
    }
}