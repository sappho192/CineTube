using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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

        public IActionResult Index(string result = null)
        {
            if (result != null)
            {
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
                        GetGenres(영화번호, 장르들);

                        // 해당 영화의 한줄평 긁어오기
                        List<MovieComment> 한줄평들 = new List<MovieComment>();
                        GetMovieComments(영화번호, 한줄평들);

                        List<ActorInfo> 배우들 = new List<ActorInfo>();
                        GetActors(영화번호, 배우들);

                        recentMoviesInfo.Add(new MovieInfo(영화번호, 제목, 금액, 예고편경로, 영화경로, 개봉연도, 줄거리, 관람제한, 영화시간, 제작사, 감독,
                            장르들, 한줄평들, 배우들));
                    }

                    ViewData["RecentMoviesInfo"] = recentMoviesInfo;
                }
            }

            // 최근 게시글 긁어오기
            using (var connection = new SqlConnection(GlobalVariables.connectionUrl))
            {
                string commandRecentArticleStr =
                    "SELECT TOP 5 ID, 제목, 게시글번호 FROM 게시글, 사용자 WHERE 게시글.사용자번호=사용자.사용자번호 ORDER BY 게시글번호 DESC";

                var commandRecentPage = new SqlCommand(commandRecentArticleStr, connection);
                connection.Open();
                using (var reader = commandRecentPage.ExecuteReader())
                {
                    List<RecentArticle> recentArticles = new List<RecentArticle>();
                    while (reader.Read())
                    {
                        string ID = reader[0] is DBNull ? string.Empty : Convert.ToString(reader[0]);
                        string 제목 = reader[1] is DBNull ? string.Empty : Convert.ToString(reader[1]);
                        int 글번호 = reader[2] is DBNull ? 0 : Convert.ToInt32(reader[2]);
                        recentArticles.Add(new RecentArticle(ID, 제목, 글번호));
                    }
                    ViewData["RecentArticles"] = recentArticles;
                }
            }

            // 인기 영화 3개까지 긁어오기
            using (var connection = new SqlConnection(GlobalVariables.connectionUrl))
            {
                string commandPopularMoviesStr =
                    "SELECT TOP (3) 구매내역.영화번호, 제목,금액,예고편경로,영화경로,개봉연도,줄거리,관람제한,영화시간,제작사,감독, COUNT(1) AS \'구매횟수\'\r\n FROM 구매내역\r\n INNER JOIN 영화 ON 구매내역.영화번호 = 영화.영화번호\r\n GROUP BY 구매내역.영화번호, 제목,금액,예고편경로,영화경로,개봉연도,줄거리,관람제한,영화시간,제작사,감독\r\n  ORDER BY 구매횟수 DESC";

                var commandPopularMovies = new SqlCommand(commandPopularMoviesStr, connection);
                connection.Open();
                using (var reader = commandPopularMovies.ExecuteReader())
                {
                    List<MovieInfo> popularMoviesInfo = new List<MovieInfo>();
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
                        GetGenres(영화번호, 장르들);

                        // 해당 영화의 한줄평 긁어오기
                        List<MovieComment> 한줄평들 = new List<MovieComment>();
                        GetMovieComments(영화번호, 한줄평들);

                        List<ActorInfo> 배우들 = new List<ActorInfo>();
                        GetActors(영화번호, 배우들);

                        popularMoviesInfo.Add(new MovieInfo(영화번호, 제목, 금액, 예고편경로, 영화경로, 개봉연도, 줄거리, 관람제한, 영화시간, 제작사, 감독,
                            장르들, 한줄평들, 배우들));
                    }

                    ViewData["PopularMoviesInfo"] = popularMoviesInfo;
                }
            }

            ViewData["Title"] = "영화는 역시 Cinetube!";
            ViewData["CurrentAction"] = "Index";
            return View();
        }

        private static void GetActors(int 영화번호, List<ActorInfo> 배우들)
        {
            using (var commentConnection = new SqlConnection(GlobalVariables.connectionUrl))
            {
                string commandStr =
                    $"SELECT 이름, 출연.배우번호 FROM 출연 INNER JOIN 배우 ON 출연.배우번호 = 배우.배우번호 WHERE 영화번호 = {영화번호}";
                var command = new SqlCommand(commandStr, commentConnection);
                commentConnection.Open();
                using (var reader = command.ExecuteReader())
                {
                    //사용자번호,ID,한줄평내용,평점,작성시각
                    while (reader.Read())
                    {
                        string 이름 = reader[0] is DBNull
                            ? string.Empty
                            : Convert.ToString(reader[0]);
                        int 배우번호 = reader[1] is DBNull ? 0 : Convert.ToInt32(reader[1]);
                        배우들.Add(new ActorInfo(이름, 배우번호));
                    }
                }
            }
        }

        private static void GetMovieComments(int 영화번호, List<MovieComment> 한줄평들)
        {
            using (var commentConnection = new SqlConnection(GlobalVariables.connectionUrl))
            {
                string commandMovieCommentStr =
                    $"SELECT TOP(7) 한줄평.사용자번호,ID,한줄평내용,평점,작성시각 FROM 한줄평\r\nINNER JOIN 사용자 ON 한줄평.사용자번호 = 사용자.사용자번호\r\nWHERE 영화번호 = {영화번호}";
                var commandMovieComment = new SqlCommand(commandMovieCommentStr, commentConnection);
                commentConnection.Open();
                using (var commentReader = commandMovieComment.ExecuteReader())
                {
                    //사용자번호,ID,한줄평내용,평점,작성시각
                    while (commentReader.Read())
                    {
                        int 사용자번호 = commentReader[0] is DBNull ? 0 : Convert.ToInt32(commentReader[0]);
                        string 아이디 = commentReader[1] is DBNull
                            ? string.Empty
                            : Convert.ToString(commentReader[1]);
                        string 내용 = commentReader[2] is DBNull
                            ? string.Empty
                            : Convert.ToString(commentReader[2]);
                        float 평점 = commentReader[3] is DBNull ? 0 : Convert.ToSingle(commentReader[3]);
                        string 작성시각 = commentReader[4] is DBNull
                            ? string.Empty
                            : Convert.ToString(commentReader[4]);
                        한줄평들.Add(new MovieComment(사용자번호, 아이디, 내용, 평점, 작성시각));
                    }
                }
            }
        }

        private static void GetGenres(int 영화번호, List<string> 장르들)
        {
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

            // 영화 장르 긁어오기
            List<string> genres = GetGenreList();
            ViewData["GenreList"] = genres;

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
                        GetGenres(영화번호, 장르들);

                        List<ActorInfo> 배우들 = new List<ActorInfo>();
                        GetActors(영화번호, 배우들);

                        // 해당 영화의 한줄평 긁어오기
                        List<MovieComment> 한줄평들 = new List<MovieComment>();
                        GetMovieComments(영화번호, 한줄평들);
                        moviesInfo.Add(new MovieInfo(영화번호, 제목, 금액, 예고편경로, 영화경로, 개봉연도, 줄거리, 관람제한, 영화시간, 제작사, 감독, 장르들, 한줄평들, 배우들));
                    }

                    ViewData["MoviesInfo"] = moviesInfo;
                }
            }

            ViewData["Title"] = "영화 찾기";
            ViewData["CurrentAction"] = "AllMovies";
            return View();
        }

        [HttpPost]
        public IActionResult AllMovies(string 금액, string 금액범위, string 개봉연도, string 개봉연도범위,
            string 제목, string 줄거리, string 관람제한, string 제작사, string 감독, string 장르, string 배우, string result = null)
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

            // 영화 장르 긁어오기
            List<string> genres = GetGenreList();
            ViewData["GenreList"] = genres;

            // null이 아닌 검색조건들을 보관
            Dictionary<string, object> searchFilter = new Dictionary<string, object>();
            if (제목 != null) { searchFilter.Add(nameof(제목), 제목); }
            if (금액 != null) { searchFilter.Add(nameof(금액), 금액); }
            if (개봉연도 != null) { searchFilter.Add(nameof(개봉연도), 개봉연도); }
            if (줄거리 != null) { searchFilter.Add(nameof(줄거리), 줄거리); }
            if (관람제한 != null) { searchFilter.Add(nameof(관람제한), 관람제한); }
            if (제작사 != null) { searchFilter.Add(nameof(제작사), 제작사); }
            if (감독 != null) { searchFilter.Add(nameof(감독), 감독); }
            if (장르 != null) { searchFilter.Add(nameof(장르), 장르); }
            if (배우 != null) { searchFilter.Add(nameof(배우), 배우); }
            if (searchFilter.Count == 0)
            {
                return RedirectToAction("AllMovies", "Home");
            }

            string query = makeSearchQueryString(searchFilter, 금액범위, 개봉연도범위);
            // 검색 조건에 맞는 영화 긁어오기
            using (var connection = new SqlConnection(GlobalVariables.connectionUrl))
            {
                int index = 0;
                var commandFilteredPage = new SqlCommand(query, connection);
                connection.Open();
                using (var reader = commandFilteredPage.ExecuteReader())
                {
                    List<MovieInfo> moviesInfo = new List<MovieInfo>();
                    while (reader.Read())
                    {
                        // 영화번호,제목,금액,예고편경로,영화경로,개봉연도,줄거리,관람제한,영화시간,제작사,감독
                        int _영화번호 = reader[0] is DBNull ? 0 : Convert.ToInt32(reader[0]);
                        string _제목 = reader[1] is DBNull ? string.Empty : Convert.ToString(reader[1]);
                        int _금액 = reader[2] is DBNull ? 0 : Convert.ToInt32(reader[2]);
                        string _예고편경로 = reader[3] is DBNull ? String.Empty : Convert.ToString(reader[3]);
                        string _영화경로 = reader[4] is DBNull ? String.Empty : Convert.ToString(reader[4]);
                        int _개봉연도 = reader[5] is DBNull ? 0 : Convert.ToInt32(reader[5]);
                        string _줄거리 = reader[6] is DBNull ? String.Empty : Convert.ToString(reader[6]);
                        string _관람제한 = reader[7] is DBNull ? String.Empty : Convert.ToString(reader[7]);
                        int _영화시간 = reader[8] is DBNull ? 0 : Convert.ToInt32(reader[8]);
                        string _제작사 = reader[9] is DBNull ? String.Empty : Convert.ToString(reader[9]);
                        string _감독 = reader[10] is DBNull ? String.Empty : Convert.ToString(reader[10]);

                        // 해당 영화의 장르 긁어오기
                        List<string> 장르들 = new List<string>();
                        GetGenres(_영화번호, 장르들);

                        // 해당 영화의 한줄평 긁어오기
                        List<MovieComment> 한줄평들 = new List<MovieComment>();
                        GetMovieComments(_영화번호, 한줄평들);

                        List<ActorInfo> 배우들 = new List<ActorInfo>();
                        GetActors(_영화번호, 배우들);

                        moviesInfo.Add(new MovieInfo(_영화번호, _제목, _금액, _예고편경로, _영화경로, _개봉연도, _줄거리, _관람제한, _영화시간, _제작사, _감독, 장르들, 한줄평들, 배우들));
                    }

                    if(moviesInfo.Count == 0) {ViewData["Result"] = "0SEARCH";}
                    ViewData["MoviesInfo"] = moviesInfo;
                }
            }
            ViewData["Title"] = "영화 찾기";
            ViewData["CurrentAction"] = "AllMovies";
            return View();
        }

        private static List<string> GetGenreList()
        {
            List<string> genres;
            using (var connection = new SqlConnection(GlobalVariables.connectionUrl))
            {
                string commandGenresStr = "SELECT DISTINCT 장르 FROM 장르들 ORDER BY 장르 ASC ";
                var commandGenres = new SqlCommand(commandGenresStr, connection);
                connection.Open();
                using (var reader = commandGenres.ExecuteReader())
                {
                    genres = new List<string>();
                    while (reader.Read())
                    {
                        // 영화번호,제목,금액,예고편경로,영화경로,개봉연도,줄거리,관람제한,영화시간,제작사,감독
                        string genre = reader[0] is DBNull ? string.Empty : Convert.ToString(reader[0]);
                        genres.Add(genre);
                    }
                }
            }

            return genres;
        }

        private static string makeSearchQueryString(Dictionary<string, object> searchFilter, string 금액범위, string 개봉연도범위)
        {
            StringBuilder searchQueryBuilder = new StringBuilder();
            
            if (searchFilter.ContainsKey("장르"))
            {
                searchQueryBuilder.Append("SELECT TOP (10) 영화.영화번호,제목,금액,예고편경로,영화경로,개봉연도,줄거리,관람제한,영화시간,제작사,감독 FROM 영화 ");
                searchQueryBuilder.Append("INNER JOIN 장르들 ON 영화.영화번호 = 장르들.영화번호 ");
            }
            else if (searchFilter.ContainsKey("배우"))
            {
                searchQueryBuilder.Append("SELECT TOP (10) 영화.영화번호,제목,금액,예고편경로,영화경로,개봉연도,줄거리,관람제한,영화시간,제작사,감독 FROM 영화 ");
                searchQueryBuilder.Append("INNER JOIN 출연 ON 영화.영화번호 = 출연.영화번호 ");
            }
            else
            {
                searchQueryBuilder.Append("SELECT TOP (10) 영화번호,제목,금액,예고편경로,영화경로,개봉연도,줄거리,관람제한,영화시간,제작사,감독 FROM 영화 ");
            }
            searchQueryBuilder.Append("WHERE ");

            if (searchFilter.Count == 1)
            {
                foreach (var filter in searchFilter)
                {
                    if (GlobalVariables.StringFilter.Contains(filter.Key))
                    {
                        if (filter.Key == "관람제한")
                        {
                            searchQueryBuilder.Append($"관람제한 = \'{filter.Value}\'");
                        } else if (filter.Key == "배우")
                        {
                            List<ActorInfo> actorList = GetSameNameActors(filter.Value as string);
                            for (int i = 0; i < actorList.Count; i++)
                            {
                                searchQueryBuilder.Append(i == 0
                                    ? $" 배우번호 = {actorList[i].배우번호} "
                                    : $" OR 배우번호 = {actorList[i].배우번호} ");
                            }
                        }
                        else
                        {
                            searchQueryBuilder.Append($"{filter.Key} LIKE \'%{filter.Value}%\'");
                        }
                    }
                    else if (GlobalVariables.IntFilter.Contains(filter.Key))
                    {
                        /* 금액이면 금액 {금액범위} {금액}
                         * 예시 1) "금액 < 10000"
                         * 예시 2) "개봉연도 > 1980"
                         */
                        if (filter.Key == "금액")
                        {
                            searchQueryBuilder.Append($"금액 {금액범위} {filter.Value}");
                        }
                        else if (filter.Key == "개봉연도")
                        {
                            searchQueryBuilder.Append($"개봉연도 {개봉연도범위} {filter.Value}");
                        }
                    }
                }

                searchQueryBuilder.Append(" ");
            }
            else
            {
                for (int i = 0; i < searchFilter.Count; i++)
                {
                    var filter = searchFilter.ElementAt(i);
                    if (i == 0)
                    {
                        if (GlobalVariables.StringFilter.Contains(filter.Key))
                        {
                            if (filter.Key == "관람제한")
                            {
                                // 예시) 관람제한 = '12세 이상 관람가' OR 관람제한 = '15세 이상 관람가'
                                // 로 하지 않을거다.
                                searchQueryBuilder.Append($"관람제한 = \'{filter.Value}\'");
                            }
                            else if (filter.Key == "배우")
                            {
                                List<ActorInfo> actorList = GetSameNameActors(filter.Value as string);
                                for (int j = 0; j < actorList.Count; j++)
                                {
                                    searchQueryBuilder.Append(j == 0
                                        ? $" AND (배우번호 = {actorList[i].배우번호} "
                                        : $" OR 배우번호 = {actorList[i].배우번호} ");
                                }

                                searchQueryBuilder.Append(")");
                            }
                            else
                            {
                                searchQueryBuilder.Append($"{filter.Key} LIKE \'%{filter.Value}%\'");
                            }
                        }
                        else if (GlobalVariables.IntFilter.Contains(filter.Key))
                        {
                            /* 금액이면 금액 {금액범위} {금액}
                             * 예시 1) "금액 < 10000"
                             * 예시 2) "개봉연도 > 1980"
                             */
                            if (filter.Key == "금액")
                            {
                                searchQueryBuilder.Append($"금액 {금액범위} {filter.Value}");
                            }
                            else if (filter.Key == "개봉연도")
                            {
                                searchQueryBuilder.Append($"개봉연도 {개봉연도범위} {filter.Value}");
                            }
                        }
                    }
                    else
                    {
                        if (GlobalVariables.StringFilter.Contains(filter.Key))
                        {
                            if (filter.Key == "관람제한")
                            {
                                searchQueryBuilder.Append($"AND 관람제한 = \'{filter.Value}\'");
                            }
                            else if (filter.Key == "배우")
                            {
                                List<ActorInfo> actorList = GetSameNameActors(filter.Value as string);
                                for (int j = 0; j < actorList.Count; j++)
                                {
                                    searchQueryBuilder.Append(j == 0
                                        ? $" AND (배우번호 = {actorList[i].배우번호} "
                                        : $" OR 배우번호 = {actorList[i].배우번호} ");
                                }

                                searchQueryBuilder.Append(")");
                            }
                            else
                            {
                                searchQueryBuilder.Append($"AND {filter.Key} LIKE \'%{filter.Value}%\'");
                            }
                        }
                        else if (GlobalVariables.IntFilter.Contains(filter.Key))
                        {
                            /* 금액이면 금액 {금액범위} {금액}
                             * 예시 1) "금액 < 10000"
                             * 예시 2) "개봉연도 > 1980"
                             */
                            if (filter.Key == "금액")
                            {
                                searchQueryBuilder.Append($"AND 금액 {금액범위} {filter.Value}");
                            }
                            else if (filter.Key == "개봉연도")
                            {
                                searchQueryBuilder.Append($"AND 개봉연도 {개봉연도범위} {filter.Value}");
                            }
                        }
                    }

                    searchQueryBuilder.Append(" ");
                }
            }

            searchQueryBuilder.Append("ORDER BY 개봉연도 DESC");

            return searchQueryBuilder.ToString();
        }

        private static List<ActorInfo> GetSameNameActors(string name)
        {
            List<ActorInfo> 배우들 = new List<ActorInfo>();
            using (var commentConnection = new SqlConnection(GlobalVariables.connectionUrl))
            {
                string commandStr =
                    $"SELECT 배우번호,이름 FROM 배우 WHERE 이름 = \'{name}\'";
                var command = new SqlCommand(commandStr, commentConnection);
                commentConnection.Open();
                using (var reader = command.ExecuteReader())
                {
                    //사용자번호,ID,한줄평내용,평점,작성시각
                    while (reader.Read())
                    {
                        int 배우번호 = reader[0] is DBNull ? 0 : Convert.ToInt32(reader[0]);
                        string 이름 = reader[1] is DBNull
                            ? string.Empty
                            : Convert.ToString(reader[1]);
                        배우들.Add(new ActorInfo(이름, 배우번호));
                    }
                }
            }

            return 배우들;
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
            ViewData["CurrentAction"] = "Privacy";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Login()
        {
            ViewData["CurrentAction"] = "Login";
            return View();
        }

        public IActionResult Authenticate(string ID, string PW, string cameFrom, int cameFromSub)
        {
            string temp_ID = ID;
            string temp_PW = PW;
            if (ID == null || PW == null)
            {
                string result = "NOID";
                return RedirectToAction2(cameFrom, cameFromSub, result);
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
                        } else if (id >= 1 && pw != 1)
                        {
                            return RedirectToAction2(cameFrom, cameFromSub, "WRONGPW");
                        } else if (id == 0)
                        {
                            return RedirectToAction2(cameFrom, cameFromSub, "NOID");
                        }
                    }
                }
            }

            return RedirectToAction("Index", "Home");
        }

        private IActionResult RedirectToAction2(string cameFrom, int cameFromSub, string result)
        {
            if (cameFromSub == 0)
            {
                return RedirectToAction(cameFrom, "Home", new {result});
            }

            switch (cameFrom)
            {
                case "Article":
                    return RedirectToAction(cameFrom, new {articleNo = cameFromSub, result});
                case "Board":
                    return RedirectToAction(cameFrom, new { pageNum = cameFromSub, result });
                default:
                    return Index(result);
            }
        }

        public IActionResult Logout()
        {
            session.Clear();

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Board(string result = null)
        {
            return Board(1, result);
        }

        [Route("Home/[action]/{pageNum}")]
        [HttpGet]
        public IActionResult Board(int pageNum, string result = null)
        {
            if (result != null)
            {
                ViewData["Result"] = result;
            }
            else
            {
                ViewData["Result"] = null;
            }

            var list = new List<BoardModel>();
            int total = 0;
            int last_page = 0;

            using (var connection = new SqlConnection(GlobalVariables.connectionUrl))
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

            ViewData["CurrentAction"] = "Board";
            ViewData["CurrentSubActionID"] = pageNum;
            ViewData["Result"] = result;
            return View(list);
        }

        [Route("Home/[action]/{articleNo}")]
        [HttpGet]
        public IActionResult Article(int articleNo, string result = null)
        {
            var list = new List<SubarticleModel>();
            using (var connection = new SqlConnection(GlobalVariables.connectionUrl))
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

            ViewData["CurrentAction"] = "Article";
            ViewData["CurrentSubActionID"] = articleNo;
            ViewData["Result"] = result;
            return View(list);
        }

        public IActionResult NewArticle()
        {
            ViewData["CurrentAction"] = "NewArticle";
            return View();
        }

        public IActionResult AddArticle()
        {
            string title = Request.Form["title"].ToString();
            string context = Request.Form["context"].ToString();
            string userNo = session.GetString("userNo");

            using (var connection = new SqlConnection(GlobalVariables.connectionUrl))
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

            using (var connection = new SqlConnection(GlobalVariables.connectionUrl))
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

        public IActionResult SignUp(string ID, string PW, string name, string birth, string ssn, string phone, int PWHintNo, string PWAns, string cameFrom)
        {
            if (ID == null || PW == null || name == null || birth == null || ssn == null || phone == null || PWAns == null)
            {
                return RedirectToAction(cameFrom, "Home", new {result = "NOID"});
            }

            // 이미 있는 ID인지 확인
            using (var connection = new SqlConnection(GlobalVariables.connectionUrl))
            {
                var command = new SqlCommand($"SELECT ID FROM 사용자 WHERE ID = \'{ID}\'", connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return RedirectToAction2(cameFrom, 0, "IDEXIST");
                    }
                }
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
            ViewData["CurrentAction"] = "MyPage";
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
                    $"DECLARE @NOW DATE\r\nSET @NOW = GETDATE();\r\nDECLARE @USERNUM INT\r\nSET @USERNUM = (SELECT 사용자번호 FROM 사용자 WHERE ID = \'{ID}\')\r\nSELECT 제목,구매번호,구매시각,만료일자,구매내역.영화번호 FROM 구매내역\r\nINNER JOIN 영화 ON 구매내역.영화번호 = 영화.영화번호\r\nWHERE CAST(@NOW AS DATE) <= CAST(만료일자 AS DATE) AND 사용자번호 = @USERNUM";
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
                        int 영화번호 = reader[4] is DBNull ? 0 : Convert.ToInt32(reader[4]);
                        availableMovies.Add(new AvailableMovie(제목, 구매번호, 구매시각, 만료일자, 영화번호));
                        Console.WriteLine($"제목: {제목}, 구매번호: {구매번호}, 구매시각: {구매시각}, 만료일자: {만료일자}");
                    }

                    ViewData["AvailableMovies"] = availableMovies;
                }
            }

            ViewData["Title"] = "내 영화";
            ViewData["CurrentAction"] = "MyMovies";
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
            ViewData["CurrentAction"] = "Charge";
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

        public IActionResult Purchase(string purchaseType, int movieNum, string cameFrom)
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
                return RedirectToAction(cameFrom, "Home", new { result = "ALREADY" });
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
                        moviePrice = (isLend == 1 ? (moviePrice / 4) : moviePrice);
                        Console.WriteLine(isLend);
                    }
                }
            }

            if (balance < moviePrice)
            {
                return RedirectToAction(cameFrom, "Home", new { result = "EXPENSIVE" });
            }

            using (var connection = new SqlConnection(GlobalVariables.connectionUrl))
            {
                string commandStr =
                    $"DECLARE @NUM INT\r\nDECLARE @USER INT = {userNo}\r\nDECLARE @MOVIE INT = {movieNum}\r\nDECLARE @LIMIT DATE = DATEADD(YEAR, 100, GETDATE())\r\n\r\nSET @NUM = (SELECT count(*) FROM 구매내역 WHERE 사용자번호=@USER and 영화번호=@MOVIE)\r\nIF (@NUM != 0) SET @NUM = (SELECT MAX(구매번호) FROM 구매내역 WHERE 사용자번호=@USER and 영화번호=@MOVIE) + 1\r\nIF ({isLend} = 1) SET @LIMIT = DATEADD(DAY, 7, GETDATE())\r\nINSERT INTO 구매내역 VALUES(@USER, @MOVIE, @NUM, GETDATE(), @LIMIT)\r\n\r\nUPDATE 회원\r\nSET 보유금액 -= {moviePrice}\r\nWHERE 사용자번호 = @USER";
                var command = new SqlCommand(commandStr, connection);
                connection.Open();
                var result = command.ExecuteNonQuery();

                return RedirectToAction(cameFrom, "Home", new { result = "SUCCESS" });
            }
        }

        [Route("Home/[action]/{movieNo}")]
        [HttpGet]
        public IActionResult Movie(int movieNo)
        {
            if (!session.Keys.Contains("Loggedin"))
            {
                return RedirectToAction("Index", "Home", new { result = "NOTLOGIN" });
            }

            // 영화파일 이름 가져오기
            using (var connection = new SqlConnection(GlobalVariables.connectionUrl))
            {
                string commandStr = $"SELECT 영화경로, 제목 FROM 영화 WHERE 영화번호 = {movieNo}";
                var commandMoviePrice = new SqlCommand(commandStr, connection);
                connection.Open();
                using (var reader = commandMoviePrice.ExecuteReader())
                {
                    string 파일명 = string.Empty;
                    string 제목 = string.Empty;
                    while (reader.Read())
                    {
                        파일명 = reader[0] is DBNull ? String.Empty : Convert.ToString(reader[0]);
                        제목 = reader[1] is DBNull ? String.Empty : Convert.ToString(reader[1]);
                    }

                    ViewData["Title"] = 제목;
                    ViewData["FileLink"] = $"https://cinetubecdn.blob.core.windows.net/files/{파일명}";
                }
            }

            ViewData["CurrentAction"] = "Movie";
            return View();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private string GetCurrentMethod()
        {
            var st = new StackTrace();
            var sf = st.GetFrame(1);

            return sf.GetMethod().Name;
        }
    }
}