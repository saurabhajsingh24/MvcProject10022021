﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KeystoneProject.Models.Master;
using KeystoneProject.Buisness_Logic.Hospital;

namespace KeystoneProject.Controllers.Master
{
    public class NarrationController : Controller
    {
        //
        // GET: /Narration/
        public ActionResult Narration()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Bank(FormCollection collection)
        {
            //HospitalLocation Location = new HospitalLocation();
            //collection
            //return View();
            Narration location = new Narration();
            location.HospitalId = Convert.ToInt32(collection["hospital_name_hid_name"]);
            location.LocationId = 0;




            return View();
        }
	}
}