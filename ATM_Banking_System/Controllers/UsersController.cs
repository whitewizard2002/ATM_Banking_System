﻿using ATM_Banking_System.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ATM_Banking_System.Controllers
{
    public class UsersController : Controller
    {
        private readonly IConfiguration configuration;
        public readonly IHttpContextAccessor httpContextAccessor;
        private readonly MyAppDbContext myAppDbContext;
        public string connectionString;

        public UsersController(MyAppDbContext myAppDbContext, IConfiguration iconfig, IHttpContextAccessor contxtAccessor)
        {
            this.myAppDbContext = myAppDbContext;
            this.configuration = iconfig;
            this.httpContextAccessor = contxtAccessor;
            this.connectionString = configuration.GetConnectionString("MyAppConnString");
        }

        [HttpGet]
        public IActionResult UserDashboard()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Deposit()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Deposit(UserDeposit ud)//Ready
        {
            string query = "SELECT currAmountOfATM FROM [Transaction] WHERE id= (SELECT MAX (Id) FROM [Transaction])";
            SqlConnection con = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(query, con);
            con.Open();
            SqlDataAdapter sda=new SqlDataAdapter(cmd);
            DataTable ds = new DataTable();
            sda.Fill(ds);
            con.Close();

            int amount= Convert.ToInt32(ds.Rows[0][0]);

            string query1 = "SELECT Balance FROM Users WHERE AccNo=@AccNo";
            SqlCommand cmd1 = new SqlCommand(query1, con);
            cmd1.Parameters.AddWithValue("@AccNo", Convert.ToInt64(httpContextAccessor.HttpContext.Session.GetInt32("AccNo")));
            con.Open();
            SqlDataAdapter sda1 = new SqlDataAdapter(cmd1);
            DataTable ds1 = new DataTable();
            sda1.Fill(ds1);
            con.Close();

            string query2 = " UPDATE Users SET Balance = @Balance WHERE AccNo=@AccNo;";
            SqlCommand cmd2 = new SqlCommand(query2,con);
            cmd2.Parameters.AddWithValue("@Balance", (Convert.ToInt32(ds1.Rows[0][0])+ud.Amount));
            cmd2.Parameters.AddWithValue("@AccNo", Convert.ToInt64(httpContextAccessor.HttpContext.Session.GetInt32("AccNo")));
            con.Open();
            cmd2.ExecuteNonQuery();
            con.Close();

            Transaction t =new Transaction();
            t.Type = "Deposit";
            t.AccNo = Convert.ToInt64(httpContextAccessor.HttpContext.Session.GetInt32("AccNo"));
            t.dtOfTransaction=DateTime.Now;
            t.Amount=ud.Amount;
            t.currAmountOfATM+=(amount+t.Amount);
            await myAppDbContext.Transaction.AddAsync(t);
            await myAppDbContext.SaveChangesAsync();

            return RedirectToAction("Deposit");
        }

        [HttpGet]
        public IActionResult Withdraw()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Withdraw(UserDeposit ud)//Ready
        {
            string query = "SELECT currAmountOfATM FROM [Transaction] WHERE id= (SELECT MAX (Id) FROM [Transaction])";
            SqlConnection con = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(query, con);
            con.Open();
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable ds = new DataTable();
            sda.Fill(ds);
            con.Close();

            int amount = Convert.ToInt32(ds.Rows[0][0]);

            string query1 = "SELECT Balance FROM Users WHERE AccNo=@AccNo";
            SqlCommand cmd1 = new SqlCommand(query1, con);
            cmd1.Parameters.AddWithValue("@AccNo", Convert.ToInt64(httpContextAccessor.HttpContext.Session.GetInt32("AccNo")));
            con.Open();
            SqlDataAdapter sda1 = new SqlDataAdapter(cmd1);
            DataTable ds1 = new DataTable();
            sda1.Fill(ds1);
            con.Close();

            string query2 = "UPDATE Users SET Balance = @Balance WHERE AccNo=@AccNo;";
            SqlCommand cmd2 = new SqlCommand(query2, con);
            cmd2.Parameters.AddWithValue("@Balance", (Convert.ToInt32(ds1.Rows[0][0]) - ud.Amount));
            cmd2.Parameters.AddWithValue("@AccNo", Convert.ToInt64(httpContextAccessor.HttpContext.Session.GetInt32("AccNo")));
            con.Open();
            cmd2.ExecuteNonQuery();
            con.Close();

            Transaction t = new Transaction();
            t.Type = "Withdraw";
            t.AccNo = Convert.ToInt64(httpContextAccessor.HttpContext.Session.GetInt32("AccNo"));
            t.dtOfTransaction = DateTime.Now;
            t.Amount = ud.Amount;
            t.currAmountOfATM = (amount - ud.Amount);
            await myAppDbContext.Transaction.AddAsync(t);
            await myAppDbContext.SaveChangesAsync();

            return RedirectToAction("Withdraw");

            //return View();
        }

        [HttpGet]
        public IActionResult BalanceInquiry()//Ready
        {
            SqlConnection con = new SqlConnection(connectionString);
            string query = "SELECT Balance FROM Users WHERE AccNo=@AccNo";
            SqlCommand cmd= new SqlCommand(query,con);
            cmd.Parameters.AddWithValue("@AccNo",Convert.ToInt64(httpContextAccessor.HttpContext.Session.GetInt32("AccNo")));
            con.Open();
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);
            con.Close();
            UserBalanceInquiry uBI = new UserBalanceInquiry();
            uBI.Balance = Convert.ToInt32(dt.Rows[0][0]);
            ViewData["Balance"] = uBI;
            return View();
        }

        [HttpGet]
        public IActionResult TransactionLog() //Ready
        {
            List<Transaction> lT=new List<Transaction>();
            SqlConnection con = new SqlConnection(connectionString);
            string query = "SELECT * FROM [Transaction] WHERE [AccNo]=@AccNo";
            SqlCommand cmd = new SqlCommand(query,con);
            cmd.Parameters.AddWithValue("@AccNo",Convert.ToInt64(httpContextAccessor.HttpContext.Session.GetInt32("AccNo")));
            con.Open();
            SqlDataReader sdr;
            sdr=cmd.ExecuteReader();
            while (sdr.Read())
            {
                lT.Add(new Transaction()
                {
                    Type = sdr["Type"].ToString(),
                    AccNo = Convert.ToInt32(sdr["AccNo"]),
                    dtOfTransaction = Convert.ToDateTime(sdr["dtOfTransaction"]),
                    Amount = Convert.ToInt32(sdr["Amount"]),
                });
            }
            con.Close();
            ViewData["TransactionData"] = lT;
            return View();
        }

        [HttpGet]
        public IActionResult ChangePIN()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChangePIN(UserChangePIN ucPIN)//Ready
        {
            string query = "UPDATE Users SET PIN=@PIN WHERE AccNo=@AccNo";
            SqlConnection con = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@AccNo",Convert.ToInt64(httpContextAccessor.HttpContext.Session.GetInt32("AccNo")));
            cmd.Parameters.AddWithValue("@PIN", ucPIN.PIN);
            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();

            return RedirectToAction("ChangePIN");
        }

        [HttpGet]
        public IActionResult FastCash()
        {

            return View();
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> FastCash(int denomination)
        {
            string query = "SELECT currAmountOfATM FROM [Transaction] WHERE id= (SELECT MAX (Id) FROM [Transaction])";
            SqlConnection con = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(query, con);
            con.Open();
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable ds = new DataTable();
            sda.Fill(ds);
            con.Close();

            int amount = Convert.ToInt32(ds.Rows[0][0]);

            string query1 = "SELECT Balance FROM Users WHERE AccNo=@AccNo";
            SqlCommand cmd1 = new SqlCommand(query1, con);
            cmd1.Parameters.AddWithValue("@AccNo", Convert.ToInt64(httpContextAccessor.HttpContext.Session.GetInt32("AccNo")));
            con.Open();
            SqlDataAdapter sda1 = new SqlDataAdapter(cmd1);
            DataTable ds1 = new DataTable();
            sda1.Fill(ds1);
            con.Close();

            string query2 = "UPDATE Users SET Balance = @Balance WHERE AccNo=@AccNo;";
            SqlCommand cmd2 = new SqlCommand(query2, con);
            cmd2.Parameters.AddWithValue("@Balance", (Convert.ToInt32(ds1.Rows[0][0]) - denomination));
            cmd2.Parameters.AddWithValue("@AccNo", Convert.ToInt64(httpContextAccessor.HttpContext.Session.GetInt32("AccNo")));
            con.Open();
            cmd2.ExecuteNonQuery();
            con.Close();

            Transaction t = new Transaction();
            t.Type = "Withdraw";
            t.AccNo = Convert.ToInt64(httpContextAccessor.HttpContext.Session.GetInt32("AccNo"));
            t.dtOfTransaction = DateTime.Now;
            t.Amount = denomination;
            t.currAmountOfATM = (amount - denomination);
            await myAppDbContext.Transaction.AddAsync(t);
            await myAppDbContext.SaveChangesAsync();

            //return "The value you clicked is " + denomination;
            return RedirectToAction("FastCash");
        }
    }
}
