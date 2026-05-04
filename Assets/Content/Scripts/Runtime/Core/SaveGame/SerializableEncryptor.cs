using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using UHFPS.Scriptable;

namespace UHFPS.Runtime
{
    public static class SerializableEncryptor
    {
        public static async Task Encrypt(SerializationAsset serializationAsset, string path, string text)
        {
            if (serializationAsset.EncryptSaves)
            {
                byte[] iv = new byte[16];
                byte[] array;

                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(serializationAsset.EncryptionKey);
                    aes.IV = iv;

                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                            {
                                await streamWriter.WriteAsync(text);
                            }

                            array = memoryStream.ToArray();
                        }
                    }
                }

                await File.WriteAllBytesAsync(path, array);
                return;
            }

            await File.WriteAllTextAsync(path, text);
        }

        public static async Task<string> Decrypt(SerializationAsset serializationAsset, string path)
        {
            if (serializationAsset.EncryptSaves)
            {
                byte[] buffer = await File.ReadAllBytesAsync(path);
                byte[] iv = new byte[16];

                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(serializationAsset.EncryptionKey);
                    aes.IV = iv;

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                    using (MemoryStream memoryStream = new MemoryStream(buffer))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader streamReader = new StreamReader(cryptoStream))
                            {
                                return await streamReader.ReadToEndAsync();
                            }
                        }
                    }
                }
            }

            return await File.ReadAllTextAsync(path);
        }
    }
}