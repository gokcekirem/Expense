using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Net.Mail;
using System.Text;
using Data;
using ExpenseProject.Models;

namespace ExpenseProject.Controllers
{
    [Authorize(Roles = "Accounting")]
    public class ExpensesForAccountingController : Controller
    {
        private ExpenseEntities db = new ExpenseEntities(); //ExpenseEntities.Instance;
        private ApplicationUserManager _userManager;

        public decimal totalSum;

        public ExpensesForAccountingController(ApplicationUserManager userManager) { _userManager = userManager; }

        public ExpensesForAccountingController() { }

        // GET: Expenses
        public ActionResult Index(int? StatusId)
        {
            string userId = User.Identity.GetUserId();

            var expenseStatus = new List<SelectListItem>();

            expenseStatus.Add(new SelectListItem { Text = "All", Value = "-1" });
            expenseStatus.Add(new SelectListItem { Text = "Paid", Value = ((int)ExpenseProject.Models.ExpenseStatus.Paid).ToString() });
            expenseStatus.Add(new SelectListItem { Text = "Pending", Value = ((int)ExpenseProject.Models.ExpenseStatus.AccountingPaymentPending).ToString() });

            ViewBag.expenseStatus = expenseStatus;

            if (StatusId == null || StatusId == -1)
            {
                var pendingExpenses = db.Expense.Where(q => q.StatusId == (int)ExpenseProject.Models.ExpenseStatus.AccountingPaymentPending).Include(e => e.AspNetUsers).Include(e => e.ExpenseStatus);
                var paidExpenses = db.Expense.Where(q => q.StatusId == (int)ExpenseProject.Models.ExpenseStatus.Paid).Include(e => e.AspNetUsers).Include(e => e.ExpenseStatus);
                pendingExpenses.ToList().AddRange(paidExpenses.ToList());
                var allExpenses = pendingExpenses.ToList().Concat(paidExpenses.ToList());
                ViewBag.selected = "All";
                return View(allExpenses);

            }
            var expense = db.Expense.Where(q => q.StatusId == StatusId).Include(e => e.AspNetUsers).Include(e => e.ExpenseStatus);

            if (StatusId == (int)ExpenseProject.Models.ExpenseStatus.AccountingPaymentPending)
                ViewBag.selected = "Pending";
            if (StatusId == (int)ExpenseProject.Models.ExpenseStatus.Paid)
                ViewBag.selected = "Paid";

            return View(expense.ToList());

        }

        // GET: Expenses/Details/5
        public ActionResult Details(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Expense expense = db.Expense.Find(id);
            if (expense == null)
            {
                return HttpNotFound();
            }
            return RedirectToAction("IndexItem", "ExpensesForAccounting", new { expenseId = id });
        }
        
        public ActionResult Pay(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Expense expense = db.Expense.Find(id);
            TempData["UserId"] = expense.UserId;
            if (expense == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "Email", expense.UserId);
            ViewBag.StatusId = new SelectList(db.ExpenseStatus, "Id", "Description", expense.StatusId);
            //return View(expense);
            return Pay(expense);
        }
        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Pay(Expense expense)
        {
            string currentUserId = User.Identity.GetUserId();
            var temp = TempData["UserId"];
            db.Entry(expense).State = EntityState.Modified;
            expense.UserId = (string)temp;
            expense.StatusId = (int)ExpenseProject.Models.ExpenseStatus.Paid;
            expense.LastModifiedBy = currentUserId;
            db.SaveChanges();

            // mail to the employee
            SendEmail(expense);
            return RedirectToAction("Index", "ExpensesForAccounting");
        }

        public void  SendEmail(Expense expense)
        {
            // string senderEmail = System.Configuration.ConfigurationManager.AppSettings["SenderEmail"];
            // string senderPassword = System.Configuration.ConfigurationManager.AppSettings["SenderPassword"];
            using (SmtpClient client = new SmtpClient())
            {
                client.Timeout = 10000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                string subject = "Expense Payment";
                string body = "The Expense " + expense.Description + " has been paid.";
                MailMessage mailMessage = new MailMessage("irem.gokcek@veripark.com", "irem.gokcek@veripark.com", subject, body); // expense.AspNetUsers.Email
                mailMessage.IsBodyHtml = true;
                mailMessage.BodyEncoding = System.Text.Encoding.ASCII;
                mailMessage.Priority = MailPriority.Normal;
                client.Send(mailMessage);
                ViewBag.Message = "A mail has been sent to the employee";
                client.Dispose();
            }
        }

        // GET: ExpenseItems
        public ActionResult IndexItem(int expenseId)
        {
            // the current expense
            ViewBag.CurrentExpense = db.Expense.FirstOrDefault(q => q.Id == expenseId);

            // to get the expense items of the selected expense
            var expenseItem = db.ExpenseItem.Where(q => q.ExpenseId == expenseId).Include(e => e.Expense);

            // model 
            ExpenseItemModel model = new Models.ExpenseItemModel();
            model.ExpenseItem = expenseItem.ToList();
            model.TotalSum = totalSum;
            model.ExpenseID = expenseId;
            
            return View(model);

        }

        // GET: ExpenseItems/Details/5
        public ActionResult DetailsItem(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExpenseItem expenseItem = db.ExpenseItem.Find(id);
            if (expenseItem == null)
            {
                return HttpNotFound();
            }
            return View(expenseItem);
        }
        public ActionResult ExpenseHistoryIndex(int? id)
        {
            var expenseHistory = db.ExpenseHistory.Where(q => q.ExpenseId == id).Include(e => e.AspNetUsers).Include(e => e.Expense).Include(e => e.ExpenseStatus);
            return View(expenseHistory.ToList());
        }
    }
}