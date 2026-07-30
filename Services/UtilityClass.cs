using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace GeocodingAPI.Services
{
    public class Utility
    {
        public static byte[] GetAddressSHA256(string address)
        {
            byte[] hash = Array.Empty<byte>();
            address = Regex.Replace(address, @"\s*[.,]", "");

            byte[] _byadd = Encoding.UTF8.GetBytes(address);
            byte[] _byhash = SHA256.Create().ComputeHash(_byadd);
            if (_byhash != null && _byhash.Length > 0)
            {
                hash = _byhash;
            }
            return hash;
        }
    }
    public class NotFoundException : Exception
    {
        public NotFoundException(string message)
        : base(message) { }

    }

    public class ExternalAPIException : Exception
    {
        public ExternalAPIException(string message)
        : base(message) { }

    }

    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message) { }
    }

    public class ServerError : Exception
    {
        public ServerError(string message) : base(message) { }
    }

    public class ErrorResponse
    {
        public string Message { get; set; }
        public int Status { get; set; } = 0;
        public string Title { get; set; }
        public DateTime TimeStamp { get; set; }

    }
    public static class GeoCodeErrors
    {
        public const string NotFound = "The requested location does not exist.";
        public const string InvalidInput = "Bad request, kindly recheck the address entered.";
        public const string ServerError = "Internal server error.";
        public const string ExternalError = "Unable to connect to external APIs.";
    }
}
