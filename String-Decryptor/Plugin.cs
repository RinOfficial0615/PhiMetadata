/*
    Copyright 2017-2021 Katy Coe - http://www.djkaty.com - https://github.com/djkaty

    All rights reserved.
*/

using Il2CppInspector.PluginAPI;
using Il2CppInspector.PluginAPI.V100;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Il2CppInspector.Plugins.Core
{
    // Plugin definition
    public class StringXorPlugin : IPlugin, ICorePlugin, ILoadPipeline
    {
        public string Id => "string-xor-phi";
        public string Name => "Metadata strings XOR decryptor";
        public string Author => "Il2CppInspector & RikkaRin";
        public string Version => "2025.1.0";
        public string Description => "Automatic detection and decryption of XOR-encrypted metadata strings";
        public List<IPluginOption> Options => null;

        // Decrypt XOR-encrypted strings in global-metadata.dat
        public void PostProcessMetadata(Metadata metadata, PluginPostProcessMetadataEventInfo data) {

            // To check for encryption, find every single string start position by scanning all of the definitions
            var stringOffsets = metadata.Images.Select(x => x.NameIndex)
                        .Concat(metadata.Assemblies.Select(x => x.Aname.NameIndex))
                        .Concat(metadata.Assemblies.Select(x => x.Aname.CultureIndex))
                        .Concat(metadata.Assemblies.Select(x => x.Aname.HashValueIndex)) // <=24.3
                        .Concat(metadata.Assemblies.Select(x => x.Aname.PublicKeyIndex))
                        .Concat(metadata.Events.Select(x => x.NameIndex))
                        .Concat(metadata.Fields.Select(x => x.NameIndex))
                        .Concat(metadata.Methods.Select(x => x.NameIndex))
                        .Concat(metadata.Params.Select(x => x.NameIndex))
                        .Concat(metadata.Properties.Select(x => x.NameIndex))
                        .Concat(metadata.Types.Select(x => x.NameIndex))
                        .Concat(metadata.Types.Select(x => x.NamespaceIndex))
                        .Concat(metadata.GenericParameters.Select(x => x.NameIndex))
                        .OrderBy(x => x)
                        .Distinct()
                        .ToList();

            // Now confirm that all the keys are present in the string dictionary
            if (metadata.Header.StringSize == 0 || !stringOffsets.Except(metadata.Strings.Keys).Any())
                return;

            // If they aren't, that means one or more of the null terminators wasn't null, indicating potential encryption
            // Only do this if we need to because it's very slow
            PluginServices.For(this).StatusUpdate("Decrypting Phigros strings");

            // Start again
            metadata.Strings.Clear();
            metadata.Position = metadata.Header.StringOffset;

            while (metadata.Position < metadata.Header.StringOffset + metadata.Header.StringSize)
            {
                int index = (int)(metadata.Position - metadata.Header.StringOffset);
                var bytes = new List<byte>();
                byte b = (byte)(index % 0xFF);
                while ((b = (byte)(metadata.ReadByte() ^ b)) != 0)
                    bytes.Add(b);
                metadata.Strings.Add(index, Encoding.UTF8.GetString(bytes.ToArray()));
            }

            // Write changes back in case the user wants to save the metadata file
            metadata.Position = metadata.Header.StringOffset;
            foreach (var str in metadata.Strings.OrderBy(s => s.Key))
                metadata.WriteNullTerminatedString(str.Value);
            metadata.Flush();

            data.IsDataModified = true;
            data.IsStreamModified = true;
        }
    }
}
