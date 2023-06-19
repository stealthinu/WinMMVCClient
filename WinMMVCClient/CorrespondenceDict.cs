using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace WinMMVCClient
{
    public class Correspondence
    {
        public int AdjustSemitones { get; set; }

        [Index(0)]
        public int Id { get; set; }
        [Index(1)]
        public float F0 { get; set; }
        [Index(2)]
        public string? Name { get; set; }
    }

    public class CorrespondenceDictReader
    {
        public static Dictionary<int, Correspondence> ReadDataFromFile(string filePath, int sourceId = 0)
        {
            var config = new CsvConfiguration(System.Globalization.CultureInfo.CurrentCulture)
            {
                Delimiter = "|",
                HasHeaderRecord = false
            };

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, config))
            {
                var records = csv.GetRecords<Correspondence>();
                var dict = new Dictionary<int, Correspondence>();
                foreach (var record in records)
                {
                    dict[record.Id] = record;
                }
                var sourceF0 = dict[sourceId].F0;
                foreach (var (key, value) in dict)
                {
                    value.AdjustSemitones = PitchUtils.GetSemitoneDifference(sourceF0, value.F0);
                }

                return dict;
            }
        }
    }
}
