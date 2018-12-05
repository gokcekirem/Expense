using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data;

namespace ExpenseProject.Models
{
    public class ExpenseItemModel
    {

        public IEnumerable<Data.ExpenseItem> ExpenseItem;
        public decimal TotalSum;
        public int ExpenseID;

    }
    public enum ExpenseStatus
    {
        Draft = 0,
        ManagerApprovalPending = 1,
        AccountingPaymentPending = 2,
        ReturnedForCorrection = 3,
        Deleted = 4,
        Paid = 5
    }
}