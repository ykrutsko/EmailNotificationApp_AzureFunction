using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Net;
using Newtonsoft.Json;

namespace EmailNotificationApp
{
    public static class EmailNotificationFunc
    {
        [FunctionName("EmailNotificationFunc")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)
        {
            // Getting data from the request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string email = data.email;
            string message = data.message;

            // Checking if an email address is entered
            if (string.IsNullOrEmpty(email))
            {
                return new BadRequestObjectResult("Email address is required.");
            }

            try
            {
                // Setting the parameters for sending the letter
                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.Timeout = 5000;
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential("YourGmail", "PasswordForApplicationsWithoutTwo-factorAuthentication");

                var mailMessage = new MailMessage(new MailAddress("YourGmail", "Azure BLOB informer"), new MailAddress(email))
                {
                    Subject = "File upload notification",
                    Body = message,
                };

                // Sending the letter
                await smtpClient.SendMailAsync(mailMessage);
                return new OkObjectResult("Email sent successfully.");
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult($"An error occurred while sending the email: {ex.Message}");
            }
        }
    }
}
