using System.Text;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System.Security.Cryptography;
using Masuit.Tools.Security;
using Org.BouncyCastle.Asn1.GM;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Math;

namespace EasyTemplate.Tool;

public class Crypto
{
    public static readonly string? aeskey = Setting.Get<string>("cryptogram:aesKey");//AESKey
    public static readonly string? deskey = Setting.Get<string>("cryptogram:descKey");//DESCKey
    public static readonly string? rsaPublicKey = Setting.Get<string>("cryptogram:rsaPublicKey");//RSAPublicKey
    public static readonly string? rsaPrivateKey = Setting.Get<string>("cryptogram:rsaPrivateKey");//RSAPrivateKey
    public static readonly string? sm2PublicKey = Setting.Get<string>("cryptogram:sm2PublicKey"); // 公钥
    public static readonly string? sm2PrivateKey = Setting.Get<string>("cryptogram:sm2PrivateKey"); // 私钥
    public static readonly string? sm4Key = Setting.Get<string>("cryptogram:sm4Key");//SM4Key
    public static readonly string? sm4IV = Setting.Get<string>("cryptogram:sm4IV");//SM4IV

    public static string Encrypt(string plain)
        => (Global.CrypType?.ToLower()) switch
        {
            "md5" => MD5Encrypt(plain),
            "aes" => AESEncrypt(plain),
            "des" => DESEncrypt(plain),
            "rsa" => RSAEncrypt(plain),
            "sm2" => SM2Encrypt(plain),
            "sm4" => SM4EncryptCBC(plain),
            _ => plain,
        };

    public static string Decrypt(string cipher)
        => (Global.CrypType?.ToLower()) switch
        {
            "aes" => AESDecrypt(cipher),
            "des" => DESDecrypt(cipher),
            "rsa" => RSADecrypt(cipher),
            "sm2" => SM2Decrypt(cipher),
            "sm4" => SM4DecryptCBC(cipher),
            _ => cipher,
        };

    public static string MD5Encrypt(string plain)
        => plain.MDString();

    public static string AESEncrypt(string plain)
        => plain.AESEncrypt(aeskey);

    public static string AESDecrypt(string cipher)
        => cipher.AESDecrypt(aeskey);

    public static string DESEncrypt(string plain)
        => plain.DesEncrypt(deskey);

    public static string DESDecrypt(string cipher)
        => cipher.DesDecrypt(deskey);
    public static string TokenDESEncrypt(string originalValue, string key)
    {
        return TokenDESEncrypt(originalValue, key, key);
    }

    public static string TokenDESEncrypt(string originalValue, string key, string IV)
    {
        if (string.IsNullOrEmpty(originalValue))
        {
            originalValue = string.Empty;
        }

        key += "abcdefgh";
        IV += "01234567";
        key = key.Substring(0, 8);
        IV = IV.Substring(0, 8);
        string result = string.Empty;
        SymmetricAlgorithm symmetricAlgorithm;
        ICryptoTransform cryptoTransform;
        using (MemoryStream memoryStream = new MemoryStream())
        {
            symmetricAlgorithm = new DESCryptoServiceProvider();
            symmetricAlgorithm.Key = Encoding.UTF8.GetBytes(key);
            symmetricAlgorithm.IV = Encoding.UTF8.GetBytes(IV);
            cryptoTransform = symmetricAlgorithm.CreateEncryptor();
            byte[] bytes = Encoding.UTF8.GetBytes(originalValue);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write);
            cryptoStream.Write(bytes, 0, bytes.Length);
            cryptoStream.FlushFinalBlock();
            cryptoStream.Dispose();
            result = Convert.ToBase64String(memoryStream.ToArray());
        }

