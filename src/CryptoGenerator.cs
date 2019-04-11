using System;
using System.Security.Cryptography;
using System.Text;

namespace Pivotal.Redis.Session.Buildpack
{
    internal class CryptoGenerator
    {
        public static String CreateKey(int numBytes)
        {
            var rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[numBytes];

            rng.GetBytes(buff);
            return BytesToHexString(buff);
        }

        static String BytesToHexString(byte[] bytes)
        {
            StringBuilder hexString = new StringBuilder(64);

            for (int counter = 0; counter < bytes.Length; counter++)
            {
                hexString.Append(String.Format("{0:X2}", bytes[counter]));
            }
            return hexString.ToString();
        }
    }
}
