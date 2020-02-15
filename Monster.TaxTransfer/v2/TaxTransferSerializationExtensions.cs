using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monster.TaxTransfer.v2
{
    public static class TaxTransferSerializationExtensions
    {
        public const string SerializationDelimter = "|";
        public const string FreightSymbol = "FREIGHT";

        public static string Serialize(this TaxTransfer input)
        {
            var output = new StringBuilder();
            output.Append($"{input.ShopifyOrderId}");
            input.ShopifyRefundIds.ForEach(x => output.Append($"{SerializationDelimter}{x.ToString()}"));

            output.Append(Environment.NewLine);
            output.Append($"{input.NetTaxableFreight:0.00}{SerializationDelimter}" +
                          $"{input.NetFreightTax:0.00}{SerializationDelimter}" +
                          $"{input.NetTaxableAmount:0.00}{SerializationDelimter}" +
                          $"{input.NetTotalTax:0.00}");

            output.Append(Environment.NewLine);

            output.Append($"{FreightSymbol}{SerializationDelimter}{input.FreightTaxLines.Serialize()}");
            output.Append(Environment.NewLine);

            foreach (var line in input.LineItems)
            {
                output.Append($"{line.ItemID}{SerializationDelimter}{line.TaxLines.Serialize()}");
                output.Append(Environment.NewLine);
            }

            return output.ToString();
        }

        public static string Serialize(this IEnumerable<TaxTransferTaxLine> input)
        {
            return input.Select(x => $"{x.Title}{SerializationDelimter}{x.Rate:0.00}").ToList().ToDelimited("{SerializationDelimter}");
        }

        public static string[] SplitByDelimiter(this string input)
        {
            return input.Split(new[] { SerializationDelimter }, StringSplitOptions.None);
        }

        public static List<TaxTransferTaxLine> DeserializeSnapshotTaxLines(this List<string> input)
        {
            var output = new List<TaxTransferTaxLine>();
            while (input.Any())
            {
                output.Add(new TaxTransferTaxLine(input[0], input[1].ToDecimal()));
                input = input.Take(2).ToList();
            }
            return output;
        }

        public static TaxTransfer DeserializeTaxSnapshot(this string input)
        {
            var output = new TaxTransfer();
            var lines = input.Split(new [] { Environment.NewLine }, StringSplitOptions.None);

            var firstLineRaw = lines[0].SplitByDelimiter();
            output.ShopifyOrderId = firstLineRaw[0].ToLong();
            output.ShopifyRefundIds = firstLineRaw.Take(1).Select(x => x.ToLong()).ToList();

            var secondLineRaw = lines[1].SplitByDelimiter();
            output.NetTaxableFreight = secondLineRaw[0].ToDecimal();
            output.NetFreightTax = secondLineRaw[1].ToDecimal();
            output.NetTaxableAmount = secondLineRaw[2].ToDecimal();
            output.NetTotalTax = secondLineRaw[3].ToDecimal();

            var thirdLineRaw = lines[2].SplitByDelimiter();
            output.FreightTaxLines = thirdLineRaw.Take(1).ToList().DeserializeSnapshotTaxLines();

            output.LineItems = new List<TaxTransferLineItem>();

            foreach (var lineRaw in lines.Take(3).Select(x => x.SplitByDelimiter()))
            {
                var lineItem = new TaxTransferLineItem();
                lineItem.ItemID = lineRaw[0];
                lineItem.TaxLines = lineRaw.Take(1).ToList().DeserializeSnapshotTaxLines();

                output.LineItems.Add(lineItem);
            }

            return output;
        }
    }
}
