using System.Security.Cryptography;

namespace FileShareBusinessLayer.Helper
{
    public static class Pdkd2Helper
    {
        public static string CreateHash(byte[] value)
        {
            // Create the salt value with a cryptographic PRNG
            var salt = new byte[16];
            new RNGCryptoServiceProvider().GetBytes(salt);

            // Create the Rfc2898DeriveBytes and get the hash value
            var pbkdf2 = new Rfc2898DeriveBytes(value, salt, 100000);
            var hash = pbkdf2.GetBytes(20);

            // Combine the salt and password bytes for later use
            var hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            // Turn the combined salt+hash into a string
            var savedPasswordHash = Convert.ToBase64String(hashBytes);
            return savedPasswordHash;
        }

        public static bool Verify(byte[] value, string savedValue)
        {
            if (savedValue.Length != 64)
            {
                return false;
            }

            // Extract the bytes
            var hashBytes = Convert.FromBase64String(savedValue);

            // Get the salt
            var salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            // Compute the hash on the password the user entered
            var pbkdf2 = new Rfc2898DeriveBytes(value, salt, 100000);
            var hash = pbkdf2.GetBytes(20);

            // Compare the results
            var result = false;
            for (var i = 0; i < 20; i++)
                result = hashBytes[i + 16] == hash[i];

            return result;
        }
    }
}
