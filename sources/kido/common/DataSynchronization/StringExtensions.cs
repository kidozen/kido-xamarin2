using System.Text;

namespace Kidozen.iOS
{
    static class StringExtensions
    {
        public static string ValueOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value) ? string.Empty : value;
        }

        public static string EnsureOnlyASCII(this string value)
        {
            var result = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(value));
            if (string.Compare(value, result, false) != 0)
                throw new DataSyncException("Couchbase database names cannot contain uppercase letters! The only legal characters are lowercase ASCII letters, digits, and the special characters _$()+-/ ");
            else return value;
        }
    }
}
