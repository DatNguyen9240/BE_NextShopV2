CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF to_regclass('public."Advertisements"') IS NULL THEN
            CREATE TABLE "Advertisements" (
                "Id" uuid NOT NULL,
                "PublicId" text NOT NULL,
                "Title" text NOT NULL,
                "ImageUrl" text NOT NULL,
                "Type" text NOT NULL,
                "SortOrder" integer NOT NULL,
                "CreatedAt" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_Advertisements" PRIMARY KEY ("Id")
            );
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF to_regclass('public."Announcements"') IS NULL THEN
            CREATE TABLE "Announcements" (
                "Id" uuid NOT NULL,
                "ClassName" text NOT NULL,
                "IsActive" boolean NOT NULL,
                "CreatedAt" timestamp with time zone NOT NULL,
                "UpdatedAt" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_Announcements" PRIMARY KEY ("Id")
            );
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF to_regclass('public."Categories"') IS NULL THEN
            CREATE TABLE "Categories" (
                "CategoryId" uuid NOT NULL,
                "Name" text NOT NULL,
                "ParentId" uuid,
                "CreatedAt" timestamp with time zone NOT NULL,
                "ImageUrl" text,
                "Icon" text,
                CONSTRAINT "PK_Categories" PRIMARY KEY ("CategoryId"),
                CONSTRAINT "FK_Categories_Categories_ParentId" FOREIGN KEY ("ParentId") REFERENCES "Categories" ("CategoryId")
            );
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF to_regclass('public."Coupons"') IS NULL THEN
            CREATE TABLE "Coupons" (
                "CouponId" uuid NOT NULL,
                "Code" text NOT NULL,
                "DiscountPercent" numeric(10,2) NOT NULL,
                "MinOrderAmount" numeric(18,2),
                "MaxDiscountAmount" numeric(18,2),
                "StartDate" timestamp with time zone NOT NULL,
                "EndDate" timestamp with time zone NOT NULL,
                "UsageLimit" integer,
                "UsedCount" integer NOT NULL,
                "IsActive" boolean NOT NULL,
                CONSTRAINT "PK_Coupons" PRIMARY KEY ("CouponId")
            );
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF to_regclass('public."FooterInfos"') IS NULL THEN
            CREATE TABLE "FooterInfos" (
                "Id" uuid NOT NULL,
                "ClassName" text NOT NULL,
                "CreatedAt" timestamp with time zone NOT NULL,
                "UpdatedAt" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_FooterInfos" PRIMARY KEY ("Id")
            );
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF to_regclass('public."NotificationHistories"') IS NULL THEN
            CREATE TABLE "NotificationHistories" (
                "NotificationHistoryId" uuid NOT NULL,
                "Title" text NOT NULL,
                "Body" text NOT NULL,
                "ImageUrl" text,
                "TargetToken" text,
                "UserId" text,
                "IsSuccessful" boolean NOT NULL,
                "ErrorMessage" text,
                "FirebaseResponse" text,
                "Data" text,
                "RecipientCount" integer NOT NULL,
                "Status" text,
                "CreatedAt" timestamp with time zone NOT NULL,
                "SentAt" timestamp with time zone,
                CONSTRAINT "PK_NotificationHistories" PRIMARY KEY ("NotificationHistoryId")
            );
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF to_regclass('public."ProductAttributes"') IS NULL THEN
            CREATE TABLE "ProductAttributes" (
                "AttributeId" uuid NOT NULL,
                "Name" text NOT NULL,
                "InputType" text,
                "IsActive" boolean NOT NULL,
                CONSTRAINT "PK_ProductAttributes" PRIMARY KEY ("AttributeId")
            );
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF to_regclass('public."Products"') IS NULL THEN
            CREATE TABLE "Products" (
                "ProductId" uuid NOT NULL,
                "Name" text NOT NULL,
                "Description" text,
                "AdditionalInfo" text,
                "GenderTarget" text,
                "Brand" text,
                "AverageRating" numeric(5,2) NOT NULL,
                "TotalReviews" integer NOT NULL,
                "TotalLikes" integer NOT NULL,
                "CreatedAt" timestamp with time zone NOT NULL,
                "UpdatedAt" timestamp with time zone,
                "IsActive" boolean NOT NULL,
                "TagsJson" text,
                CONSTRAINT "PK_Products" PRIMARY KEY ("ProductId")
            );
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF to_regclass('public."PushTokens"') IS NULL THEN
            CREATE TABLE "PushTokens" (
                "PushTokenId" uuid NOT NULL,
                "UserId" uuid,
                "Token" text NOT NULL,
                "Platform" character varying(50) NOT NULL,
                "DeviceId" text,
                "IsActive" boolean NOT NULL,
                "CreatedAt" timestamp with time zone NOT NULL,
                "LastSeenAt" timestamp with time zone,
                CONSTRAINT "PK_PushTokens" PRIMARY KEY ("PushTokenId")
            );
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF to_regclass('public."Users"') IS NULL THEN
            CREATE TABLE "Users" (
                "Id" uuid NOT NULL,
                "FullName" text NOT NULL,
                "Email" text NOT NULL,
                "PasswordHash" text NOT NULL,
                "Phone" character varying(450),
                "Role" text NOT NULL,
                "Gender" text,
                "Avatar" text,
                "GoogleId" text,
                "EmailVerified" boolean NOT NULL,
                "MfaEnabled" boolean NOT NULL,
                "MfaType" text,
                "CreatedAt" timestamp with time zone NOT NULL,
                "UpdatedAt" timestamp with time zone,
                CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
            );
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF to_regclass('public."AttributeValues"') IS NULL THEN
            CREATE TABLE "AttributeValues" (
                "AttributeValueId" uuid NOT NULL,
                "AttributeId" uuid NOT NULL,
                "Value" text NOT NULL,
                "DisplayOrder" integer NOT NULL,
                "IsActive" boolean NOT NULL,
                CONSTRAINT "PK_AttributeValues" PRIMARY KEY ("AttributeValueId"),
                CONSTRAINT "FK_AttributeValues_ProductAttributes_AttributeId" FOREIGN KEY ("AttributeId") REFERENCES "ProductAttributes" ("AttributeId") ON DELETE CASCADE
            );
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF to_regclass('public."CategoryAttributes"') IS NULL THEN
            CREATE TABLE "CategoryAttributes" (
                "CategoryId" uuid NOT NULL,
                "AttributeId" uuid NOT NULL,
                "AssignedAt" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_CategoryAttributes" PRIMARY KEY ("CategoryId", "AttributeId"),
                CONSTRAINT "FK_CategoryAttributes_Categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES "Categories" ("CategoryId") ON DELETE CASCADE,
                CONSTRAINT "FK_CategoryAttributes_ProductAttributes_AttributeId" FOREIGN KEY ("AttributeId") REFERENCES "ProductAttributes" ("AttributeId") ON DELETE CASCADE
            );
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF to_regclass('public."ProductCategories"') IS NULL THEN
            CREATE TABLE "ProductCategories" (
                "ProductId" uuid NOT NULL,
                "CategoryId" uuid NOT NULL,
                "AssignedAt" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_ProductCategories" PRIMARY KEY ("ProductId", "CategoryId"),
                CONSTRAINT "FK_ProductCategories_Categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES "Categories" ("CategoryId") ON DELETE CASCADE,
                CONSTRAINT "FK_ProductCategories_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("ProductId") ON DELETE CASCADE
            );
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF to_regclass('public."ProductVariants"') IS NULL THEN
            CREATE TABLE "ProductVariants" (
                "VariantId" uuid NOT NULL,
                "ProductId" uuid NOT NULL,
                "SKU" text NOT NULL,
                "BasePrice" numeric(18,0) NOT NULL,
                "DiscountPercent" numeric(10,2) NOT NULL,
                "DiscountAmount" numeric(18,0) NOT NULL,
                "PriceAfterDiscount" numeric(18,0) NOT NULL,
                "StockQuantity" integer NOT NULL,
                "IsDefault" boolean NOT NULL,
                "DisplayOrder" integer NOT NULL,
                "ImageUrl" text,
                "ImgHover" text,
                "ThumbnailUrl" text,
                "IsActive" boolean NOT NULL,
                CONSTRAINT "PK_ProductVariants" PRIMARY KEY ("VariantId"),
                CONSTRAINT "FK_ProductVariants_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("ProductId") ON DELETE CASCADE
            );
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF to_regclass('public."Addresses"') IS NULL THEN
            CREATE TABLE "Addresses" (
                "AddressId" uuid NOT NULL,
                "UserId" uuid NOT NULL,
                "RecipientName" text NOT NULL,
                "FullAddress" text NOT NULL,
                "Latitude" double precision,
                "Longitude" double precision,
                "IsDefault" boolean NOT NULL,
                CONSTRAINT "PK_Addresses" PRIMARY KEY ("AddressId"),
                CONSTRAINT "FK_Addresses_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
            );
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF to_regclass('public."Carts"') IS NULL THEN
            CREATE TABLE "Carts" (
                "CartId" uuid NOT NULL,
                "UserId" uuid NOT NULL,
                "CreatedAt" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_Carts" PRIMARY KEY ("CartId"),
                CONSTRAINT "FK_Carts_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
            );
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF to_regclass('public."Orders"') IS NULL THEN
            CREATE TABLE "Orders" (
                "OrderId" uuid NOT NULL,
                "UserId" uuid NOT NULL,
                "OrderDate" timestamp with time zone NOT NULL,
                "Status" text NOT NULL,
                "CancelReason" text,
                "AdminCancelReason" text,
                "CancelledBy" text,
                "CancelledAt" timestamp with time zone,
                "SubTotal" numeric(18,0) NOT NULL,
                "DiscountAmount" numeric(18,0) NOT NULL,
                "TotalAmount" numeric(18,0) NOT NULL,
                "BuyerName" text,
                "BuyerPhone" text,
                "ShippingAddress" text,
                "ShippingLat" double precision,
                "ShippingLng" double precision,
                CONSTRAINT "PK_Orders" PRIMARY KEY ("OrderId"),
                CONSTRAINT "FK_Orders_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
            );
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF to_regclass('public."Passkeys"') IS NULL THEN
            CREATE TABLE "Passkeys" (
                "Id" character varying(450) NOT NULL,
                "UserId" uuid NOT NULL,
                "CredentialId" text NOT NULL,
                "PublicKey" text NOT NULL,
                "Counter" bigint NOT NULL DEFAULT 0,
                "Transports" text,
                "CreatedAt" timestamp with time zone NOT NULL,
                "LastUsedAt" timestamp with time zone,
                CONSTRAINT "PK_Passkeys" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_Passkeys_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
            );
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF to_regclass('public."ProductLikes"') IS NULL THEN
            CREATE TABLE "ProductLikes" (
                "ProductId" uuid NOT NULL,
                "UserId" uuid NOT NULL,
                "CreatedAt" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_ProductLikes" PRIMARY KEY ("ProductId", "UserId"),
                CONSTRAINT "FK_ProductLikes_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("ProductId") ON DELETE CASCADE,
                CONSTRAINT "FK_ProductLikes_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
            );
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF to_regclass('public."Reviews"') IS NULL THEN
            CREATE TABLE "Reviews" (
                "ReviewId" uuid NOT NULL,
                "ProductId" uuid NOT NULL,
                "UserId" uuid NOT NULL,
                "Rating" integer NOT NULL,
                "Comment" text NOT NULL,
                "CreatedAt" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_Reviews" PRIMARY KEY ("ReviewId"),
                CONSTRAINT "FK_Reviews_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("ProductId") ON DELETE CASCADE,
                CONSTRAINT "FK_Reviews_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
            );
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF to_regclass('public."InventoryTransactions"') IS NULL THEN
            CREATE TABLE "InventoryTransactions" (
                "TransactionId" uuid NOT NULL,
                "VariantId" uuid NOT NULL,
                "ChangeQty" integer NOT NULL,
                "Reason" text NOT NULL,
                "CreatedAt" timestamp with time zone NOT NULL,
                "CreatedBy" text NOT NULL,
                CONSTRAINT "PK_InventoryTransactions" PRIMARY KEY ("TransactionId"),
                CONSTRAINT "FK_InventoryTransactions_ProductVariants_VariantId" FOREIGN KEY ("VariantId") REFERENCES "ProductVariants" ("VariantId") ON DELETE CASCADE
            );
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF to_regclass('public."VariantAttributeValues"') IS NULL THEN
            CREATE TABLE "VariantAttributeValues" (
                "VariantId" uuid NOT NULL,
                "AttributeValueId" uuid NOT NULL,
                CONSTRAINT "PK_VariantAttributeValues" PRIMARY KEY ("VariantId", "AttributeValueId"),
                CONSTRAINT "FK_VariantAttributeValues_AttributeValues_AttributeValueId" FOREIGN KEY ("AttributeValueId") REFERENCES "AttributeValues" ("AttributeValueId") ON DELETE CASCADE,
                CONSTRAINT "FK_VariantAttributeValues_ProductVariants_VariantId" FOREIGN KEY ("VariantId") REFERENCES "ProductVariants" ("VariantId") ON DELETE CASCADE
            );
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF to_regclass('public."CartItems"') IS NULL THEN
            CREATE TABLE "CartItems" (
                "CartItemId" uuid NOT NULL,
                "CartId" uuid NOT NULL,
                "VariantId" uuid NOT NULL,
                "Quantity" integer NOT NULL,
                CONSTRAINT "PK_CartItems" PRIMARY KEY ("CartItemId"),
                CONSTRAINT "FK_CartItems_Carts_CartId" FOREIGN KEY ("CartId") REFERENCES "Carts" ("CartId") ON DELETE CASCADE,
                CONSTRAINT "FK_CartItems_ProductVariants_VariantId" FOREIGN KEY ("VariantId") REFERENCES "ProductVariants" ("VariantId") ON DELETE CASCADE
            );
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF to_regclass('public."OrderCoupons"') IS NULL THEN
            CREATE TABLE "OrderCoupons" (
                "OrderId" uuid NOT NULL,
                "CouponId" uuid NOT NULL,
                "DiscountAmount" numeric(18,2) NOT NULL,
                "Status" text NOT NULL,
                "ReservedAt" timestamp with time zone,
                "AppliedAt" timestamp with time zone,
                CONSTRAINT "PK_OrderCoupons" PRIMARY KEY ("OrderId", "CouponId"),
                CONSTRAINT "FK_OrderCoupons_Coupons_CouponId" FOREIGN KEY ("CouponId") REFERENCES "Coupons" ("CouponId") ON DELETE CASCADE,
                CONSTRAINT "FK_OrderCoupons_Orders_OrderId" FOREIGN KEY ("OrderId") REFERENCES "Orders" ("OrderId") ON DELETE CASCADE
            );
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF to_regclass('public."OrderItems"') IS NULL THEN
            CREATE TABLE "OrderItems" (
                "OrderItemId" uuid NOT NULL,
                "OrderId" uuid NOT NULL,
                "VariantId" uuid,
                "ProductId" uuid,
                "Quantity" integer NOT NULL,
                "UnitPrice" numeric(18,2) NOT NULL,
                "DiscountAmount" numeric(18,2) NOT NULL,
                "TaxAmount" numeric(18,2) NOT NULL,
                "TaxRate" numeric(5,4) NOT NULL,
                "TotalAmount" numeric(18,2) NOT NULL,
                "ProductName" text,
                "ProductSku" text,
                "VariantSku" text,
                "VariantOptionsJson" text,
                CONSTRAINT "PK_OrderItems" PRIMARY KEY ("OrderItemId"),
                CONSTRAINT "FK_OrderItems_Orders_OrderId" FOREIGN KEY ("OrderId") REFERENCES "Orders" ("OrderId") ON DELETE CASCADE,
                CONSTRAINT "FK_OrderItems_ProductVariants_VariantId" FOREIGN KEY ("VariantId") REFERENCES "ProductVariants" ("VariantId") ON DELETE SET NULL
            );
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF to_regclass('public."Payments"') IS NULL THEN
            CREATE TABLE "Payments" (
                "PaymentId" uuid NOT NULL,
                "OrderId" uuid NOT NULL,
                "Method" text NOT NULL,
                "Amount" numeric(18,2) NOT NULL,
                "Status" text NOT NULL,
                "ProviderPaymentId" text,
                "ProviderData" text,
                "CreatedAt" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_Payments" PRIMARY KEY ("PaymentId"),
                CONSTRAINT "FK_Payments_Orders_OrderId" FOREIGN KEY ("OrderId") REFERENCES "Orders" ("OrderId") ON DELETE CASCADE
            );
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF to_regclass('public."Shipments"') IS NULL THEN
            CREATE TABLE "Shipments" (
                "ShipmentId" uuid NOT NULL,
                "OrderId" uuid NOT NULL,
                "ShipperId" uuid,
                "Carrier" text NOT NULL,
                "TrackingNumber" text NOT NULL,
                "Status" text NOT NULL,
                "CreatedAt" timestamp with time zone NOT NULL,
                "CurrentLat" double precision,
                "CurrentLng" double precision,
                "LastLocationUpdate" timestamp with time zone,
                "DeliveryAddress" text,
                "DeliveryLat" double precision,
                "DeliveryLng" double precision,
                CONSTRAINT "PK_Shipments" PRIMARY KEY ("ShipmentId"),
                CONSTRAINT "FK_Shipments_Orders_OrderId" FOREIGN KEY ("OrderId") REFERENCES "Orders" ("OrderId") ON DELETE CASCADE,
                CONSTRAINT "FK_Shipments_Users_ShipperId" FOREIGN KEY ("ShipperId") REFERENCES "Users" ("Id")
            );
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF to_regclass('public."TrackingEvents"') IS NULL THEN
            CREATE TABLE "TrackingEvents" (
                "TrackingEventId" uuid NOT NULL,
                "ShipmentId" uuid NOT NULL,
                "Status" text NOT NULL,
                "Description" text NOT NULL,
                "Location" text,
                "EventTime" timestamp with time zone NOT NULL,
                "CreatedAt" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_TrackingEvents" PRIMARY KEY ("TrackingEventId"),
                CONSTRAINT "FK_TrackingEvents_Shipments_ShipmentId" FOREIGN KEY ("ShipmentId") REFERENCES "Shipments" ("ShipmentId") ON DELETE CASCADE
            );
        END IF;
    END IF;
