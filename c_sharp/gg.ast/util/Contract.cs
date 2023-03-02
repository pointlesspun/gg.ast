/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System;
using System.Collections.Generic;
using System.Linq;

namespace gg.ast.util
{
    public class ContractException : Exception
    {
        public ContractException()
        {
        }

        public ContractException(string message) : base(message)
        {
        }

        public ContractException(object expected, object actual)
            : base($"Contract failed: expected ${expected} actual ${actual})")
        {
        }
    }


    public static class Contract
    {
        [System.Diagnostics.Conditional("DEBUG")]
        public static void Requires(bool test, string message = "")
        {
            if (!test)
            {
                throw new ContractException(message);
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void RequiresNoNullValues<T>(IEnumerable<T> values, string message = "") where T : class
        {
            if (values == null || values.Any(x => x == null))
            {
                throw new ContractException(message ?? "encountered one or more null values");
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void RequiresNotNull(object value, string message = "")
        {
            if (value == null)
            {
                throw new ContractException(message ?? "expected a non null value");
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void RequiresNotNullOrEmpty(string value, string message = "")
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ContractException(message ?? "expected a non null or not empty string");
            }
        }
    }
}
