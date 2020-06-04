using System;

namespace Api.Exceptions
{
    public class ItemNotFoundException: Exception
    {
        public ItemNotFoundException() : base("The entity does not exist") // we need to deal with localization...
        {

        }
    }
}
