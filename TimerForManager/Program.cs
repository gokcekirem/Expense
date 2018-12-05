using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Topshelf;
using Topshelf.Runtime;
using System.Data.Entity;
using Data;
using System.Net.Mail;

namespace TimerForManager
{

    public class MyService
    {
        public MyService(HostSettings settings)
        {
        }

        private SemaphoreSlim _semaphoreToRequestStop;
        private Thread _thread;
        private ExpenseEntities db = new ExpenseEntities();

        public void Start()
        {
            _semaphoreToRequestStop = new SemaphoreSlim(0);
            //DoWork();
            _thread = new Thread(DoWork);
            _thread.Start();
        }

        public void Stop()
        {
            _semaphoreToRequestStop.Release();
            _thread.Join();
        }

        private void DoWork()
        {

            while (true)
            {
                ////

                var expenseList = db.Expense.Where(q => q.StatusId == 1).Where(q => q.IsNotificationSent == false).Include(e => e.AspNetUsers).Include(e => e.ExpenseStatus);

                foreach (Expense expense in expenseList)
                {
                    using (var myDB = new ExpenseEntities())
                    {
                        var correspondingExpenseHistory = myDB.ExpenseHistory.Where(q => q.ExpenseId == expense.Id).Where(q => q.StatusId == myDB.ExpenseStatus.FirstOrDefault(o => o.Description == "Manager Approval Pending").Id);

                        List<ExpenseHistory> sortedHistoryList = correspondingExpenseHistory.OrderBy(o => o.ModifyDate).ToList();
                        
                        ExpenseHistory expenseHistory = sortedHistoryList.FirstOrDefault();
                        DateTime sendDate = expenseHistory.ModifyDate;
                        //if ((DateTime.Now - sendDate).TotalHours > 48)
                        if ((DateTime.Now - sendDate).TotalSeconds > 10)
                        {
                            Console.WriteLine("************");
                            Console.WriteLine(expense.Description + " needs a notification.");
                            Console.WriteLine("************");
                            
                            SendEmail(expense);
                            try
                            {
                                myDB.Expense.Find(expense.Id).IsNotificationSent = true;
                                myDB.Expense.Find(expense.Id).NotificationDate = DateTime.Now;
                                myDB.SaveChanges();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("There is a problem");
                            }
                        }
                    }
                }

                Console.WriteLine("checking...");
                
                if (_semaphoreToRequestStop.Wait(500 * 10 / 2))
                {
                    Console.WriteLine("Stopped");
                    break;
                }
            }
        }

        public void SendEmail(Expense expense)
        {
            // string senderEmail = System.Configuration.ConfigurationManager.AppSettings["SenderEmail"];
            // string senderPassword = System.Configuration.ConfigurationManager.AppSettings["SenderPassword"];
            using (SmtpClient client = new SmtpClient())
            {
                client.Timeout = 10000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                string subject = "Pending Expense";
                string body = "The Expense " + expense.Description + " needs manager approval.";
                MailMessage mailMessage = new MailMessage("irem.gokcek@veripark.com", "irem.gokcek@veripark.com", subject, body); // expense.AspNetUsers.Email
                mailMessage.IsBodyHtml = true;
                mailMessage.BodyEncoding = System.Text.Encoding.ASCII;
                mailMessage.Priority = MailPriority.Normal;
                client.Send(mailMessage);
                client.Dispose();
            }
        }

      /*  private ExpenseHistory getLastElement(IQueryable<ExpenseHistory> lst)
        {
            return lst.ToList()[lst.ToList().Count - 1];
        }*/
    }
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.StartAutomatically(); // Start the service automatically

                x.EnableServiceRecovery(rc =>
                {
                    rc.RestartService(1); // restart the service after 1 minute
                });


                x.Service<MyService>(s =>
                {
                    s.ConstructUsing(hostSettings => new MyService(hostSettings));
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsLocalSystem();

                x.SetDescription("MyDescription");
                x.SetDisplayName("MyDisplayName");
                x.SetServiceName("MyServiceName");

            });
        }
    }
}
