using System;

namespace TestCertificateStorageWithRSA {
	class Program {
		static void Main(string[] args) {
			var certificate = CryptoUtils.CreateSelfSignedCertificate("Test Certificate with RSA Private Key");
			Console.WriteLine("This is the object that was stored in the X509 store:");
			Console.WriteLine($"  Name: {certificate.SubjectName.Name}\n  Thumbprint: {certificate.Thumbprint}\n  HasPrivateKey: {certificate.HasPrivateKey}");
			var retrievedCertificate = CryptoUtils.GetCertificate(certificate.Thumbprint);
			Console.WriteLine("This is the object that was retrieved from the same X509 store, it should be equal to the one above:");
			Console.WriteLine($"  Name: {retrievedCertificate.SubjectName.Name}\n  Thumbprint: {retrievedCertificate.Thumbprint}\n  HasPrivateKey: {retrievedCertificate.HasPrivateKey}");
			Console.ReadLine();
		}
	}
}