        cryptoTransform.Dispose();
        symmetricAlgorithm.Clear();
        return result;
    }
    public static string TokenDESDecrypt(string encryptedValue, string key)
    {
        return TokenDESDecrypt(encryptedValue, key, key);
    }

    public static string TokenDESDecrypt(string encryptedValue, string key, string IV)
    {
        key += "abcdefgh";
        IV += "01234567";
        key = key.Substring(0, 8);
        IV = IV.Substring(0, 8);
        SymmetricAlgorithm symmetricAlgorithm = new DESCryptoServiceProvider();
        symmetricAlgorithm.Key = Encoding.UTF8.GetBytes(key);
        symmetricAlgorithm.IV = Encoding.UTF8.GetBytes(IV);
        ICryptoTransform cryptoTransform = symmetricAlgorithm.CreateDecryptor();
        byte[] array = Convert.FromBase64String(encryptedValue);
        MemoryStream memoryStream = new MemoryStream();
        CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write);
        cryptoStream.Write(array, 0, array.Length);
        cryptoStream.FlushFinalBlock();
        cryptoStream.Dispose();
        memoryStream.Dispose();
        cryptoTransform.Dispose();
        symmetricAlgorithm.Clear();
        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public static string RSAEncrypt(string plain)
        => plain.RSAEncrypt(rsaPublicKey);

    public static string RSADecrypt(string cipher)
        => cipher.RSADecrypt(rsaPrivateKey);

    public static string SM2Encrypt(string plain)
        => GMUtil.SM2Encrypt(sm2PublicKey, plain);

    public static string SM2Decrypt(string cipher)
    {
        try
        {
            return GMUtil.SM2Decrypt(sm2PrivateKey, cipher);
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public static string SM4EncryptECB(string plain)
        => GMUtil.SM4EncryptECB(sm4Key, plain);

    public static string SM4DecryptECB(string cipher)
    {
        try
        {
            return GMUtil.SM2Decrypt(sm4Key, cipher);
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public static string SM4EncryptCBC(string plain)
        => GMUtil.SM4EncryptCBC(sm4Key, sm4IV, plain);

    public static string SM4DecryptCBC(string cipher)
    {
        try
        {
            return GMUtil.SM4DecryptCBC(sm4Key, sm4IV, cipher);
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// 签名
    /// </summary>
    /// <param name="halg">算法如:SHA256</param>
    /// <param name="text">明文内容</param>
    /// <param name="privateKey">私钥</param>
    /// <returns>签名内容</returns>
    public static string RSASign(string halg, string text, string privateKey)
    {
        string encryptedContent;
        using var rsa = new System.Security.Cryptography.RSACryptoServiceProvider();
        rsa.FromXmlString(privateKey);

        var encryptedData = rsa.SignData(Encoding.Default.GetBytes(text), halg);
        encryptedContent = Convert.ToBase64String(encryptedData);

        return encryptedContent;
    }

    /// <summary>
    /// 校验
    /// </summary>
    /// <param name="halg">算法如:SHA256</param>
    /// <param name="text">明文内容</param>
    /// <param name="publicKey">公钥</param>
    /// <param name="sign">签名内容</param>
    /// <returns>是否一致</returns>
    public static bool RSAVerify(string halg, string text, string publicKey, string sign)
    {
        using var rsa = new System.Security.Cryptography.RSACryptoServiceProvider();
        rsa.FromXmlString(publicKey);

        return rsa.VerifyData(Encoding.Default.GetBytes(text), halg, Convert.FromBase64String(sign));
    }
    /// <summary> 
    /// 公钥格式的转换
    /// </summary>
    /// <param name="publicKey"></param>
    /// <returns></returns>
    public static string RSAPublicKeyToXml(string publicKey)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(publicKey))
            {
                return "";
            }
            if (publicKey.Contains("<RSAKeyValue>"))
            {
                return publicKey;
            }
            RsaKeyParameters publicKeyParam;
            //尝试进行java格式的密钥读取
            try
            {
                publicKeyParam = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKey));
            }
            catch
            {
                publicKeyParam = null;
            }
            //非java格式密钥进行pem格式的密钥读取
            if (publicKeyParam == null)
            {
                try
                {
                    var pemKey = publicKey;
                    if (!pemKey.Contains("BEGIN RSA PRIVATE KEY"))
                    {
                        pemKey = @"-----BEGIN RSA PRIVATE KEY-----
                           " + publicKey + @"
                           -----END RSA PRIVATE KEY-----";
                    }
                    var array = Encoding.ASCII.GetBytes(pemKey);
                    var stream = new MemoryStream(array);
                    var reader = new StreamReader(stream);
                    var pemReader = new Org.BouncyCastle.OpenSsl.PemReader(reader);
                    publicKeyParam = (RsaKeyParameters)pemReader.ReadObject();
                }
                catch
                {
                    publicKeyParam = null;
                }
            }
            //如果都解析失败，则返回原串
            if (publicKeyParam == null)
            {
                return publicKey;
            }
            //输出XML格式密钥
            return $@"<RSAKeyValue><Modulus>{Convert.ToBase64String(publicKeyParam.Modulus.ToByteArrayUnsigned())}</Modulus><Exponent>{Convert.ToBase64String(publicKeyParam.Exponent.ToByteArrayUnsigned())}</Exponent></RSAKeyValue>";
        }
        catch
        {
            return "";
        }
    }

    /// <summary>
    /// 私钥格式转换
    /// </summary>
    /// <param name="privateKey"></param>
    /// <returns></returns>
    public static string RSAPrivateKeyToXml(string privateKey)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(privateKey))
            {
                return "";
            }
            if (privateKey.Contains("<RSAKeyValue>"))
            {
                return privateKey;
            }
            RsaPrivateCrtKeyParameters privateKeyParam;
            //尝试进行java格式的密钥读取
            try
            {
                privateKeyParam = (RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey(Convert.FromBase64String(privateKey));
            }
            catch
            {
                privateKeyParam = null;
            }
            //非java格式密钥进行pem格式的密钥读取
            if (privateKeyParam == null)
            {
                try
                {
                    var pemKey = privateKey;
                    if (!pemKey.Contains("BEGIN RSA PRIVATE KEY"))
                    {
                        pemKey = @"-----BEGIN RSA PRIVATE KEY-----
                           " + privateKey + @"
                           -----END RSA PRIVATE KEY-----";
                    }
                    var array = Encoding.ASCII.GetBytes(pemKey);
                    var stream = new MemoryStream(array);
                    var reader = new StreamReader(stream);
                    var pemReader = new Org.BouncyCastle.OpenSsl.PemReader(reader);
                    var keyPair = (AsymmetricCipherKeyPair)pemReader.ReadObject();
                    privateKeyParam = (RsaPrivateCrtKeyParameters)keyPair.Private;
                }
                catch
                {
                    privateKeyParam = null;
                }
            }
            //如果都解析失败，则返回原串
            if (privateKeyParam == null)
            {
                return privateKey;
            }
            //输出XML格式密钥
            return $@"<RSAKeyValue><Modulus>{Convert.ToBase64String(privateKeyParam.Modulus.ToByteArrayUnsigned())}</Modulus><Exponent>{Convert.ToBase64String(privateKeyParam.PublicExponent.ToByteArrayUnsigned())}</Exponent><P>{Convert.ToBase64String(privateKeyParam.P.ToByteArrayUnsigned())}</P><Q>{Convert.ToBase64String(privateKeyParam.Q.ToByteArrayUnsigned())}</Q><DP>{Convert.ToBase64String(privateKeyParam.DP.ToByteArrayUnsigned())}</DP><DQ>{Convert.ToBase64String(privateKeyParam.DQ.ToByteArrayUnsigned())}</DQ><InverseQ>{Convert.ToBase64String(privateKeyParam.QInv.ToByteArrayUnsigned())}</InverseQ><D>{Convert.ToBase64String(privateKeyParam.Exponent.ToByteArrayUnsigned())}</D></RSAKeyValue>";
        }
        catch
        {
            return "";
        }
    }
}

