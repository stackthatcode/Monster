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
                Quantity =  3,
                UnitPrice = 114.95m,
                TaxAmount = 26.73m,
                TaxLines = TestTaxes.DefaultTaxes
            });

            // Act
            Assert.AreEqual(26.73m, transfer.TotalTax);
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
                Quantity = 3,
                UnitPrice = 114.95m,
                TaxAmount = 26.73m,
                TaxLines = TestTaxes.DefaultTaxes
            });

            transfer.Freight.TaxLines = TestTaxes.DefaultTaxes;
            transfer.Freight.IsTaxable = true;
            transfer.Freight.Price = 50.0m;
            transfer.Freight.TaxAmount = 5.00m;

            // Act
            Assert.AreEqual(31.73m, transfer.TotalTax);
            Assert.AreEqual(26.73m, transfer.TotalLineItemTaxAfterRefunds);
            Assert.AreEqual(5.00m, transfer.TotalFreightTaxAfterRefunds);
            Assert.AreEqual(344.85m, transfer.TotalTaxableLineAmountsAfterRefund);
            Assert.AreEqual(50.0m, transfer.TotalTaxableFreightAfterRefund);
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
                Quantity = 3,
                UnitPrice = 114.95m,
                TaxAmount = 26.73m,
                TaxLines = TestTaxes.DefaultTaxes
            });

            transfer.Refunds.Add(new TransferRefund
            {
                RefundAmount = 114.95m,
                TotalTaxableLineAmounts = 114.95m,
                TotalLineItemsTax = 8.91m,
                TaxableFreightAmount = 0m,
                FreightTax = 0m,
            });

            transfer.Refunds.Add(new TransferRefund
            {
                RefundAmount = 114.95m,
                TotalTaxableLineAmounts = 114.95m,
                TotalLineItemsTax = 8.91m,
                TaxableFreightAmount = 0m,
                FreightTax = 0m,
            });

            // Act
            Assert.AreEqual(26.73m, transfer.TotalTax);
            Assert.AreEqual(8.91m, transfer.TotalLineItemTaxAfterRefunds);
            Assert.AreEqual(0m, transfer.TotalFreightTaxAfterRefunds);
            Assert.AreEqual(114.95m, transfer.TotalTaxableLineAmountsAfterRefund);
            Assert.AreEqual(0m, transfer.TotalTaxableFreightAfterRefund);
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
                Quantity = 3,
                UnitPrice = 114.95m,
                TaxAmount = 26.73m,
                TaxLines = TestTaxes.DefaultTaxes
            });

            transfer.Freight.TaxLines = TestTaxes.DefaultTaxes;
            transfer.Freight.IsTaxable = true;
            transfer.Freight.Price = 50.0m;
            transfer.Freight.TaxAmount = 5.00m;

            transfer.Refunds.Add(new TransferRefund
            {
                RefundAmount = 114.95m,
                TotalTaxableLineAmounts = 114.95m,
                TotalLineItemsTax = 8.91m,
                TaxableFreightAmount = 0m,
                FreightTax = 0m,
            });

            // Act
            Assert.AreEqual(31.73m, transfer.TotalTax);
            Assert.AreEqual(17.82m, transfer.TotalLineItemTaxAfterRefunds);
            Assert.AreEqual(5.00m, transfer.TotalFreightTaxAfterRefunds);
            Assert.AreEqual(229.90m, transfer.TotalTaxableLineAmountsAfterRefund);
            Assert.AreEqual(50.0m, transfer.TotalTaxableFreightAfterRefund);
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
                Quantity = 3,
                UnitPrice = 114.95m,
                TaxAmount = 26.73m,
                TaxLines = TestTaxes.DefaultTaxes
            });

            transfer.Freight.TaxLines = TestTaxes.DefaultTaxes;
            transfer.Freight.IsTaxable = true;
            transfer.Freight.Price = 50.0m;
            transfer.Freight.TaxAmount = 5.00m;

            transfer.Refunds.Add(new TransferRefund
            {
                RefundAmount = 114.95m,
                TotalTaxableLineAmounts = 114.95m,
                TotalLineItemsTax = 8.91m,
                TaxableFreightAmount = 0m,
                FreightTax = 0m,
            });

            transfer.Refunds.Add(new TransferRefund
            {
                RefundAmount = 25.00m,
                TotalTaxableLineAmounts = 0.00m,
                TotalLineItemsTax = 0.00m,
                TaxableFreightAmount = 25.00m,
                FreightTax = 2.50m,
            });

            // Act
            Assert.AreEqual(31.73m, transfer.TotalTax);
            Assert.AreEqual(17.82m, transfer.TotalLineItemTaxAfterRefunds);
            Assert.AreEqual(2.50m, transfer.TotalFreightTaxAfterRefunds);
            Assert.AreEqual(229.90m, transfer.TotalTaxableLineAmountsAfterRefund);
            Assert.AreEqual(25.00m, transfer.TotalTaxableFreightAfterRefund);

        }
    }
}
