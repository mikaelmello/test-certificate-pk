﻿using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.X509;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace TestCertificateStorageWithRSA {
	public static class CryptoUtils {
		private const string SignatureAlgorithmOid = "1.2.840.113549.1.1.11"; // SHA-256 with RSA
		private const int KeySize = 4096;

		private static readonly HashAlgorithmName SignatureAlgorithmName = HashAlgorithmName.SHA256;
		private static readonly RSASignaturePadding SignaturePadding = RSASignaturePadding.Pkcs1;

		private static StoreName DefaultStoreName = StoreName.My;

		private static StoreLocation DefaultStoreLocation = StoreLocation.CurrentUser;

		/// <summary>
		/// Creates a self-signed X509 certificate and stores it in the specified StoreLocation
		/// </summary>
		public static X509Certificate2 CreateSelfSignedCertificate(string commonName = "localhost") {
			RSA key = RSA.Create(KeySize);
			var cert = IssueSelfSignedCertificate(key, commonName);
			var certWithKey = StoreCertificate(cert, key);
			return certWithKey;
		}

		private static X509Certificate2 IssueSelfSignedCertificate(RSA rsa, string commonName) {

			var publicParams = rsa.ExportParameters(false);
			var signatureAlgIdentifier = new AlgorithmIdentifier(new DerObjectIdentifier(SignatureAlgorithmOid), DerNull.Instance);
			var subjectName = new X509Name($"CN={commonName}", new X509DefaultEntryConverter());

			var certGen = new V3TbsCertificateGenerator();
			certGen.SetIssuer(subjectName);
			certGen.SetSubject(subjectName);
			certGen.SetSerialNumber(new DerInteger(new Org.BouncyCastle.Math.BigInteger(1, Guid.NewGuid().ToByteArray())));
			certGen.SetStartDate(new Time(DateTime.UtcNow));
			certGen.SetEndDate(new Time(DateTime.UtcNow.AddYears(10)));
			certGen.SetSignature(signatureAlgIdentifier);
			certGen.SetSubjectPublicKeyInfo(SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(new RsaKeyParameters(
				false,
				new Org.BouncyCastle.Math.BigInteger(1, publicParams.Modulus),
				new Org.BouncyCastle.Math.BigInteger(1, publicParams.Exponent)
			)));

			var tbsCert = certGen.GenerateTbsCertificate();
			var signature = rsa.SignData(tbsCert.GetDerEncoded(), SignatureAlgorithmName, SignaturePadding);
			var certEncoded = new X509CertificateStructure(tbsCert, signatureAlgIdentifier, new DerBitString(signature)).GetDerEncoded();
			var cert = new X509Certificate2(certEncoded);

			return cert;
		}

		/// <summary>
		/// Associate the key with the certificate.
		/// </summary>
		private static X509Certificate2 StoreCertificate(X509Certificate2 cert, RSA rsa) {

			var certWithKey = cert.CopyWithPrivateKey(rsa);

			// Add the certificate with associated key to the operating system key store

			var store = new X509Store(DefaultStoreName, DefaultStoreLocation, OpenFlags.ReadWrite);
			try {
				store.Add(certWithKey);
			} finally {
				store.Close();
			}

			return certWithKey;
		}

		/// <summary>
		/// Gets certificate with specified certThumbprint from the specified StoreLocation
		/// </summary>
		public static X509Certificate2 GetCertificate(string certThumbprint) {
			X509Certificate2 cert;
			var store = new X509Store(DefaultStoreName, DefaultStoreLocation);
			store.Open(OpenFlags.ReadOnly);
			try {
				var certCollection = store.Certificates.Find(X509FindType.FindByThumbprint, certThumbprint, false);
				if (certCollection.Count == 0) {
					throw new Exception(certThumbprint);
				}
				cert = certCollection[0];
			} finally {
				store.Close();
			}
			return cert;
		}
	}
}
