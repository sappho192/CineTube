namespace Cinetube.Models
{
    public class MovieComment
    {
        public MovieComment(int userNo, string id, string content, float grade, string date)
        {
            사용자번호 = userNo;
            아이디 = id;
            내용 = content;
            평점 = grade;
            작성시각 = date;
        }

        public int 사용자번호 { get; }
        public string 아이디 { get; }
        public string 내용 { get; }
        public float 평점 { get; }
        public string 작성시각 { get; }
    }
}