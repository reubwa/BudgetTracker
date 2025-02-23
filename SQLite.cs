using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.SQLite;
namespace BudgetTracker
{
    public class SQLite
    {
        public static SQLiteConnection initConnection()
        {
            SQLiteConnection sqlConn;
            sqlConn = new SQLiteConnection("Data Source=budgetTracker.db;Version=3;New=True;Compress=True;");
            try
            {
                sqlConn.Open();
            }
            catch (Exception ex) 
            { 
            
            }
            return sqlConn;
        }
        public static void createTables()
        {
            SQLiteConnection sqlConn = initConnection();
            SQLiteCommand sqlComm;
            string createAccount = "CREATE TABLE IF NOT EXISTS Account (accountID int NOT NULL UNIQUE, name TEXT NOT NULL, interestRate REAL NOT NULL, balance REAL NOT NULL, type TEXT NOT NULL, PRIMARY KEY(accountID AUTOINCREMENT));";
            string createPayment = "CREATE TABLE IF NOT EXISTS Payment (paymentID int NOT NULL UNIQUE, name TEXT NOT NULL, category TEXT NOT NULL, date NUMERIC NOT NULL, cost REAL NOT NULL, accountID int NOT NULL, budgetID int, PRIMARY KEY(paymentID AUTOINCREMENT), FOREIGN KEY(accountID) REFERENCES Account(accountID) ON UPDATE CASCADE ON DELETE CASCADE, FOREIGN KEY(budgetID) REFERENCES Budget(budgetID) ON UPDATE CASCADE ON DELETE CASCADE);";
            string createBudget = "CREATE TABLE IF NOT EXISTS Budget (budgetID int NOT NULL UNIQUE, name TEXT NOT NULL, startingAmount REAL NOT NULL, goalID int, PRIMARY KEY(budgetID AUTOINCREMENT), FOREIGN KEY(goalID) REFERENCES Goal(goalID) ON UPDATE CASCADE ON DELETE CASCADE);";
            string createGoal = "CREATE TABLE IF NOT EXISTS Goal (goalID int NOT NULL UNIQUE, name TEXT NOT NULL, amount REAL NOT NULL, targetDate NUMERIC NOT NULL, PRIMARY KEY(goalID AUTOINCREMENT));";
            sqlComm = sqlConn.CreateCommand();
            sqlComm.CommandText = createAccount;
            sqlComm.ExecuteNonQueryAsync();
            sqlComm.CommandText = createPayment;
            sqlComm.ExecuteNonQueryAsync();
            sqlComm.CommandText = createBudget;
            sqlComm.ExecuteNonQueryAsync();
            sqlComm.CommandText = createGoal;
            sqlComm.ExecuteNonQueryAsync();
        }
        public static void readData()
        {
            SQLiteConnection sqlConn = initConnection();
            SQLiteCommand sqlComm;
            SQLiteDataReader sqlDre;
            sqlComm = sqlConn.CreateCommand();
            sqlComm.CommandText = "SELECT * FROM Account";
            sqlDre = sqlComm.ExecuteReader();
            while(sqlDre.Read()) 
            {
                var newAcc = new Account();
                newAcc.id = (int)(Int64)sqlDre.GetValue(sqlDre.GetOrdinal("accountID"));
                newAcc.name = (string)sqlDre.GetValue(sqlDre.GetOrdinal("name"));
                newAcc.interestRate = (double)sqlDre.GetValue(sqlDre.GetOrdinal("interestRate"));
                newAcc.balance = (double)sqlDre.GetValue(sqlDre.GetOrdinal("balance"));
                newAcc.type = (string)sqlDre.GetValue(sqlDre.GetOrdinal("type"));
                DB.accountTable.Add(newAcc);
            }
            sqlDre.Close();
            sqlComm.CommandText = "SELECT * FROM Budget";
            sqlDre = sqlComm.ExecuteReader();
            while (sqlDre.Read())
            {
                var newBud = new Budget();
                newBud.id = (Int64)sqlDre.GetValue(sqlDre.GetOrdinal("budgetID"));
                newBud.name = (string)sqlDre.GetValue(sqlDre.GetOrdinal("name"));
                if(sqlDre.GetValue(sqlDre.GetOrdinal("goalID")) is null){ newBud.goalID = 0; }
                else{newBud.goalID = (int)sqlDre.GetValue(sqlDre.GetOrdinal("goalID"));}
                DB.budgetTable.Add(newBud);
            }
            sqlDre.Close();
            sqlComm.CommandText = "SELECT * FROM Goal";
            sqlDre = sqlComm.ExecuteReader();
            while (sqlDre.Read()) 
            { 
                var newGoa = new Goal();
                newGoa.id = (Int64)sqlDre.GetValue(sqlDre.GetOrdinal("goalID"));
                newGoa.name = (string)sqlDre.GetValue(sqlDre.GetOrdinal("name"));
                newGoa.amount = (double)sqlDre.GetValue(sqlDre.GetOrdinal("amount"));
                var dateStr = sqlDre.GetValue(sqlDre.GetOrdinal("targetDate")).ToString();
                newGoa.targetDate = Convert.ToDateTime(dateStr);
                DB.goalTable.Add(newGoa);
            }
            sqlDre.Close();
            sqlComm.CommandText = "SELECT * FROM Payment";
            sqlDre = sqlComm.ExecuteReader();
            while (sqlDre.Read()) 
            {
                var newPay = new Payment();
                newPay.id = (Int64)sqlDre.GetValue(sqlDre.GetOrdinal("paymentID"));
                newPay.name = (string)sqlDre.GetValue(sqlDre.GetOrdinal("name"));
                newPay.category = (string)sqlDre.GetValue(sqlDre.GetOrdinal("category"));
                var dateStr = sqlDre.GetValue(sqlDre.GetOrdinal("date")).ToString();
                newPay.date =  Convert.ToDateTime(dateStr);
                newPay.cost = (double)sqlDre.GetValue(sqlDre.GetOrdinal("cost"));
                if (sqlDre.GetValue(sqlDre.GetOrdinal("accountID")) is null) { newPay.accountID = 0; }
                else { newPay.accountID = (int)sqlDre.GetValue(sqlDre.GetOrdinal("accountID")); }
                if (sqlDre.GetValue(sqlDre.GetOrdinal("budgetID")) is null) { newPay.budgetID = 0; }
                else { newPay.budgetID = (int)sqlDre.GetValue(sqlDre.GetOrdinal("budgetID")); }
                DB.paymentTable.Add(newPay);
            }
            sqlConn.Close();
        }
        public static void writeData()
        {
            SQLiteConnection sqlConn = initConnection();
            SQLiteCommand sqlComm;
            sqlComm = sqlConn.CreateCommand();
            foreach (Goal goal in DB.goalTable) { sqlComm.CommandText = "INSERT INTO Goal (goalID, name, amount, targetDate) VALUES (" + goal.id + ",'" + goal.name + "'," + goal.amount + "," + goal.targetDate + ");"; sqlComm.ExecuteNonQueryAsync(); }
            foreach (Payment payment in DB.paymentTable) { sqlComm.CommandText = "INSERT INTO Payment (paymentID, name, category, date, cost, accountID, budgetID) VALUES ("+payment.id+",'"+payment.name+"','"+payment.category+"',"+payment.date+","+payment.cost+","+payment.accountID+","+payment.budgetID+");"; sqlComm.ExecuteNonQueryAsync(); }
            foreach (Budget budget in DB.budgetTable) { sqlComm.CommandText = "INSERT INTO Budget (budgetID, name, startingAmount, goalID) VALUES (" + budget.id + ",'" + budget.name + "'," + budget.startingAmount + "," + budget.goalID + ");";sqlComm.ExecuteNonQueryAsync(); }
            foreach (Account account in DB.accountTable) { sqlComm.CommandText = "INSERT INTO Account (accountID, name, interestRate, balance, type) VALUES (" + account.id + ",'" + account.name + "'," + account.interestRate + "," + account.balance + ",'" + account.type + "');";sqlComm.ExecuteNonQueryAsync(); }
            sqlConn.Close();
        }

