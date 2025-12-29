using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Common.Logging;
using Grpc.Core;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.IO.Pem;
using Org.BouncyCastle.X509;

namespace Warpinator
{
    class Authenticator
    {
        static readonly ILog log = Program.Log.GetLogger("Authenticator");

        public static string GroupCode = DefaultGroupCode;
        const string DefaultGroupCode = "Warpinator";
        const string CertificateFileName = ".self.pem";
        const string KeyFileName = ".self.pem_key";
        
        static byte[] serverCertificate;
        static Dictionary<string, string> remoteCertificates = new Dictionary<string, string>();

        public static byte[] GetBoxedCertificate()
        {
            if (serverCertificate == null)
                throw new InvalidOperationException("GetKeyCertificatPair must be called for certificate to be initialized");
            
            byte[] codeBytes = Encoding.UTF8.GetBytes(GroupCode);
            byte[] key;
            using (var sha = SHA256.Create())
                key = sha.ComputeHash(codeBytes);
            var box = new NaCl.XSalsa20Poly1305(key); //SecretBox

            byte[] nonce = new byte[NaCl.XSalsa20Poly1305.NonceLength];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(nonce);

            var data = File.ReadAllBytes(Path.Combine(Utils.GetCertDir(), ".self.pem"));
            byte[] result = new byte[nonce.Length + data.Length + NaCl.XSalsa20Poly1305.TagLength];
            box.Encrypt(result, nonce.Length, data, 0, data.Length, nonce, 0);

            nonce.CopyTo(result, 0);
            return result;
        }

        public static bool SaveRemoteCertificate(byte[] data, string name)
        {
            byte[] codeBytes = Encoding.UTF8.GetBytes(GroupCode);
            byte[] key;
            using (var sha = SHA256.Create())
                key = sha.ComputeHash(codeBytes);
            var box = new NaCl.XSalsa20Poly1305(key); //SecretBox

            byte[] message = new byte[data.Length - 24 - NaCl.XSalsa20Poly1305.TagLength];
            if (!box.TryDecrypt(message, 0, data, 24, data.Length - 24, data, 0))
                return false;

            string pemString = Encoding.ASCII.GetString(message);
            remoteCertificates[name] = pemString;
            return true;
        }

        public static string GetRemoteCertificate(string name)
        {
            return remoteCertificates[name];
        }

        public static KeyCertificatePair GetKeyCertificatePair()
        {
            if (File.Exists(Path.Combine(Utils.GetCertDir(), CertificateFileName)))
            {
                var pair1 = LoadKeyCertificatePair();
                var cert = new X509CertificateParser().ReadCertificate(serverCertificate);
                try
                {
                    var l = cert.GetSubjectAlternativeNames();
                    var ipStr = (string)(l[0])[1];
                    var addr = IPAddress.Parse(ipStr);
                    
                    cert.CheckValidity(); //Throws if invalid
                    if (!addr.Equals(Server.current.SelectedIP))
                        throw new Org.BouncyCastle.Security.Certificates.CertificateExpiredException("IP address changed");
                    return pair1;
                } catch (Exception e) {
                    log.Warn($"Certificate was not loaded, new one will be created ({e.Message})");
                }
            }

            KeyCertificatePair pair = CreateKeyCertificatePair(Utils.GetHostname(), Server.current.SelectedIP.ToString());
            Directory.CreateDirectory(Utils.GetCertDir());
            File.WriteAllText(Path.Combine(Utils.GetCertDir(), CertificateFileName), pair.CertificateChain);
            File.WriteAllText(Path.Combine(Utils.GetCertDir(), KeyFileName), pair.PrivateKey);
            return pair;
        }

        private static KeyCertificatePair CreateKeyCertificatePair(string subjectName, string ip)
        {
            log.Debug($"New cert for IP {ip}");
            var randomGenerator = new CryptoApiRandomGenerator();
            var random = new SecureRandom(randomGenerator);

            var certificateGenerator = new X509V3CertificateGenerator();

            var serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(Int64.MaxValue), random);
            certificateGenerator.SetSerialNumber(serialNumber);

            var subjectDN = new X509Name("CN="+subjectName);
            var issuerDN = subjectDN;
            certificateGenerator.SetIssuerDN(issuerDN);
            certificateGenerator.SetSubjectDN(subjectDN);

            var notBefore = DateTime.UtcNow.Date;
            var notAfter = notBefore.AddDays(30);

            certificateGenerator.SetNotBefore(notBefore);
            certificateGenerator.SetNotAfter(notAfter);
            certificateGenerator.AddExtension(X509Extensions.SubjectAlternativeName, true, new GeneralNames(new GeneralName(GeneralName.IPAddress, ip)));

            const int strength = 2048;
            var keyGenerationParameters = new KeyGenerationParameters(random, strength);

            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);
            var keyPair = keyPairGenerator.GenerateKeyPair();

            const string signatureAlgorithm = "SHA256WithRSA";
            certificateGenerator.SetPublicKey(keyPair.Public);
            var certificate = certificateGenerator.Generate(new Asn1SignatureFactory(signatureAlgorithm, keyPair.Private, random));

            StringWriter certString = new StringWriter();
            PemWriter certWriter = new PemWriter(certString);
            serverCertificate = certificate.GetEncoded();
            certWriter.WriteObject(new PemObject("CERTIFICATE", serverCertificate));

            StringWriter keyString = new StringWriter();
            PemWriter keyWriter = new PemWriter(keyString);
            byte[] keyData = Org.BouncyCastle.Pkcs.PrivateKeyInfoFactory.CreatePrivateKeyInfo(keyPair.Private).ToAsn1Object().GetEncoded();
            keyWriter.WriteObject(new PemObject("PRIVATE KEY", keyData));

            return new KeyCertificatePair(certString.ToString(), keyString.ToString());
        }

        private static KeyCertificatePair LoadKeyCertificatePair()
        {
            string certDir = Utils.GetCertDir();
            string certPath = Path.Combine(certDir, CertificateFileName);
            string keyPath = Path.Combine(certDir, KeyFileName);
            string certString = File.ReadAllText(certPath);
            using (StringReader sr = new StringReader(certString))
            {
                PemReader pr = new PemReader(sr);
                serverCertificate = pr.ReadPemObject().Content;
            }
            return new KeyCertificatePair(certString, File.ReadAllText(keyPath));
        }
    }
}
