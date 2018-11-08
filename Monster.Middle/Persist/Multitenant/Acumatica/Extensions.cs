using System.Collections.Generic;
using System.Linq;

namespace Monster.Middle.Persist.Multitenant.Acumatica
{
    public static class Extensions
    {
        public static bool 
                MatchByAcumaticaIds(
                    this UsrAcumaticaSoShipment input,
                    UsrAcumaticaSoShipment compareWith)
        {
            return input.AcumaticaSalesOrderId == compareWith.AcumaticaSalesOrderId
                   && input.AcumaticaShipmentId == compareWith.AcumaticaShipmentId;
        }

        public static UsrAcumaticaSoShipment
                FindByAcumaticaIds(
                    this IEnumerable<UsrAcumaticaSoShipment> input, 
                    UsrAcumaticaSoShipment compareWith)
        {
            return input.FirstOrDefault(
                x => x.MatchByAcumaticaIds(compareWith));
        }

        public static bool
                AnyMatchByAcumaticaIds(
                    this IEnumerable<UsrAcumaticaSoShipment> input,
                    UsrAcumaticaSoShipment compareWith)
        {
            return input.FindByAcumaticaIds(compareWith) != null;
        }
    }
}