        public static void deleteItem(Goal goal)
        {
            SQLiteConnection sqlConn = initConnection();
            SQLiteCommand sqlComm;
            DB.goalTable.Remove(goal);
            sqlComm = sqlConn.CreateCommand();
            sqlComm.CommandText = "DELETE FROM Goal WHERE goalID = " + goal.id;
            sqlComm.ExecuteNonQueryAsync();
            sqlConn.Close();
        }

        public static void deleteItem(Payment payment)
        {
            SQLiteConnection sqlConn = initConnection();
            SQLiteCommand sqlComm;
            DB.paymentTable.Remove(payment);
            sqlComm = sqlConn.CreateCommand();
            sqlComm.CommandText = "DELETE FROM Payment WHERE paymentID = " + payment.id;
            sqlComm.ExecuteNonQueryAsync();
            sqlConn.Close();
        }

        public static void deleteItem(Budget budget)
        {
            SQLiteConnection sqlConn = initConnection();
            SQLiteCommand sqlComm;
            DB.budgetTable.Remove(budget);
            sqlComm = sqlConn.CreateCommand();
            sqlComm.CommandText = "DELETE FROM Budget WHERE budgetID = " + budget.id;
            sqlComm.ExecuteNonQueryAsync();
            sqlConn.Close();
        }

