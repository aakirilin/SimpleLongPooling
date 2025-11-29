using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LognPoolingLib
{
    public class Cripto<T> where T : class, new()
    {
        private byte[] key;
        private byte[] iv;

        public Cripto(byte[] key, byte[] iv)
        {
            this.key = key;
            this.iv = iv;
        }

        public string Crypt(string text)
        {
            SymmetricAlgorithm algorithm = DES.Create();
            ICryptoTransform transform = algorithm.CreateEncryptor(key, iv);
            byte[] inputbuffer = Encoding.Unicode.GetBytes(text);
            byte[] outputBuffer = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);
            return Convert.ToBase64String(outputBuffer);
        }

        public string Decrypt(string text)
        {
            SymmetricAlgorithm algorithm = DES.Create();
            ICryptoTransform transform = algorithm.CreateDecryptor(key, iv);
            byte[] inputbuffer = Convert.FromBase64String(text);
            byte[] outputBuffer = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);
            return Encoding.Unicode.GetString(outputBuffer);
        }

        public string EncryptObject(T obj)
        {
            var text = JsonSerializer.Serialize(obj);
            var eString = Crypt(text);
            return eString;
        }

        public T DecryptObject(string text)
        {
            var result = Decrypt(text);
            return JsonSerializer.Deserialize<T>(result);
        }
    }
}