public class GM
{
    private static X9ECParameters x9ECParameters = GMNamedCurves.GetByName("sm2p256v1");
    private static ECDomainParameters ecDomainParameters = new(x9ECParameters.Curve, x9ECParameters.G, x9ECParameters.N);

    /**
     *
     * @param msg
     * @param userId
     * @param privateKey
     * @return r||s，直接拼接byte数组的rs
     */

    public static byte[] SignSm3WithSm2(byte[] msg, byte[] userId, AsymmetricKeyParameter privateKey)
    {
        return RsAsn1ToPlainByteArray(SignSm3WithSm2Asn1Rs(msg, userId, privateKey));
    }

    /**
      * @param msg
      * @param userId
      * @param privateKey
      * @return rs in <b>asn1 format</b>
      */

    public static byte[] SignSm3WithSm2Asn1Rs(byte[] msg, byte[] userId, AsymmetricKeyParameter privateKey)
    {
        ISigner signer = SignerUtilities.GetSigner("SM3withSM2");
        signer.Init(true, new ParametersWithID(privateKey, userId));
        signer.BlockUpdate(msg, 0, msg.Length);
        byte[] sig = signer.GenerateSignature();
        return sig;
    }

    /**
    *
    * @param msg
    * @param userId
    * @param rs r||s，直接拼接byte数组的rs
    * @param publicKey
    * @return
    */

