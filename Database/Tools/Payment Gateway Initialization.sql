USE MonsterSys;
GO


DELETE FROM PaymentGateways;

INSERT INTO PaymentGateways VALUES ( 'shopify_payments', 'Shopify Payments' );
INSERT INTO PaymentGateways VALUES ( 'paypal', 'PayPal' );
INSERT INTO PaymentGateways VALUES ( 'amazon_payments', 'Amazon' );
INSERT INTO PaymentGateways VALUES ( 'bogus', 'Bogus Gateway (TEST)' );


