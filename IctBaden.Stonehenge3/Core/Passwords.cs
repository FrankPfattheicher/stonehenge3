using System.IO;
using CryptSharp;

namespace IctBaden.Stonehenge3.Core
{
    public class Passwords
    {
        private readonly string _fileName;

        /// <summary>
        /// Use mosquitto_passwd to create and maintain password file
        /// </summary>
        /// <param name="fileName"></param>
        public Passwords(string fileName)
        {
            _fileName = fileName;
        }

        // ReSharper disable once UnusedMember.Global
        public bool IsValid(string user, string password)
        {
            if (string.IsNullOrEmpty(password)) return false;

            var lines = File.ReadAllLines(_fileName);
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var line in lines)
            {
                var userPw = line.Split(':');
                if (userPw.Length != 2) continue;
                if (userPw[0] != user) continue;

                return Crypter.CheckPassword(password, userPw[1]);
            }

            return false;
        }

    }
}

