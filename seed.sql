SET XACT_ABORT ON;

BEGIN TRANSACTION;

/* Roles */
SET IDENTITY_INSERT [Roles] ON;
IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Id] = 1)
BEGIN
	INSERT INTO [Roles] ([Id], [RoleName], [IsActive], [CreatedBy], [CreatedOn])
	VALUES (1, N'Customer', 1, N'seed', SYSUTCDATETIME());
END;
IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Id] = 2)
BEGIN
	INSERT INTO [Roles] ([Id], [RoleName], [IsActive], [CreatedBy], [CreatedOn])
	VALUES (2, N'Administrator', 1, N'seed', SYSUTCDATETIME());
END;
SET IDENTITY_INSERT [Roles] OFF;

/* Users. The sample BCrypt hash is for a development-only dummy password. */
SET IDENTITY_INSERT [Users] ON;
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Id] = 1)
BEGIN
	INSERT INTO [Users]
		([Id], [FirstName], [LastName], [Email], [Password], [IsActive], [CreatedBy], [CreatedOn])
	VALUES
		(1, N'Rahul', N'Pandey', N'customer@example.com',
		 N'$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy',
		 1, N'seed', SYSUTCDATETIME());
END;
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Id] = 2)
BEGIN
	INSERT INTO [Users]
		([Id], [FirstName], [LastName], [Email], [Password], [IsActive], [CreatedBy], [CreatedOn])
	VALUES
		(2, N'Admin', N'User', N'admin@example.com',
		 N'$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy',
		 1, N'seed', SYSUTCDATETIME());
END;
SET IDENTITY_INSERT [Users] OFF;

/* User-role assignments */
SET IDENTITY_INSERT [UserRoles] ON;
IF NOT EXISTS (SELECT 1 FROM [UserRoles] WHERE [Id] = 1)
BEGIN
	INSERT INTO [UserRoles] ([Id], [UserId], [RoleId], [CreatedBy], [CreatedOn])
	VALUES (1, 1, 1, N'seed', SYSUTCDATETIME());
END;
IF NOT EXISTS (SELECT 1 FROM [UserRoles] WHERE [Id] = 2)
BEGIN
	INSERT INTO [UserRoles] ([Id], [UserId], [RoleId], [CreatedBy], [CreatedOn])
	VALUES (2, 2, 2, N'seed', SYSUTCDATETIME());
END;
SET IDENTITY_INSERT [UserRoles] OFF;

/* Product categories */
SET IDENTITY_INSERT [Categories] ON;
IF NOT EXISTS (SELECT 1 FROM [Categories] WHERE [Id] = 1)
BEGIN
	INSERT INTO [Categories] ([Id], [Name], [Description], [CreatedBy], [CreatedOn])
	VALUES (1, N'Footwear', N'Shoes and other footwear.', N'seed', SYSUTCDATETIME());
END;
IF NOT EXISTS (SELECT 1 FROM [Categories] WHERE [Id] = 2)
BEGIN
	INSERT INTO [Categories] ([Id], [Name], [Description], [CreatedBy], [CreatedOn])
	VALUES (2, N'Accessories', N'Everyday accessories and apparel.', N'seed', SYSUTCDATETIME());
END;
SET IDENTITY_INSERT [Categories] OFF;

/* Products */
SET IDENTITY_INSERT [Products] ON;
IF NOT EXISTS (SELECT 1 FROM [Products] WHERE [Id] = 1)
BEGIN
	INSERT INTO [Products]
		([Id], [Name], [Description], [Price], [StockQuantity], [CategoryId], [IsActive], [CreatedBy], [CreatedOn])
	VALUES
		(1, N'Everyday Running Shoes', N'Lightweight running shoes for daily use.', 79.99, 50, 1, 1, N'seed', SYSUTCDATETIME());
END;
IF NOT EXISTS (SELECT 1 FROM [Products] WHERE [Id] = 2)
BEGIN
	INSERT INTO [Products]
		([Id], [Name], [Description], [Price], [StockQuantity], [CategoryId], [IsActive], [CreatedBy], [CreatedOn])
	VALUES
		(2, N'Leather Backpack', N'Compact backpack with a padded laptop compartment.', 59.50, 25, 2, 1, N'seed', SYSUTCDATETIME());
END;
SET IDENTITY_INSERT [Products] OFF;

/* Orders */
SET IDENTITY_INSERT [Orders] ON;
IF NOT EXISTS (SELECT 1 FROM [Orders] WHERE [Id] = 1)
BEGIN
	INSERT INTO [Orders]
		([Id], [OrderNumber], [TotalAmount], [Status], [UserId], [CreatedBy], [CreatedOn])
	VALUES
		(1, N'ORD-SEED-0001', 219.48, N'Pending', 1, N'1', SYSUTCDATETIME());
