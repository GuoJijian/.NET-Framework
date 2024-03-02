using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Webapi.Core {
    public static partial class PasswordUtil {
        static byte[] abc = { 178, 12, 56, 21, 57, 90, 1, 0, 34, 56, 7, 98, 2, 255 };
        public static string GetPasswordHash(string password) {
            using (var md5 = MD5.Create("md5")) {
                var hash = md5.ComputeHash(md5.ComputeHash(Encoding.UTF8.GetBytes(password)).Concat(abc).ToArray());
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }

        public static bool VerificationPassword(string password, string passwordhash) {
            return string.Equals(passwordhash, GetPasswordHash(password));
        }
    }
}
