using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Utiles
{
    public  class HashearClave
    {
        public  string HashearSHA256(string input)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hash = sha.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        public  bool EsHashSHA256(string clave)
        {
            return clave.Length == 64 && clave.All(c => Uri.IsHexDigit(c));
        }
    }
}
