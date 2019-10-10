using System.Diagnostics;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Monster.TaxTransfer.Tests
{
    [TestFixture]
    public class JsonFactoryFixture
    {
        [Test]
        public void GenerateTransferWithShipping()
        {
            // Arrange
            var transfer = new Transfer();
            transfer.ExternalRefNbr = "99123452879250";

            transfer.LineItems.Add(new TransferLineItem
            {
                ExternalRefNbr = "11111112223",
                InventoryID = "ROUNDING-DOOM",
                IsTaxable = true,
                OriginalQuantity = 3,
                UnitPrice = 114.95m,
                OriginalTotalTax = 26.73m,
                TaxLines = TestTaxes.DefaultTaxes
            });

            transfer.Freight.TaxLines = TestTaxes.DefaultTaxes;
            transfer.Freight.IsTaxable = true;
            transfer.Freight.OriginalPrice = 50.00m;
            transfer.Freight.OriginalTotalTax = 5.00m;

            // Emit
            Debug.WriteLine(JsonConvert.SerializeObject(transfer));
        }

        [Test]
        public void GenerateTransferWithShippingAndRefund()
        {
            // Arrange
            var transfer = new Transfer();
            transfer.LineItems.Add(new TransferLineItem
            {
                ExternalRefNbr = "1234567890",
                InventoryID = "ROUNDING-DOOM",
                IsTaxable = true,
                OriginalQuantity = 3,
                UnitPrice = 114.95m,
                OriginalTotalTax = 26.73m,
                TaxLines = TestTaxes.DefaultTaxes
            });

            transfer.Freight.TaxLines = TestTaxes.DefaultTaxes;
            transfer.Freight.IsTaxable = true;
            transfer.Freight.OriginalTotalTax = 5.00m;

            transfer.Refunds.Add(new TransferRefund
            {
                ExternalRefNbr = "777777777777",
                FreightTax = 0.00m,
                NonFreightTax = 8.91m,
                RefundAmount = 114.95m,
            });

            // Emit
            Debug.WriteLine(JsonConvert.SerializeObject(transfer));
        }
    }
}
