using System;
using System.Collections.Generic;
using System.Net;
using System.Windows.Documents;
using System.Windows.Input;
using ForgeManifest;
using MinecraftManifest;
using MinecraftVersionManifest_NS;

namespace SKProCHLauncher
{
    public class GameUtilities
    {
        private string GetForgeVersionsManifest(string url = @"http://files.minecraftforge.net/maven/net/minecraftforge/forge/json") {
            try{
                WebClient wc = new WebClient();
                return wc.DownloadString(url);
            }
            catch (Exception){
                return null;
            }
        }

        public Forge DeserializeForge(string jsonstring = null) {
            if (jsonstring == null){
                jsonstring = GetForgeVersionsManifest();
            }
            return Forge.FromJson(jsonstring);
        }

        public List<long> GetForgeVersionsIDByMCVersion(string version, Forge forgemanifest = null) {
            if (forgemanifest == null){
                forgemanifest = DeserializeForge();
            }

            if (forgemanifest.Mcversion.ContainsKey(version)){
                foreach (var VARIABLE in forgemanifest.Mcversion){
                    if (VARIABLE.Key == version){
                        return VARIABLE.Value;
                    }
                }
            }
            else{
                return null;
            }
            return null;
        }

        public List<ForgeManifest.Number> GetForgeVersionsByMCVersion(string version, Forge forgemanifest = null) {
            var ForgeIDs = GetForgeVersionsIDByMCVersion(version, forgemanifest);
            if (forgemanifest == null){
                forgemanifest = DeserializeForge();
            }

            List<ForgeManifest.Number> listtoreturn = new List<Number>();
            foreach (var number in forgemanifest.Number){
                if (ForgeIDs.Contains(Convert.ToInt32(number.Key))){
                    listtoreturn.Add(number.Value);
                }
            }
            return listtoreturn;
        }

        //public string GetForgeManifest(string version) {

        //}






        public string GetMinecraftVersionsManifest(string url = "https://launchermeta.mojang.com/mc/game/version_manifest.json") {
            using (WebClient wc = new WebClient()){
                return wc.DownloadString(url);
            }
        }

        public Minecraft GetMinecraftManifest(string rawjson = null) {
            if (rawjson == null){
                GetMinecraftVersionsManifest();
            }
            return Minecraft.FromJson(rawjson);
        }

        public MinecraftVersionManifest GetMinecraftVersionManifest(string version, Minecraft mcmanifest = null) {
            if (mcmanifest == null){
                mcmanifest = GetMinecraftManifest();
            }
            foreach (var VARIABLE in mcmanifest.Versions){
                if (VARIABLE.Id == version)
                {
                    using (WebClient wc = new WebClient()){
                        return MinecraftVersionManifest.FromJson(wc.DownloadString(VARIABLE.Url));
                    }
                }
            }
            return null;
        }

        public string InstallMinecraft(string version, string launcherfolder, Minecraft mcmanifest = null) {
            if (mcmanifest == null){
                mcmanifest = GetMinecraftManifest();
            }

            var manifest = GetMinecraftVersionManifest(version, mcmanifest);
            if (manifest == null){
                return "VnE";
            }

            foreach (var VARIABLE in manifest.Libraries){
                if (VARIABLE.Downloads){
                    VARIABLE.Downloads.Classifiers.
                }
            }
        }
    }
}