    public static bool VerifySm3WithSm2(byte[] msg, byte[] userId, byte[] rs, AsymmetricKeyParameter publicKey)
    {
        if (rs == null || msg == null || userId == null) return false;
        if (rs.Length != RS_LEN * 2) return false;
        return VerifySm3WithSm2Asn1Rs(msg, userId, RsPlainByteArrayToAsn1(rs), publicKey);
    }

    /**
     *
     * @param msg
     * @param userId
     * @param rs in <b>asn1 format</b>
     * @param publicKey
     * @return
     */

    public static bool VerifySm3WithSm2Asn1Rs(byte[] msg, byte[] userId, byte[] sign, AsymmetricKeyParameter publicKey)
    {
        ISigner signer = SignerUtilities.GetSigner("SM3withSM2");
        signer.Init(false, new ParametersWithID(publicKey, userId));
        signer.BlockUpdate(msg, 0, msg.Length);
        return signer.VerifySignature(sign);
    }

    /**
     * bc加解密使用旧标c1||c2||c3，此方法在加密后调用，将结果转化为c1||c3||c2
     * @param c1c2c3
     * @return
     */

    private static byte[] ChangeC1C2C3ToC1C3C2(byte[] c1c2c3)
    {
        int c1Len = (x9ECParameters.Curve.FieldSize + 7) / 8 * 2 + 1; //sm2p256v1的这个固定65。可看GMNamedCurves、ECCurve代码。
        const int c3Len = 32; //new SM3Digest().getDigestSize();
        byte[] result = new byte[c1c2c3.Length];
        Buffer.BlockCopy(c1c2c3, 0, result, 0, c1Len); //c1
        Buffer.BlockCopy(c1c2c3, c1c2c3.Length - c3Len, result, c1Len, c3Len); //c3
        Buffer.BlockCopy(c1c2c3, c1Len, result, c1Len + c3Len, c1c2c3.Length - c1Len - c3Len); //c2
        return result;
    }

    /**
     * bc加解密使用旧标c1||c3||c2，此方法在解密前调用，将密文转化为c1||c2||c3再去解密
     * @param c1c3c2
     * @return
     */

    private static byte[] ChangeC1C3C2ToC1C2C3(byte[] c1c3c2)
    {
        int c1Len = (x9ECParameters.Curve.FieldSize + 7) / 8 * 2 + 1; //sm2p256v1的这个固定65。可看GMNamedCurves、ECCurve代码。
        const int c3Len = 32; //new SM3Digest().GetDigestSize();
        byte[] result = new byte[c1c3c2.Length];
        Buffer.BlockCopy(c1c3c2, 0, result, 0, c1Len); //c1: 0->65
        Buffer.BlockCopy(c1c3c2, c1Len + c3Len, result, c1Len, c1c3c2.Length - c1Len - c3Len); //c2
        Buffer.BlockCopy(c1c3c2, c1Len, result, c1c3c2.Length - c3Len, c3Len); //c3
        return result;
    }

    /**
     * c1||c3||c2
     * @param data
     * @param key
     * @return
     */

    public static byte[] Sm2Decrypt(byte[] data, AsymmetricKeyParameter key)
    {
        return Sm2DecryptOld(ChangeC1C3C2ToC1C2C3(data), key);
    }

    /**
     * c1||c3||c2
     * @param data
     * @param key
     * @return
     */

    public static byte[] Sm2Encrypt(byte[] data, AsymmetricKeyParameter key)
    {
        return ChangeC1C2C3ToC1C3C2(Sm2EncryptOld(data, key));
    }

    /**
     * c1||c2||c3
     * @param data
     * @param key
     * @return
     */

    public static byte[] Sm2EncryptOld(byte[] data, AsymmetricKeyParameter pubkey)
    {
        SM2Engine sm2Engine = new SM2Engine();
        sm2Engine.Init(true, new ParametersWithRandom(pubkey, new SecureRandom()));
        return sm2Engine.ProcessBlock(data, 0, data.Length);
    }

    /**
     * c1||c2||c3
     * @param data
     * @param key
     * @return
     */

