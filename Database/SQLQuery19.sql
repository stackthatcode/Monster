
USE Bundler;
GO


DELETE FROM ProductVariants;
DELETE FROM ProductTypes;
DELETE FROM ExclusionConstraints;


INSERT INTO ProductTypes VALUES ( 1, '3D Printer' );
INSERT INTO ProductTypes VALUES ( 2, 'Enhanced Service Plan' );
INSERT INTO ProductTypes VALUES ( 3, 'Filament Bundle' );
INSERT INTO ProductTypes VALUES ( 4, '3D Scanner' );
INSERT INTO ProductTypes VALUES ( 5, '3D Printer Clean Enclosure' );



INSERT INTO ProductVariants VALUES ( 1, 1, 'Ultimaker S5', 'ultimaker-s5', 'UMS5' );
INSERT INTO ProductVariants VALUES ( 2, 1, 'Ultimaker 3 Extended', 'ultimaker-3-extended', 'UM3EXT' );
INSERT INTO ProductVariants VALUES ( 3, 1, 'Ultimaker 3', 'ultimaker-3', 'UM3'  );
INSERT INTO ProductVariants VALUES ( 4, 1, 'Ultimaker 2 Extended+', 'ultimaker-2-extended-plus', 'UM2EXTPLUS' );
INSERT INTO ProductVariants VALUES ( 5, 1, 'Ultimaker 2+', 'ultimaker-2-plus', 'UM2PLUS' );
INSERT INTO ProductVariants VALUES ( 6, 1, 'Ultimaker Original Plus Kit', 'ultimaker-original-plus-kit', 'UMOPLUS' );



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
INSERT INTO ExclusionConstraints VALUES ( 6, 7 );
INSERT INTO ExclusionConstraints VALUES ( 6, 8 );
INSERT INTO ExclusionConstraints VALUES ( 6, 9 );



INSERT INTO ProductVariants VALUES ( 10, 3, '3DU PLA Bundle (750g)', '3d-universe-2-85mm-pla-filament-bundle', '3DUPLA285_750g_Bundle' );
INSERT INTO ProductVariants VALUES ( 11, 3, '3DU PLA Bundle (1kg)', '3d-universe-2-85mm-pla-filament-bundle', '3DUPLA285_1kg_Bundle' );
INSERT INTO ProductVariants VALUES ( 12, 3, '3DU Large PLA Bundle (750g)', '3d-universe-2-85mm-large-pla-filament-bundle', '3DUPLA285_750g_Large_Bundle' );
INSERT INTO ProductVariants VALUES ( 13, 3, '3DU Large PLA Bundle (1kg)', '3d-universe-2-85mm-large-pla-filament-bundle', '3DUPLA285_1kg_Large_Bundle' );
INSERT INTO ProductVariants VALUES ( 14, 3, '3DU Multi-material bundle', '3d-universe-2-85mm-multi-material-filament-bundle', '3DU_Multi_Bundle' );
INSERT INTO ProductVariants VALUES ( 15, 3, 'UM Small PLA Bundle', 'ultimaker-filament-bundle', 'UM-PLA-BUNDLE-12' );
INSERT INTO ProductVariants VALUES ( 16, 3, 'UM Small PLA/PVA Bundle', 'ultimaker-filament-bundle', 'UM-PLAPVA-BUNDLE-14' );
INSERT INTO ProductVariants VALUES ( 17, 3, 'UM Large PLA Bundle', 'ultimaker-filament-bundle', 'UM-PLA-BUNDLE-18' );
INSERT INTO ProductVariants VALUES ( 18, 3, 'UM Large PLA/PVA Bundle', 'ultimaker-filament-bundle', 'UM-PLAPVA-BUNDLE-20' );
INSERT INTO ProductVariants VALUES ( 19, 3, 'UM Multi-Material Bundle for single extrusion', 'ultimaker-multi-material-filament-bundle', 'UM_Multi_Bundle' );
INSERT INTO ProductVariants VALUES ( 20, 3, 'UM Multi-Material Bundle for dual extrusion', 'ultimaker-multi-material-filament-bundle', 'UM_Multi_Bundle' );

INSERT INTO ExclusionConstraints VALUES ( 1, 11 );
INSERT INTO ExclusionConstraints VALUES ( 1, 13 );
INSERT INTO ExclusionConstraints VALUES ( 2, 11 );
INSERT INTO ExclusionConstraints VALUES ( 2, 13 );
INSERT INTO ExclusionConstraints VALUES ( 3, 11 );
INSERT INTO ExclusionConstraints VALUES ( 3, 13 );
INSERT INTO ExclusionConstraints VALUES ( 4, 16 );
INSERT INTO ExclusionConstraints VALUES ( 4, 18 );
INSERT INTO ExclusionConstraints VALUES ( 4, 20 );
INSERT INTO ExclusionConstraints VALUES ( 5, 16 );
INSERT INTO ExclusionConstraints VALUES ( 5, 18 );
INSERT INTO ExclusionConstraints VALUES ( 5, 20 );
INSERT INTO ExclusionConstraints VALUES ( 6, 16 );
INSERT INTO ExclusionConstraints VALUES ( 6, 18 );
INSERT INTO ExclusionConstraints VALUES ( 6, 20 );



INSERT INTO ProductVariants VALUES ( 21, 4, 'Matter and Form Scanner', 'matter-and-form-3d-scanner', 'MFS1V1R' );

INSERT INTO ProductVariants VALUES ( 22, 5, 'Model 660', '3d-print-clean-model-660-enclosure', 'MD660' );
INSERT INTO ProductVariants VALUES ( 23, 5, 'Model 660 Pro', '3d-print-clean-model-660-pro-enclosure', 'MD660P' );
INSERT INTO ProductVariants VALUES ( 24, 5, 'Model 870', '3d-print-clean-model-870-enclosure', 'MD870' );
INSERT INTO ProductVariants VALUES ( 25, 5, 'Model 870 Pro', '3d-print-clean-model-870-pro-enclosure', 'MD870P' );