namespace ForgeManifest
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class Forge
    {
        [JsonProperty("adfocus", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(ParseStringConverter))]
        public long? Adfocus { get; set; }

        [JsonProperty("artifact", NullValueHandling = NullValueHandling.Ignore)]
        public string Artifact { get; set; }

        [JsonProperty("branches", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, List<long>> Branches { get; set; }

        [JsonProperty("homepage", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Homepage { get; set; }

        [JsonProperty("mcversion", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, List<long>> Mcversion { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("number", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, Number> Number { get; set; }

        [JsonProperty("promos", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, long> Promos { get; set; }

        [JsonProperty("webpath", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Webpath { get; set; }
    }

    public partial class Number
    {
        [JsonProperty("branch")]
        public BranchUnion? Branch { get; set; }

        [JsonProperty("build", NullValueHandling = NullValueHandling.Ignore)]
        public long? Build { get; set; }

        [JsonProperty("files", NullValueHandling = NullValueHandling.Ignore)]
        public List<List<string>> Files { get; set; }

        [JsonProperty("mcversion", NullValueHandling = NullValueHandling.Ignore)]
        public Mcversion? Mcversion { get; set; }

        [JsonProperty("modified", NullValueHandling = NullValueHandling.Ignore)]
        public long? Modified { get; set; }

        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }
    }

    public enum BranchEnum { EhUnit, Failtests, Mc172, New, Prerelease, The1100, The111X, The1710, The1710Ls, The18, The188, The189, The19, The190, The194 };

    public enum Mcversion { The11, The110, The1102, The111, The1112, The112, The1121, The1122, The123, The124, The125, The132, The140, The141, The142, The143, The144, The145, The146, The147, The15, The151, The152, The161, The162, The163, The164, The1710, The1710_Pre4, The172, The18, The188, The189, The19, The194 };

    public partial struct BranchUnion
    {
        public BranchEnum? Enum;
        public long? Integer;

        public static implicit operator BranchUnion(BranchEnum Enum) => new BranchUnion { Enum = Enum };
        public static implicit operator BranchUnion(long Integer) => new BranchUnion { Integer = Integer };
        public bool IsNull => Integer == null && Enum == null;
    }

    public partial class Forge
    {
        public static Forge FromJson(string json) => JsonConvert.DeserializeObject<Forge>(json, ForgeManifest.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Forge self) => JsonConvert.SerializeObject(self, ForgeManifest.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                BranchUnionConverter.Singleton,
                BranchEnumConverter.Singleton,
                McversionConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }

    internal class BranchUnionConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(BranchUnion) || t == typeof(BranchUnion?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Null:
                    return new BranchUnion { };
                case JsonToken.String:
                case JsonToken.Date:
                    var stringValue = serializer.Deserialize<string>(reader);
                    switch (stringValue)
                    {
                        case "1.10.0":
                            return new BranchUnion { Enum = BranchEnum.The1100 };
                        case "1.11.x":
                            return new BranchUnion { Enum = BranchEnum.The111X };
                        case "1.7.10":
                            return new BranchUnion { Enum = BranchEnum.The1710 };
                        case "1.8":
                            return new BranchUnion { Enum = BranchEnum.The18 };
                        case "1.8.8":
                            return new BranchUnion { Enum = BranchEnum.The188 };
                        case "1.8.9":
                            return new BranchUnion { Enum = BranchEnum.The189 };
                        case "1.9":
                            return new BranchUnion { Enum = BranchEnum.The19 };
                        case "1.9.0":
                            return new BranchUnion { Enum = BranchEnum.The190 };
                        case "1.9.4":
                            return new BranchUnion { Enum = BranchEnum.The194 };
                        case "1710ls":
                            return new BranchUnion { Enum = BranchEnum.The1710Ls };
                        case "EHUnit":
                            return new BranchUnion { Enum = BranchEnum.EhUnit };
                        case "failtests":
                            return new BranchUnion { Enum = BranchEnum.Failtests };
                        case "mc172":
                            return new BranchUnion { Enum = BranchEnum.Mc172 };
                        case "new":
                            return new BranchUnion { Enum = BranchEnum.New };
                        case "prerelease":
                            return new BranchUnion { Enum = BranchEnum.Prerelease };
                    }
                    long l;
                    if (Int64.TryParse(stringValue, out l))
                    {
                        return new BranchUnion { Integer = l };
                    }
                    break;
            }
            throw new Exception("Cannot unmarshal type BranchUnion");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (BranchUnion)untypedValue;
            if (value.IsNull)
            {
                serializer.Serialize(writer, null);
                return;
            }
            if (value.Enum != null)
            {
                switch (value.Enum)
                {
                    case BranchEnum.The1100:
                        serializer.Serialize(writer, "1.10.0");
                        return;
                    case BranchEnum.The111X:
                        serializer.Serialize(writer, "1.11.x");
                        return;
                    case BranchEnum.The1710:
                        serializer.Serialize(writer, "1.7.10");
                        return;
                    case BranchEnum.The18:
                        serializer.Serialize(writer, "1.8");
                        return;
                    case BranchEnum.The188:
                        serializer.Serialize(writer, "1.8.8");
                        return;
                    case BranchEnum.The189:
                        serializer.Serialize(writer, "1.8.9");
                        return;
                    case BranchEnum.The19:
                        serializer.Serialize(writer, "1.9");
                        return;
                    case BranchEnum.The190:
                        serializer.Serialize(writer, "1.9.0");
                        return;
                    case BranchEnum.The194:
                        serializer.Serialize(writer, "1.9.4");
                        return;
                    case BranchEnum.The1710Ls:
                        serializer.Serialize(writer, "1710ls");
                        return;
                    case BranchEnum.EhUnit:
                        serializer.Serialize(writer, "EHUnit");
                        return;
                    case BranchEnum.Failtests:
                        serializer.Serialize(writer, "failtests");
                        return;
                    case BranchEnum.Mc172:
                        serializer.Serialize(writer, "mc172");
                        return;
                    case BranchEnum.New:
                        serializer.Serialize(writer, "new");
                        return;
                    case BranchEnum.Prerelease:
                        serializer.Serialize(writer, "prerelease");
                        return;
                }
            }
            if (value.Integer != null)
            {
                serializer.Serialize(writer, value.Integer.Value.ToString());
                return;
            }
            throw new Exception("Cannot marshal type BranchUnion");
        }

        public static readonly BranchUnionConverter Singleton = new BranchUnionConverter();
    }

    internal class BranchEnumConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(BranchEnum) || t == typeof(BranchEnum?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "1.10.0":
                    return BranchEnum.The1100;
                case "1.11.x":
                    return BranchEnum.The111X;
                case "1.7.10":
                    return BranchEnum.The1710;
                case "1.8":
                    return BranchEnum.The18;
                case "1.8.8":
                    return BranchEnum.The188;
                case "1.8.9":
                    return BranchEnum.The189;
                case "1.9":
                    return BranchEnum.The19;
                case "1.9.0":
                    return BranchEnum.The190;
                case "1.9.4":
                    return BranchEnum.The194;
                case "1710ls":
                    return BranchEnum.The1710Ls;
                case "EHUnit":
                    return BranchEnum.EhUnit;
                case "failtests":
                    return BranchEnum.Failtests;
                case "mc172":
                    return BranchEnum.Mc172;
                case "new":
                    return BranchEnum.New;
                case "prerelease":
                    return BranchEnum.Prerelease;
            }
            throw new Exception("Cannot unmarshal type BranchEnum");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (BranchEnum)untypedValue;
            switch (value)
            {
                case BranchEnum.The1100:
                    serializer.Serialize(writer, "1.10.0");
                    return;
                case BranchEnum.The111X:
                    serializer.Serialize(writer, "1.11.x");
                    return;
                case BranchEnum.The1710:
                    serializer.Serialize(writer, "1.7.10");
                    return;
                case BranchEnum.The18:
                    serializer.Serialize(writer, "1.8");
                    return;
                case BranchEnum.The188:
                    serializer.Serialize(writer, "1.8.8");
                    return;
                case BranchEnum.The189:
                    serializer.Serialize(writer, "1.8.9");
                    return;
                case BranchEnum.The19:
                    serializer.Serialize(writer, "1.9");
                    return;
                case BranchEnum.The190:
                    serializer.Serialize(writer, "1.9.0");
                    return;
                case BranchEnum.The194:
                    serializer.Serialize(writer, "1.9.4");
                    return;
                case BranchEnum.The1710Ls:
                    serializer.Serialize(writer, "1710ls");
                    return;
                case BranchEnum.EhUnit:
                    serializer.Serialize(writer, "EHUnit");
                    return;
                case BranchEnum.Failtests:
                    serializer.Serialize(writer, "failtests");
                    return;
                case BranchEnum.Mc172:
                    serializer.Serialize(writer, "mc172");
                    return;
                case BranchEnum.New:
                    serializer.Serialize(writer, "new");
                    return;
                case BranchEnum.Prerelease:
                    serializer.Serialize(writer, "prerelease");
                    return;
            }
            throw new Exception("Cannot marshal type BranchEnum");
        }

        public static readonly BranchEnumConverter Singleton = new BranchEnumConverter();
    }

    internal class McversionConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Mcversion) || t == typeof(Mcversion?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "1.1":
                    return Mcversion.The11;
                case "1.10":
                    return Mcversion.The110;
                case "1.10.2":
                    return Mcversion.The1102;
                case "1.11":
                    return Mcversion.The111;
                case "1.11.2":
                    return Mcversion.The1112;
                case "1.12":
                    return Mcversion.The112;
                case "1.12.1":
                    return Mcversion.The1121;
                case "1.12.2":
                    return Mcversion.The1122;
                case "1.2.3":
                    return Mcversion.The123;
                case "1.2.4":
                    return Mcversion.The124;
                case "1.2.5":
                    return Mcversion.The125;
                case "1.3.2":
                    return Mcversion.The132;
                case "1.4.0":
                    return Mcversion.The140;
                case "1.4.1":
                    return Mcversion.The141;
                case "1.4.2":
                    return Mcversion.The142;
                case "1.4.3":
                    return Mcversion.The143;
                case "1.4.4":
                    return Mcversion.The144;
                case "1.4.5":
                    return Mcversion.The145;
                case "1.4.6":
                    return Mcversion.The146;
                case "1.4.7":
                    return Mcversion.The147;
                case "1.5":
                    return Mcversion.The15;
                case "1.5.1":
                    return Mcversion.The151;
                case "1.5.2":
                    return Mcversion.The152;
                case "1.6.1":
                    return Mcversion.The161;
                case "1.6.2":
                    return Mcversion.The162;
                case "1.6.3":
                    return Mcversion.The163;
                case "1.6.4":
                    return Mcversion.The164;
                case "1.7.10":
                    return Mcversion.The1710;
                case "1.7.10_pre4":
                    return Mcversion.The1710_Pre4;
                case "1.7.2":
                    return Mcversion.The172;
                case "1.8":
                    return Mcversion.The18;
                case "1.8.8":
                    return Mcversion.The188;
                case "1.8.9":
                    return Mcversion.The189;
                case "1.9":
                    return Mcversion.The19;
                case "1.9.4":
                    return Mcversion.The194;
            }
            throw new Exception("Cannot unmarshal type Mcversion");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Mcversion)untypedValue;
            switch (value)
            {
                case Mcversion.The11:
                    serializer.Serialize(writer, "1.1");
                    return;
                case Mcversion.The110:
                    serializer.Serialize(writer, "1.10");
                    return;
                case Mcversion.The1102:
                    serializer.Serialize(writer, "1.10.2");
                    return;
                case Mcversion.The111:
                    serializer.Serialize(writer, "1.11");
                    return;
                case Mcversion.The1112:
                    serializer.Serialize(writer, "1.11.2");
                    return;
                case Mcversion.The112:
                    serializer.Serialize(writer, "1.12");
                    return;
                case Mcversion.The1121:
                    serializer.Serialize(writer, "1.12.1");
                    return;
                case Mcversion.The1122:
                    serializer.Serialize(writer, "1.12.2");
                    return;
                case Mcversion.The123:
                    serializer.Serialize(writer, "1.2.3");
                    return;
                case Mcversion.The124:
                    serializer.Serialize(writer, "1.2.4");
                    return;
                case Mcversion.The125:
                    serializer.Serialize(writer, "1.2.5");
                    return;
                case Mcversion.The132:
                    serializer.Serialize(writer, "1.3.2");
                    return;
                case Mcversion.The140:
                    serializer.Serialize(writer, "1.4.0");
                    return;
                case Mcversion.The141:
                    serializer.Serialize(writer, "1.4.1");
                    return;
                case Mcversion.The142:
                    serializer.Serialize(writer, "1.4.2");
                    return;
                case Mcversion.The143:
                    serializer.Serialize(writer, "1.4.3");
                    return;
                case Mcversion.The144:
                    serializer.Serialize(writer, "1.4.4");
                    return;
                case Mcversion.The145:
                    serializer.Serialize(writer, "1.4.5");
                    return;
                case Mcversion.The146:
                    serializer.Serialize(writer, "1.4.6");
                    return;
                case Mcversion.The147:
                    serializer.Serialize(writer, "1.4.7");
                    return;
                case Mcversion.The15:
                    serializer.Serialize(writer, "1.5");
                    return;
                case Mcversion.The151:
                    serializer.Serialize(writer, "1.5.1");
                    return;
                case Mcversion.The152:
                    serializer.Serialize(writer, "1.5.2");
                    return;
                case Mcversion.The161:
                    serializer.Serialize(writer, "1.6.1");
                    return;
                case Mcversion.The162:
                    serializer.Serialize(writer, "1.6.2");
                    return;
                case Mcversion.The163:
                    serializer.Serialize(writer, "1.6.3");
                    return;
                case Mcversion.The164:
                    serializer.Serialize(writer, "1.6.4");
                    return;
                case Mcversion.The1710:
                    serializer.Serialize(writer, "1.7.10");
                    return;
                case Mcversion.The1710_Pre4:
                    serializer.Serialize(writer, "1.7.10_pre4");
                    return;
                case Mcversion.The172:
                    serializer.Serialize(writer, "1.7.2");
                    return;
                case Mcversion.The18:
                    serializer.Serialize(writer, "1.8");
                    return;
                case Mcversion.The188:
                    serializer.Serialize(writer, "1.8.8");
                    return;
                case Mcversion.The189:
                    serializer.Serialize(writer, "1.8.9");
                    return;
                case Mcversion.The19:
                    serializer.Serialize(writer, "1.9");
                    return;
                case Mcversion.The194:
                    serializer.Serialize(writer, "1.9.4");
                    return;
            }
            throw new Exception("Cannot marshal type Mcversion");
        }

        public static readonly McversionConverter Singleton = new McversionConverter();
    }
}

namespace MinecraftManifest
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class Minecraft
    {
        [JsonProperty("latest", NullValueHandling = NullValueHandling.Ignore)]
        public Latest Latest { get; set; }

        [JsonProperty("versions", NullValueHandling = NullValueHandling.Ignore)]
        public List<Version> Versions { get; set; }
    }

    public partial class Latest
    {
        [JsonProperty("release", NullValueHandling = NullValueHandling.Ignore)]
        public string Release { get; set; }

        [JsonProperty("snapshot", NullValueHandling = NullValueHandling.Ignore)]
        public string Snapshot { get; set; }
    }

    public partial class Version
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public TypeEnum? Type { get; set; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Url { get; set; }

        [JsonProperty("time", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? Time { get; set; }

        [JsonProperty("releaseTime", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? ReleaseTime { get; set; }
    }

    public enum TypeEnum { OldAlpha, OldBeta, Release, Snapshot };

    public partial class Minecraft
    {
        public static Minecraft FromJson(string json) => JsonConvert.DeserializeObject<Minecraft>(json, MinecraftManifest.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Minecraft self) => JsonConvert.SerializeObject(self, MinecraftManifest.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                TypeEnumConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class TypeEnumConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(TypeEnum) || t == typeof(TypeEnum?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "old_alpha":
                    return TypeEnum.OldAlpha;
                case "old_beta":
                    return TypeEnum.OldBeta;
                case "release":
                    return TypeEnum.Release;
                case "snapshot":
                    return TypeEnum.Snapshot;
            }
            throw new Exception("Cannot unmarshal type TypeEnum");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (TypeEnum)untypedValue;
            switch (value)
            {
                case TypeEnum.OldAlpha:
                    serializer.Serialize(writer, "old_alpha");
                    return;
                case TypeEnum.OldBeta:
                    serializer.Serialize(writer, "old_beta");
                    return;
                case TypeEnum.Release:
                    serializer.Serialize(writer, "release");
                    return;
                case TypeEnum.Snapshot:
                    serializer.Serialize(writer, "snapshot");
                    return;
            }
            throw new Exception("Cannot marshal type TypeEnum");
        }

        public static readonly TypeEnumConverter Singleton = new TypeEnumConverter();
    }
}

namespace MinecraftVersionManifest_NS
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class MinecraftVersionManifest
    {
        [JsonProperty("assetIndex", NullValueHandling = NullValueHandling.Ignore)]
        public AssetIndex AssetIndex { get; set; }

        [JsonProperty("assets", NullValueHandling = NullValueHandling.Ignore)]
        public string Assets { get; set; }

        [JsonProperty("downloads", NullValueHandling = NullValueHandling.Ignore)]
        public MinecraftVersionManifestDownloads Downloads { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("libraries", NullValueHandling = NullValueHandling.Ignore)]
        public List<Library> Libraries { get; set; }

        [JsonProperty("logging", NullValueHandling = NullValueHandling.Ignore)]
        public Logging Logging { get; set; }

        [JsonProperty("mainClass", NullValueHandling = NullValueHandling.Ignore)]
        public string MainClass { get; set; }

        [JsonProperty("minecraftArguments", NullValueHandling = NullValueHandling.Ignore)]
        public string MinecraftArguments { get; set; }

        [JsonProperty("minimumLauncherVersion", NullValueHandling = NullValueHandling.Ignore)]
        public long? MinimumLauncherVersion { get; set; }

        [JsonProperty("releaseTime", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? ReleaseTime { get; set; }

        [JsonProperty("time", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? Time { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }
    }

    public partial class AssetIndex
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("sha1", NullValueHandling = NullValueHandling.Ignore)]
        public string Sha1 { get; set; }

        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public long? Size { get; set; }

        [JsonProperty("totalSize", NullValueHandling = NullValueHandling.Ignore)]
        public long? TotalSize { get; set; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Url { get; set; }
    }

    public partial class MinecraftVersionManifestDownloads
    {
        [JsonProperty("client", NullValueHandling = NullValueHandling.Ignore)]
        public ServerClass Client { get; set; }

        [JsonProperty("server", NullValueHandling = NullValueHandling.Ignore)]
        public ServerClass Server { get; set; }

        [JsonProperty("windows_server", NullValueHandling = NullValueHandling.Ignore)]
        public ServerClass WindowsServer { get; set; }
    }

    public partial class ServerClass
    {
        [JsonProperty("sha1", NullValueHandling = NullValueHandling.Ignore)]
        public string Sha1 { get; set; }

        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public long? Size { get; set; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Url { get; set; }

        [JsonProperty("path", NullValueHandling = NullValueHandling.Ignore)]
        public string Path { get; set; }
    }

    public partial class Library
    {
        [JsonProperty("downloads", NullValueHandling = NullValueHandling.Ignore)]
        public LibraryDownloads Downloads { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("extract", NullValueHandling = NullValueHandling.Ignore)]
        public Extract Extract { get; set; }

        [JsonProperty("natives", NullValueHandling = NullValueHandling.Ignore)]
        public Natives Natives { get; set; }

        [JsonProperty("rules", NullValueHandling = NullValueHandling.Ignore)]
        public List<Rule> Rules { get; set; }
    }

    public partial class LibraryDownloads
    {
        [JsonProperty("artifact", NullValueHandling = NullValueHandling.Ignore)]
        public ServerClass Artifact { get; set; }

        [JsonProperty("classifiers", NullValueHandling = NullValueHandling.Ignore)]
        public Classifiers Classifiers { get; set; }
    }

    public partial class Classifiers
    {
        [JsonProperty("natives-linux", NullValueHandling = NullValueHandling.Ignore)]
        public ServerClass NativesLinux { get; set; }

        [JsonProperty("natives-osx", NullValueHandling = NullValueHandling.Ignore)]
        public ServerClass NativesOsx { get; set; }

        [JsonProperty("natives-windows", NullValueHandling = NullValueHandling.Ignore)]
        public ServerClass NativesWindows { get; set; }

        [JsonProperty("natives-windows-32", NullValueHandling = NullValueHandling.Ignore)]
        public ServerClass NativesWindows32 { get; set; }

        [JsonProperty("natives-windows-64", NullValueHandling = NullValueHandling.Ignore)]
        public ServerClass NativesWindows64 { get; set; }
    }

    public partial class Extract
    {
        [JsonProperty("exclude", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Exclude { get; set; }
    }

    public partial class Natives
    {
        [JsonProperty("linux", NullValueHandling = NullValueHandling.Ignore)]
        public string Linux { get; set; }

        [JsonProperty("osx", NullValueHandling = NullValueHandling.Ignore)]
        public string Osx { get; set; }

        [JsonProperty("windows", NullValueHandling = NullValueHandling.Ignore)]
        public string Windows { get; set; }
    }

    public partial class Rule
    {
        [JsonProperty("action", NullValueHandling = NullValueHandling.Ignore)]
        public string Action { get; set; }

        [JsonProperty("os", NullValueHandling = NullValueHandling.Ignore)]
        public Os Os { get; set; }
    }

    public partial class Os
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
    }

    public partial class Logging
    {
        [JsonProperty("client", NullValueHandling = NullValueHandling.Ignore)]
        public LoggingClient Client { get; set; }
    }

    public partial class LoggingClient
    {
        [JsonProperty("argument", NullValueHandling = NullValueHandling.Ignore)]
        public string Argument { get; set; }

        [JsonProperty("file", NullValueHandling = NullValueHandling.Ignore)]
        public AssetIndex File { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }
    }

    public partial class MinecraftVersionManifest
    {
        public static MinecraftVersionManifest FromJson(string json) => JsonConvert.DeserializeObject<MinecraftVersionManifest>(json, MinecraftVersionManifest_NS.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this MinecraftVersionManifest self) => JsonConvert.SerializeObject(self, MinecraftVersionManifest_NS.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}

namespace MinecraftAssetsManifest
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class AssetsManifest
    {
        [JsonProperty("objects", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, Object> Objects { get; set; }
    }

    public partial class Object
    {
        [JsonProperty("hash", NullValueHandling = NullValueHandling.Ignore)]
        public string Hash { get; set; }

        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public long? Size { get; set; }
    }

    public partial class AssetsManifest
    {
        public static AssetsManifest FromJson(string json) => JsonConvert.DeserializeObject<AssetsManifest>(json, MinecraftManifest.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this AssetsManifest self) => JsonConvert.SerializeObject(self, MinecraftManifest.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
                                                                 {
                                                                     MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                                                                     DateParseHandling        = DateParseHandling.None,
                                                                     Converters =
                                                                     {
                                                                         new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
                                                                     },
                                                                 };
    }
}



