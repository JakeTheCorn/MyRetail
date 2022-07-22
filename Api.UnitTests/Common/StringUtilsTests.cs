using NUnit.Framework;
using static Api.Common.StringUtils;

namespace Api.UnitTests.Common
{
    public class ContainsOnlyAlphanumericTests
    {
        [DatapointSource] public ContainsOnlyAlphanumericTestCase[] Values =
        {
            new("String contains symbols", "&^", false),
            new("String contains whitespace", " ", false),
            new("String is alphanumeric", "ab123", true),
            new("String is empty", "", true),
            new("String is null", null, false),
        };
        
        [Theory]
        public void It_Returns_True_For_Any_String_Which_Contains_Only_Alphanumeric(ContainsOnlyAlphanumericTestCase testCase)
        {
            var result = ContainsOnlyAlphanumeric(testCase.Input);
            
            Assert.AreEqual(testCase.Expected, result, testCase.Message);
        }
    }

    public class ContainsOnlyAlphanumericTestCase
    {
        public readonly string Message;
        public readonly string Input;
        public readonly bool Expected;

        public ContainsOnlyAlphanumericTestCase(string message, string input, bool expected)
        {
            Message = message;
            Input = input;
            Expected = expected;
        }
    }
}