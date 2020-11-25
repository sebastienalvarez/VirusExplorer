using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleRansomCli
{
    class Options
    {
        [Option('d', "decrypt", HelpText = "Specify if the action to perform is decryption (default is encryption)")]
        public bool IsDecryptionAction { get; set; }

        [Option('f', "folder", Required = true, HelpText = "Folder to process")]
        public string Folder { get; set; }

        [Option('k', "key", Required = true, HelpText = "Key for encryption")]
        public string Key { get; set; }

        [Option('s', "subfolder", HelpText = "Specify if files that belong to subfolders have to be processed")]
        public bool AreSubFoldersIncluded { get; set; }

        [Option('m', "message", HelpText = "Message to include at the end of each file after encrypted data")]
        public string Message { get; set; }
    }
}
