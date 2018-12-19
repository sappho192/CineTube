namespace Cinetube.Models
{
    public class ActorInfo
    {
        public ActorInfo(string name, int actorNo)
        {
            이름 = name;
            배우번호 = actorNo;
        }

        public string 이름 { get; }
        public int 배우번호 { get; }
    }
}