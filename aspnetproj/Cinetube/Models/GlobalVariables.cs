using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cinetube.Models
{
    public class GlobalVariables
    {
        delegate List<string> GetHintList();

        private static readonly GetHintList getHintList = () =>
        {
            List<string> list = new List<string>();
            using (var connection =
                new SqlConnection(GlobalVariables.connectionUrl))
            {
                var command = new SqlCommand(
                    "SELECT 힌트번호, 힌트질문  FROM 비밀번호힌트",
                    connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var hintNo = Convert.ToInt32(reader[0]);
                        var hintStr = Convert.ToString(reader[1]);
                        Console.WriteLine($"힌트번호: {hintNo}, 힌트: {hintStr}");
                        list.Add(hintStr);
                    }
                }
            }

            return list;
        };

        public static bool Loggedin = false;

        public static readonly string connectionUrl =
            "server = sappho192.iptime.org,21433;database = CinetubeDB2;uid=cinetube;pwd=qwer12#$;";

        public static List<string> PwHintList = getHintList();

        public static readonly Regex PhoneRegex = new Regex(@"\d{11}");
    }
}
