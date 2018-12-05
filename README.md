# Expense
Expense Project

I developed  this project during my summer internship in Istanbul in 2018.

The main purpose is to send an expense to manager's approval so that the employee can get his/her money from accounting. 
When the application is started, username and password is asked. Only a manager can register another user. 
After logging in, the user is directed to a page depending on their role (is the user an employee or a manager or someone from accouting?)

On this page, an employee can add their expense with its items. 
(For example, Expense: Trip to Stokholm, Expense Items: Flight tickets, taxi to the airpor, etc.)
After adding the expense, the employee sends it to his/her manager for approval.

The manager can see the details of the expense, approve or reject it. When rejecting, metions the reasoning behing that decision.
If a manager doesn't approve or reject an expense in 48 hours, he/she receives a reminding email.

After manager approval, the expense is sent to the accounting. The accounting pays the employee the total amount of the expense. 
The employee receives an email stating that his/her expense has been paid. 

IMPORTANT:
The database is not connected to the project, so it will give errors when you clone it. It was impossible for me to upload the Expense Server.

There is a screenshot provided for you to see the database design. For more details, you can check the Data folder. 
