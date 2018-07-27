
USE Bundler;
GO

DELETE FROM BundleVariantReferences;
DELETE FROM BundleUnifiedVariants;
DELETE FROM BundleProducts;
	
DELETE FROM ExclusionConstraints;
DELETE FROM ProductVariants;
DELETE FROM ProductTypes;

DBCC CHECKIDENT ('[BundleVariantReferences]', RESEED, 0);
GO
DBCC CHECKIDENT ('[BundleUnifiedVariants]', RESEED, 0);
GO
DBCC CHECKIDENT ('[BundleProducts]', RESEED, 0);
GO

INSERT INTO ProductTypes VALUES ( 1, '3D Printer', 1 );
INSERT INTO ProductTypes VALUES ( 2, 'Enhanced Service Plan', 2 );
INSERT INTO ProductTypes VALUES ( 3, 'Filament Bundle', 3 );
INSERT INTO ProductTypes VALUES ( 4, '3D Scanner', 4 );
INSERT INTO ProductTypes VALUES ( 5, '3D Printer Clean Enclosure', 5 );



INSERT INTO ProductVariants VALUES ( 1, 1, 'Ultimaker S5', 'ultimaker-s5', 'UMS5' );
INSERT INTO ProductVariants VALUES ( 2, 1, 'Ultimaker 3 Extended', 'ultimaker-3-extended', 'UM3EXT' );
INSERT INTO ProductVariants VALUES ( 3, 1, 'Ultimaker 3', 'ultimaker-3', 'UM3'  );
INSERT INTO ProductVariants VALUES ( 4, 1, 'Ultimaker 2 Extended+', 'ultimaker-2-extended-plus', 'UM2EXTPLUS' );
INSERT INTO ProductVariants VALUES ( 5, 1, 'Ultimaker 2+', 'ultimaker-2-plus', 'UM2PLUS' );



INSERT INTO ProductVariants VALUES ( 7, 2, 'Enhanced Service Plan for UMS5', 'enhanced-service-plan-for-ultimaker-s5', 'UM-ESP-UMS5' );
INSERT INTO ProductVariants VALUES ( 8, 2, 'Enhanced Service Plan for UM3', 'enhanced-service-plan-for-ultimaker-3', 'UM-ESP-UM3' );
INSERT INTO ProductVariants VALUES ( 9, 2, 'Enhanced Service Plan for UM2', 'enhanced-service-plan-for-ultimaker-2', 'UM-ESP-UM2PLUS' );

INSERT INTO ExclusionConstraints VALUES ( 1, 8 );
INSERT INTO ExclusionConstraints VALUES ( 1, 9 );
INSERT INTO ExclusionConstraints VALUES ( 2, 7 );
INSERT INTO ExclusionConstraints VALUES ( 2, 9 );
INSERT INTO ExclusionConstraints VALUES ( 3, 7 );
INSERT INTO ExclusionConstraints VALUES ( 3, 9 );
INSERT INTO ExclusionConstraints VALUES ( 4, 7 );
INSERT INTO ExclusionConstraints VALUES ( 4, 8 );
INSERT INTO ExclusionConstraints VALUES ( 5, 7 );
INSERT INTO ExclusionConstraints VALUES ( 5, 8 );



