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
using ExpenseProject.Models;
using Data;

namespace ExpenseProject.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ExpensesForManagerController : Controller
    {
        private ExpenseEntities db = new ExpenseEntities(); //ExpenseEntities2.Instance;
        private ApplicationUserManager _userManager;

        public decimal totalSum;

        public ExpensesForManagerController(ApplicationUserManager userManager) { _userManager = userManager; }

        public ExpensesForManagerController() { }

        // GET: Expenses
        public ActionResult Index(int? StatusId)
        {
            string userId = User.Identity.GetUserId();

            var expenseStatus = new List<SelectListItem>();

            expenseStatus.Add(new SelectListItem { Text = "All", Value = "-1"});
            expenseStatus.Add(new SelectListItem { Text = "Pending", Value = ((int)ExpenseProject.Models.ExpenseStatus.ManagerApprovalPending).ToString() });
            expenseStatus.Add(new SelectListItem { Text = "Accepted", Value = ((int)ExpenseProject.Models.ExpenseStatus.AccountingPaymentPending).ToString() }); // can be already paid as well
            expenseStatus.Add(new SelectListItem { Text = "Rejected", Value = ((int)ExpenseProject.Models.ExpenseStatus.ReturnedForCorrection).ToString() });

            ViewBag.expenseStatus = expenseStatus;

            if (StatusId == null || StatusId == -1)
            {
                var accountingPaymentPendingExpenses = db.Expense.Where(q => q.StatusId == (int)ExpenseProject.Models.ExpenseStatus.AccountingPaymentPending).Include(e => e.AspNetUsers).Include(e => e.ExpenseStatus);
                var paidExpenses = db.Expense.Where(q => q.StatusId == (int)ExpenseProject.Models.ExpenseStatus.Paid).Include(e => e.AspNetUsers).Include(e => e.ExpenseStatus);
                var managerApprovalPendingExpenses = db.Expense.Where(q => q.StatusId == (int)ExpenseProject.Models.ExpenseStatus.ManagerApprovalPending).Include(e => e.AspNetUsers).Include(e => e.ExpenseStatus);
                var returnedExpenses = db.Expense.Where(q => q.StatusId == (int)ExpenseProject.Models.ExpenseStatus.ReturnedForCorrection).Include(e => e.AspNetUsers).Include(e => e.ExpenseStatus);
                
                accountingPaymentPendingExpenses.ToList().AddRange(paidExpenses.ToList());
                var expenseLst1 = accountingPaymentPendingExpenses.ToList().Concat(paidExpenses.ToList());
                managerApprovalPendingExpenses.ToList().AddRange(returnedExpenses.ToList());
                var expenseLst2 = managerApprovalPendingExpenses.ToList().Concat(returnedExpenses.ToList());
                expenseLst1.ToList().AddRange(expenseLst2.ToList());
                var allExpenses = expenseLst1.ToList().Concat(expenseLst2.ToList());

                ViewBag.selected = "All";
                return View(allExpenses);
            }
            else if (StatusId == (int)ExpenseProject.Models.ExpenseStatus.AccountingPaymentPending) // Accepted : Paid or Accounting payment pending
            {
                var accountingPaymentPendingExpenses = db.Expense.Where(q => q.StatusId == (int)ExpenseProject.Models.ExpenseStatus.AccountingPaymentPending).Include(e => e.AspNetUsers).Include(e => e.ExpenseStatus);
                var paidExpenses = db.Expense.Where(q => q.StatusId == (int)ExpenseProject.Models.ExpenseStatus.Paid).Include(e => e.AspNetUsers).Include(e => e.ExpenseStatus);
                accountingPaymentPendingExpenses.ToList().AddRange(paidExpenses.ToList());
                var acceptedExpenses = accountingPaymentPendingExpenses.ToList().Concat(paidExpenses.ToList());

                ViewBag.selected = "Accepted";
                return View(acceptedExpenses);
            }
            
            var expense = db.Expense.Where(q => q.StatusId == (int)StatusId).Include(e => e.AspNetUsers).Include(e => e.ExpenseStatus);

            if (StatusId == (int)ExpenseProject.Models.ExpenseStatus.ReturnedForCorrection)
                ViewBag.selected = "Rejected";
            if (StatusId == (int)ExpenseProject.Models.ExpenseStatus.ManagerApprovalPending)
                ViewBag.selected = "Pending";

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

            return RedirectToAction("IndexItem", "ExpensesForManager", new { expenseId = id });
        }

        /* Approve button */
        
        public ActionResult Approve(int? id)
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
       
            return Approve(expense);
        }
        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Approve(Expense expense)
        {
            string currentUserId = User.Identity.GetUserId();
            var temp = TempData["UserId"];
            db.Entry(expense).State = EntityState.Modified;
            expense.UserId = (string)temp;
            expense.StatusId = (int)ExpenseProject.Models.ExpenseStatus.AccountingPaymentPending;
            expense.LastModifiedBy = currentUserId;
            db.SaveChanges();
            return RedirectToAction("Index", "ExpensesForManager");
        }
        
        public ActionResult Reject(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Expense expense = db.Expense.Find(id);
            TempData["UserId"] = expense.UserId;
            TempData["Description"] = expense.Description;
            if (expense == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "Email", expense.UserId);
            ViewBag.StatusId = new SelectList(db.ExpenseStatus, "Id", "Description", expense.StatusId);
            return View(expense);
        }

        // POST: Expenses/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reject([Bind(Include = "Id,Description,StatusId,RejectionReason,IsNotificationSent,NotificationDate")] Expense expense)
        {
            if (ModelState.IsValid)
            {
                string currentUserId = User.Identity.GetUserId();
                var temp = TempData["UserId"];
                var desc = TempData["Description"];
                db.Entry(expense).State = EntityState.Modified;
                expense.UserId = (string)temp;
                expense.Description = (string) desc;
                expense.StatusId = (int)ExpenseProject.Models.ExpenseStatus.ReturnedForCorrection;
                expense.LastModifiedBy = currentUserId;
                db.SaveChanges();
                return RedirectToAction("Index", "ExpensesForManager");
            }
            ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "Email", expense.UserId);
            ViewBag.StatusId = new SelectList(db.ExpenseStatus, "Id", "Description", expense.StatusId);
            return View(expense);
        }
        
        // GET: ExpenseItems
        public ActionResult IndexItem(int expenseId)
        {

            // the current expense
            ViewBag.CurrentExpense = db.Expense.FirstOrDefault(q => q.Id == expenseId); 

            // to get the expense items of the selected expense
            var expenseItem = db.ExpenseItem.Where(q => q.ExpenseId == expenseId).Include(e => e.Expense);

            // model 
            Models.ExpenseItemModel model = new Models.ExpenseItemModel();
            model.ExpenseItem = expenseItem.ToList();
            model.TotalSum = totalSum;
            model.ExpenseID = expenseId;

            return View(model);

        }
        public ActionResult ExpenseHistoryIndex(int? id)
        {
            var expenseHistory = db.ExpenseHistory.Where(q => q.ExpenseId == id).Include(e => e.AspNetUsers).Include(e => e.Expense).Include(e => e.ExpenseStatus);
            return View(expenseHistory.ToList());
        }
    }
}