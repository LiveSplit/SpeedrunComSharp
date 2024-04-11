using System;

namespace SpeedrunComSharp
{
    public class RunSystem
    {
        public string PlatformID { get; private set; }
        public bool IsEmulated { get; private set; }
        public string RegionID { get; private set; }

        #region Links

        internal Lazy<Platform> platform;
        internal Lazy<Region> region;

        public Platform Platform { get { return platform.Value; } }
        public Region Region { get { return region.Value; } }

        #endregion

        private RunSystem() { }

        public static RunSystem Parse(SpeedrunComClient client, dynamic systemElement)
        {
            var system = new RunSystem();

            system.IsEmulated = (bool)systemElement.emulated;

            if (!string.IsNullOrEmpty(systemElement.platform as string))
            {
                system.PlatformID = systemElement.platform as string;
                system.platform = new Lazy<Platform>(() => client.Platforms.GetPlatform(system.PlatformID));
            }
            else
            {
                system.platform = new Lazy<Platform>(() => null);
            }

            if (!string.IsNullOrEmpty(systemElement.region as string))
            {
                system.RegionID = systemElement.region as string;
                system.region = new Lazy<Region>(() => client.Regions.GetRegion(system.RegionID));
            }
            else
            {
                system.region = new Lazy<Region>(() => null);
            }

            return system;
        }
    }
}
