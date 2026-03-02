using DestLoungeSalesandBooking.Models;
using DestLoungeSalesandBooking.Models.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DestLoungeSalesandBooking.Controllers
{
    public class FAQController : Controller
    {
        private DestLoungeSalesandBookingContext db = new DestLoungeSalesandBookingContext();

        // GET: /FAQ/GetAllFAQs
        [HttpGet]
        public JsonResult GetAllFAQs()
        {
            try
            {
                var faqs = db.tbl_faqs
                    .Where(f => f.isActive)
                    .OrderByDescending(f => f.createdAt)
                    .Select(f => new
                    {
                        faqId = f.faqID,
                        question = f.question,
                        answer = f.answer,
                        createdAt = f.createdAt,
                        updatedAt = f.updatedAt
                    })
                    .ToList();

                return Json(new { success = true, data = faqs }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetAllFAQs Error: " + ex.Message);
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /FAQ/GetFAQById/1
        [HttpGet]
        public JsonResult GetFAQById(int id)
        {
            try
            {
                var faq = db.tbl_faqs.FirstOrDefault(f => f.faqID == id && f.isActive);

                if (faq == null)
                    return Json(new { success = false, message = "FAQ not found" }, JsonRequestBehavior.AllowGet);

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        faqId = faq.faqID,
                        question = faq.question,
                        answer = faq.answer,
                        createdAt = faq.createdAt,
                        updatedAt = faq.updatedAt
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetFAQById Error: " + ex.Message);
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: /FAQ/CreateFAQ
        [HttpPost]
        public JsonResult CreateFAQ(string question, string answer)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(question) || string.IsNullOrWhiteSpace(answer))
                    return Json(new { success = false, message = "Question and Answer are required" });

                var existingFaq = db.tbl_faqs
                    .FirstOrDefault(f => f.question.ToLower() == question.ToLower().Trim());

                if (existingFaq != null)
                    return Json(new { success = false, message = "This FAQ question already exists" });

                var newFaq = new tbl_faqs
                {
                    question = question.Trim(),
                    answer = answer.Trim(),
                    createdAt = DateTime.Now,
                    updatedAt = DateTime.Now,
                    isActive = true
                };

                db.tbl_faqs.Add(newFaq);
                db.SaveChanges();

                return Json(new { success = true, message = "FAQ created successfully", data = newFaq.faqID });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("CreateFAQ Error: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /FAQ/UpdateFAQ/1
        [HttpPost]
        public JsonResult UpdateFAQ(int id, string question, string answer)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(question) || string.IsNullOrWhiteSpace(answer))
                    return Json(new { success = false, message = "Question and Answer are required" });

                var faq = db.tbl_faqs.FirstOrDefault(f => f.faqID == id);

                if (faq == null)
                    return Json(new { success = false, message = "FAQ not found" });

                var duplicateQuestion = db.tbl_faqs
                    .FirstOrDefault(f => f.question.ToLower() == question.ToLower().Trim() && f.faqID != id);

                if (duplicateQuestion != null)
                    return Json(new { success = false, message = "This FAQ question already exists" });

                faq.question = question.Trim();
                faq.answer = answer.Trim();
                faq.updatedAt = DateTime.Now;

                db.SaveChanges();

                return Json(new { success = true, message = "FAQ updated successfully" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("UpdateFAQ Error: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /FAQ/DeleteFAQ/1
        [HttpPost]
        public JsonResult DeleteFAQ(int id)
        {
            try
            {
                var faq = db.tbl_faqs.FirstOrDefault(f => f.faqID == id);

                if (faq == null)
                    return Json(new { success = false, message = "FAQ not found" });

                faq.isActive = false;
                faq.updatedAt = DateTime.Now;

                db.SaveChanges();

                return Json(new { success = true, message = "FAQ deleted successfully" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("DeleteFAQ Error: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}