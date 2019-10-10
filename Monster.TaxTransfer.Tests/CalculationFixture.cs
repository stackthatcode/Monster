using NUnit.Framework;

namespace Monster.TaxTransfer.Tests
{
    [TestFixture]
    public class CalculationFixture
    {
        [Test]
        public void TaxesWithNoShipping()
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
            Assert.AreEqual(26.73m, transfer.OriginalTotalTax);
            Assert.AreEqual(26.73m, transfer.TotalLineItemTaxAfterRefunds);
            Assert.AreEqual(0.00m, transfer.TotalFreightTaxAfterRefunds);
        }
        
        [Test]
        public void TaxesWithShipping()
        {
            // Arrange
            var transfer = new Transfer();

            transfer.LineItems.Add(new TransferLineItem
            {
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

            // Act
            Assert.AreEqual(31.73m, transfer.OriginalTotalTax);
            Assert.AreEqual(26.73m, transfer.TotalLineItemTaxAfterRefunds);
            Assert.AreEqual(5.00m, transfer.TotalFreightTaxAfterRefunds);
        }

        [Test]
        public void TaxesNoShippingAndTwoLineItemRefunds()
        {
            // Arrange
            var transfer = new Transfer();

            transfer.LineItems.Add(new TransferLineItem
            {
                InventoryID = "ROUNDING-DOOM",
                IsTaxable = true,
                OriginalQuantity = 3,
                UnitPrice = 114.95m,
                OriginalTotalTax = 26.73m,
                TaxLines = TestTaxes.DefaultTaxes
            });

            transfer.Refunds.Add(new TransferRefund
            {
                FreightTax = 0m,
                NonFreightTax = 8.91m,
                RefundAmount = 114.95m,
            });

            transfer.Refunds.Add(new TransferRefund
            {
                FreightTax = 0m,
                NonFreightTax = 8.91m,
                RefundAmount = 114.95m,
            });

            // Act
            Assert.AreEqual(26.73m, transfer.OriginalTotalTax);
            Assert.AreEqual(8.91m, transfer.TotalLineItemTaxAfterRefunds);
            Assert.AreEqual(0m, transfer.TotalFreightTaxAfterRefunds);
        }

        [Test]
        public void TaxesWithShippingAndOneLineItemRefund()
        {
            // Arrange
            var transfer = new Transfer();

            transfer.LineItems.Add(new TransferLineItem
            {
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
                FreightTax = 0m,
                NonFreightTax = 8.91m,
                RefundAmount = 114.95m,
            });

            // Act
            Assert.AreEqual(31.73m, transfer.OriginalTotalTax);
            Assert.AreEqual(17.82m, transfer.TotalLineItemTaxAfterRefunds);
            Assert.AreEqual(5.00m, transfer.TotalFreightTaxAfterRefunds);
        }

        [Test]
        public void TaxesWithShippingAndLineItemRefundAndPartialShippingRefund()
        {
            // Arrange
            var transfer = new Transfer();

            transfer.LineItems.Add(new TransferLineItem
            {
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
                FreightTax = 0.00m,
                NonFreightTax = 8.91m,
                RefundAmount = 114.95m,
            });

            transfer.Refunds.Add(new TransferRefund
            {
                FreightTax = 2.50m,
                NonFreightTax = 0.00m,
                RefundAmount = 25.00m,
            });

            // Act
            Assert.AreEqual(31.73m, transfer.OriginalTotalTax);
            Assert.AreEqual(17.82m, transfer.TotalLineItemTaxAfterRefunds);
            Assert.AreEqual(2.50m, transfer.TotalFreightTaxAfterRefunds);
        }
    }
}
