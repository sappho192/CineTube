using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
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
            session = httpContextAccessor.HttpContext.Session;
        }

        public IActionResult Index()
        {
            using (var connection = new SqlConnection(GlobalVariables.connectionUrl))
            {
                int index = 0;
                string commandRecentPageStr =
                    $"SELECT TOP 3 * FROM (SELECT 영화번호,제목,금액,예고편경로,영화경로,개봉연도,줄거리,관람제한,영화시간,제작사,감독\r\nFROM 영화 WHERE 관람제한 != \'청소년 관람불가\'\r\nORDER BY 영화번호 DESC OFFSET {index}\r\nROWS) AS A";

                var commandRecentPage = new SqlCommand(commandRecentPageStr, connection);
                connection.Open();
                using (var reader = commandRecentPage.ExecuteReader())
                {
                    List<MovieInfo> recentMoviesInfo = new List<MovieInfo>();
                    while (reader.Read())
                    {
                        // 영화번호,제목,금액,예고편경로,영화경로,개봉연도,줄거리,관람제한,영화시간,제작사,감독
                        int 영화번호 = reader[0] is DBNull ? 0 : Convert.ToInt32(reader[0]);
                        string 제목 = reader[1] is DBNull ? string.Empty : Convert.ToString(reader[1]);
                        int 금액 = reader[2] is DBNull ? 0 : Convert.ToInt32(reader[2]);
                        string 예고편경로 = reader[3] is DBNull ? String.Empty : Convert.ToString(reader[3]);
                        string 영화경로 = reader[4] is DBNull ? String.Empty : Convert.ToString(reader[4]);
                        int 개봉연도 = reader[5] is DBNull ? 0 : Convert.ToInt32(reader[5]);
                        string 줄거리 = reader[6] is DBNull ? String.Empty : Convert.ToString(reader[6]);
                        string 관람제한 = reader[7] is DBNull ? String.Empty : Convert.ToString(reader[7]);
                        int 영화시간 = reader[8] is DBNull ? 0 : Convert.ToInt32(reader[8]);
                        string 제작사 = reader[9] is DBNull ? String.Empty : Convert.ToString(reader[9]);
                        string 감독 = reader[10] is DBNull ? String.Empty : Convert.ToString(reader[10]);

                        // 해당 영화의 장르 긁어오기
                        List<string> 장르들 = new List<string>();
                        using (var genreConnection = new SqlConnection(GlobalVariables.connectionUrl))
                        {
                            string commandMovieGenreStr =
                                $"SELECT 장르　FROM 장르들　WHERE 영화번호 = {영화번호}";
                            var commandMovieGenre = new SqlCommand(commandMovieGenreStr, genreConnection);
                            genreConnection.Open();
                            using (var genreReader = commandMovieGenre.ExecuteReader())
                            {
                                while (genreReader.Read())
                                {
                                    장르들.Add(Convert.ToString(genreReader[0]));
                                }
                            }

                        }

                        // 해당 영화의 한줄평 긁어오기
                        List<MovieComment> 한줄평들 = new List<MovieComment>();
                        using (var commentConnection = new SqlConnection(GlobalVariables.connectionUrl))
                        {
                            string commandMovieCommentStr =
                                $"SELECT 한줄평.사용자번호,ID,한줄평내용,평점,작성시각 FROM 한줄평\r\nINNER JOIN 사용자 ON 한줄평.사용자번호 = 사용자.사용자번호\r\nWHERE 영화번호 = {영화번호}";
                            var commandMovieComment = new SqlCommand(commandMovieCommentStr, commentConnection);
                            commentConnection.Open();
                            using (var commentReader = commandMovieComment.ExecuteReader())
                            {
                                //사용자번호,ID,한줄평내용,평점,작성시각
                                while (commentReader.Read())
                                {
                                    int 사용자번호 = commentReader[0] is DBNull ? 0 : Convert.ToInt32(commentReader[0]);
                                    string 아이디 = commentReader[1] is DBNull ? string.Empty : Convert.ToString(commentReader[1]);
                                    string 내용 = commentReader[2] is DBNull ? string.Empty : Convert.ToString(commentReader[2]);
                                    float 평점 = commentReader[3] is DBNull ? 0 : Convert.ToSingle(commentReader[3]);
                                    string 작성시각 = commentReader[4] is DBNull ? string.Empty : Convert.ToString(commentReader[4]);
                                    한줄평들.Add(new MovieComment(사용자번호, 아이디, 내용, 평점, 작성시각));
                                }
                            }
                        }
                        recentMoviesInfo.Add(new MovieInfo(영화번호, 제목, 금액, 예고편경로, 영화경로, 개봉연도, 줄거리, 관람제한, 영화시간, 제작사, 감독, 장르들, 한줄평들));
                    }

                    ViewData["RecentMoviesInfo"] = recentMoviesInfo;
                }
            }

            return View();
        }

        public IActionResult AllMovies(string result = null)
        {
            if (result != null)
            {
                Console.WriteLine($"구매 결과: {result}");
                ViewData["Result"] = result;
            }
            else
            {
                ViewData["Result"] = null;
            }
            // 잔액확인하기
            if (session.Keys.Contains("Loggedin"))
            {
                string userNo = getCurrentUserNo();
                int balance = 0;
                using (var connection = new SqlConnection(GlobalVariables.connectionUrl))
                {
                    string balanceStr =
                        $"SELECT 보유금액 FROM 회원 WHERE (사용자번호 = {userNo})";
                    var commandBalanceInfo = new SqlCommand(balanceStr, connection);
                    connection.Open();
                    using (var reader = commandBalanceInfo.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            balance = reader[0] is DBNull ? 0 : Convert.ToInt32(reader[0]);
                        }
                    }
                }

                ViewData["Balance"] = balance;
            }

            // 최근 영화 긁어오기
            using (var connection = new SqlConnection(GlobalVariables.connectionUrl))
            {
                int index = 0;
                string commandGeneralPageStr =
                    $"SELECT TOP 10 * FROM (SELECT 영화번호,제목,금액,예고편경로,영화경로,개봉연도,줄거리,관람제한,영화시간,제작사,감독\r\nFROM 영화 WHERE 관람제한 != \'청소년 관람불가\'\r\nORDER BY 영화번호 DESC OFFSET {index}\r\nROWS) AS A";
                var commandGeneralPage = new SqlCommand(commandGeneralPageStr, connection);
                connection.Open();
                using (var reader = commandGeneralPage.ExecuteReader())
                {
                    List<MovieInfo> moviesInfo = new List<MovieInfo>();
                    while (reader.Read())
                    {
                        // 영화번호,제목,금액,예고편경로,영화경로,개봉연도,줄거리,관람제한,영화시간,제작사,감독
                        int 영화번호 = reader[0] is DBNull ? 0 : Convert.ToInt32(reader[0]);
                        string 제목 = reader[1] is DBNull ? string.Empty : Convert.ToString(reader[1]);
                        int 금액 = reader[2] is DBNull ? 0 : Convert.ToInt32(reader[2]);
                        string 예고편경로 = reader[3] is DBNull ? String.Empty : Convert.ToString(reader[3]);
                        string 영화경로 = reader[4] is DBNull ? String.Empty : Convert.ToString(reader[4]);
                        int 개봉연도 = reader[5] is DBNull ? 0 : Convert.ToInt32(reader[5]);
                        string 줄거리 = reader[6] is DBNull ? String.Empty : Convert.ToString(reader[6]);
                        string 관람제한 = reader[7] is DBNull ? String.Empty : Convert.ToString(reader[7]);
                        int 영화시간 = reader[8] is DBNull ? 0 : Convert.ToInt32(reader[8]);
                        string 제작사 = reader[9] is DBNull ? String.Empty : Convert.ToString(reader[9]);
                        string 감독 = reader[10] is DBNull ? String.Empty : Convert.ToString(reader[10]);

                        // 해당 영화의 장르 긁어오기
                        List<string> 장르들 = new List<string>();
                        using (var genreConnection = new SqlConnection(GlobalVariables.connectionUrl))
                        {
                            string commandMovieGenreStr =
                                $"SELECT 장르　FROM 장르들　WHERE 영화번호 = {영화번호}";
                            var commandMovieGenre = new SqlCommand(commandMovieGenreStr, genreConnection);
                            genreConnection.Open();
                            using (var genreReader = commandMovieGenre.ExecuteReader())
                            {
                                while (genreReader.Read())
                                {
                                    장르들.Add(Convert.ToString(genreReader[0]));
                                }
                            }

                        }

                        // 해당 영화의 한줄평 긁어오기
                        List<MovieComment> 한줄평들 = new List<MovieComment>();
                        using (var commentConnection = new SqlConnection(GlobalVariables.connectionUrl))
                        {
                            string commandMovieCommentStr =
                                $"SELECT TOP 5 한줄평.사용자번호,ID,한줄평내용,평점,작성시각 FROM 한줄평\r\nINNER JOIN 사용자 ON 한줄평.사용자번호 = 사용자.사용자번호\r\nWHERE 영화번호 = {영화번호} ORDER BY 작성시각 DESC";
                            var commandMovieComment = new SqlCommand(commandMovieCommentStr, commentConnection);
                            commentConnection.Open();
                            using (var commentReader = commandMovieComment.ExecuteReader())
                            {
                                //사용자번호,ID,한줄평내용,평점,작성시각
                                while (commentReader.Read())
                                {
                                    int 사용자번호 = commentReader[0] is DBNull ? 0 : Convert.ToInt32(commentReader[0]);
                                    string 아이디 = commentReader[1] is DBNull ? string.Empty : Convert.ToString(commentReader[1]);
                                    string 내용 = commentReader[2] is DBNull ? string.Empty : Convert.ToString(commentReader[2]);
                                    float 평점 = commentReader[3] is DBNull ? 0 : Convert.ToSingle(commentReader[3]);
                                    string 작성시각 = commentReader[4] is DBNull ? string.Empty : Convert.ToString(commentReader[4]);
                                    한줄평들.Add(new MovieComment(사용자번호, 아이디, 내용, 평점, 작성시각));
                                }
                            }
                        }
                        moviesInfo.Add(new MovieInfo(영화번호, 제목, 금액, 예고편경로, 영화경로, 개봉연도, 줄거리, 관람제한, 영화시간, 제작사, 감독, 장르들, 한줄평들));
                    }

                    ViewData["MoviesInfo"] = moviesInfo;
                }
            }

            ViewData["Title"] = "영화 찾기";
            return View();
        }

        public IActionResult NewMovieComment(string content, int userNo, int movieNum, float grade)
        {
            using (var connection = new SqlConnection(GlobalVariables.connectionUrl))
            {
                string commandNewCommentStr =
                    $"INSERT INTO 한줄평 VALUES({movieNum}, {userNo}, \'{content}\', {grade}, GETDATE())";
                var command = new SqlCommand(commandNewCommentStr, connection);
                connection.Open();
                command.ExecuteReader();
            }

            return RedirectToAction("AllMovies", "Home");
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

            using (var connection = new SqlConnection(GlobalVariables.connectionUrl))
            {
                var command = new SqlCommand($"DECLARE @INPUT_ID VARCHAR(20) = \'{ID}\'\r\nDECLARE @INPUT_PW VARCHAR(20) = \'{PW}\'\r\nDECLARE @ID INT = 0\r\nDECLARE @PW INT = 0\r\nDECLARE @UNUM INT = 0\r\nDECLARE @USERNO INT = 0\r\n\r\nSET @ID = (select 1 from 사용자 where ID IN (@INPUT_ID))\r\nIF @ID=1\r\n   SET @UNUM = (select 1 from 사용자, 관리자 where ID=@INPUT_ID and 사용자.사용자번호=관리자.사용자번호)\r\nIF @UNUM=1\r\n   SET @ID=2\r\nIF @ID>0\r\n   SET @PW = (select 1 from 사용자 where ID=@INPUT_ID and PW=@INPUT_PW)\r\nSET @USERNO = (select 사용자번호 from 사용자 where ID=@INPUT_ID and PW=@INPUT_PW)\r\n\r\nselect @ID as id, @PW as pw, @USERNO as userno", connection);
                connection.Open();
                Console.WriteLine($"ID: {ID}, PW: {PW}");
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader[0] is DBNull ? 0 : Convert.ToInt32(reader[0]);
                        int pw = reader[1] is DBNull ? 0 : Convert.ToInt32(reader[1]);
                        int userno = reader[2] is DBNull ? 0 : Convert.ToInt32(reader[2]);

                        if (id >= 1 && pw == 1)
                        {
                            session.SetString("userNo", userno.ToString());
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
                    $"DECLARE @NUM INT \r\nSET @NUM = (SELECT COUNT(*) FROM 사용자) \r\nIF (@NUM != 0) SET @NUM = (SELECT MAX(사용자번호) FROM 사용자) + 1 \r\nELSE SET @NUM = 1\r\nINSERT INTO 사용자 VALUES(@NUM,\'{ID}\',\'{PW}\',\'{name}\',\'{birth}\',{ssn},\'{phone}\');\r\nINSERT INTO 회원 VALUES(@NUM,0,{PWHintNo},\'{PWAns}\');";
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
            var ID = getCurrentID();

            using (var connection = new SqlConnection(GlobalVariables.connectionUrl))
            {
                string 회원정보 =
                    $"SELECT ID ,이름, 생년월일,핸드폰번호, 보유금액\r\n  FROM 사용자\r\n  INNER JOIN 회원 ON 사용자.사용자번호 = 회원.사용자번호 WHERE ID=\'{ID}\'";
                string 구매내역 =
                    $"DECLARE @MOVIENUM INT\r\nDECLARE @USERNUM INT\r\nSET @USERNUM = (SELECT 사용자번호 FROM 사용자 WHERE ID = \'{ID}\')\r\nSELECT 제목,영화.영화번호,구매번호 ,구매시각 ,만료일자 FROM 구매내역\r\n  INNER JOIN 영화 ON 구매내역.영화번호 = 영화.영화번호 WHERE 구매내역.사용자번호 = @USERNUM";
                string 충전내역 = $"SELECT 충전금액, 충전시각 FROM 충전내역\r\n  WHERE 사용자번호 = (SELECT 사용자번호 FROM 사용자 WHERE ID = \'{ID}\')";
                var commandUserInfo = new SqlCommand(회원정보, connection);
                var commandPurchased = new SqlCommand(구매내역, connection);
                var commandCharged = new SqlCommand(충전내역, connection);
                connection.Open();
                using (var reader = commandUserInfo.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // ID, 이름, 생년월일, 폰번호, 잔액
                        string userID = reader[0] is DBNull ? string.Empty : Convert.ToString(reader[0]);
                        string 이름 = reader[1] is DBNull ? String.Empty : Convert.ToString(reader[1]);
                        string 생년월일 = reader[2] is DBNull ? String.Empty : Convert.ToString(reader[2]);
                        string 폰번호 = reader[3] is DBNull ? String.Empty : Convert.ToString(reader[3]);
                        int 잔액 = reader[4] is DBNull ? 0 : Convert.ToInt32(reader[4]);
                        ViewData["UserInfo"] = new UserInfo(userID, 이름, 생년월일, 폰번호, 잔액);
                        Console.WriteLine($"ID: {userID}, 이름: {이름}, 생년월일: {생년월일}, 폰번호: {폰번호}");
                    }
                }

                // 영화번호, 구매번호, 구매시각, 만료일자
                List<PurchaseHistory> purchaseHistories = new List<PurchaseHistory>();
                using (var reader = commandPurchased.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string 영화제목 = reader[0] is DBNull ? string.Empty : Convert.ToString(reader[0]);
                        int 영화번호 = reader[1] is DBNull ? 0 : Convert.ToInt32(reader[1]);
                        int 구매번호 = reader[2] is DBNull ? 0 : Convert.ToInt32(reader[2]);
                        string 구매시각 = reader[3] is DBNull ? String.Empty : Convert.ToString(reader[3]);
                        string 만료일자 = reader[4] is DBNull ? String.Empty : Convert.ToString(reader[4]);
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

            ViewData["Title"] = "내 정보";
            return View();
        }

        private string getCurrentID()
        {
            string ID = String.Empty;
            foreach (String key in session.Keys)
            {
                if (key.Equals("Loggedin") && session.GetString(key) == "true")
                {
                    ID = session.GetString("ID");
                }
            }

            return ID;
        }

        private string getCurrentUserNo()
        {
            string userNo = String.Empty;
            foreach (String key in session.Keys)
            {
                if (key.Equals("Loggedin") && session.GetString(key) == "true")
                {
                    userNo = session.GetString("userNo");
                }
            }

            return userNo;
        }

        public IActionResult MyMovies()
        {
            string ID = getCurrentID();

            using (var connection = new SqlConnection(GlobalVariables.connectionUrl))
            {
                string commandStr =
                    $"DECLARE @NOW DATE\r\nSET @NOW = GETDATE();\r\nDECLARE @USERNUM INT\r\nSET @USERNUM = (SELECT 사용자번호 FROM 사용자 WHERE ID = \'{ID}\')\r\nSELECT 제목,구매번호,구매시각,만료일자 FROM 구매내역\r\nINNER JOIN 영화 ON 구매내역.영화번호 = 영화.영화번호\r\nWHERE CAST(@NOW AS DATE) <= CAST(만료일자 AS DATE) AND 사용자번호 = @USERNUM";
                var command = new SqlCommand(commandStr, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    List<AvailableMovie> availableMovies = new List<AvailableMovie>();
                    while (reader.Read())
                    {
                        // 제목,구매번호,구매시각,만료일자
                        string 제목 = reader[0] is DBNull ? string.Empty : Convert.ToString(reader[0]);
                        int 구매번호 = reader[1] is DBNull ? 0 : Convert.ToInt32(reader[1]);
                        string 구매시각 = reader[2] is DBNull ? String.Empty : Convert.ToString(reader[2]);
                        string 만료일자 = reader[3] is DBNull ? String.Empty : Convert.ToString(reader[3]);
                        availableMovies.Add(new AvailableMovie(제목, 구매번호, 구매시각, 만료일자));
                        Console.WriteLine($"제목: {제목}, 구매번호: {구매번호}, 구매시각: {구매시각}, 만료일자: {만료일자}");
                    }

                    ViewData["AvailableMovies"] = availableMovies;
                }
            }

            ViewData["Title"] = "내 영화";
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

            string userNo = getCurrentUserNo();
            using (var connection = new SqlConnection(GlobalVariables.connectionUrl))
            {
                string balanceStr =
                    $"SELECT 보유금액 FROM 회원 WHERE (사용자번호 = {userNo})";
                var commandBalanceInfo = new SqlCommand(balanceStr, connection);
                connection.Open();
                using (var reader = commandBalanceInfo.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int 잔액 = reader[0] is DBNull ? 0 : Convert.ToInt32(reader[0]);
                        ViewData["Balance"] = 잔액.ToString();
                    }
                }
            }


            ViewData["Title"] = "충전";
            return View();
        }

        public IActionResult doCharge(int price)
        {
            string ID = getCurrentID();

            using (var connection =
                new SqlConnection(
                    GlobalVariables.connectionUrl))
            {
                string commandStr =
                    $"DECLARE @USER INT = (SELECT 사용자번호 FROM 사용자 WHERE ID = \'{ID}\')\r\nDECLARE @NUM INT\r\nDECLARE @PRICE INT = {price}\r\nSET @NUM = (SELECT COUNT(*) FROM 충전내역 WHERE 사용자번호=@USER)\r\nIF (@NUM != 0) SET @NUM = (SELECT MAX(충전번호) FROM 충전내역 WHERE 사용자번호=@USER) + 1\r\nELSE SET @NUM = 1\r\n\r\nINSERT INTO 충전내역 VALUES(@USER, @NUM, @PRICE, GETDATE())\r\n\r\nUPDATE 회원\r\nSET 보유금액 += @PRICE\r\nWHERE 사용자번호 = @USER";
                Console.WriteLine(commandStr);
                var command = new SqlCommand(commandStr, connection);
                connection.Open();
                var result = command.ExecuteNonQuery();
                return RedirectToAction("Charge", "Home", new { result = "SUCCESS" });
            }
        }

        public IActionResult Purchase(string purchaseType, int movieNum)
        {
            // 이미 보유 중인 영화인지 확인
            string userNo = getCurrentUserNo();
            List<int> availableMovies = new List<int>();

            using (var connection = new SqlConnection(GlobalVariables.connectionUrl))
            {
                string commandStr =
                    $"DECLARE @NOW DATE\r\nSET @NOW = GETDATE();\r\nDECLARE @USERNUM INT\r\nSET @USERNUM = {userNo}\r\nSELECT 영화번호 FROM 구매내역\r\nWHERE CAST(@NOW AS DATE) <= CAST(만료일자 AS DATE) AND 사용자번호 = @USERNUM";
                var command = new SqlCommand(commandStr, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // 제목,구매번호,구매시각,만료일자
                        int 영화번호 = reader[0] is DBNull ? 0 : Convert.ToInt32(reader[0]);
                        availableMovies.Add(영화번호);
                    }
                }
            }

            if (availableMovies.Contains(movieNum))
            {
                return RedirectToAction("AllMovies", "Home", new { result = "ALREADY" });
            }


            int isLend = purchaseType.Equals("lend") ? 1 : 0;
            int balance = 0;
            int moviePrice = 0;

            // 잔액확인하기
            using (var connection = new SqlConnection(GlobalVariables.connectionUrl))
            {
                string balanceStr =
                    $"SELECT 보유금액 FROM 회원 WHERE (사용자번호 = {userNo})";
                var commandBalanceInfo = new SqlCommand(balanceStr, connection);
                connection.Open();
                using (var reader = commandBalanceInfo.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        balance = reader[0] is DBNull ? 0 : Convert.ToInt32(reader[0]);
                    }
                }
            }
            // 영화 금액 확인하기
            using (var connection = new SqlConnection(GlobalVariables.connectionUrl))
            {
                string moviePriceStr =
                    $"SELECT 금액 FROM 영화 WHERE (영화번호 = {movieNum})";
                var commandMoviePrice = new SqlCommand(moviePriceStr, connection);
                connection.Open();
                using (var reader = commandMoviePrice.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        moviePrice = reader[0] is DBNull ? 0 : Convert.ToInt32(reader[0]);
                        moviePrice = isLend == 1 ? moviePrice : moviePrice * 3;
                    }
                }
            }

            if (balance < moviePrice)
            {
                return RedirectToAction("AllMovies", "Home", new { result = "EXPENSIVE" });
            }

            using (var connection = new SqlConnection(GlobalVariables.connectionUrl))
            {
                string commandStr =
                    $"DECLARE @NUM INT\r\nDECLARE @PRICE INT\r\nDECLARE @USER INT = {userNo}\r\nDECLARE @MOVIE INT = {movieNum}\r\nDECLARE @LIMIT DATE = DATEADD(YEAR, 100, GETDATE())\r\n\r\nSET @NUM = (SELECT count(*) FROM 구매내역 WHERE 사용자번호=@USER and 영화번호=@MOVIE)\r\nSET @PRICE = (SELECT 금액 FROM 영화 WHERE 영화번호=@MOVIE)\r\n\r\nIF (@NUM != 0) SET @NUM = (SELECT MAX(구매번호) FROM 구매내역 WHERE 사용자번호=@USER and 영화번호=@MOVIE) + 1\r\nIF ({isLend} = 1) SET @LIMIT = DATEADD(DAY, 7, GETDATE())\r\nELSE SET @PRICE = @PRICE * 3\r\nINSERT INTO 구매내역 VALUES(@USER, @MOVIE, @NUM, GETDATE(), @LIMIT)\r\n\r\nUPDATE 회원\r\nSET 보유금액 -= @PRICE\r\nWHERE 사용자번호 = @USER";
                var command = new SqlCommand(commandStr, connection);
                connection.Open();
                var result = command.ExecuteNonQuery();

                return RedirectToAction("AllMovies", "Home", new { result = "SUCCESS" });
            }
        }
    }
}