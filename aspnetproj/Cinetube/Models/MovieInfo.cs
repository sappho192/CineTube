using System.Collections.Generic;

namespace Cinetube.Models
{
    public class MovieInfo
    {
        public MovieInfo(int movienum, string title, int price, string previewuri, string movieuri, int openyear,
            string story, string agelimit, int duration, string company, string director, List<string> genres, List<MovieComment> comments)
        {
            영화번호 = movienum;
            제목 = title;
            금액 = price;
            예고편경로 = previewuri;
            영화경로 = movieuri;
            개봉연도 = openyear;
            줄거리 = story;
            관람제한 = agelimit;
            영화시간 = duration;
            제작사 = company;
            감독 = director;
            장르 = genres;
            한줄평 = comments;
        }
        public int 영화번호 { get; }
        public string 제목 { get; }
        public int 금액 { get; }
        public string 예고편경로 { get; }
        public string 영화경로 { get; }
        public int 개봉연도 { get; }
        public string 줄거리 { get; }
        public string 관람제한 { get; }
        public int 영화시간 { get; }
        public string 제작사 { get; }
        public string 감독 { get; }
        public List<string> 장르 { get; }
        public List<MovieComment> 한줄평 { get; }
    }
}