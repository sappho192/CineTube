using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
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

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

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
            string temp_ID = ID;
            string temp_PW = PW;
            if (ID == null || PW == null)
            {
                return RedirectToAction("Index", "Home");
            }

            using (var connection = new SqlConnection("server = sappho192.iptime.org,21433;database = CinetubeDB2;uid=cinetube;pwd=qwer12#$;"))
            {
                string myquery = "DECLARE @INPUT_ID VARCHAR(20)='" + temp_ID + "'\r\nDECLARE @INPUT_PW VARCHAR(20)='" + temp_PW +
                    "'\r\nDECLARE @ID INT = 0\r\nDECLARE @PW INT = 0\r\nDECLARE @UNUM INT = 0\r\nDECLARE @USERNO INT = 0\r\nSET @ID = (select 1 from 사용자 where ID IN(@INPUT_ID))" +
                    "\r\nIF @ID = 1\r\nSET @UNUM = (select 1 from 사용자, 관리자 where ID = @INPUT_ID and 사용자.사용자번호 = 관리자.사용자번호) " +
                    "\r\nIF @UNUM = 1\r\nSET @ID = 2\r\nIF @ID > 0\r\nSET @PW = (select 1 from 사용자 where ID = @INPUT_ID and PW = @INPUT_PW) " +
                    "\r\nSET @USERNO = (select 사용자번호 from 사용자 where ID = @INPUT_ID and PW = @INPUT_PW) " +
                    "\r\nselect @ID as id, @PW as pw, @USERNO as userno";
                var command = new SqlCommand(myquery, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = Convert.ToInt32(reader[0]);
                        var pw = Convert.ToInt32(reader[1]);
                        var userno = Convert.ToString(reader[2]);

                        if (id >= 1 && pw == 1)
                        {
                            session.SetString("userNo", userno);
                            session.SetString("ID", ID);
                            session.SetString("Loggedin", "true");
                            session.SetString("SessionID", Guid.NewGuid().ToString());
                            Console.WriteLine($"ID correct: {id}, PW correct: {pw}, userno: {userno}");
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

        public IActionResult Board()
        {
            return Board(1);
        }

        [Route("Home/[action]/{pageNum}")]
        [HttpGet]
        public IActionResult Board(int pageNum)
        {
            var list = new List<BoardModel>();
            int total = 0;
            int last_page = 0;

            using (var connection = new SqlConnection("server = sappho192.iptime.org,21433;database = CinetubeDB2;uid=cinetube;pwd=qwer12#$;"))
            {
                var command = new SqlCommand("SELECT COUNT(*) FROM 게시글", connection);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        total = Convert.ToInt32(reader[0]);
                    }
                }
                last_page = (total + 9) / 10;
                int five = (pageNum - 1) / 5;

                ViewData["curr"] = pageNum;
                ViewData["last"] = last_page;
                ViewData["prev"] = (pageNum == 1 ? 1 : 0);
                ViewData["next"] = (pageNum == last_page ? 1 : 0);
                ViewData["five"] = five;

                command = new SqlCommand("SELECT 게시글번호, ID, 제목, 작성시각 FROM 게시글, 사용자 WHERE 게시글.사용자번호=사용자.사용자번호 ORDER BY 게시글번호 DESC OFFSET "
                        + (pageNum - 1) * 10 + " ROWS FETCH NEXT 10 ROWS ONLY", connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var temp = new BoardModel
                        {
                            articleNo = Convert.ToInt32(reader[0]),
                            ID = Convert.ToString(reader[1]),
                            title = Convert.ToString(reader[2]),
                            writeTime = Convert.ToDateTime(reader[3])
                        };
                        list.Add(temp);
                    }
                }
            }

            return View(list);
        }

        [Route("Home/[action]/{articleNo}")]
        [HttpGet]
        public IActionResult Article(int articleNo)
        {
            var list = new List<SubarticleModel>();
            using (var connection = new SqlConnection("server = sappho192.iptime.org,21433;database = CinetubeDB2;uid=cinetube;pwd=qwer12#$;"))
            {
                var command = new SqlCommand("SELECT 게시글번호, ID, 제목, 작성시각, 내용 FROM 게시글, 사용자 WHERE 게시글.사용자번호=사용자.사용자번호 and 게시글번호=" + articleNo, connection);
                connection.Open();
                
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ViewData["articleNo"] = Convert.ToInt32(reader[0]);
                        ViewData["ID"] = Convert.ToString(reader[1]);
                        ViewData["title"] = Convert.ToString(reader[2]);
                        ViewData["writeTime"] = Convert.ToDateTime(reader[3]);
                        ViewData["context"] = Convert.ToString(reader[4]);
                    }
                }

                command = new SqlCommand("SELECT 댓글번호, ID, 내용 FROM 댓글, 사용자 WHERE 사용자.사용자번호=댓글.사용자번호 and 게시글번호=" + articleNo, connection);
                
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var temp = new SubarticleModel
                        {
                            subNo = Convert.ToInt32(reader[0]),
                            subID = Convert.ToString(reader[1]),
                            subContext = Convert.ToString(reader[2])
                        };
                        list.Add(temp);
                    }
                }
            }
            
            return View(list);
        }

        public IActionResult NewArticle()
        {
            return View();
        }

        public IActionResult AddArticle()
        {
            string title = Request.Form["title"].ToString();
            string context = Request.Form["context"].ToString();
            string userNo = session.GetString("userNo");
            
            using (var connection = new SqlConnection("server = sappho192.iptime.org,21433;database = CinetubeDB2;uid=cinetube;pwd=qwer12#$;"))
            {
                string myquery = "DECLARE @NUM INT\r\nSET @NUM = (SELECT COUNT(*) FROM 게시글)" +
                    "\r\nIF(@NUM != 0)\r\nSET @NUM = (SELECT MAX(게시글번호) FROM 게시글) + 1\r\nELSE SET @NUM = 1" +
                    "\r\nINSERT INTO 게시글 VALUES(@NUM, " + userNo + ", '" + title + "', '" + context + "', GETDATE());";

                var command = new SqlCommand(myquery, connection);
                connection.Open();
                command.ExecuteReader();
            }

            return RedirectToAction("Board", "Home");
        }

        public IActionResult NewSubarticle()
        {
            string articleNo = Request.Form["articleNo"].ToString();
            string context = Request.Form["context"].ToString();
            string userNo = session.GetString("userNo");

            using (var connection = new SqlConnection("server = sappho192.iptime.org,21433;database = CinetubeDB2;uid=cinetube;pwd=qwer12#$;"))
            {
                string myquery = "DECLARE @NUM INT\r\nDECLARE @ARTICLE INT=" + articleNo +
                    "\r\nSET @NUM = (SELECT COUNT(*) FROM 댓글 WHERE 게시글번호 = @ARTICLE)" +
                    "\r\nIF(@NUM != 0) SET @NUM = (SELECT MAX(댓글번호) FROM 댓글 WHERE 게시글번호 = @ARTICLE) + 1\r\nELSE SET @NUM = 1" +
                    "\r\nINSERT INTO 댓글 VALUES(@ARTICLE, @NUM, " + userNo + ", '" + context + "')";
                var command = new SqlCommand(myquery, connection);
                connection.Open();
                command.ExecuteReader();
            }
            
            return RedirectToAction("Article", "Home", new { id = articleNo });
        }
    }
}