END $EF$;

-- Index creations: make them idempotent using pg_class checks and safe exception handling for unique index
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'IX_Addresses_UserId' AND relkind = 'i') THEN
            CREATE INDEX "IX_Addresses_UserId" ON "Addresses" ("UserId");
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'IX_AttributeValues_AttributeId' AND relkind = 'i') THEN
            CREATE INDEX "IX_AttributeValues_AttributeId" ON "AttributeValues" ("AttributeId");
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'IX_CartItems_CartId' AND relkind = 'i') THEN
            CREATE INDEX "IX_CartItems_CartId" ON "CartItems" ("CartId");
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'IX_CartItems_VariantId' AND relkind = 'i') THEN
            CREATE INDEX "IX_CartItems_VariantId" ON "CartItems" ("VariantId");
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'IX_Carts_UserId' AND relkind = 'i') THEN
            CREATE INDEX "IX_Carts_UserId" ON "Carts" ("UserId");
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'IX_Categories_ParentId' AND relkind = 'i') THEN
            CREATE INDEX "IX_Categories_ParentId" ON "Categories" ("ParentId");
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'IX_CategoryAttributes_AttributeId' AND relkind = 'i') THEN
            CREATE INDEX "IX_CategoryAttributes_AttributeId" ON "CategoryAttributes" ("AttributeId");
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'IX_InventoryTransactions_VariantId' AND relkind = 'i') THEN
            CREATE INDEX "IX_InventoryTransactions_VariantId" ON "InventoryTransactions" ("VariantId");
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'IX_OrderCoupons_CouponId' AND relkind = 'i') THEN
            CREATE INDEX "IX_OrderCoupons_CouponId" ON "OrderCoupons" ("CouponId");
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'IX_OrderItems_OrderId' AND relkind = 'i') THEN
            CREATE INDEX "IX_OrderItems_OrderId" ON "OrderItems" ("OrderId");
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'IX_OrderItems_VariantId' AND relkind = 'i') THEN
            CREATE INDEX "IX_OrderItems_VariantId" ON "OrderItems" ("VariantId");
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'IX_Orders_UserId' AND relkind = 'i') THEN
            CREATE INDEX "IX_Orders_UserId" ON "Orders" ("UserId");
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'IX_Passkeys_UserId' AND relkind = 'i') THEN
            CREATE INDEX "IX_Passkeys_UserId" ON "Passkeys" ("UserId");
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'IX_Payments_OrderId' AND relkind = 'i') THEN
            CREATE INDEX "IX_Payments_OrderId" ON "Payments" ("OrderId");
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'IX_ProductCategories_CategoryId' AND relkind = 'i') THEN
            CREATE INDEX "IX_ProductCategories_CategoryId" ON "ProductCategories" ("CategoryId");
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'IX_ProductLikes_UserId' AND relkind = 'i') THEN
            CREATE INDEX "IX_ProductLikes_UserId" ON "ProductLikes" ("UserId");
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'IX_ProductVariants_ProductId' AND relkind = 'i') THEN
            CREATE INDEX "IX_ProductVariants_ProductId" ON "ProductVariants" ("ProductId");
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'IX_Reviews_ProductId' AND relkind = 'i') THEN
            CREATE INDEX "IX_Reviews_ProductId" ON "Reviews" ("ProductId");
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'IX_Reviews_UserId' AND relkind = 'i') THEN
            CREATE INDEX "IX_Reviews_UserId" ON "Reviews" ("UserId");
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'IX_Shipments_OrderId' AND relkind = 'i') THEN
            CREATE UNIQUE INDEX "IX_Shipments_OrderId" ON "Shipments" ("OrderId");
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'IX_Shipments_ShipperId' AND relkind = 'i') THEN
            CREATE INDEX "IX_Shipments_ShipperId" ON "Shipments" ("ShipperId");
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'IX_TrackingEvents_ShipmentId' AND relkind = 'i') THEN
            CREATE INDEX "IX_TrackingEvents_ShipmentId" ON "TrackingEvents" ("ShipmentId");
        END IF;
    END IF;
END $EF$;

-- Unique index on Users.Phone: attempt creation but skip on errors (e.g., duplicates)
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'IX_Users_Phone' AND relkind = 'i') THEN
            BEGIN
                CREATE UNIQUE INDEX "IX_Users_Phone" ON "Users" ("Phone");
            EXCEPTION WHEN OTHERS THEN
                RAISE NOTICE 'Skipping unique index "IX_Users_Phone": %', SQLERRM;
            END;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        IF NOT EXISTS (SELECT 1 FROM pg_class WHERE relname = 'IX_VariantAttributeValues_AttributeValueId' AND relkind = 'i') THEN
            CREATE INDEX "IX_VariantAttributeValues_AttributeValueId" ON "VariantAttributeValues" ("AttributeValueId");
        END IF;
    END IF;
END $EF$;

-- Finally, record migration if not present (and transaction succeeded)
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260128043004_InitCreate') THEN
        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        VALUES ('20260128043004_InitCreate', '9.0.9');
    END IF;
END $EF$;
COMMIT;
