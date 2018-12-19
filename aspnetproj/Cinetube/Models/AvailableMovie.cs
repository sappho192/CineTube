namespace Cinetube.Models
{
    public class AvailableMovie
    {
        public AvailableMovie(string title, int purchasenum, string purchasedate, string expirydate, int movieNo)
        {
            제목 = title;
            구매번호 = purchasenum;
            구매시각 = purchasedate;
            만료일자 = expirydate;
            영화번호 = movieNo;
        }
        // 제목,구매번호,구매시각,만료일자
        public string 제목 { get; }
        public int 구매번호 { get; }
        public string 구매시각 { get; }
        public string 만료일자 { get; }
        public int 영화번호 { get; }
    }
}