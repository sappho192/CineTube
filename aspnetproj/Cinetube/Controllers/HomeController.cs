using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

            using (var connection = new SqlConnection("server = sappho192.iptime.org,21433;database = CinetubeDB2;uid=cinetube;pwd=qwer12#$;"))
            {
                var command = new SqlCommand($"DECLARE @ID INT = 0\r\nDECLARE @PW INT = 0\r\nSET @ID = (select 1 from 사용자 where ID IN (\'{ID}\'))\r\nSET @PW = (select 1 from 사용자 where ID = \'{ID}\' and PW = \'{PW}\')\r\n\r\nselect @ID as id, @PW as pw", connection);
                connection.Open();
                Console.WriteLine($"ID: {ID}, PW: {PW}");
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader[0] is DBNull ? 0: Convert.ToInt32(reader[0]);
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

            using (var connection = new SqlConnection("server = sappho192.iptime.org,21433;database = CinetubeDB2;uid=cinetube;pwd=qwer12#$;"))
            {
                string commandStr =
                    $"DECLARE @NUM INT SET @NUM = (SELECT COUNT(*) FROM 사용자) IF (@NUM != 0) SET @NUM = (SELECT MAX(사용자번호) FROM 사용자) + 1 INSERT INTO 사용자 VALUES(@NUM,\'{ID}\',\'{PW}\',\'{name}\',\'{birth}\',{ssn},\'{phone}\');\r\nINSERT INTO 회원 VALUES(@NUM,0,{PWHintNo},\'{PWAns}\');";
                var command = new SqlCommand(commandStr, connection);
                connection.Open();
                var result = command.ExecuteNonQuery();
                Console.WriteLine($"ID: {ID}, PW: {PW}, name: {name}, birth: {birth}, 주민번호: {ssn}, phone: {phone}, PWHintNo: {PWHintNo}, PWAns: {PWAns}");
                Console.WriteLine($"Insert result: {(result == 2 ? "SUCCESS": "FAILED")}");
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult MyPage()
        {
            return View();
        }

        public IActionResult MyMovies()
        {
            return View();
        }

        public IActionResult Charge()
        {
            return View();
        }
    }
}
