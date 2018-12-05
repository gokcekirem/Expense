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
    [Authorize(Roles = "Accounting,Employee,Manager")]
    public class ExpensesController : Controller
    {
        private ExpenseEntities db = new ExpenseEntities(); //ExpenseEntities2.Instance;
        private ApplicationUserManager _userManager;
        
        public ExpensesController(ApplicationUserManager userManager) { _userManager = userManager; }

        public ExpensesController() { }

        // GET: Expenses
        public ActionResult Index()
        {
            string userId = User.Identity.GetUserId();
            //to get the expenses of the currently logged user
            var expense = db.Expense.Where(q => q.UserId == userId).Include(e => e.AspNetUsers).Include(e => e.ExpenseStatus);
            
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
            if (expense.StatusId != (int) ExpenseProject.Models.ExpenseStatus.Draft 
                && expense.StatusId != (int)ExpenseProject.Models.ExpenseStatus.ReturnedForCorrection)
            {
                return RedirectToAction("IndexItemWithoutCreate", "Expenses", new { expenseId = id });
            }
            return RedirectToAction("IndexItem","Expenses", new { expenseId=id });
        }

        // GET: Expenses/Create
        public ActionResult Create()
        {
           
            ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "Email");
            ViewBag.StatusId = new SelectList(db.ExpenseStatus, "Id", "Description");
            return View();
        }

        // POST: Expenses/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,UserId,Description,StatusId,RejectionReason,IsNotificationSent,NotificationDate")] Expense expense)
        {
            if (ModelState.IsValid)
            {
                string userId = User.Identity.GetUserId();
                expense.UserId = userId;
                expense.LastModifiedBy = userId;
                expense.RejectionReason = null;
                expense.StatusId = (int)ExpenseProject.Models.ExpenseStatus.Draft;
                expense.IsNotificationSent = false; //since its draft, notification is not sent
                expense.NotificationDate = null; //since notification is not sent, there is no date
                                
                db.Expense.Add(expense);
                db.SaveChanges();
                return RedirectToAction("Index","Expenses");
            }

            ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "Email", expense.UserId);
            ViewBag.StatusId = new SelectList(db.ExpenseStatus, "Id", "Description", expense.StatusId);
            return View(expense);
        }
        
        public ActionResult Send(int? id)
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
     
            return Send(expense);
        }
        [HttpPost]
       // [ValidateAntiForgeryToken]
        public ActionResult Send(Expense expense)
        {
            var temp = TempData["UserId"];
            db.Entry(expense).State = EntityState.Modified;
            expense.RejectionReason = null;
            expense.IsNotificationSent = false; 
            expense.NotificationDate = null;
            expense.UserId = (string)temp;
            expense.LastModifiedBy=(string)temp;
           // var stat = (int)ExpenseProject.Models.ExpenseStatus.ManagerApprovalPending; 
            expense.StatusId= (int)ExpenseProject.Models.ExpenseStatus.ManagerApprovalPending;
            
            db.SaveChanges();
            return RedirectToAction("Index", "Expenses");;
        }

        public ActionResult DeleteStatus(int? id)
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

            return DeleteStatus(expense);
        }
        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult DeleteStatus(Expense expense)
        {
            var temp = TempData["UserId"];
            db.Entry(expense).State = EntityState.Modified;
            expense.UserId = (string)temp;
            //var stat = (int)ExpenseProject.Models.ExpenseStatus.Deleted;
            expense.StatusId = (int)ExpenseProject.Models.ExpenseStatus.Deleted;
            expense.LastModifiedBy = (string)temp;
            db.SaveChanges();
            return RedirectToAction("Index", "Expenses");
        }
        // GET: Expenses/Edit/5
        public ActionResult Edit(int? id)
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
            return View(expense);
        }

        // POST: Expenses/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Description,StatusId,RejectionReason,IsNotificationSent,NotificationDate")] Expense expense)
        {
            if (ModelState.IsValid)
            {
                var temp = TempData["UserId"];
                db.Entry(expense).State = EntityState.Modified;
                expense.UserId = (string) temp;
                expense.LastModifiedBy = (string)temp;
                db.SaveChanges();
                return RedirectToAction("Index","Expenses");
            }
            ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "Email", expense.UserId);
            ViewBag.StatusId = new SelectList(db.ExpenseStatus, "Id", "Description", expense.StatusId);
            return View(expense);
        }

        // GET: Expenses/Delete/5
        public ActionResult Delete(int? id)
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
            return DeleteConfirmed(expense);
        }

        // POST: Expenses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Expense expense)
        {
            foreach (ExpenseItem expenseItem in db.ExpenseItem)
            {
                if (expenseItem.ExpenseId == expense.Id) {
                    db.ExpenseItem.Remove(expenseItem);
                }
            }
            db.Expense.Remove(expense);
            db.SaveChanges();
            return RedirectToAction("Index","Expenses");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
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
            model.ExpenseID = expenseId;
            
            return View(model);
            
        }
        public ActionResult IndexItemWithoutCreate(int expenseId)
        {

            // the current expense
            ViewBag.CurrentExpense = db.Expense.FirstOrDefault(q => q.Id == expenseId); 

            // to get the expense items of the selected expense
            var expenseItem = db.ExpenseItem.Where(q => q.ExpenseId == expenseId).Include(e => e.Expense);

            // model 
            ExpenseItemModel model = new Models.ExpenseItemModel();
            model.ExpenseItem = expenseItem.ToList();
            model.ExpenseID = expenseId;
            
            return View(model);

        }
        // GET: ExpenseItems/Create
        public ActionResult CreateItem(int expenseId)
        {
            TempData["Id"] = expenseId;
            return View();
        }

        // POST: ExpenseItems/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateItem([Bind(Include = "Id,ExpenseId,Amount,Date,Description")] ExpenseItem expenseItem)
        {
            if (ModelState.IsValid)
            {
                expenseItem.ExpenseId = (int)TempData["Id"];

                db.ExpenseItem.Add(expenseItem);

                db.SaveChanges();
                return RedirectToAction("IndexItem","Expenses",new {expenseId=expenseItem.ExpenseId });
            }

            ViewBag.ExpenseId = new SelectList(db.Expense, "Id", "UserId", expenseItem.ExpenseId);
            return View(expenseItem);
        }

        // GET: ExpenseItems/Edit/5
        public ActionResult EditItem(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExpenseItem expenseItem = db.ExpenseItem.Find(id);
            TempData["ExpenseId"] = expenseItem.ExpenseId;
            if (expenseItem == null)
            {
                return HttpNotFound();
            }
            ViewBag.ExpenseId = new SelectList(db.Expense, "Id", "UserId", expenseItem.ExpenseId);
            return View(expenseItem);
        }
          
        // POST: ExpenseItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
       // [ValidateAntiForgeryToken]
        public ActionResult EditItem([Bind(Include = "Id,ExpenseId,Amount,Date,Description")] ExpenseItem expenseItem)
        {
            if (ModelState.IsValid)
            {
                var temp = TempData["ExpenseId"];
                db.Entry(expenseItem).State = EntityState.Modified;
                expenseItem.ExpenseId = (int)temp;
                db.SaveChanges();
                return RedirectToAction("IndexItem","Expenses", new { expenseId = expenseItem.ExpenseId });
            }
            ViewBag.ExpenseId = new SelectList(db.Expense, "Id", "UserId", expenseItem.ExpenseId);
            return View(expenseItem);
        }

        // GET: ExpenseItems/Delete/5
        public ActionResult DeleteItem(int? id)
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
            //return View(expenseItem);
            return DeleteConfirmedItem(expenseItem);
        }

        // POST: ExpenseItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmedItem(ExpenseItem expenseItem)
        {
            var temporaryExpenseId = expenseItem.ExpenseId;
            db.ExpenseItem.Remove(expenseItem);
            expenseItem.ExpenseId = temporaryExpenseId; // cannot be null error
            db.SaveChanges();
            return RedirectToAction("IndexItem", "Expenses", new { expenseId = expenseItem.ExpenseId });
        }
        public ActionResult ExpenseHistoryIndex(int? id)
        {
            var expenseHistory = db.ExpenseHistory.Where(q => q.ExpenseId == id).Include(e => e.AspNetUsers).Include(e => e.Expense).Include(e => e.ExpenseStatus);
            return View(expenseHistory.ToList());
        }

    }
}