    public static byte[] Sm2DecryptOld(byte[] data, AsymmetricKeyParameter key)
    {
        SM2Engine sm2Engine = new SM2Engine();
        sm2Engine.Init(false, key);
        return sm2Engine.ProcessBlock(data, 0, data.Length);
    }

    /**
     * @param bytes
     * @return
     */

    public static byte[] Sm3(byte[] bytes)
    {
        SM3Digest digest = new();
        digest.BlockUpdate(bytes, 0, bytes.Length);
        byte[] result = DigestUtilities.DoFinal(digest);
        return result;
    }

    private const int RS_LEN = 32;

    private static byte[] BigIntToFixexLengthBytes(BigInteger rOrS)
    {
        // for sm2p256v1, n is 00fffffffeffffffffffffffffffffffff7203df6b21c6052b53bbf40939d54123,
        // r and s are the result of mod n, so they should be less than n and have length<=32
        byte[] rs = rOrS.ToByteArray();
        if (rs.Length == RS_LEN) return rs;
        else if (rs.Length == RS_LEN + 1 && rs[0] == 0) return Arrays.CopyOfRange(rs, 1, RS_LEN + 1);
        else if (rs.Length < RS_LEN)
        {
            byte[] result = new byte[RS_LEN];
            Arrays.Fill(result, (byte)0);
            Buffer.BlockCopy(rs, 0, result, RS_LEN - rs.Length, rs.Length);
            return result;
        }
        else
        {
            throw new ArgumentException("err rs: " + Hex.ToHexString(rs));
        }
    }

    /**
     * BC的SM3withSM2签名得到的结果的rs是asn1格式的，这个方法转化成直接拼接r||s
     * @param rsDer rs in asn1 format
     * @return sign result in plain byte array
     */

    private static byte[] RsAsn1ToPlainByteArray(byte[] rsDer)
    {
        Asn1Sequence seq = Asn1Sequence.GetInstance(rsDer);
        byte[] r = BigIntToFixexLengthBytes(DerInteger.GetInstance(seq[0]).Value);
        byte[] s = BigIntToFixexLengthBytes(DerInteger.GetInstance(seq[1]).Value);
        byte[] result = new byte[RS_LEN * 2];
        Buffer.BlockCopy(r, 0, result, 0, r.Length);
        Buffer.BlockCopy(s, 0, result, RS_LEN, s.Length);
        return result;
    }

    /**
     * BC的SM3withSM2验签需要的rs是asn1格式的，这个方法将直接拼接r||s的字节数组转化成asn1格式
     * @param sign in plain byte array
     * @return rs result in asn1 format
     */

    private static byte[] RsPlainByteArrayToAsn1(byte[] sign)
    {
        if (sign.Length != RS_LEN * 2) throw new ArgumentException("err rs. ");
        BigInteger r = new BigInteger(1, Arrays.CopyOfRange(sign, 0, RS_LEN));
        BigInteger s = new BigInteger(1, Arrays.CopyOfRange(sign, RS_LEN, RS_LEN * 2));
        Asn1EncodableVector v = new Asn1EncodableVector
        {
            new DerInteger(r),
            new DerInteger(s)
        };

        return new DerSequence(v).GetEncoded("DER");
    }

    // 生成公私匙对
    public static AsymmetricCipherKeyPair GenerateKeyPair()
    {
        ECKeyPairGenerator kpGen = new();
        kpGen.Init(new ECKeyGenerationParameters(ecDomainParameters, new SecureRandom()));
        return kpGen.GenerateKeyPair();
    }

    public static ECPrivateKeyParameters GetPrivatekeyFromD(BigInteger d)
    {
        return new ECPrivateKeyParameters(d, ecDomainParameters);
    }

    public static ECPublicKeyParameters GetPublickeyFromXY(BigInteger x, BigInteger y)
    {
        return new ECPublicKeyParameters(x9ECParameters.Curve.CreatePoint(x, y), ecDomainParameters);
    }

    public static AsymmetricKeyParameter GetPublickeyFromX509File(FileInfo file)
    {
        FileStream fileStream = null;
        try
        {
            //file.DirectoryName + "\\" + file.Name
            fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
            X509Certificate certificate = new X509CertificateParser().ReadCertificate(fileStream);
            return certificate.GetPublicKey();
        }
        catch (Exception)
        {
            //log.Error(file.Name + "读取失败，异常：" + e);
        }
        finally
        {
            if (fileStream != null)
                fileStream.Close();
        }
        return null;
    }

