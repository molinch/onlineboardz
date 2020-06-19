using System;

namespace Api.Persistence
{
    public class InsertException : Exception
    {
        public InsertException(): base("Insert didn't happen")
        {

        }
    }
}
