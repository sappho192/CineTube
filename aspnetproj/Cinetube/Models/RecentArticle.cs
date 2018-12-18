namespace Cinetube.Models
{
    public class RecentArticle
    {
        public RecentArticle(string id, string title, int articleNo)
        {
            ID = id;
            제목 = title;
            글번호 = articleNo;
        }

        public string ID { get; }
        public string 제목 { get; }
        public int 글번호 { get; }
    }
}