using System;
using System.Linq;
using NUnit.Framework;

namespace Api.UnitTests
{
    public static class TestUtils
    {
        public static string BuildStringOfLength(int length)
        {
            var random = new Random();

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var resultString = new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            
            Assert.AreEqual(length, resultString.Length);

            return resultString;
        }
        
        public static string GetValidTestSku()
        {
            const string validSku = "0123456789abcdef";

            Assert.AreEqual(16, validSku.Length);
            StringAssert.IsMatch("^[a-zA-Z0-9]*$", validSku);
            
            return validSku;
        }
    }
}