    public class Sm2Cert
    {
        public AsymmetricKeyParameter privateKey;
        public AsymmetricKeyParameter publicKey;
        public string certId;
    }

    private static byte[] ToByteArray(int i)
    {
        byte[] byteArray = new byte[4];
        byteArray[0] = (byte)(i >> 24);
        byteArray[1] = (byte)((i & 0xFFFFFF) >> 16);
        byteArray[2] = (byte)((i & 0xFFFF) >> 8);
        byteArray[3] = (byte)(i & 0xFF);
        return byteArray;
    }

    /**
     * 字节数组拼接
     *
     * @param params
     * @return
     */

    private static byte[] Join(params byte[][] byteArrays)
    {
        List<byte> byteSource = new();
        for (int i = 0; i < byteArrays.Length; i++)
        {
            byteSource.AddRange(byteArrays[i]);
        }
        byte[] data = byteSource.ToArray();
        return data;
    }

    /**
     * 密钥派生函数
     *
     * @param Z
     * @param klen
     * 生成klen字节数长度的密钥
     * @return
     */

    private static byte[] KDF(byte[] Z, int klen)
    {
        int ct = 1;
        int end = (int)Math.Ceiling(klen * 1.0 / 32);
        List<byte> byteSource = new();

        for (int i = 1; i < end; i++)
        {
            byteSource.AddRange(Sm3(Join(Z, ToByteArray(ct))));
            ct++;
        }
        byte[] last = Sm3(Join(Z, ToByteArray(ct)));
        if (klen % 32 == 0)
        {
            byteSource.AddRange(last);
        }
        else
            byteSource.AddRange(Arrays.CopyOfRange(last, 0, klen % 32));
        return byteSource.ToArray();
    }

    public static byte[] Sm4DecryptCBC(byte[] keyBytes, byte[] cipher, byte[] iv, string algo)
    {
        if (keyBytes.Length != 16) throw new ArgumentException("err key length");
        if (cipher.Length % 16 != 0 && algo.Contains("NoPadding")) throw new ArgumentException("err data length");

        KeyParameter key = ParameterUtilities.CreateKeyParameter("SM4", keyBytes);
        IBufferedCipher c = CipherUtilities.GetCipher(algo);
        if (iv == null) iv = ZeroIv(algo);
        c.Init(false, new ParametersWithIV(key, iv));
        return c.DoFinal(cipher);
    }

    public static byte[] Sm4EncryptCBC(byte[] keyBytes, byte[] plain, byte[] iv, string algo)
    {
        if (keyBytes.Length != 16) throw new ArgumentException("err key length");
        if (plain.Length % 16 != 0 && algo.Contains("NoPadding")) throw new ArgumentException("err data length");

        KeyParameter key = ParameterUtilities.CreateKeyParameter("SM4", keyBytes);
        IBufferedCipher c = CipherUtilities.GetCipher(algo);
        if (iv == null) iv = ZeroIv(algo);
        c.Init(true, new ParametersWithIV(key, iv));
        return c.DoFinal(plain);
    }

    public static byte[] Sm4EncryptECB(byte[] keyBytes, byte[] plain, string algo)
    {
        if (keyBytes.Length != 16) throw new ArgumentException("err key length");
        //NoPadding 的情况下需要校验数据长度是16的倍数.
        if (plain.Length % 16 != 0 && algo.Contains("NoPadding")) throw new ArgumentException("err data length");

        KeyParameter key = ParameterUtilities.CreateKeyParameter("SM4", keyBytes);
        IBufferedCipher c = CipherUtilities.GetCipher(algo);
        c.Init(true, key);
        return c.DoFinal(plain);
    }

    public static byte[] Sm4DecryptECB(byte[] keyBytes, byte[] cipher, string algo)
    {
        if (keyBytes.Length != 16) throw new ArgumentException("err key length");
        if (cipher.Length % 16 != 0 && algo.Contains("NoPadding")) throw new ArgumentException("err data length");

        KeyParameter key = ParameterUtilities.CreateKeyParameter("SM4", keyBytes);
        IBufferedCipher c = CipherUtilities.GetCipher(algo);
        c.Init(false, key);
        return c.DoFinal(cipher);
    }

    public const string SM4_ECB_NOPADDING = "SM4/ECB/NoPadding";
    public const string SM4_CBC_NOPADDING = "SM4/CBC/NoPadding";
    public const string SM4_CBC_PKCS7PADDING = "SM4/CBC/PKCS7Padding";