        public static void deleteItem(Account account)
        {
            SQLiteConnection sqlConn = initConnection();
            SQLiteCommand sqlComm;
            DB.accountTable.Remove(account);
            sqlComm = sqlConn.CreateCommand();
            sqlComm.CommandText = "DELETE FROM Account WHERE accountID = " + account.id;
            sqlComm.ExecuteNonQueryAsync();
            sqlConn.Close();
        }

        public static void updateItem(Goal updGoal)
        {
            SQLiteConnection sqlConn = initConnection();
            SQLiteCommand sqlComm;
            DB.goalTable[DB.goalTable.IndexOf(DB.goalTable.Find(x=>x.id==updGoal.id))] = updGoal;
            sqlComm = sqlConn.CreateCommand();
            sqlComm.CommandText = "UPDATE Goal SET name = '"+updGoal.name+"',"+updGoal.amount+","+updGoal.targetDate+" WHERE goalID = " + updGoal.id;
            sqlComm.ExecuteNonQueryAsync();
        }

        public static void updateItem(Payment payment)
        {
            SQLiteConnection sqlConn = initConnection();
            SQLiteCommand sqlComm;
            DB.paymentTable[DB.paymentTable.IndexOf(DB.paymentTable.Find(x => x.id==payment.id))] = payment;
            sqlComm = sqlConn.CreateCommand();
            sqlComm.CommandText = "UPDATE Payment SET name = '"+payment.name+"','"+payment.category+"',"+payment.date+","+payment.cost+","+payment.accountID+","+payment.budgetID+" WHERE paymentID = " + payment.id;
            sqlComm.ExecuteNonQueryAsync();
        }

        public static void updateItem(Budget budget)
        {
            SQLiteConnection sqlConn = initConnection();
            SQLiteCommand sqlComm;
            DB.budgetTable[DB.budgetTable.IndexOf(DB.budgetTable.Find(x => x.id == budget.id))] = budget;
            sqlComm = sqlConn.CreateCommand();
            sqlComm.CommandText = "UPDATE Budget SET name = '"+budget.name+"',"+budget.startingAmount+","+budget.goalID+" WHERE budgetID = " + budget.id;
            sqlComm.ExecuteNonQueryAsync();
        }

        public static void updateItem(Account account)
        {
            SQLiteConnection sqlConn = initConnection();
            SQLiteCommand sqlComm;
            DB.accountTable[DB.accountTable.IndexOf(DB.accountTable.Find(x => x.id == account.id))] = account;
            sqlComm = sqlConn.CreateCommand();
            sqlComm.CommandText = "UPDATE Account SET name = '" + account.name+"',"+account.interestRate+","+account.balance+",'"+account.type+"'" + " WHERE accountID = " + account.id;
            sqlComm.ExecuteNonQueryAsync();
        }
    }
    public static class DB
    {
        public static List<Goal> goalTable = new List<Goal>();
        public static List<Payment> paymentTable = new List<Payment>();
        public static List<Budget> budgetTable = new List<Budget>();
        public static List<Account> accountTable = new List<Account>();
    }
    public class Goal
    {
        public Int64 id { get; set; }
        public string name { get; set; }
        public double amount { get; set; }
        public DateTime targetDate { get; set; }
    }
    public class Payment
    {
        public Int64 id { get; set; }
        public string name { get; set; }
        public string category { get; set; }
        public DateTime date { get; set; }
        public double cost { get; set; }
        public int accountID { get; set; }
        public int budgetID { get; set; }
    }
    public class Budget
    {
        public Int64 id { get; set; }
        public string name { get; set; }
        public double startingAmount { get; set; }
        public int goalID { get; set; }
    }
    public class Account
    {
        public Int64 id { get; set;}
        public string name { get; set; }
        public double interestRate { get; set; }
        public double balance { get; set; }
        public string type { get; set; }
    }
}
