

using System;

namespace Cw6.Exceptions
{
    public class EnrollmentException : Exception
    {
        public EnrollmentException()
        {
        }

        public EnrollmentException(string message) : base(message)
        { 
        }
    }
}
