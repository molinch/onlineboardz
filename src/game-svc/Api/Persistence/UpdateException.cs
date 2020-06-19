using System;

namespace Api.Persistence
{
    public class UpdateException: Exception
    {
        public UpdateException(): base("Update didn't change anything")
        {

        }
    }
}
