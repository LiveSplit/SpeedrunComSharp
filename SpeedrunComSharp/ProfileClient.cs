using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeedrunComSharp
{
    public class ProfileClient
    {
        public ProfileClient(SpeedrunComClient baseClient)
        {
        }

        public User GetProfile()
        {
            throw new NotAuthorizedException();
        }
    }
}
