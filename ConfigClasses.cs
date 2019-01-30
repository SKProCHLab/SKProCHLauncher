using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace SKProCHLauncher
{
    [Magic]
    public class UserConfig : PropertyChangedBase
    {
        public UserConfig() {
            LocalModpacks    = new List<ModPack>();
            ExportedModpacks = new List<ModPack>();
            Accounts         = new List<Account>();
        }

        public List<ModPack> LocalModpacks    { get; set; }
        public List<ModPack> ExportedModpacks { get; set; }
        public List<Account> Accounts         { get; set; }

        public class Account : PropertyChangedBase
        {
            public string NickName { get; set; }
            public string UUID     { get; set; }

            public bool IsLegacy { get; set; }

            public string Login    { get; set; }
            public string Password { get; set; }
        }

        public class ModPack : PropertyChangedBase
        {
            private bool   DownLoadsScheduled;
            private object locker = new object();

            private string NewVersion;
            public  string Name                 { get; set; }
            public  string ServerName           { get; set; }
            public  string UpdatesURL           { get; set; }
            public  int[]  CurrentVersion       { get; set; }
            public  int    MaxNumbersInVersions { get; set; }
            public  string Icon                 { get; set; }
            public  string PathToClient         { get; set; }

            public bool IsLocal { get; set; }

            //OnlyLocal
            public string        MinecraftVersion    { get; set; } //Minecraft Version
            public string        ForgeID             { get; set; } //Forge Version
            public List<Account> Accounts            { get; set; }
            public int           ChoosenAccountIndex { get; set; }
            public bool          IsLocalAssets       { get; set; }
            public bool          IsLocalLibraties    { get; set; }

            private void CheckUpdates() {
                try{
                    var wc = new WebClient();
                    wc.DownloadFile(UpdatesURL, Path.Combine(Path.GetTempPath(), @"\SKProCH's Version File.tmp"));
                    try{
                        var VersionsContent = File.ReadAllText(Path.Combine(Path.GetTempPath(), @"\SKProCH's Version File.tmp"));
                        File.Delete(Path.Combine(Path.GetTempPath(), @"\SKProCH's Version File.tmp"));

                        try{
                            var MU = JsonConvert.DeserializeObject<ModpackInfo>(VersionsContent);

                            //Checking new versions
                            var VersionsToUpdate = new List<ModpackInfo.Version>();
                            foreach (var version in MU.Versions){
                                if (VersionCompare(CurrentVersion, version.VersionID, MU.MaxNumbersInVersions)) VersionsToUpdate.Add(version);
                            }

                            //Updating if we have something to update
                            if (VersionsToUpdate.Count != 0)
                                foreach (var version in VersionsToUpdate){
                                    //Let's delete
                                    var FilesToDelete = new List<string>();
                                    foreach (var VARIABLE in version.ListToDelete){
                                        try{
                                            if (!Directory.Exists(VARIABLE) && File.Exists(VARIABLE))
                                                FilesToDelete.Add(VARIABLE);
                                            else if (Directory.Exists(VARIABLE) && !File.Exists(VARIABLE))
                                                foreach (var folder in RecursiveFolderScan(VARIABLE)){
                                                    FilesToDelete.AddRange(Directory.GetFiles(folder));
                                                }
                                        }
                                        catch (Exception){
                                        }
                                    }
                                    foreach (var VARIABLE in FilesToDelete){
                                        try{
                                            File.SetAttributes(VARIABLE, FileAttributes.Normal);
                                            File.Delete(VARIABLE);
                                        }
                                        catch (Exception e){
                                            //
                                        }
                                    }

                                    //Let's download
                                    foreach (var download in version.ListToDownload){
                                    }
                                }
                        }
                        catch (Exception){
                        }
                    }
                    catch (Exception){
                    }
                }
                catch (Exception){
                }
            }

            public void InstallModpack(bool isExport, string path) {
            }

            /// <summary>
            ///     Return TRUE if Comparewith > Current
            /// </summary>
            public static bool VersionCompare(int[] current, int[] latest, int maxcount) {
                var pattern = new string('0', maxcount);
                var ones    = "1";
                var twos    = "1";

                foreach (var i in current){
                    ones += (pattern + Convert.ToString(i)).Remove(0, (pattern + Convert.ToString(i)).Length - maxcount >= 0 ? (pattern + Convert.ToString(i)).Length - maxcount : 0);
                }

                foreach (var i in latest){
                    twos += (pattern + Convert.ToString(i)).Remove(0, (pattern + Convert.ToString(i)).Length - maxcount >= 0 ? (pattern + Convert.ToString(i)).Length - maxcount : 0);
                }

                if (Convert.ToUInt32(ones) < Convert.ToUInt32(twos)) return true;
                return false;
            }

            public static List<string> RecursiveFolderScan(string dirname) {
                var ToReturn = new List<string>();
                if (Directory.Exists(dirname))
                    foreach (var VARIABLE in Directory.GetDirectories(dirname)){
                        ToReturn.AddRange(RecursiveFolderScan(VARIABLE));
                        ToReturn.Add(VARIABLE);
                    }
                return ToReturn;
            }
        }
    }


    public class ModpackInfo
    {
        public ModpackInfo() {
            Versions = new List<Version>();
        }

        public string ModpackName   { get; set; }
        public string ModpackServer { get; set; }
        public string URL           { get; set; }
        public string Icon          { get; set; } //URL To Icon
        public string MCVersion     { get; set; } //Minecraft Version
        public string ForgeVersion  { get; set; } //Forge Version

        public int[] CurrentVersion       { get; set; } //Versioning
        public int   MaxNumbersInVersions { get; set; }

        public bool IsOnlyForLocal       { get; set; }
        public bool IsRequireLocalAssets { get; set; }
        public bool IsRequireLocalLibs   { get; set; }

        public List<Version>     Versions     { get; set; }
        public List<Enchansment> Enchansments { get; set; }

        public class Version
        {
            public Version() {
                ListToDelete   = new List<string>();
                ListToDownload = new List<string>();
            }
            public int[]        VersionID      { get; set; }
            public List<string> ListToDelete   { get; set; }
            public List<string> ListToDownload { get; set; }
        }

        public class Enchansment
        {
        }
    }

    #region AvailableModpacks

    [Magic]
    public class AvailableModpack : PropertyChangedBase
    {
        public string ID             { get; set; }
        public string Name           { get; set; }
        public string Icon           { get; set; }
        public string Server         { get; set; }
        public string McVersion      { get; set; }
        public string PathToManifest { get; set; }
    }

    #endregion
}
