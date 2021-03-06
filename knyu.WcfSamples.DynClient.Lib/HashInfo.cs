﻿using System.Runtime.Serialization;

namespace knyu.WcfSamples.DynClient.Lib
{
    [DataContract]
    public class HashInfo
    {
        [DataMember]
        public string HashAlgorithm
        {
            get { return m_hashAlgorithm; }
            set { m_hashAlgorithm = value; }
        }

        [DataMember]
        public byte[] Hash
        {
            get { return m_hash; }
            set { m_hash = value; }
        }

        private string m_hashAlgorithm;
        private byte[] m_hash;
    }
}