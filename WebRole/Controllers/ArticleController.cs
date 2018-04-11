using Data;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebRole.Controllers
{
    public class ArticleController : Controller
    {
        // GET: Article
        public ActionResult Index()
        {
            return View("Index");
        }

        [HttpPost]
        public ActionResult ProcessItem(String identifier)
        {
            try
            {
                CloudQueue queue = QueueHelper.GetQueueReference("mojRed");
                queue.AddMessage(new CloudQueueMessage(identifier));

                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }
    }
}