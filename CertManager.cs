using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace EposBridge;

public static class CertManager
{
    public static X509Certificate2 GetOrGenerateCertificate()
    {
        try 
        {
            Console.WriteLine("[DEBUG] CertManager: Abriendo Store...");
            using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);

            Console.WriteLine("[DEBUG] CertManager: Buscando certificado existente...");
            var certs = store.Certificates.Find(X509FindType.FindBySubjectDistinguishedName, "CN=EPOS-Bridge-Localhost", false);
            if (certs.Count > 0)
            {
                var validCert = certs.Cast<X509Certificate2>().FirstOrDefault(c => DateTime.Now < c.NotAfter);
                if (validCert != null) 
                {
                    Console.WriteLine("[DEBUG] CertManager: Certificado válido encontrado.");
                    return validCert;
                }
            }

            Console.WriteLine("[DEBUG] CertManager: Generando nuevo certificado...");
            using var rsa = RSA.Create(2048);
            var request = new CertificateRequest("CN=EPOS-Bridge-Localhost", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
            request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, false));
            request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, false)); // Server Auth
            
            var sanBuilder = new SubjectAlternativeNameBuilder();
            sanBuilder.AddDnsName("localhost");
            request.CertificateExtensions.Add(sanBuilder.Build());

            var cert = request.CreateSelfSigned(DateTimeOffset.Now.AddMinutes(-5), DateTimeOffset.Now.AddYears(5));
            
            // Export/Import to persist private key and friendly name
            var pfx = cert.Export(X509ContentType.Pfx, "");
            var persistentCert = new X509Certificate2(pfx, "", X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.UserKeySet);
            persistentCert.FriendlyName = "EPOS Bridge Localhost";

            store.Add(persistentCert);
            return persistentCert;
        }
        catch (Exception ex)
        {
            // Fallback for dev if store fails - though in production this should be handled
            Console.WriteLine("Certificate Error: " + ex.Message);
            throw;
        }
    }
}
