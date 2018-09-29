using BlogApp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace BlogApp.Controllers
{
    [RequireHttps]
    public class HomeController : Controller
    {
        private int p;

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            EmailModel model = new EmailModel();
            return View(model);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Contact(EmailModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var body = "<p>Email From: <bold>{0}</bold> ({1})</p><p>MailMessage:</p><p>{2}</p>";
                    var from = WebConfigurationManager.AppSettings["emailto"];

                    var email = new MailMessage(from,
                                ConfigurationManager.AppSettings["emailto"])
                    {
                        Body = string.Format(body, model.FromName, model.FromEmail,
                                             model.Body),
                        IsBodyHtml = true
                    };
                    email.Subject = model.Subject;

                    var svc = new PersonalEmailService();
                    await svc.SendAsync(email);

                    return View(new EmailModel());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await Task.FromResult(0);
                }
            }
            return View(model);
        }


    }
}