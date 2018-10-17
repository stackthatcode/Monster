﻿using Monster.Acumatica.Http;
using Push.Foundation.Utilities.Logging;
using Push.Shopify.Api.Product;

namespace Monster.Middle.Processes.Inventory
{
    public class InventoryManager
    {
        private readonly LocationWorker _locationWorker;
        private readonly AcumaticaHttpContext _acumaticaContext;
        private readonly ShopifyInventoryWorker _shopifyProductWorker;
        private readonly AcumaticaProductWorker _acumaticaProductWorker;
        private readonly IPushLogger _logger;

        public InventoryManager(
                AcumaticaHttpContext acumaticaContext,
                ShopifyInventoryWorker shopifyProductWorker, 
                AcumaticaProductWorker acumaticaProductWorker, 
                LocationWorker locationWorker, IPushLogger logger)
        {
            _acumaticaContext = acumaticaContext;
            _shopifyProductWorker = shopifyProductWorker;
            _acumaticaProductWorker = acumaticaProductWorker;
            _locationWorker = locationWorker;
            _logger = logger;
        }

        public void BaselineSync()
        {
            // Pull all possible Products from Shopify
            _shopifyProductWorker.BaselinePullLocations();
            _shopifyProductWorker.BaselinePullProducts();

            // Acumatica Pull
            _acumaticaContext.Begin();
            _locationWorker.BaselinePullWarehouses();
            _acumaticaProductWorker.BaselinePullStockItems();
        }

        public void DiffSync()
        {

        }
    }
}
