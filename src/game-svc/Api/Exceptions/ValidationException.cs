using System;

namespace Api.Exceptions
{
    public class ValidationException: Exception
    {
        public ValidationException(string message): base(message) // we need to deal with localization...
        {

        }
    }
}
