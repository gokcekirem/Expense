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
    
    public partial class Expense
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Expense()
        {
            this.ExpenseHistory = new HashSet<ExpenseHistory>();
            this.ExpenseItem = new HashSet<ExpenseItem>();
        }
    
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Description { get; set; }
        public int StatusId { get; set; }
        public string RejectionReason { get; set; }
        public bool IsNotificationSent { get; set; }
        public Nullable<System.DateTime> NotificationDate { get; set; }
        public string LastModifiedBy { get; set; }
    
        public virtual AspNetUsers AspNetUsers { get; set; }
        public virtual ExpenseStatus ExpenseStatus { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ExpenseHistory> ExpenseHistory { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ExpenseItem> ExpenseItem { get; set; }
    }
}
