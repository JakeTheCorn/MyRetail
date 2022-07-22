using System.Linq;

namespace Api.Common
{
    public static class StringUtils
    {
        public static bool ContainsOnlyAlphanumeric(string s)
        {
            if (s is null)
                return false;

            return s.All(char.IsLetterOrDigit);
        }
    }
}