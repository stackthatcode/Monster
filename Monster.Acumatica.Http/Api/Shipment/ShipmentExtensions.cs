using System.Collections.Generic;
using System.Linq;

namespace Monster.Acumatica.Api.Shipment
{
    public static class ShipmentExtensions
    {
        public static void AppendPackageRefs(this List<Shipment> shipments, List<Shipment> appends)
        {
            foreach (var shipment in shipments)
            {
                var append = appends.FirstOrDefault(x => shipment.ShipmentNbr.value == x.ShipmentNbr.value);
                if (append == null)
                {
                    continue;
                }

                shipment.Packages = append.Packages;
            }
        }
    }
}
