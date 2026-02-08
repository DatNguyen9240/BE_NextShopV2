CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
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

CREATE TABLE "Announcements" (
    "Id" uuid NOT NULL,
    "ClassName" text NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Announcements" PRIMARY KEY ("Id")
);

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

CREATE TABLE "FooterInfos" (
    "Id" uuid NOT NULL,
    "ClassName" text NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_FooterInfos" PRIMARY KEY ("Id")
);

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

CREATE TABLE "ProductAttributes" (
    "AttributeId" uuid NOT NULL,
    "Name" text NOT NULL,
    "InputType" text,
    "IsActive" boolean NOT NULL,
    CONSTRAINT "PK_ProductAttributes" PRIMARY KEY ("AttributeId")
);

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

CREATE TABLE "Users" (
    "Id" uuid NOT NULL,
    "FullName" text NOT NULL,
    "Email" text NOT NULL,
    "PasswordHash" text NOT NULL,
    "Phone" text,
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

CREATE TABLE "AttributeValues" (
    "AttributeValueId" uuid NOT NULL,
    "AttributeId" uuid NOT NULL,
    "Value" text NOT NULL,
    "DisplayOrder" integer NOT NULL,
    "IsActive" boolean NOT NULL,
    CONSTRAINT "PK_AttributeValues" PRIMARY KEY ("AttributeValueId"),
    CONSTRAINT "FK_AttributeValues_ProductAttributes_AttributeId" FOREIGN KEY ("AttributeId") REFERENCES "ProductAttributes" ("AttributeId") ON DELETE CASCADE
);

CREATE TABLE "CategoryAttributes" (
    "CategoryId" uuid NOT NULL,
    "AttributeId" uuid NOT NULL,
    "AssignedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_CategoryAttributes" PRIMARY KEY ("CategoryId", "AttributeId"),
    CONSTRAINT "FK_CategoryAttributes_Categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES "Categories" ("CategoryId") ON DELETE CASCADE,
    CONSTRAINT "FK_CategoryAttributes_ProductAttributes_AttributeId" FOREIGN KEY ("AttributeId") REFERENCES "ProductAttributes" ("AttributeId") ON DELETE CASCADE
);

CREATE TABLE "ProductCategories" (
    "ProductId" uuid NOT NULL,
    "CategoryId" uuid NOT NULL,
    "AssignedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_ProductCategories" PRIMARY KEY ("ProductId", "CategoryId"),
    CONSTRAINT "FK_ProductCategories_Categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES "Categories" ("CategoryId") ON DELETE CASCADE,
    CONSTRAINT "FK_ProductCategories_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("ProductId") ON DELETE CASCADE
);

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

CREATE TABLE "Carts" (
    "CartId" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Carts" PRIMARY KEY ("CartId"),
    CONSTRAINT "FK_Carts_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

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
    "TaxAmount" numeric(18,0) NOT NULL DEFAULT 0,
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

CREATE TABLE "Passkeys" (
    "Id" text NOT NULL,
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

CREATE TABLE "ProductLikes" (
    "ProductId" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_ProductLikes" PRIMARY KEY ("ProductId", "UserId"),
    CONSTRAINT "FK_ProductLikes_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("ProductId") ON DELETE CASCADE,
    CONSTRAINT "FK_ProductLikes_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

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

CREATE TABLE "VariantAttributeValues" (
    "VariantId" uuid NOT NULL,
    "AttributeValueId" uuid NOT NULL,
    CONSTRAINT "PK_VariantAttributeValues" PRIMARY KEY ("VariantId", "AttributeValueId"),
    CONSTRAINT "FK_VariantAttributeValues_AttributeValues_AttributeValueId" FOREIGN KEY ("AttributeValueId") REFERENCES "AttributeValues" ("AttributeValueId") ON DELETE CASCADE,
    CONSTRAINT "FK_VariantAttributeValues_ProductVariants_VariantId" FOREIGN KEY ("VariantId") REFERENCES "ProductVariants" ("VariantId") ON DELETE CASCADE
);

CREATE TABLE "CartItems" (
    "CartItemId" uuid NOT NULL,
    "CartId" uuid NOT NULL,
    "VariantId" uuid NOT NULL,
    "Quantity" integer NOT NULL,
    CONSTRAINT "PK_CartItems" PRIMARY KEY ("CartItemId"),
    CONSTRAINT "FK_CartItems_Carts_CartId" FOREIGN KEY ("CartId") REFERENCES "Carts" ("CartId") ON DELETE CASCADE,
    CONSTRAINT "FK_CartItems_ProductVariants_VariantId" FOREIGN KEY ("VariantId") REFERENCES "ProductVariants" ("VariantId") ON DELETE CASCADE
);

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

CREATE INDEX "IX_Addresses_UserId" ON "Addresses" ("UserId");

CREATE INDEX "IX_AttributeValues_AttributeId" ON "AttributeValues" ("AttributeId");

CREATE INDEX "IX_CartItems_CartId" ON "CartItems" ("CartId");

CREATE INDEX "IX_CartItems_VariantId" ON "CartItems" ("VariantId");

CREATE INDEX "IX_Carts_UserId" ON "Carts" ("UserId");

CREATE INDEX "IX_Categories_ParentId" ON "Categories" ("ParentId");

CREATE INDEX "IX_CategoryAttributes_AttributeId" ON "CategoryAttributes" ("AttributeId");

CREATE INDEX "IX_InventoryTransactions_VariantId" ON "InventoryTransactions" ("VariantId");

CREATE INDEX "IX_OrderCoupons_CouponId" ON "OrderCoupons" ("CouponId");

CREATE INDEX "IX_OrderItems_OrderId" ON "OrderItems" ("OrderId");

CREATE INDEX "IX_OrderItems_VariantId" ON "OrderItems" ("VariantId");

CREATE INDEX "IX_Orders_UserId" ON "Orders" ("UserId");

CREATE INDEX "IX_Passkeys_UserId" ON "Passkeys" ("UserId");

CREATE INDEX "IX_Payments_OrderId" ON "Payments" ("OrderId");

CREATE INDEX "IX_ProductCategories_CategoryId" ON "ProductCategories" ("CategoryId");

CREATE INDEX "IX_ProductLikes_UserId" ON "ProductLikes" ("UserId");

CREATE INDEX "IX_ProductVariants_ProductId" ON "ProductVariants" ("ProductId");

CREATE INDEX "IX_Reviews_ProductId" ON "Reviews" ("ProductId");

CREATE INDEX "IX_Reviews_UserId" ON "Reviews" ("UserId");

CREATE UNIQUE INDEX "IX_Shipments_OrderId" ON "Shipments" ("OrderId");

CREATE INDEX "IX_Shipments_ShipperId" ON "Shipments" ("ShipperId");

CREATE INDEX "IX_TrackingEvents_ShipmentId" ON "TrackingEvents" ("ShipmentId");

CREATE UNIQUE INDEX "IX_Users_Phone" ON "Users" ("Phone");

CREATE INDEX "IX_VariantAttributeValues_AttributeValueId" ON "VariantAttributeValues" ("AttributeValueId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260128064705_InitCreate', '9.0.9');

COMMIT;