INSERT INTO ProductVariants VALUES ( 10, 3, '3DU PLA Bundle (750g)', '3d-universe-2-85mm-pla-filament-bundle', '3DUPLA285_750g_Bundle' );
INSERT INTO ProductVariants VALUES ( 11, 3, '3DU PLA Bundle (1kg)', '3d-universe-2-85mm-pla-filament-bundle', '3DUPLA285_1kg_Bundle' );
INSERT INTO ProductVariants VALUES ( 12, 3, '3DU Large PLA Bundle (750g)', '3d-universe-2-85mm-large-pla-filament-bundle', '3DUPLA285_750g_Large_Bundle' );
INSERT INTO ProductVariants VALUES ( 13, 3, '3DU Large PLA Bundle (1kg)', '3d-universe-2-85mm-large-pla-filament-bundle', '3DUPLA285_1kg_Large_Bundle' );
INSERT INTO ProductVariants VALUES ( 14, 3, '3DU Multi-Material Bundle', '3d-universe-2-85mm-multi-material-filament-bundle', '3DU_Multi_Bundle' );
INSERT INTO ProductVariants VALUES ( 15, 3, 'UM Small PLA Bundle', 'ultimaker-filament-bundle', 'UM-PLA-BUNDLE-12' );
INSERT INTO ProductVariants VALUES ( 16, 3, 'UM Small PLA/PVA Bundle', 'ultimaker-filament-bundle', 'UM-PLAPVA-BUNDLE-14' );
INSERT INTO ProductVariants VALUES ( 17, 3, 'UM Large PLA Bundle', 'ultimaker-filament-bundle', 'UM-PLA-BUNDLE-18' );
INSERT INTO ProductVariants VALUES ( 18, 3, 'UM Large PLA/PVA Bundle', 'ultimaker-filament-bundle', 'UM-PLAPVA-BUNDLE-20' );
INSERT INTO ProductVariants VALUES ( 19, 3, 'UM Multi-Material Bundle (Single Extruder)', 'ultimaker-multi-material-filament-bundle', 'UM_Multi_Bundle_Single' );
INSERT INTO ProductVariants VALUES ( 20, 3, 'UM Multi-Material Bundle (Dual Extruder)', 'ultimaker-multi-material-filament-bundle', 'UM_Multi_Bundle_Dual' );
INSERT INTO ProductVariants VALUES ( 21, 3, 'UM Small Tough PLA Bundle', 'ultimaker-tough-pla-filament-bundle', 'UM-TOUGH-PLA-BUNDLE-12' );
INSERT INTO ProductVariants VALUES ( 22, 3, 'UM Small Tough PLA/PVA Bundle', 'ultimaker-tough-pla-filament-bundle', 'UM-TOUGH-PLA-BUNDLE-14' );
INSERT INTO ProductVariants VALUES ( 23, 3, 'UM Large Tough PLA Bundle', 'ultimaker-tough-pla-filament-bundle', 'UM-TOUGH-PLA-BUNDLE-18' );
INSERT INTO ProductVariants VALUES ( 24, 3, 'UM Large Tough PLA/PVA Bundle', 'ultimaker-tough-pla-filament-bundle', 'UM-TOUGH-PLA-BUNDLE-20' );

INSERT INTO ExclusionConstraints VALUES ( 1, 11 );
INSERT INTO ExclusionConstraints VALUES ( 1, 13 );
INSERT INTO ExclusionConstraints VALUES ( 2, 11 );
INSERT INTO ExclusionConstraints VALUES ( 2, 13 );
INSERT INTO ExclusionConstraints VALUES ( 3, 11 );
INSERT INTO ExclusionConstraints VALUES ( 3, 13 );
INSERT INTO ExclusionConstraints VALUES ( 4, 16 );
INSERT INTO ExclusionConstraints VALUES ( 4, 18 );
INSERT INTO ExclusionConstraints VALUES ( 4, 20 );
INSERT INTO ExclusionConstraints VALUES ( 4, 22 );
INSERT INTO ExclusionConstraints VALUES ( 4, 24 );
INSERT INTO ExclusionConstraints VALUES ( 5, 16 );
INSERT INTO ExclusionConstraints VALUES ( 5, 18 );
INSERT INTO ExclusionConstraints VALUES ( 5, 20 );
INSERT INTO ExclusionConstraints VALUES ( 5, 22 );
INSERT INTO ExclusionConstraints VALUES ( 5, 24 );



INSERT INTO ProductVariants VALUES ( 25, 4, 'Matter and Form 3D Scanner', 'matter-and-form-3d-scanner', 'MFS1V1R' );


INSERT INTO ProductVariants VALUES ( 26, 5, '3D Print Clean 660 Enclosure', '3d-print-clean-model-660-enclosure', 'MD660' );
INSERT INTO ProductVariants VALUES ( 27, 5, '3D Print Clean 660 Pro Enclosure', '3d-print-clean-model-660-pro-enclosure', 'MD660P' );
INSERT INTO ProductVariants VALUES ( 28, 5, '3D Print Clean 870 Enclosure', '3d-print-clean-model-870-enclosure', 'MD870' );
INSERT INTO ProductVariants VALUES ( 29, 5, '3D Print Clean 870 Pro Enclosure', '3d-print-clean-model-870-pro-enclosure', 'MD870P' );

INSERT INTO ExclusionConstraints VALUES ( 1, 26 );
INSERT INTO ExclusionConstraints VALUES ( 1, 27 );




/*
SELECT * FROM BundleProducts;
SELECT * FROM BundleUnifiedVariants;
SELECT * FROM BundleVariantReferences;

SELECT * FROM ProductVariants WHERE Id IN ( SELECT ProductVariantId FROM BundleVariantReferences WHERE BundleUnifiedVariantId = 170 );
*/


