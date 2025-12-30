using System;
using System.Security.Cryptography;

namespace RestoBook_MiniProjet
{
    // Utility program to generate password hashes for the SQL file
    // Run this to get the hashes, then copy them into RestoBook_Database.sql
    public class GeneratePasswordHashes
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 10000;
        private static readonly HashAlgorithmName _algorithm = HashAlgorithmName.SHA256;
        private const char SegmentDelimiter = ':';

        public static void Main(string[] args)
        {
            Console.WriteLine("=== RestoBook Password Hash Generator ===\n");

            // Generate hashes for the test accounts
            var passwords = new Dictionary<string, string>
            {
                { "Admin User (admin@restobook.com)", "Admin@123" },
                { "John Doe (john.doe@email.com)", "Customer1@123" },
                { "Jane Smith (jane.smith@email.com)", "Customer2@123" },
                { "Michael Johnson (michael.j@email.com)", "Customer3@123" }
            };

            foreach (var kvp in passwords)
            {
                string hash = HashPassword(kvp.Value);
                Console.WriteLine($"{kvp.Key}");
                Console.WriteLine($"Password: {kvp.Value}");
                Console.WriteLine($"Hash: {hash}");
                Console.WriteLine();
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        private static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                _algorithm,
                KeySize
            );

            return string.Join(
                SegmentDelimiter,
                Convert.ToBase64String(salt),
                Convert.ToBase64String(hash)
            );
        }
    }
}
