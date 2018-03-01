// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.X509;

namespace Test.Utility.Signing
{
    /// <summary>
    /// Test certificate pair.
    /// </summary>
    public class TestCertificate
    {
        private readonly X509Certificate2 _certificate;

        /// <summary>
        /// Public cert.
        /// </summary>
        public X509Certificate2 PublicCert => SigningTestUtility.GetPublicCert(_certificate);

        /// <summary>
        /// Public cert.
        /// </summary>
        public X509Certificate2 PublicCertWithPrivateKey => SigningTestUtility.GetPublicCertWithPrivateKey(_certificate);

        /// <summary>
        /// Certificate Revocation List associated with a certificate.
        /// This will be null if the certificate was not created as a CA certificate.
        /// </summary>
        public CertificateRevocationList Crl { get; set; }

        public string Fingerprint => _certificate.Thumbprint;

        public TestCertificate(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException(nameof(certificate));
            }

            _certificate = certificate;
        }

        public X509Certificate2 GetCertificate()
        {
            return new X509Certificate2(_certificate.RawData);
        }

        public byte[] ExportCertificate(X509ContentType contentType, string password)
        {
            return _certificate.Export(contentType, password);
        }

        /// <summary>
        /// Trust the PublicCert cert for the life of the object.
        /// </summary>
        /// <remarks>Dispose of the object returned!</remarks>
        public StoreCertificate<TestCertificate> WithTrust(StoreName storeName = StoreName.TrustedPeople, StoreLocation storeLocation = StoreLocation.CurrentUser)
        {
            return new StoreCertificate<TestCertificate>(this, e => PublicCert, storeName, storeLocation);
        }

        /// <summary>
        /// Trust the PublicCert cert for the life of the object.
        /// </summary>
        /// <remarks>Dispose of the object returned!</remarks>
        public StoreCertificate<TestCertificate> WithPrivateKeyAndTrust(StoreName storeName = StoreName.TrustedPeople, StoreLocation storeLocation = StoreLocation.CurrentUser)
        {
            return new StoreCertificate<TestCertificate>(this, e => PublicCertWithPrivateKey, storeName, storeLocation);
        }

        public static string GenerateCertificateName()
        {
            return "NuGetTest-" + Guid.NewGuid().ToString();
        }

        public static TestCertificate Generate(Action<X509V3CertificateGenerator> modifyGenerator = null, ChainCertificateRequest chainCertificateRequest = null)
        {
            var certName = GenerateCertificateName();
            var cert = SigningTestUtility.GenerateCertificate(certName, modifyGenerator, chainCertificateRequest: chainCertificateRequest);
            CertificateRevocationList crl = null;

            // create a crl only if the certificate is part of a chain and it is a CA
            if (chainCertificateRequest != null && chainCertificateRequest.IsCA)
            {
                crl = CertificateRevocationList.CreateCrl(cert, chainCertificateRequest.CrlLocalBaseUri);
            }

            var testCertificate = new TestCertificate(cert)
            {
                Crl = crl
            };

            return testCertificate;
        }
    }
}
