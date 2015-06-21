using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeedrunComSharp
{
    public class ProfileClient
    {
        private SpeedrunComClient baseClient;

        public ProfileClient(SpeedrunComClient baseClient)
        {
            this.baseClient = baseClient;
        }

        public User GetProfile()
        {
            throw new NotAuthorizedException();
        }
    }
}