    /**
     * cfca官网CSP沙箱导出的sm2文件
     * @param pem 二进制原文
     * @param pwd 密码
     * @return
     */

    public static Sm2Cert ReadSm2File(byte[] pem, string pwd)
    {
        Sm2Cert sm2Cert = new();

        Asn1Sequence asn1Sequence = (Asn1Sequence)Asn1Object.FromByteArray(pem);
        //            ASN1Integer asn1Integer = (ASN1Integer) asn1Sequence.getObjectAt(0); //version=1
        Asn1Sequence priSeq = (Asn1Sequence)asn1Sequence[1];//private key
        Asn1Sequence pubSeq = (Asn1Sequence)asn1Sequence[2];//public key and x509 cert

        //            ASN1ObjectIdentifier sm2DataOid = (ASN1ObjectIdentifier) priSeq.getObjectAt(0);
        //            ASN1ObjectIdentifier sm4AlgOid = (ASN1ObjectIdentifier) priSeq.getObjectAt(1);
        Asn1OctetString priKeyAsn1 = (Asn1OctetString)priSeq[2];
        byte[] key = KDF(System.Text.Encoding.UTF8.GetBytes(pwd), 32);
        byte[] priKeyD = Sm4DecryptCBC(Arrays.CopyOfRange(key, 16, 32),
                priKeyAsn1.GetOctets(),
                Arrays.CopyOfRange(key, 0, 16), SM4_CBC_PKCS7PADDING);
        sm2Cert.privateKey = GetPrivatekeyFromD(new BigInteger(1, priKeyD));
        //            log.Info(Hex.toHexString(priKeyD));

        //            ASN1ObjectIdentifier sm2DataOidPub = (ASN1ObjectIdentifier) pubSeq.getObjectAt(0);
        Asn1OctetString pubKeyX509 = (Asn1OctetString)pubSeq[1];
        X509Certificate x509 = new X509CertificateParser().ReadCertificate(pubKeyX509.GetOctets());
        sm2Cert.publicKey = x509.GetPublicKey();
        sm2Cert.certId = x509.SerialNumber.ToString(10); //这里转10进制，有啥其他进制要求的自己改改
        return sm2Cert;
    }

    /**
     *
     * @param cert
     * @return
     */

    public static Sm2Cert ReadSm2X509Cert(byte[] cert)
    {
        Sm2Cert sm2Cert = new();

        X509Certificate x509 = new X509CertificateParser().ReadCertificate(cert);
        sm2Cert.publicKey = x509.GetPublicKey();
        sm2Cert.certId = x509.SerialNumber.ToString(10); //这里转10进制，有啥其他进制要求的自己改改
        return sm2Cert;
    }

    public static byte[] ZeroIv(string algo)
    {
        IBufferedCipher cipher = CipherUtilities.GetCipher(algo);
        int blockSize = cipher.GetBlockSize();
        byte[] iv = new byte[blockSize];
        Arrays.Fill(iv, (byte)0);
        return iv;
    }

}

/// <summary>
/// GM工具类
/// </summary>
public class GMUtil
{
    /// <summary>
    /// SM2加密
    /// </summary>
    /// <param name="publicKeyHex"></param>
    /// <param name="data_string"></param>
    /// <returns></returns>
    public static string SM2Encrypt(string publicKeyHex, string data_string)
    {
        // 如果是130位公钥，.NET使用的话，把开头的04截取掉
        if (publicKeyHex.Length == 130)
        {
            publicKeyHex = publicKeyHex.Substring(2, 128);
        }
        // 公钥X，前64位
        string x = publicKeyHex.Substring(0, 64);
        // 公钥Y，后64位
        string y = publicKeyHex.Substring(64);
        // 获取公钥对象
        AsymmetricKeyParameter publicKey1 = GM.GetPublickeyFromXY(new BigInteger(x, 16), new BigInteger(y, 16));
        // Sm2Encrypt: C1C3C2
        // Sm2EncryptOld: C1C2C3
        byte[] digestByte = GM.Sm2Encrypt(Encoding.UTF8.GetBytes(data_string), publicKey1);
        string strSM2 = Hex.ToHexString(digestByte);
        return strSM2;
    }

