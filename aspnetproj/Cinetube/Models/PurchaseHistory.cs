using System.ComponentModel.DataAnnotations;

namespace Cinetube.Models
{
    public class PurchaseHistory
    {
        public PurchaseHistory(string moviename, int movie, int purchased, string purchasedTime, string expiryTime)
        {
            영화제목 = moviename;
            영화번호 = movie;
            구매번호 = purchased;
            구매시각 = purchasedTime;
            만료일자 = expiryTime;
        }

        public string 영화제목 { get; }
        public int 영화번호 { get; }
        public int 구매번호 { get; }
        public string 구매시각 { get; }
        public string 만료일자 { get; }
    }
}