using System;
using System.Linq;

namespace KhaoSat.Utils
{
    public static class CodeGenerator
    {
        private static Random random = new Random();

        public static string GenerateCode(int length = 6)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZqwertyuiopasdfghjklzxcvbnm0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
