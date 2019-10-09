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
                Quantity =  3,
                UnitPrice = 114.95m,
                TaxLines = TestTaxes.DefaultTaxes
            });

            // Act
            Assert.AreEqual(transfer.TotalNonTaxableLineItemAmount + transfer.TotalTaxableLineItemAmount, 344.85m);
            Assert.AreEqual(transfer.TotalTax, 26.73m);
            Assert.AreEqual(transfer.TotalTaxAfterRefunds, 26.73m);
        }

        public void ModifiedQuantityCalculation()
        {
            // Arrange
            var transfer = new Transfer();

            transfer.LineItems.Add(new TransferLineItem
            {
                InventoryID = "ROUNDING-DOOM",
                IsTaxable = true,
                Quantity = 3,
                UnitPrice = 114.95m,
                TaxLines = TestTaxes.DefaultTaxes
            });

            // Act
            Assert.AreEqual(transfer.TotalNonTaxableLineItemAmount + transfer.TotalTaxableLineItemAmount, 344.85m);
            Assert.AreEqual(transfer.TotalTax, 26.73m);
            Assert.AreEqual(transfer.TotalTaxAfterRefunds, 26.73m);
        }
    }
}
