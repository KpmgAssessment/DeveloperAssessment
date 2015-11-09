using Kpmg.Assessment.UIClient.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Kpmg.Assessment.UIClient.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            //Ideally validation should occur verifying the existence of that endpoint...
            string serverEndpoint = ConfigurationManager.AppSettings["ApiEndpoint"];
            ICollection<TransactionData> viewModel = ServerHelper.GetAllFromServer<TransactionData>(serverEndpoint);

            TempData["collection"] = viewModel;
            return View();
        }

        public ActionResult EditTransactionData(int Id)
        {
            string endpoint = ConfigurationManager.AppSettings["BaseEndpoint"] + "TransactionData/GetById?id=" + Id;
            TransactionData fromServer = ServerHelper.GetOneFromServer<TransactionData>(endpoint);
            TempData["TransactionData"] = fromServer;


            return RedirectToAction("UpdateTransactionData", "Upload");
        }

        public ActionResult About()
        {
            ViewBag.Message = "About Us";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Contact Us";

            return View();
        }
    }
}