    /// <summary>
    /// SM2解密
    /// </summary>
    /// <param name="privateKey_string"></param>
    /// <param name="encryptedData_string"></param>
    /// <returns></returns>
    public static string SM2Decrypt(string privateKey_string, string encryptedData_string)
    {
        if (!encryptedData_string.StartsWith("04"))
            encryptedData_string = "04" + encryptedData_string;
        BigInteger d = new(privateKey_string, 16);
        // 先拿到私钥对象，用ECPrivateKeyParameters 或 AsymmetricKeyParameter 都可以
        // ECPrivateKeyParameters bcecPrivateKey = GmUtil.GetPrivatekeyFromD(d);
        AsymmetricKeyParameter bcecPrivateKey = GM.GetPrivatekeyFromD(d);
        byte[] byToDecrypt = Hex.Decode(encryptedData_string);
        byte[] byDecrypted = GM.Sm2Decrypt(byToDecrypt, bcecPrivateKey);
        string strDecrypted = Encoding.UTF8.GetString(byDecrypted);
        return strDecrypted;
    }

    /// <summary>
    /// SM4加密（ECB）
    /// </summary>
    /// <param name="key_string"></param>
    /// <param name="plainText"></param>
    /// <returns></returns>
    public static string SM4EncryptECB(string key_string, string plainText)
    {
        byte[] key = Hex.Decode(key_string);
        byte[] bs = GM.Sm4EncryptECB(key, Encoding.UTF8.GetBytes(plainText), GM.SM4_CBC_PKCS7PADDING);//NoPadding 的情况下需要校验数据长度是16的倍数. 使用 HandleSm4Padding 处理
        return Hex.ToHexString(bs);
    }

    /// <summary>
    /// SM4解密（ECB）
    /// </summary>
    /// <param name="key_string"></param>
    /// <param name="cipherText"></param>
    /// <returns></returns>
    public static string SM4DecryptECB(string key_string, string cipherText)
    {
        byte[] key = Hex.Decode(key_string);
        byte[] bs = GM.Sm4DecryptECB(key, Hex.Decode(cipherText), GM.SM4_CBC_PKCS7PADDING);
        return Encoding.UTF8.GetString(bs);
    }

    /// <summary>
    /// SM4加密（CBC）
    /// </summary>
    /// <param name="key_string"></param>
    /// <param name="iv_string"></param>
    /// <param name="plainText"></param>
    /// <returns></returns>
    public static string SM4EncryptCBC(string key_string, string iv_string, string plainText)
    {
        byte[] key = Hex.Decode(key_string);
        byte[] iv = Hex.Decode(iv_string);
        byte[] bs = GM.Sm4EncryptCBC(key, Encoding.UTF8.GetBytes(plainText), iv, GM.SM4_CBC_PKCS7PADDING);
        return Hex.ToHexString(bs);
    }

    /// <summary>
    /// SM4解密（CBC）
    /// </summary>
    /// <param name="key_string"></param>
    /// <param name="iv_string"></param>
    /// <param name="cipherText"></param>
    /// <returns></returns>
    public static string SM4DecryptCBC(string key_string, string iv_string, string cipherText)
    {
        byte[] key = Hex.Decode(key_string);
        byte[] iv = Hex.Decode(iv_string);
        byte[] bs = GM.Sm4DecryptCBC(key, Hex.Decode(cipherText), iv, GM.SM4_CBC_PKCS7PADDING);
        return Encoding.UTF8.GetString(bs);
    }

    /// <summary>
    /// 补足 16 进制字符串的 0 字符，返回不带 0x 的16进制字符串
    /// </summary>
    /// <param name="input"></param>
    /// <param name="mode">1表示加密，0表示解密</param>
    /// <returns></returns>
    private static byte[] HandleSm4Padding(byte[] input, int mode)
    {
        if (input == null)
        {
            return null;
        }
        byte[] ret = (byte[])null;
        if (mode == 1)
        {
            int p = 16 - input.Length % 16;
            ret = new byte[input.Length + p];
            Array.Copy(input, 0, ret, 0, input.Length);
            for (int i = 0; i < p; i++)
            {
                ret[input.Length + i] = (byte)p;
            }
        }
        else
        {
            int p = input[input.Length - 1];
            ret = new byte[input.Length - p];
            Array.Copy(input, 0, ret, 0, input.Length - p);
        }
        return ret;
    }
}