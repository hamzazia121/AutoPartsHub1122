//using System.Text;

//namespace AutoPartsHub.Views.Home
//{
//    public class Class
//    {
//        static List<char> chars = new List<char>();
//        static void Main(string[] args)
//        {
//            addChars(ref chars);
//            while (true)
//            {
//                Console.WriteLine("enter the length of the password");
//                int length = 0;
//                if (int.TryParse(Console.ReadLine(), out length))
//                {
//                    Console.WriteLine(generatePassword(length));
//                }
//            }
//        }

//        static string generatePassword(int length)
//        {
//            StringBuilder sb = new StringBuilder();
//            Random rnd = new Random();
//            int j = 0;
//            while (j < length)
//            {
//                sb.Append(chars[rnd.Next(0, chars.Count)]);
//                j++;
//            }
//            return sb.ToString();
//        }

//        static void addChars(ref List<char> chars)
//        {
//            for (char c = 'a'; c <= 'z'; c++)
//            {
//                {
//                    chars.Add(c);
//                }
//            }
//            {
//                for (char c = 'A'; c <= 'Z'; c++)
//                {
//                    {
//                        chars.Add(c);
//                    }
//                }
//                for (char c = '!'; c <= '?'; c++)
//                {
//                    {
//                        chars.Add(c);
//                    }
//                }
//            }
//        } 
//    }
//    }
