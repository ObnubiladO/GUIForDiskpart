using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;


namespace GUIForDiskpart.Database.Retrievers
{
    public class Disk
    {
        private const string WIN32_VOL_CHANGE_EVENT_QUERY = 
            "SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2 or EventType = 3";
        private const string WIN32_DISKDRIVE_QUERY = "select * from Win32_DiskDrive";
        private const string OP_TYPE_PATH = @"root\Microsoft\Windows\Storage";

        private static string OPSelectQuery(uint diskIndex) => $"select * from MSFT_Disk WHERE Number={diskIndex}";
        private readonly Dictionary<string, ushort> mediaTypes = new(StringComparer.OrdinalIgnoreCase);

        public void LoadMediaTypes()
        {
            mediaTypes.Clear();
            try
            {
                ManagementScope scope = new(OP_TYPE_PATH);
                using ManagementObjectSearcher searcher = new(scope, new SelectQuery("SELECT FriendlyName, MediaType FROM MSFT_PhysicalDisk"));
                using ManagementObjectCollection disks = searcher.Get();
                foreach (ManagementObject disk in disks)
                {
                    string? name = disk["FriendlyName"]?.ToString()?.Trim();
                    if (string.IsNullOrEmpty(name) || disk["MediaType"] == null) continue;
                    mediaTypes[name] = System.Convert.ToUInt16(disk["MediaType"]);
                }
            }
            catch (ManagementException)
            {
                // Media type is optional metadata; drive loading can continue without it.
            }
            catch (UnauthorizedAccessException)
            {
                // Media type is optional metadata; drive loading can continue without it.
            }
        }

        public void SetupDiskChangedWatcher()
        {
            try
            {
                WqlEventQuery query = new WqlEventQuery(WIN32_VOL_CHANGE_EVENT_QUERY);
                ManagementEventWatcher watcher = new ManagementEventWatcher();
                watcher.Query = query;
                watcher.EventArrived += DiskService.ExecuteOnDiskChanged;
                watcher.Options.Timeout = TimeSpan.FromSeconds(3);
                watcher.Start();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        public List<ManagementObject> GetAllWMIObjects()
        {
            ManagementObjectSearcher diskDriveQuery = new ManagementObjectSearcher(WIN32_DISKDRIVE_QUERY);

            List<ManagementObject> retVal = new();
            foreach (ManagementObject disk in diskDriveQuery.Get())
            {
                retVal.Add(disk);
            }

            return retVal;
        }

        public ushort? GetMediaTypeValue(string friendlyName)
        {
            return MatchMediaType(friendlyName, mediaTypes);
        }

        internal static ushort? MatchMediaType(string friendlyName, IReadOnlyDictionary<string, ushort> mediaTypes)
        {
            if (string.IsNullOrWhiteSpace(friendlyName)) return null;

            friendlyName = friendlyName.Trim();
            if (mediaTypes.TryGetValue(friendlyName, out ushort exactMatch)) return exactMatch;

            // Win32_DiskDrive sometimes appends a device suffix to the storage name.
            var matches = mediaTypes.Where(item => friendlyName.Contains(item.Key, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(item => item.Key.Length).Take(2).ToArray();

            if (matches.Length == 0 || (matches.Length == 2 && matches[0].Key.Length == matches[1].Key.Length))
                return null;

            return matches[0].Value;
        }

        public ushort[] GetOperationalStatus(uint diskIndex)
        {
            ManagementScope scope = new ManagementScope(OP_TYPE_PATH);
            SelectQuery query = new SelectQuery(OPSelectQuery(diskIndex));
            ManagementObjectSearcher msftDiskQuery = new ManagementObjectSearcher(scope, query);
            foreach (ManagementObject msftDisk in msftDiskQuery.Get())
            {
                return (ushort[])msftDisk.Properties["OperationalStatus"].Value;
            }
            return new ushort[1] { 0 };
        }
    }
}
