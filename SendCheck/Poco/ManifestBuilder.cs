using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SendCheck.Poco
{
    public static class ManifestBuilder
    {
        public static ManifestEnvelope Build(IReadOnlyList<InternalDbFileManifestItem> items, string baseUrl)
        {
            if (items == null)
                items = Array.Empty<InternalDbFileManifestItem>();

            List<InternalDbFileManifestItem> cleaned = items
                                                       .Where(m => m != null && !string.IsNullOrWhiteSpace(m.FileName))
                                                       .ToList();

            int datCount = cleaned.Count(m => m.FileName.EndsWith(".DAT", StringComparison.OrdinalIgnoreCase));

            int? maxDatIndex = cleaned
                .Select(m => TryParseDatIndex(m.FileName))
                .Where(i => i.HasValue)
                .Select(i => i.Value)
                .DefaultIfEmpty()
                .Max();

            if (maxDatIndex == 0)
                maxDatIndex = null;

            // canonicalize items before hashing
            var canonical = cleaned
                .OrderBy(m => m.FileName, StringComparer.OrdinalIgnoreCase)
                .Select(m => new
                {
                    FileName = m.FileName,
                    UrlToFile = m.UrlToFile,
                    ExpectedFileSize = m.ExpectedFileSize,
                    Crc = m.Crc
                })
                .ToList();

            string json = JsonConvert.SerializeObject(canonical);

            string sha = ComputeSha256Hex(json);

            return new ManifestEnvelope
            {
                Header = new ManifestHeader
                {
                    SchemaVersion = 1,
                    GeneratedUtc = DateTime.UtcNow,
                    BaseUrl = baseUrl,
                    ItemCount = cleaned.Count,
                    DatCount = datCount,
                    MaxDatIndex = maxDatIndex,
                    ItemsSha256 = sha
                },
                Items = cleaned
            };
        }

        private static int? TryParseDatIndex(string fileName)
        {
            // NavSync000.DAT .. NavSync063.DAT
            if (string.IsNullOrWhiteSpace(fileName)) return null;
            if (!fileName.EndsWith(".DAT", StringComparison.OrdinalIgnoreCase)) return null;
            if (!fileName.StartsWith("NavSync", StringComparison.OrdinalIgnoreCase)) return null;

            string mid = fileName.Substring(
                "NavSync".Length,
                fileName.Length - "NavSync".Length - ".DAT".Length);

            int idx;
            if (int.TryParse(mid, NumberStyles.Integer, CultureInfo.InvariantCulture, out idx))
                return idx;

            return null;
        }

        private static string ComputeSha256Hex(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text ?? string.Empty);

            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(bytes);

                var sb = new StringBuilder(hash.Length * 2);
                for (int i = 0; i < hash.Length; i++)
                    sb.Append(hash[i].ToString("x2"));

                return sb.ToString();
            }
        }
    }
}
