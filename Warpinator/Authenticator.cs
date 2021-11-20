using System;
using System.Collections;
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
        static readonly ILog log = LogManager.GetLogger<Authenticator>();

        public static string GroupCode = DefaultGroupCode;
        const string DefaultGroupCode = "Warpinator";
        const string CertificateFileName = ".self.pem";
        const string KeyFileName = ".self.pem_key";
        
        static byte[] serverCertificate;

        // https://stackoverflow.com/a/13806300
        /*public static KeyCertificatePair CreateSelfSignedCertificate(string subjectName)
        {
            // create DN for subject and issuer
            var dn = new CX500DistinguishedName();
            dn.Encode("CN=" + subjectName, X500NameFlags.XCN_CERT_NAME_STR_NONE);

            // create a new private key for the certificate
            CX509PrivateKey privateKey = new CX509PrivateKey();
            privateKey.ProviderName = "Microsoft Base Cryptographic Provider v1.0";
            privateKey.MachineContext = true;
            privateKey.Length = 2048;
            privateKey.KeySpec = X509KeySpec.XCN_AT_SIGNATURE; // use is not limited
            privateKey.ExportPolicy = X509PrivateKeyExportFlags.XCN_NCRYPT_ALLOW_PLAINTEXT_EXPORT_FLAG;
            privateKey.Create();

            string pkeyStr = "-----BEGIN RSA PRIVATE KEY-----\n" + privateKey.Export("PRIVATEBLOB", EncodingType.XCN_CRYPT_STRING_BASE64) + "-----END RSA PRIVATE KEY-----";
            Console.WriteLine("Key: " + pkeyStr);

            // Use the stronger SHA512 hashing algorithm
            var hashobj = new CObjectId();
            hashobj.InitializeFromAlgorithmName(ObjectIdGroupId.XCN_CRYPT_HASH_ALG_OID_GROUP_ID,
                ObjectIdPublicKeyFlags.XCN_CRYPT_OID_INFO_PUBKEY_ANY,
                AlgorithmFlags.AlgorithmFlagsNone, "SHA512");

            // add extended key usage if you want - look at MSDN for a list of possible OIDs
            var oid = new CObjectId();
            oid.InitializeFromValue("1.3.6.1.5.5.7.3.1"); // SSL server
            var oidlist = new CObjectIds();
            oidlist.Add(oid);
            var eku = new CX509ExtensionEnhancedKeyUsage();
            eku.InitializeEncode(oidlist);

            // Create the self signing request
            var cert = new CX509CertificateRequestCertificate();
            
            cert.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextMachine, privateKey, "");
            cert.Subject = dn;
            cert.Issuer = dn; // the issuer and the subject are the same
            cert.NotBefore = DateTime.Now;
            cert.NotAfter = DateTime.Now.AddDays(365);
            cert.X509Extensions.Add((CX509Extension)eku); // add the EKU
            cert.HashAlgorithm = hashobj; // Specify the hashing algorithm
            cert.Encode(); // encode the certificate

            string certStr = cert.RawData[EncodingType.XCN_CRYPT_STRING_BASE64HEADER];
            Console.WriteLine("Cert: " + certStr);

            return new KeyCertificatePair(certStr, pkeyStr);
        }
       */


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

            File.WriteAllBytes(Path.Combine(Utils.GetCertDir(), name + ".pem"), message);
            return true;
        }

        public static string GetRemoteCertificate(string name)
        {
            return File.ReadAllText(Path.Combine(Utils.GetCertDir(), name + ".pem"));
        }

        public static KeyCertificatePair GetKeyCertificatePair()
        {
            log.Debug(Utils.GetCertDir());
            if (File.Exists(Path.Combine(Utils.GetCertDir(), CertificateFileName)))
            {
                var pair1 = LoadKeyCertificatePair();
                var cert = new X509CertificateParser().ReadCertificate(serverCertificate);
                try
                {
                    var l = (ArrayList)cert.GetSubjectAlternativeNames();
                    var ipHexStr = (string)((ArrayList)l[0])[1];
                    long ipNetInt = Convert.ToInt64(ipHexStr.Substring(1), 16);
                    var addr = new IPAddress(IPAddress.NetworkToHostOrder(ipNetInt) >> 32);
                    
                    cert.CheckValidity(); //Throws if invalid
                    if (addr != Utils.GetLocalIPAddress())
                        throw new Org.BouncyCastle.Security.Certificates.CertificateExpiredException();
                    return pair1;
                } catch {}
            }

            KeyCertificatePair pair = CreateKeyCertificatePair(Utils.GetHostname(), Utils.GetLocalIPAddress().ToString());
            log.Debug(Path.Combine(Utils.GetCertDir(), CertificateFileName));
            Directory.CreateDirectory(Utils.GetCertDir());
            File.WriteAllText(Path.Combine(Utils.GetCertDir(), CertificateFileName), pair.CertificateChain);
            File.WriteAllText(Path.Combine(Utils.GetCertDir(), KeyFileName), pair.PrivateKey);
            return pair;
        }

        private static KeyCertificatePair CreateKeyCertificatePair(string subjectName, string ip)
        {
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
