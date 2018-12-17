namespace Cinetube.Models
{
    public class UserInfo
    {
        public UserInfo(string id, string name, string birth, string phone, int balance)
        {
            ID = id;
            이름 = name;
            생년월일 = birth;
            핸드폰번호 = phone;
            보유금액 = balance;
        }

        public string ID { get; }
        public string 이름 { get; }
        public string 생년월일 { get; }
        public string 핸드폰번호 { get; }

        public int 보유금액 { get; }
    }
}