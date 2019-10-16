


SELECT * FROM JobMonitor;

UPDATE SystemState SET AcumaticaConnState = 1;

SELECT * FROM ShopifyORder;


SELECT * FROM AcumaticaSalesOrder;

SELECT * FROM MonsterSys..Instance;

--DELETE FROM ShopAcuOrderSync;


SELECT OrderType, OrderNbr, OpenOrderQty, UsrTaxSnapshot FROM AcuInst0001..SOOrder;

SELECT * FROM AcuInst0001..ARTax;

UPDATE AcuInst0001..SOOrder 
SET UsrTaxSnapshot = '{    "ExternalRefNbr": "1778846826540",    "LineItems": [      {        "ExternalRefNbr": "3961233178668",        "InventoryID": "ROUNDING-DOOM",        "Quantity": 4,        "UnitPrice": 114.95,        "LineAmount": 459.80,        "IsTaxable": true,        "TaxableAmount": 459.80,        "TaxAmount": 45.97,        "TaxLines": [          {            "Name": "IL State Tax",            "Rate": 0.0625          },          {            "Name": "Cook County Tax",            "Rate": 0.0275          },          {            "Name": "Mount Prospect Municipal Tax",            "Rate": 0.01          }        ]      }    ],    "Freight": {      "Description": null,      "Price": 50.0,      "TaxAmount": 5.01,      "IsTaxable": true,      "TaxableAmount": 0.0,      "TaxLines": [        {          "Name": "IL State Tax",          "Rate": 0.0625        },        {          "Name": "Cook County Tax",          "Rate": 0.0275        },        {          "Name": "Mount Prospect Municipal Tax",          "Rate": 0.01        }      ]    },    "Refunds": [],    "TotalTaxableLineAmountsAfterRefund": 459.80,    "TotalTaxableFreightAmountAfterRefund": 0.0,    "TotalTax": 50.98,    "TotalLineItemTaxAfterRefunds": 45.97,    "TotalFreightTaxAfterRefunds": 5.01,    "TotalPrice": 560.78  }'
WHERE OrderNbr = '000016'


SELECT * FROM AcuInst0001..SOOrderShipment;

SELECT * FROM AcuInst0001..TaxTran;


SELECT t1.*
FROM AcuInst0001..TaxTran t1 
	INNER JOIN AcuInst0001..SOOrderShipment t2
		ON t1.RefNbr = t2.InvoiceNbr AND t1.TranType = t2.InvoiceType
WHERE t2.OrderNbr = 000009 AND t2.OrderType = 'SO'



SELECT * FROM AcuInst0001..SOOrder;



UPDATE AcuInst0001..SOOrder 
SET UsrTaxSnapshot = '{"ExternalRefNbr":"99123452879250","LineItems":[{"ExternalRefNbr":"11111112223","InventoryID":"ROUNDING-DOOM","Quantity":3,"UnitPrice":114.95,"LineAmount":344.85,"IsTaxable":true,"TaxableAmount":344.85,"TaxAmount":26.73,"TaxLines":[{"Name":"IL State Tax","Rate":0.0625},{"Name":"Mchenry County Tax","Rate":0.0075},{"Name":"Algonquin Municipal Tax","Rate":0.0075}]}],"Freight":{"Description":null,"Price":50.00,"TaxAmount":5.00,"IsTaxable":true,"TaxableAmount":0.0,"TaxLines":[{"Name":"IL State Tax","Rate":0.0625},{"Name":"Mchenry County Tax","Rate":0.0075},{"Name":"Algonquin Municipal Tax","Rate":0.0075}]},"Refunds":[],"TotalTax":31.73,"TotalLineItemTaxAfterRefunds":26.73,"TotalFreightTaxAfterRefunds":5.00,"TotalTaxableLineAmountsAfterRefund":344.85,"TotalTaxableFreightAfterRefund":0.0}'
WHERE OrderNbr = '000010'

