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
            
            X509Certificate2? validCertToReturn = null;
            bool needsNewCert = true;

            if (certs.Count > 0)
            {
                // Limpiar la tienda: eliminar certificados que ya expiraron o expiran en menos de 30 días.
                foreach (var cert in certs)
                {
                    // Si expira pronto o ya expiró, lo borramos de la tienda.
                    if (DateTime.Now.AddDays(30) > cert.NotAfter)
                    {
                        Console.WriteLine($"[DEBUG] CertManager: Eliminando certificado antiguo/próximo a expirar ({cert.NotAfter}).");
                        store.Remove(cert);
                    }
                    else if (needsNewCert)
                    {
                        // Si encontramos uno válido (más de 30 días de vida restante), lo usaremos.
                        validCertToReturn = cert;
                        needsNewCert = false;
                    }
                    else
                    {
                        // Si ya tenemos uno válido para usar, pero hay otros válidos extra, limpiamos los extras
                        // para evitar acumulación innecesaria en la bóveda.
                        Console.WriteLine("[DEBUG] CertManager: Eliminando certificado duplicado.");
                        store.Remove(cert);
                    }
                }
            }

            if (!needsNewCert && validCertToReturn != null)
            {
                Console.WriteLine("[DEBUG] CertManager: Certificado válido encontrado y limpiado el resto.");
                return validCertToReturn;
            }

            Console.WriteLine("[DEBUG] CertManager: Generando nuevo certificado (Validez 397 días max)...");
            using var rsa = RSA.Create(2048);
            var request = new CertificateRequest("CN=EPOS-Bridge-Localhost", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
            request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, false));
            request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, false)); // Server Auth
            
            var sanBuilder = new SubjectAlternativeNameBuilder();
            sanBuilder.AddDnsName("localhost");
            request.CertificateExtensions.Add(sanBuilder.Build());

            // Browsers limit self-signed certs to 398 days max. We set it to 397 days.
            var certCreated = request.CreateSelfSigned(DateTimeOffset.Now.AddMinutes(-5), DateTimeOffset.Now.AddDays(397));
            
            // Export/Import to persist private key and friendly name
            var pfx = certCreated.Export(X509ContentType.Pfx, "");
            var persistentCert = new X509Certificate2(pfx, "", X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.UserKeySet);
            persistentCert.FriendlyName = "EPOS Bridge Localhost";

            store.Add(persistentCert);

            // Install in Trusted Root Certification Authorities to prevent browser warnings
            try
            {
                using var rootStore = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                rootStore.Open(OpenFlags.ReadWrite);

                // Clean old root certificates
                var oldRootCerts = rootStore.Certificates.Find(X509FindType.FindBySubjectDistinguishedName, "CN=EPOS-Bridge-Localhost", false);
                foreach (var oldCert in oldRootCerts)
                {
                    rootStore.Remove(oldCert);
                }

                // Add only the public key part to Trusted Root
                var publicCert = new X509Certificate2(certCreated.Export(X509ContentType.Cert));
                publicCert.FriendlyName = "EPOS Bridge Localhost";
                rootStore.Add(publicCert);
                Console.WriteLine("[DEBUG] CertManager: Certificado instalado en Entidades de Certificación Raíz de Confianza.");
            }
            catch (Exception rootEx)
            {
                Console.WriteLine($"[WARNING] No se pudo instalar certificado en Root (Falta Administrador o UAC declinado): {rootEx.Message}");
            }

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
