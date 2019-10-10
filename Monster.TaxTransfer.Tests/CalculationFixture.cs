using System.Collections.Generic;
using NUnit.Framework;

namespace Monster.TaxTransfer.Tests
{
    [TestFixture]
    public class CalculationFixture
    {
        [Test]
        public void PlainVanillaTaxes()
        {
            // Arrange
            var transfer = new Transfer();

            transfer.LineItems.Add(new TransferLineItem
            {
                InventoryID = "ROUNDING-DOOM",
                IsTaxable = true,
                OriginalQuantity =  3,
                UnitPrice = 114.95m,
                OriginalTotalTax = 26.73m,
                TaxLines = TestTaxes.DefaultTaxes
            });

            // Act
            Assert.AreEqual(transfer.OriginalTotalTax, 26.73m);
            Assert.AreEqual(transfer.TotalLineItemTaxAfterRefunds, 26.73m);
            Assert.AreEqual(transfer.TotalFreightTaxAfterRefunds, 0.00m);
        }


        public void VerifyAfterRefundCalculations()
        {
            // Arrange
            var transfer = new Transfer();

            transfer.LineItems.Add(new TransferLineItem
            {
                InventoryID = "ROUNDING-DOOM",
                IsTaxable = true,
                OriginalQuantity = 3,
                UnitPrice = 114.95m,
                TaxLines = TestTaxes.DefaultTaxes
            });

            transfer.Refunds.Add(new TransferRefund
            {
                FreightTax = 0m,
                NonFreightTax = 8.91m,
                RefundAmount = 114.95m,
            });

            // Act
            Assert.AreEqual(transfer.OriginalTotalTax, 26.73m);
            Assert.AreEqual(transfer.TotalLineItemTaxAfterRefunds, 26.73m);
            Assert.AreEqual(transfer.TotalFreightTaxAfterRefunds, 17.82m);
        }
    }
}