END;
SET IDENTITY_INSERT [Orders] OFF;

/* Order items */
SET IDENTITY_INSERT [OrderItems] ON;
IF NOT EXISTS (SELECT 1 FROM [OrderItems] WHERE [Id] = 1)
BEGIN
	INSERT INTO [OrderItems]
		([Id], [OrderId], [ProductId], [ProductName], [Quantity], [UnitPrice], [TotalPrice], [CreatedBy], [CreatedOn])
	VALUES
		(1, 1, 1, N'Everyday Running Shoes', 2, 79.99, 159.98, N'1', SYSUTCDATETIME());
END;
IF NOT EXISTS (SELECT 1 FROM [OrderItems] WHERE [Id] = 2)
BEGIN
	INSERT INTO [OrderItems]
		([Id], [OrderId], [ProductId], [ProductName], [Quantity], [UnitPrice], [TotalPrice], [CreatedBy], [CreatedOn])
	VALUES
		(2, 1, 2, N'Leather Backpack', 1, 59.50, 59.50, N'1', SYSUTCDATETIME());
END;
SET IDENTITY_INSERT [OrderItems] OFF;

/* Refresh token */
SET IDENTITY_INSERT [RefreshTokens] ON;
IF NOT EXISTS (SELECT 1 FROM [RefreshTokens] WHERE [Id] = 1)
BEGIN
	INSERT INTO [RefreshTokens]
		([Id], [UserId], [Token], [ExpireAt], [RevokeAt], [CreatedBy], [CreatedOn])
	VALUES
		(1, 1, N'seed-refresh-token-0001', DATEADD(day, 30, SYSUTCDATETIME()), NULL, N'seed', SYSUTCDATETIME());
END;
SET IDENTITY_INSERT [RefreshTokens] OFF;

/* A published sample outbox event. It will not be sent again by the dispatcher. */
IF NOT EXISTS (SELECT 1 FROM [OutboxMessages] WHERE [EventId] = '11111111-1111-1111-1111-111111111111')
BEGIN
	INSERT INTO [OutboxMessages]
		([EventId], [EventType], [EventVersion], [OccurredAtUtc], [CorrelationId], [CausationId], [Payload],
		 [CreatedAtUtc], [AttemptCount], [NextAttemptAtUtc], [LockedUntilUtc], [PublishedAtUtc], [DeadLetteredAtUtc], [LastError])
	VALUES
		('11111111-1111-1111-1111-111111111111', N'OrderCreated', 1, SYSUTCDATETIME(),
		 N'seed-correlation-0001', NULL,
		 N'{"orderId":1,"orderNumber":"ORD-SEED-0001","userId":1,"totalAmount":219.48,"status":"Pending","items":[{"productId":1,"productName":"Everyday Running Shoes","quantity":2,"unitPrice":79.99,"totalPrice":159.98},{"productId":2,"productName":"Leather Backpack","quantity":1,"unitPrice":59.50,"totalPrice":59.50}]}',
		 SYSUTCDATETIME(), 0, SYSUTCDATETIME(), NULL, SYSUTCDATETIME(), NULL, NULL);
END;

/* Idempotency record for the sample audit consumer */
IF NOT EXISTS (SELECT 1 FROM [ProcessedIntegrationEvents]
			   WHERE [EventId] = '11111111-1111-1111-1111-111111111111'
				 AND [ConsumerName] = N'order-audit')
BEGIN
	INSERT INTO [ProcessedIntegrationEvents] ([EventId], [ConsumerName], [ProcessedAtUtc])
	VALUES ('11111111-1111-1111-1111-111111111111', N'order-audit', SYSUTCDATETIME());
END;

/* Audit record */
SET IDENTITY_INSERT [OrderAuditEntries] ON;
IF NOT EXISTS (SELECT 1 FROM [OrderAuditEntries] WHERE [Id] = 1)
BEGIN
	INSERT INTO [OrderAuditEntries]
		([Id], [OrderId], [EventId], [Action], [Details], [CreatedBy], [CreatedOn])
	VALUES
		(1, 1, '11111111-1111-1111-1111-111111111111', N'OrderCreated',
		 N'{"source":"seed.sql","note":"Sample audit entry"}', N'order-audit', SYSUTCDATETIME());
END;
SET IDENTITY_INSERT [OrderAuditEntries] OFF;

COMMIT TRANSACTION;
