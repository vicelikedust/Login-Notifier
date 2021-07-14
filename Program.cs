using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Mail;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Microsoft.Win32.TaskScheduler;

namespace email
{
    class Program
    {
        static string fromEmail = "yourfromaddress@gmail.com";
        static string emailPassword = "yoursmtppasswordhere";
        static string emailSMTP = "smtp.serverhere.com";
        static int emailPort = 587;
        static string toEmail = "toemailaddresshere";
        static string subject = "Login Alert!";
        static string emailBody = $"Someone logged into your computer!  The user logged in: {Environment.UserName}";
        static string attachmentName = "capture.jpeg";
        static string CurrentDirectory = Directory.GetCurrentDirectory();
        static void Main(string[] args)
        {
			//create task in Windows Task Scheduler and/or update the entry
            createtask();
			//start opencv instace
            VideoCapture capture = new VideoCapture(1); //In my case I had to use video device #1, typically this is Device #0
            Mat image = new Mat();
			//capture image from webcam
            capture.Read(image);
			//load image into a bitmap
            Bitmap img = MatToBitmap(image);
			//save image
            img.Save(attachmentName, ImageFormat.Jpeg);
			
			//begin email setup
            MailMessage msg = new MailMessage(
                    fromEmail,
                    toEmail,
                    subject,
                    emailBody
            );
			//add attachment to email
            msg.Attachments.Add(new Attachment(attachmentName));
			//set smtp credentials and port
            SmtpClient client = new SmtpClient(emailSMTP) { 
                Port = emailPort,
                EnableSsl = true,
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
            // Add a trigger that will fire the task
            td.Triggers.Add(new LogonTrigger { });
            td.Triggers.Add(new SessionStateChangeTrigger(TaskSessionStateChangeType.SessionUnlock) { });
            td.Actions.Add(new ExecAction($"{CurrentDirectory}\\email.exe"));
            // Register the task in the root folder
            const string taskname = "email on Login";
            TaskService.Instance.RootFolder.RegisterTaskDefinition(taskname, td);
        }
    }
}
