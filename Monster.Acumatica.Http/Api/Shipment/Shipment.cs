using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monster.Acumatica.Api.Shipment
{
    public class BaseCurrencyID
    {
        public string value { get; set; }
    }

    public class ControlQty
    {
        public double value { get; set; }
    }

    public class CreatedDateTime
    {
        public DateTime value { get; set; }
    }

    public class CurrencyID
    {
    }

    public class CurrencyRate
    {
        public double value { get; set; }
    }

    public class CurrencyRateTypeID
    {
    }

    public class CurrencyViewState
    {
        public bool value { get; set; }
    }

    public class CustomerID
    {
        public string value { get; set; }
    }

    public class Description
    {
        public string value { get; set; }
    }

    public class FreeItem
    {
        public bool value { get; set; }
    }

    public class InventoryID
    {
        public string value { get; set; }
    }

    public class LineNbr
    {
        public int value { get; set; }
    }

    public class LocationID
    {
        public string value { get; set; }
    }

    public class OpenQty
    {
        public double value { get; set; }
    }

    public class OrderedQty
    {
        public double value { get; set; }
    }

    public class OrderLineNbr
    {
        public int value { get; set; }
    }

    public class OrderNbr
    {
        public string value { get; set; }
    }

    public class OrderType
    {
        public string value { get; set; }
    }

    public class OriginalQty
    {
    }

    public class ReasonCode
    {
    }

    public class ShippedQty
    {
        public double value { get; set; }
    }

    public class UOM
    {
        public string value { get; set; }
    }

    public class WarehouseID
    {
        public string value { get; set; }
    }

    public class Custom
    {
    }

    public class Detail
    {
        public string id { get; set; }
        public int rowNumber { get; set; }
        public string note { get; set; }
        public Description Description { get; set; }
        public FreeItem FreeItem { get; set; }
        public InventoryID InventoryID { get; set; }
        public LineNbr LineNbr { get; set; }
        public LocationID LocationID { get; set; }
        public OpenQty OpenQty { get; set; }
        public OrderedQty OrderedQty { get; set; }
        public OrderLineNbr OrderLineNbr { get; set; }
        public OrderNbr OrderNbr { get; set; }
        public OrderType OrderType { get; set; }
        public OriginalQty OriginalQty { get; set; }
        public ReasonCode ReasonCode { get; set; }
        public ShippedQty ShippedQty { get; set; }
        public UOM UOM { get; set; }
        public WarehouseID WarehouseID { get; set; }
        public Custom custom { get; set; }
        public List<object> files { get; set; }
    }

    public class EffectiveDate
    {
        public DateTime value { get; set; }
    }

    public class FOBPoint
    {
    }

    public class FreightAmount
    {
        public double value { get; set; }
    }

    public class FreightCost
    {
        public double value { get; set; }
    }

    public class FreightCurrency
    {
        public string value { get; set; }
    }

    public class GroundCollect
    {
        public bool value { get; set; }
    }

    public class Hold
    {
        public bool value { get; set; }
    }

    public class Insurance
    {
        public bool value { get; set; }
    }

    public class LastModifiedDateTime
    {
        public DateTime value { get; set; }
    }

    public class Operation
    {
        public string value { get; set; }
    }

    public class Owner
    {
    }

    public class PackageCount
    {
    }

    public class PackageWeight
    {
        public double value { get; set; }
    }

    public class ReciprocalRate
    {
        public double value { get; set; }
    }

    public class ResidentialDelivery
    {
        public bool value { get; set; }
    }

    public class SaturdayDelivery
    {
        public bool value { get; set; }
    }

    public class ShipmentDate
    {
        public DateTime value { get; set; }
    }

    public class ShipmentNbr
    {
        public string value { get; set; }
    }

    public class ShippedQty2
    {
        public double value { get; set; }
    }

    public class ShippedVolume
    {
        public double value { get; set; }
    }

    public class ShippedWeight
    {
        public double value { get; set; }
    }

    public class ShippingTerms
    {
    }

    public class ShippingZoneID
    {
    }

    public class ShipVia
    {
    }

    public class Status
    {
        public string value { get; set; }
    }

    public class ToWarehouseID
    {
    }

    public class Type
    {
        public string value { get; set; }
    }

    public class WarehouseID2
    {
        public string value { get; set; }
    }

    public class WorkgroupID
    {
    }

    public class Custom2
    {
    }

    public class RootObject
    {
        public string id { get; set; }
        public int rowNumber { get; set; }
        public string note { get; set; }
        public BaseCurrencyID BaseCurrencyID { get; set; }
        public ControlQty ControlQty { get; set; }
        public CreatedDateTime CreatedDateTime { get; set; }
        public CurrencyID CurrencyID { get; set; }
        public CurrencyRate CurrencyRate { get; set; }
        public CurrencyRateTypeID CurrencyRateTypeID { get; set; }
        public CurrencyViewState CurrencyViewState { get; set; }
        public CustomerID CustomerID { get; set; }
        public List<Detail> Details { get; set; }
        public EffectiveDate EffectiveDate { get; set; }
        public FOBPoint FOBPoint { get; set; }
        public FreightAmount FreightAmount { get; set; }
        public FreightCost FreightCost { get; set; }
        public FreightCurrency FreightCurrency { get; set; }
        public GroundCollect GroundCollect { get; set; }
        public Hold Hold { get; set; }
        public Insurance Insurance { get; set; }
        public LastModifiedDateTime LastModifiedDateTime { get; set; }
        public Operation Operation { get; set; }
        public Owner Owner { get; set; }
        public PackageCount PackageCount { get; set; }
        public PackageWeight PackageWeight { get; set; }
        public ReciprocalRate ReciprocalRate { get; set; }
        public ResidentialDelivery ResidentialDelivery { get; set; }
        public SaturdayDelivery SaturdayDelivery { get; set; }
        public ShipmentDate ShipmentDate { get; set; }
        public ShipmentNbr ShipmentNbr { get; set; }
        public ShippedQty2 ShippedQty { get; set; }
        public ShippedVolume ShippedVolume { get; set; }
        public ShippedWeight ShippedWeight { get; set; }
        public ShippingTerms ShippingTerms { get; set; }
        public ShippingZoneID ShippingZoneID { get; set; }
        public ShipVia ShipVia { get; set; }
        public Status Status { get; set; }
        public ToWarehouseID ToWarehouseID { get; set; }
        public Type Type { get; set; }
        public WarehouseID2 WarehouseID { get; set; }
        public WorkgroupID WorkgroupID { get; set; }
        public Custom2 custom { get; set; }
        public List<object> files { get; set; }
    }
}
