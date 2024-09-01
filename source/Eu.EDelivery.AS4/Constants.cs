﻿using Org.BouncyCastle.Asn1.Mozilla;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Eu.EDelivery.AS4
{
    /// <summary>
    /// Constants used inside the Core
    /// </summary>
    public static class Constants
    {
        public static class SignAlgorithms
        {
            public const string Sha256 = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";

            public static readonly HashSet<string> SupportedAlgorithms = new HashSet<string> { Sha256 };

            public static bool IsSupported(string algorithm)
            {
                return SupportedAlgorithms.Contains(algorithm);
            }
        }

        public static class HashFunctions
        {
            public const string Sha256 = "http://www.w3.org/2001/04/xmlenc#sha256";
            public const string Sha1 = "http://www.w3.org/2000/09/xmldsig#sha1";

            public static readonly HashSet<string> SupportedAlgorithms = new HashSet<string> { Sha256, Sha1 };

            public static bool IsSupported(string hashFunction)
            {
                return SupportedAlgorithms.Contains(hashFunction);
            }
        }

        /// <summary>
        /// Supported Content Types
        /// </summary>
        public static class ContentTypes
        {
            public const string Soap = "application/soap+xml";
            public const string Mime = "multipart/related";
        }

        /// <summary>
        /// Supported Namespaces
        /// </summary>
        public static class Namespaces
        {
            /// <summary>
            /// EbMS Constants
            /// </summary>
            public const string EbmsXmlCore = "http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/";

            public const string EbmsXmlSignals = "http://docs.oasis-open.org/ebxml-bp/ebbp-signals-2.0";
            public const string EbmsXmlAdvanced = "http://docs.oasis-open.org/ebxml-msg/ns/v3.0/mf/2010/04/";
            public const string EbmsMultiHop = "http://docs.oasis-open.org/ebxml-msg/ns/ebms/v3.0/multihop/200902/";
            public const string EbmsNextMsh = "http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/part2/200811/nextmsh";

            public const string EbmsOneWayReceipt =
                "http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/oneWay.receipt";

            public const string EbmsOneWayError =
                "http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/oneWay.error";

            public const string EbmsSchema =
                "https://docs.oasis-open.org/ebxml-msg/ebms/v3.0/core/ebms-header-3_0-200704.xsd";

            public const string Soap11 = "http://schemas.xmlsoap.org/soap/envelope/";
            public const string Soap12 = "http://www.w3.org/2003/05/soap-envelope";
            public const string Xml = "http://www.w3.org/XML/1998/namespace";
            public const string XmlDsig = "http://www.w3.org/2000/09/xmldsig#";
            public const string XmlEnc11 = "http://www.w3.org/2009/xmlenc11#";
            public const string XmlEnc11Aes128 = "http://www.w3.org/2009/xmlenc11#aes128-gcm";
            public const string XmlEnc = "http://www.w3.org/2001/04/xmlenc#";
            public const string Addressing = "http://www.w3.org/2005/08/addressing";

            public const string EbmsDefaultMpc =
                "http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/defaultMPC";

            public const string ICloud = "http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/part2/200811/icloud";

            /// <summary>
            /// Collaboration Constants
            /// </summary>
            public const string TestService = "http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/service";

            public const string TestAction = "http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/test";

            /// <summary>
            /// Party Constants
            /// </summary>
            public const string EbmsDefaultRole =
                "http://docs.oasis-open.org/ebxml-msg/ebMS/v3.0/ns/core/200704/defaultRole";

            public const string EbmsDefaultTo =
                "http://docs.oasis-open.org/ebxml-msg/ebMS/v3.0/ns/core/200704/defaultTo";

            public const string EbmsDefaultFrom =
                "http://docs.oasis-open.org/ebxml-msg/ebMS/v3.0/ns/core/200704/defaultFrom";


            /// <summary>
            /// Security Constants
            /// </summary>
            public const string WssSecurityUtility =
                "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";

            public const string WssSecuritySecExt =
                "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";

            public const string ValueType =
                "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3";

            public const string Base64Binary =
                "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary";

            public const string SubjectKeyIdentifier =
                "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509SubjectKeyIdentifier";

            // Eu.EDelivery Constants
            public const string ProcessingMode = "eu:edelivery:as4:pmode";
        }
    }
}