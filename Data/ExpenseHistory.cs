//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class ExpenseHistory
    {
        public int Id { get; set; }
        public string ModifiedBy { get; set; }
        public int ExpenseId { get; set; }
        public int StatusId { get; set; }
        public System.DateTime ModifyDate { get; set; }
        public string Description { get; set; }
        public string RejectionReason { get; set; }
    
        public virtual AspNetUsers AspNetUsers { get; set; }
        public virtual Expense Expense { get; set; }
        public virtual ExpenseStatus ExpenseStatus { get; set; }
    }
}
