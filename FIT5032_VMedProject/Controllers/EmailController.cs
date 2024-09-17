using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;
using FIT5032_VMedProject.Models;

namespace FIT5032_VMedProject.Controllers
{
    public class EmailController : Controller
    {
        private Entities2 db = new Entities2();

        // GET: Email
        [Authorize(Roles = "Administrator,Doctor")]
        public ActionResult Index()
        {
            return View();
        }

        // GET: Email/BulkEmail
        [Authorize(Roles = "Administrator,Doctor")]
        public ActionResult BulkEmail()
        {
            var users = db.AspNetUsers.ToList(); // Replace 'AspNetUsers' with the name of your user entity in your database

            return View(users);
        }

        // GET: BackToListOne
        [Authorize(Roles = "Administrator,Doctor")]
        public ActionResult BackToListOne()
        {
            // Redirect to the "Index" action in the "AspNetUsersController"
            return RedirectToAction("Index", "AspNetUsers");
        }

        // POST: Email/SendEmail
        // References: MailKit Documentation -https://www.nuget.org/packages/MailKit/#readme-body-tab
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [Authorize(Roles = "Administrator,Doctor")]
        public ActionResult SendEmail(string toEmail, string subject, string message, HttpPostedFileBase attachment)
        {
            try
            {
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("Vincent", "vfit5032@gmail.com")); // Replace with your name and email
                emailMessage.To.Add(new MailboxAddress("Patient", toEmail));
                emailMessage.Subject = subject;

                // Create a multipart message
                var multipart = new Multipart("mixed");

                // Add the text part
                var textPart = new TextPart("plain")
                {
                    Text = message
                };
                multipart.Add(textPart);

                // Add the attachment if provided
                if (attachment != null && attachment.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(attachment.FileName);
                    var attachmentPart = new MimePart(attachment.ContentType)
                    {
                        Content = new MimeContent(attachment.InputStream),
                        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                        ContentTransferEncoding = ContentEncoding.Base64,
                        FileName = fileName
                    };
                    multipart.Add(attachmentPart);
                }

                // Set the multipart message as the message body
                emailMessage.Body = multipart;

                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587, false); // Replace with your SMTP server and port
                    client.Authenticate("vfit5032@gmail.com", "wqoypovqbrkkgtvz"); // Replace with your SMTP username and password

                    client.Send(emailMessage);
                    client.Disconnect(true);
                }

                ViewBag.Message = "Email sent successfully!";
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error sending email: " + ex.Message;
            }

            return View("Index");
        }

        // POST: Email/SendBulkEmail
        // References: MailKit Documentation -https://www.nuget.org/packages/MailKit/#readme-body-tab
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [Authorize(Roles = "Administrator,Doctor")]
        public ActionResult SendBulkEmail(string[] selectedUserIds, string subject, string message, HttpPostedFileBase attachment)
        {
            try
            {
                if (selectedUserIds == null || selectedUserIds.Length == 0)
                {
                    TempData["Error"] = "Please select at least one user.";
                    return RedirectToAction("BulkEmail");
                }

                foreach (var userId in selectedUserIds)
                {
                    var user = db.AspNetUsers.Find(userId);
                    if (user == null)
                        continue;

                    var emailMessage = new MimeMessage();
                    emailMessage.From.Add(new MailboxAddress("Vincent", "vfit5032@gmail.com")); // Replace with your name and Gmail address
                    emailMessage.To.Add(new MailboxAddress(user.UserName, user.Email)); // Use user-specific email

                    emailMessage.Subject = subject;

                    // Create a multipart message
                    var multipart = new Multipart("mixed");

                    // Add the HTML part (assuming message is in HTML format)
                    var htmlPart = new TextPart("html")
                    {
                        Text = message
                    };
                    multipart.Add(htmlPart);

                    // Add the attachment if provided
                    if (attachment != null && attachment.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(attachment.FileName);
                        var attachmentPart = new MimePart(attachment.ContentType)
                        {
                            Content = new MimeContent(attachment.InputStream),
                            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                            ContentTransferEncoding = ContentEncoding.Base64,
                            FileName = fileName
                        };
                        multipart.Add(attachmentPart);
                    }

                    // Set the multipart message as the message body
                    emailMessage.Body = multipart;

                    using (var client = new SmtpClient())
                    {
                        client.Connect("smtp.gmail.com", 587, false); // Use Gmail's SMTP server and port
                        client.Authenticate("vfit5032@gmail.com", "wqoypovqbrkkgtvz"); // Use your App Password here

                        client.Send(emailMessage);
                        client.Disconnect(true);
                    }
                }

                TempData["Message"] = "Bulk email sent successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error sending bulk email: " + ex.Message;
            }

            return RedirectToAction("BulkEmail");
        }
    }
}