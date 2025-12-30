using System.Security.Cryptography;
using System.Text;

namespace RestoBook_MiniProjet.Services
{
    // WHY: Never store plain passwords in database
    public class PasswordHasher
    {
        private const int SaltSize = 16; // 128 bits
        private const int KeySize = 32;  // 256 bits
        private const int Iterations = 10000;
        private static readonly HashAlgorithmName _algorithm = HashAlgorithmName.SHA256;

        private const char SegmentDelimiter = ':';

        public string Hash(string password)
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

        public bool Verify(string password, string hashedPassword)
        {
            try
            {
                string[] segments = hashedPassword.Split(SegmentDelimiter);

                // Backward compatibility: If no delimiter, try old SHA256
                if (segments.Length != 2)
                {
                    return VerifyLegacySha256(password, hashedPassword);
                }

                byte[] salt = Convert.FromBase64String(segments[0]);
                byte[] hash = Convert.FromBase64String(segments[1]);

                byte[] inputHash = Rfc2898DeriveBytes.Pbkdf2(
                    password,
                    salt,
                    Iterations,
                    _algorithm,
                    KeySize
                );

                return CryptographicOperations.FixedTimeEquals(hash, inputHash);
            }
            catch
            {
                // Fallback for any parsing errors
                return false;
            }
        }

        private bool VerifyLegacySha256(string password, string hashedPassword)
        {
            try 
            {
                using var sha = SHA256.Create();
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash) == hashedPassword;
            }
            catch
            {
                return false;
            }
        }
    }
}
