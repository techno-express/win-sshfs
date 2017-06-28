namespace Sshfs
{
    class Settings
    {
        public Settings(bool useNetworkDrive, bool useOfflineAttribute, int attributeCacheTimeout, int dirContentCacheTimeout)
        {
            UseNetworkDrive = useNetworkDrive;
            UseOfflineAttribute = useOfflineAttribute;
            AttributeCacheTimeout = attributeCacheTimeout;
            DirContentCacheTimeout = dirContentCacheTimeout;
        }

        public static Settings Default { get; } = new Settings(true, true, 5, 60);
        public bool UseNetworkDrive { get; }
        public bool UseOfflineAttribute { get; }
        public int AttributeCacheTimeout { get; }
        public int DirContentCacheTimeout { get; }
    }
}