namespace Cinetube.Models
{
    public class ChargeHistory
    {
        public ChargeHistory(int amount, string date)
        {
            금액 = amount;
            시각 = date;
        }

        public int 금액 { get; }
        public string 시각 { get; }
    }
}