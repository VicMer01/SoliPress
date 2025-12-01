using System;
using System.Net;
using System.Net.Mail;

class Program
{
    static void Main()
    {
        try
        {
            var host = "smtp.gmail.com";
            var port = 587;
            var username = "victorhmaria19@gmail.com";
            var password = "taku hhio qldq xlof"; // App Password
            var from = "victorhmaria19@gmail.com";
            var to = "victorhmaria19@gmail.com"; // Send to self

            Console.WriteLine($"Testing SMTP connection to {host}:{port}...");

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };

            var message = new MailMessage(from, to, "Test Email", "This is a test email from the approval system debugger.");
            
            Console.WriteLine("Sending email...");
            client.Send(message);
            Console.WriteLine("Email sent successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Error: {ex.InnerException.Message}");
            }
        }
    }
}
