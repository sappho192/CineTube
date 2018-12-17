using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Cinetube.Models;
using Microsoft.AspNetCore.Http;

namespace Cinetube.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISession session;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            this.session = httpContextAccessor.HttpContext.Session;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AllMovies()
        {
            ViewData["Message"] = "영화 목록";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Authenticate(string ID, string PW)
        {
            if (ID == null || PW == null)
            {
                return RedirectToAction("Index", "Home");
            }

            using (var connection = new SqlConnection(GlobalVariables.connectionUrl))
            {
                var command = new SqlCommand($"DECLARE @ID INT = 0\r\nDECLARE @PW INT = 0\r\nSET @ID = (select 1 from 사용자 where ID IN (\'{ID}\'))\r\nSET @PW = (select 1 from 사용자 where ID = \'{ID}\' and PW = \'{PW}\')\r\n\r\nselect @ID as id, @PW as pw", connection);
                connection.Open();
                Console.WriteLine($"ID: {ID}, PW: {PW}");
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader[0] is DBNull ? 0 : Convert.ToInt32(reader[0]);
                        int pw = reader[1] is DBNull ? 0 : Convert.ToInt32(reader[1]); ;
                        Console.WriteLine($"ID correct: {id}, PW correct: {pw}");
                        if (id == 1 && pw == 1)
                        {
                            session.SetString("ID", ID);
                            session.SetString("Loggedin", "true");
                            session.SetString("SessionID", Guid.NewGuid().ToString());
                        }
                    }
                }
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            session.Clear();

            return RedirectToAction("Index", "Home");
        }

        public IActionResult SignUp(string ID, string PW, string name, string birth, string ssn, string phone, int PWHintNo, string PWAns)
        {
            if (ID == null || PW == null || name == null || birth == null || ssn == null || phone == null || PWAns == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (GlobalVariables.PhoneRegex.Match(phone).Success)
            {
                // 010 3942 3438
                phone = phone.Insert(7, "-").Insert(3, "-");
            }

            using (var connection = new SqlConnection(GlobalVariables.connectionUrl))
            {
                string commandStr =
                    $"DECLARE @NUM INT SET @NUM = (SELECT COUNT(*) FROM 사용자) IF (@NUM != 0) SET @NUM = (SELECT MAX(사용자번호) FROM 사용자) + 1 INSERT INTO 사용자 VALUES(@NUM,\'{ID}\',\'{PW}\',\'{name}\',\'{birth}\',{ssn},\'{phone}\');\r\nINSERT INTO 회원 VALUES(@NUM,0,{PWHintNo},\'{PWAns}\');";
                var command = new SqlCommand(commandStr, connection);
                connection.Open();
                var result = command.ExecuteNonQuery();
                Console.WriteLine($"ID: {ID}, PW: {PW}, name: {name}, birth: {birth}, 주민번호: {ssn}, phone: {phone}, PWHintNo: {PWHintNo}, PWAns: {PWAns}");
                Console.WriteLine($"Insert result: {(result == 2 ? "SUCCESS" : "FAILED")}");
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult MyPage()
        {
            string ID = String.Empty;
            foreach (String key in session.Keys)
            {
                if (key.Equals("Loggedin") && session.GetString(key) == "true")
                {
                    ID = session.GetString("ID");
                }
            }

            using (var connection = new SqlConnection(GlobalVariables.connectionUrl))
            {
                string 구매내역 =
                    $"DECLARE @MOVIENUM INT\r\nDECLARE @USERNUM INT\r\nSET @USERNUM = (SELECT 사용자번호 FROM 사용자 WHERE ID = \'{ID}\')\r\nSELECT 제목,영화.영화번호,구매번호 ,구매시각 ,만료일자 FROM 구매내역\r\n  INNER JOIN 영화 ON 구매내역.영화번호 = 영화.영화번호";
                string 충전내역 = $"SELECT 충전금액, 충전시각 FROM 충전내역\r\n  WHERE 사용자번호 = (SELECT 사용자번호 FROM 사용자 WHERE ID = \'{ID}\')";
                var commandPurchased = new SqlCommand(구매내역, connection);
                var commandCharged = new SqlCommand(충전내역, connection);
                connection.Open();

                // 영화번호, 구매번호, 구매시각, 만료일자
                List<PurchaseHistory> purchaseHistories = new List<PurchaseHistory>();
                using (var reader = commandPurchased.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string 영화제목 = reader[0] is DBNull ? string.Empty : Convert.ToString(reader[0]);
                        int 영화번호 = reader[0] is DBNull ? 0 : Convert.ToInt32(reader[1]);
                        int 구매번호 = reader[1] is DBNull ? 0 : Convert.ToInt32(reader[2]);
                        string 구매시각 = reader[2] is DBNull ? String.Empty : Convert.ToString(reader[3]);
                        string 만료일자 = reader[3] is DBNull ? String.Empty : Convert.ToString(reader[4]);
                        purchaseHistories.Add(new PurchaseHistory(영화제목, 영화번호, 구매번호, 구매시각, 만료일자));
                        Console.WriteLine($"영화제목: {영화제목}, 영화번호: {영화번호}, 구매번호: {구매번호}, 구매시각: {구매시각}, 만료일자: {만료일자}");
                    }
                }

                List<ChargeHistory> chargeHistories = new List<ChargeHistory>();
                using (var reader = commandCharged.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int 금액 = reader[0] is DBNull ? 0 : Convert.ToInt32(reader[0]);
                        string 충전시각 = reader[0] is DBNull ? string.Empty : Convert.ToString(reader[1]);
                        chargeHistories.Add(new ChargeHistory(금액, 충전시각));
                        Console.WriteLine($"충전 금액: {금액}, 충전시각: {충전시각}");
                    }
                }

                ViewData["PurchaseHistories"] = purchaseHistories;
                ViewData["ChargeHistories"] = chargeHistories;
            }

            //ViewData["ChargeHistory"] = 
            return View();
        }

        public IActionResult MyMovies()
        {
            return View();
        }

        public IActionResult Charge(string result = null)
        {
            if (result != null)
            {
                Console.WriteLine($"충전 결과: {result}");
                if (result.Equals("SUCCESS"))
                {
                    ViewData["Result"] = true;
                }
                else
                {
                    ViewData["Result"] = false;
                }
            }
            else
            {
                ViewData["Result"] = null;
            }

            return View();
        }

        public IActionResult doCharge(int price)
        {
            string ID = String.Empty;
            foreach (String key in session.Keys)
            {
                if (key.Equals("Loggedin") && session.GetString(key) == "true")
                {
                    ID = session.GetString("ID");
                }
            }

            using (var connection =
                new SqlConnection(
                    GlobalVariables.connectionUrl))
            {
                string commandStr =
                    $"DECLARE @USER INT = (SELECT 사용자번호 FROM 사용자 WHERE ID = \'{ID}\')\r\nDECLARE @NUM INT\r\nDECLARE @PRICE INT = {price}\r\nSET @NUM = (SELECT COUNT(*) FROM 충전내역 WHERE 사용자번호=@USER)\r\nIF (@NUM != 0) SET @NUM = (SELECT MAX(충전번호) FROM 충전내역 WHERE 사용자번호=@USER) + 1\r\n\r\nINSERT INTO 충전내역 VALUES(@USER, @NUM, @PRICE, GETDATE())\r\n\r\nUPDATE 회원\r\nSET 보유금액 += @PRICE\r\nWHERE 사용자번호 = @USER";
                Console.WriteLine(commandStr);
                var command = new SqlCommand(commandStr, connection);
                connection.Open();
                var result = command.ExecuteNonQuery();
                return RedirectToAction("Charge", "Home", new { result = "SUCCESS" });
            }
        }
    }
}
