﻿using System;
using System.ComponentModel;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using NLog;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Security.Signing;
using Eu.EDelivery.AS4.Services.Journal;

using Abr.SecurityTokenManager;
using System.Globalization;
using Abr.AuskeyManager.KeyStore;
using System.Xml;


namespace Eu.EDelivery.AS4.Steps.Send
{
    /// <summary>
    /// Describes how the MSH signs the AS4 UserMessage
    /// </summary>
    [Info("Sign the AS4 Message if necessary")]
    [Description("This step signs the AS4 Message if signing is enabled in the Sending PMode")]
    public class SignAS4MessageStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly ICertificateRepository _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SignAS4MessageStep"/> class
        /// </summary>
        public SignAS4MessageStep() : this(Registry.Instance.CertificateRepository) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SignAS4MessageStep"/> class. 
        /// Create Signing Step with a given Certificate Store Repository
        /// </summary>
        /// <param name="repository">
        /// </param>
        public SignAS4MessageStep(ICertificateRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            _repository = repository;
        }

        /// <summary>
        /// Sign the <see cref="AS4Message" />
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            if (messagingContext == null)
            {
                throw new ArgumentNullException(nameof(messagingContext));
            }

            if (messagingContext.AS4Message == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(SignAS4MessageStep)} requires an AS4Message to sign but no AS4Message is present in the MessagingContext");
            }

            if (messagingContext.AS4Message.IsEmpty)
            {
                Logger.Debug("No signing will be performed on the message because it's empty");
                return await StepResult.SuccessAsync(messagingContext);
            }

            Signing signInfo = RetrieveSigningInformation(
                messagingContext.AS4Message,
                messagingContext.SendingPMode,
                messagingContext.ReceivingPMode);

            if (signInfo == null)
            {
                Logger.Trace("No signing will be performend on the message because no signing information was found in either Sending or Receiving PMode");
                return await StepResult.SuccessAsync(messagingContext);
            }

            if (signInfo.IsEnabled == false)
            {
                Logger.Trace("No signing will be performend on the message because the PMode siging information is disabled");
                return await StepResult.SuccessAsync(messagingContext);
            }

            Logger.Info($"(Outbound)[{messagingContext.AS4Message.GetPrimaryMessageId()}] Sign AS4Message with given signing information of the PMode");

            X509Certificate2 certificate = RetrieveCertificate(signInfo);
            var settings =
                new CalculateSignatureConfig(
                    signingCertificate: certificate,
                    referenceTokenType: signInfo.KeyReferenceMethod,
                    signingAlgorithm: signInfo.Algorithm,
                    hashFunction: signInfo.HashFunction);

            var securityToken = GetSecurityToken(certificate);
            messagingContext.AS4Message.SecurityHeader = CreateSecurityHeader(securityToken);

            SignAS4Message(settings, messagingContext.AS4Message);

            JournalLogEntry logEntry = JournalLogEntry.CreateFrom(
                messagingContext.AS4Message,
                $"Signed with certificate {settings.SigningCertificate.FriendlyName} and reference {settings.ReferenceTokenType} "
                + $"using algorithm {settings.SigningAlgorithm} and hash {settings.HashFunction}");

            return await StepResult
                .Success(messagingContext)
                .WithJournalAsync(logEntry);
        }

        
        private SecurityToken GetSecurityToken(X509Certificate2 certificate)
        {
            // ATO lib required this
            AbrProperties.SetSoftwareInfo(
                organisation: "Cashbook Genie Pty Ltd",
                product: "Cyruspay",
                version: "1",
                timestamp: DateTime.Now.ToString(CultureInfo.InvariantCulture));

            // "https://test.sbr.gov.au/services"
            var appliesTo = System.Configuration.ConfigurationManager.AppSettings.Get("abr.SecurityTokenManager.STS.appliesTo");
            Logger.Trace($"SecurityTokenManager appliesTo: {appliesTo}");

            var stm = new SecurityTokenManager();
            return stm.GetSecurityIdentityToken(appliesTo, "", certificate);
        }
        
        private SecurityHeader CreateSecurityHeader(SecurityToken securityToken)
        {
            var security =
            $"<wsse:Security xmlns:wsse=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\">{securityToken.AssertionAsString}</wsse:Security>";

            security = AddAttributesToEncryptedAssertion(security);

            var d = new XmlDocument();
            d.LoadXml(security);

            return new SecurityHeader(d.DocumentElement);
        }

        private const string WSU_NS = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
        private const string WSU_ID_ATTRIBUTE_NAME = "Id";

        private XmlNamespaceManager GetNamespaceManager()
        {
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("soapenv", "http://schemas.xmlsoap.org/soap/envelope/");
            namespaceManager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
            namespaceManager.AddNamespace("xenc", "http://www.w3.org/2001/04/xmlenc#");
            namespaceManager.AddNamespace("wsu", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
            namespaceManager.AddNamespace("s12", "http://www.w3.org/2003/05/soap-envelope");
            namespaceManager.AddNamespace("wsse", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");

            return namespaceManager;
        }

        private string AddAttributesToEncryptedAssertion(string xmlString)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);

            XmlNode securityNode = xmlDoc.SelectSingleNode("//wsse:Security", GetNamespaceManager());
            var mustUnderstand = xmlDoc.CreateAttribute("s12", "mustUnderstand", "http://www.w3.org/2003/05/soap-envelope");
            mustUnderstand.Value = "true";
            securityNode.Attributes.Append(mustUnderstand);

            // Get the saml:EncryptedAssertion node
            XmlNode encryptedAssertionNode = xmlDoc.SelectSingleNode("//saml:EncryptedAssertion", GetNamespaceManager());

            // Add the xmlns:wsu attribute
            XmlAttribute wsuAttribute = xmlDoc.CreateAttribute("xmlns:wsu");
            wsuAttribute.Value = WSU_NS;
            encryptedAssertionNode.Attributes.Append(wsuAttribute);

            // Add the wsu:Id attribute
            XmlAttribute wsuIdAttribute = xmlDoc.CreateAttribute(WSU_ID_ATTRIBUTE_NAME, WSU_NS);
            wsuIdAttribute.Value = "soapheader-2";
            encryptedAssertionNode.Attributes.Append(wsuIdAttribute);

            return xmlDoc.OuterXml;
        }

        private static Signing RetrieveSigningInformation(
            AS4Message message,
            SendingProcessingMode sendingPMode,
            ReceivingProcessingMode receivingPMode)
        {
            if (message.IsUserMessage || message.IsPullRequest)
            {
                if (sendingPMode == null)
                {
                    throw new InvalidOperationException(
                        $"{nameof(SignAS4MessageStep)} requires a SendingPMode when the primary message unit of the AS4Message is either an UserMessage or a PullRequest");
                }

                Logger.Trace($"Use SendingPMode {sendingPMode.Id} for signing because the primary message unit is a UserMessage or a PullRequest");
                return sendingPMode.Security?.Signing;
            }

            if (sendingPMode != null)
            {
                // When signal messages are forwarded, we have a sending pmode instead of a receiving pmode.
                return sendingPMode?.Security?.Signing;
            }

            if (message.PrimaryMessageUnit is Receipt)
            {
                if (receivingPMode == null)
                {
                    throw new InvalidOperationException(
                    $"{nameof(SignAS4MessageStep)} requires a ReceivingPMode when the primary message unit of the AS4Message is a Receipt");

                }

                Logger.Trace($"Use ReceivingPMode {receivingPMode.Id} for signing because the primary message unit of the AS4Message is a Receipt");
                return receivingPMode.ReplyHandling?.ResponseSigning;
            }

            if (message.PrimaryMessageUnit is Error)
            {
                if (receivingPMode == null)
                {
                    // When the error occured before there was a ReceivingPMode determined, we can't retrieve any signing information.
                    Logger.Trace("No ReceivingPMode was found for signing the AS4Message with an Error as primary message unit");
                    return null;
                }

                Logger.Trace($"Use ReceivingPMode {receivingPMode.Id} for signing because the primary message unit of the AS4Message is an Error");
                return receivingPMode.ReplyHandling?.ResponseSigning;
            }

            throw new InvalidOperationException(
                "No signing information can be retrieved from both Sending and Receiving PMode based on the message type");
        }

        private static void SignAS4Message(CalculateSignatureConfig settings, AS4Message message)
        {
            try
            {
                message.Sign(settings);
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
                if (exception.InnerException != null)
                {
                    Logger.Error(exception.InnerException.Message);
                }
                Logger.Trace(exception.StackTrace);
                throw;
            }
        }

        private X509Certificate2 RetrieveCertificate(Signing signInfo)
        {
            if (signInfo.SigningCertificateInformation == null)
            {
                throw new ConfigurationErrorsException(
                    "No signing certificate information found in PMode to perform signing. "
                    + "Please provide either a <CertificateFindCriteria/> or <PrivateKeyCertificate/> tag to the Security.Signing element");
            }

            if (signInfo.SigningCertificateInformation is CertificateFindCriteria certFindCriteria)
            {
                return _repository.GetCertificate(
                    findType: certFindCriteria.CertificateFindType,
                    privateKeyReference: certFindCriteria.CertificateFindValue);
            }

            if (signInfo.SigningCertificateInformation is PrivateKeyCertificate embeddedCertInfo)
            {
                return new X509Certificate2(
                    rawData: Convert.FromBase64String(embeddedCertInfo.Certificate),
                    password: embeddedCertInfo.Password,
                    keyStorageFlags:
                        X509KeyStorageFlags.Exportable
                        | X509KeyStorageFlags.MachineKeySet
                        | X509KeyStorageFlags.PersistKeySet);
            }

            throw new NotSupportedException(
                "The signing certificate information specified in the PMode could not be used to retrieve the certificate. " +
                "Please provide either a <CertificateFindCriteria/> or <PrivateKeyCertificate/> tag to the Security.Signing element");
        }
    }
}