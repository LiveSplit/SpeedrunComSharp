using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeedrunComSharp
{
    public class RunSystem
    {
        public string PlatformID { get; private set; }
        public bool IsEmulated { get; private set; }
        public string RegionID { get; private set; }

        #region Links

        private Lazy<Platform> platform;
        private Lazy<Region> region;

        public Platform Platform { get { return platform.Value; } }
        public Region Region { get { return region.Value; } }

        #endregion

        private RunSystem() { }

        public static RunSystem Parse(SpeedrunComClient client, dynamic systemElement)
        {
            var system = new RunSystem();

            system.PlatformID = systemElement.platform as string;
            system.IsEmulated = (bool)systemElement.emulated;
            system.RegionID = systemElement.region as string;

            if (!string.IsNullOrEmpty(system.PlatformID))
                system.platform = new Lazy<Platform>(() => client.Platforms.GetPlatform(system.PlatformID));
            else
                system.platform = new Lazy<Platform>(() => null);

            if (!string.IsNullOrEmpty(system.RegionID))
                system.region = new Lazy<Region>(() => client.Regions.GetRegion(system.RegionID));
            else
                system.region = new Lazy<Region>(() => null);

            return system;
        }
    }
}
