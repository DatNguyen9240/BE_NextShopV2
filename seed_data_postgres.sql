-- Script chèn dữ liệu mẫu Postgres đã FIX LỖI CỘT BỊ LỆCH
-- Đã xóa TaxRate khỏi Categories và Products do trong DB không tồn tại mảng này
-- Đã thêm AssignedAt vào CategoryAttributes

-- 1. USERS
INSERT INTO "Users" ("Id", "FullName", "Email", "PasswordHash", "Phone", "Role", "Gender", "EmailVerified", "MfaEnabled", "CreatedAt")
VALUES 
('10000000-0000-0000-0000-000000000001', 'Nguyễn Quản Trị', 'admin@nextshop.vn', '$2a$11$dummyhashformw1234567890123456', '0901234567', 'Admin', 'Male', true, false, CURRENT_TIMESTAMP),
('10000000-0000-0000-0000-000000000002', 'Trần Khách Hàng', 'khachhang@nextshop.vn', '$2a$11$dummyhashformw1234567890123456', '0987654321', 'User', 'Female', true, false, CURRENT_TIMESTAMP)
ON CONFLICT DO NOTHING;

-- 2. ADDRESSES
INSERT INTO "Addresses" ("AddressId", "UserId", "RecipientName", "FullAddress", "IsDefault")
VALUES
('50000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000002', 'Trần Khách Hàng', '123 Đường Lê Lợi, Phường Bến Thành, Quận 1, Hồ Chí Minh', true)
ON CONFLICT DO NOTHING;

-- 3. CATEGORIES (FIXED: Bỏ cột TaxRate)
INSERT INTO "Categories" ("CategoryId", "Name", "ParentId", "CreatedAt", "Icon")
VALUES
('20000000-0000-0000-0000-000000000001', 'Điện thoại thông minh', NULL, CURRENT_TIMESTAMP, 'fa-mobile'),
('20000000-0000-0000-0000-000000000002', 'Thời trang Nam Cao cấp', NULL, CURRENT_TIMESTAMP, 'fa-tshirt')
ON CONFLICT DO NOTHING;

-- 4. PRODUCT ATTRIBUTES
INSERT INTO "ProductAttributes" ("AttributeId", "Name", "InputType", "IsActive")
VALUES
('A0000000-0000-0000-0000-000000000001', 'Màu sắc', 'select', true),
('A0000000-0000-0000-0000-000000000002', 'Dung lượng', 'select', true),
('A0000000-0000-0000-0000-000000000003', 'Kích cỡ', 'select', true)
ON CONFLICT DO NOTHING;

-- 5. ATTRIBUTE VALUES
INSERT INTO "AttributeValues" ("AttributeValueId", "AttributeId", "Value", "DisplayOrder", "IsActive")
VALUES
('A1000000-0000-0000-0000-000000000001', 'A0000000-0000-0000-0000-000000000001', 'Đen Titan', 1, true),
('A1000000-0000-0000-0000-000000000002', 'A0000000-0000-0000-0000-000000000001', 'Bạc', 2, true),
('A1000000-0000-0000-0000-000000000003', 'A0000000-0000-0000-0000-000000000002', '256GB', 1, true),
('A1000000-0000-0000-0000-000000000004', 'A0000000-0000-0000-0000-000000000002', '512GB', 2, true),
('A1000000-0000-0000-0000-000000000005', 'A0000000-0000-0000-0000-000000000003', 'Size M', 1, true),
('A1000000-0000-0000-0000-000000000006', 'A0000000-0000-0000-0000-000000000003', 'Size L', 2, true)
ON CONFLICT DO NOTHING;

-- 6. CATEGORY ATTRIBUTES (FIXED: Thêm AssignedAt)
INSERT INTO "CategoryAttributes" ("CategoryId", "AttributeId", "AssignedAt")
VALUES
('20000000-0000-0000-0000-000000000001', 'A0000000-0000-0000-0000-000000000001', CURRENT_TIMESTAMP),
('20000000-0000-0000-0000-000000000001', 'A0000000-0000-0000-0000-000000000002', CURRENT_TIMESTAMP),
('20000000-0000-0000-0000-000000000002', 'A0000000-0000-0000-0000-000000000001', CURRENT_TIMESTAMP),
('20000000-0000-0000-0000-000000000002', 'A0000000-0000-0000-0000-000000000003', CURRENT_TIMESTAMP)
ON CONFLICT DO NOTHING;

-- 7. PRODUCTS (FIXED: Bỏ cột TaxRate)
INSERT INTO "Products" ("ProductId", "Name", "Description", "AdditionalInfo", "GenderTarget", "Brand", "AverageRating", "TotalReviews", "TotalLikes", "CreatedAt", "IsActive", "TagsJson")
VALUES
('30000000-0000-0000-0000-000000000001', 'Samsung Galaxy S24 Ultra', 'Điện thoại thông minh cao cấp nhất của Samsung năm 2024 tích hợp Galaxy AI. Chụp ảnh đêm vượt trội.', 'Bảo hành chính hãng 12 tháng tại các trung tâm trên toàn quốc', 'Unisex', 'Samsung', 4.9, 1500, 5000, CURRENT_TIMESTAMP, true, '["Samsung", "Smartphone", "AI"]'),
('30000000-0000-0000-0000-000000000002', 'Áo Polo Nam Aristino Có Cổ', 'Áo polo chất liệu cotton pima cao cấp, thấm hút mồ hôi tốt, chống nhăn hiệu quả.', 'Form dáng slimfit tôn dáng', 'Male', 'Aristino', 4.8, 450, 1200, CURRENT_TIMESTAMP, true, '["Polo", "Aristino", "Thoitrangnam"]')
ON CONFLICT DO NOTHING;

-- 8. PRODUCT CATEGORIES
INSERT INTO "ProductCategories" ("ProductId", "CategoryId", "AssignedAt")
VALUES
('30000000-0000-0000-0000-000000000001', '20000000-0000-0000-0000-000000000001', CURRENT_TIMESTAMP),
('30000000-0000-0000-0000-000000000002', '20000000-0000-0000-0000-000000000002', CURRENT_TIMESTAMP)
ON CONFLICT DO NOTHING;

-- 9. PRODUCT VARIANTS
INSERT INTO "ProductVariants" ("VariantId", "ProductId", "SKU", "BasePrice", "DiscountPercent", "DiscountAmount", "PriceAfterDiscount", "StockQuantity", "IsDefault", "DisplayOrder", "ImageUrl", "IsActive")
VALUES
('40000000-0000-0000-0000-000000000001', '30000000-0000-0000-0000-000000000001', 'SS24U-BLK-256', 33990000, 10.00, 3399000, 30591000, 25, true, 1, 'https://example.com/s24u-black.jpg', true),
('40000000-0000-0000-0000-000000000002', '30000000-0000-0000-0000-000000000001', 'SS24U-SLV-512', 37990000, 5.00, 1899500, 36090500, 10, false, 2, 'https://example.com/s24u-silver.jpg', true),
('40000000-0000-0000-0000-000000000003', '30000000-0000-0000-0000-000000000002', 'POLO-BLK-M', 450000, 0.00, 0, 450000, 100, true, 1, 'https://example.com/polo-m.jpg', true)
ON CONFLICT DO NOTHING;

-- 10. VARIANT ATTRIBUTE VALUES
INSERT INTO "VariantAttributeValues" ("VariantId", "AttributeValueId")
VALUES
('40000000-0000-0000-0000-000000000001', 'A1000000-0000-0000-0000-000000000001'),
('40000000-0000-0000-0000-000000000001', 'A1000000-0000-0000-0000-000000000003'),
('40000000-0000-0000-0000-000000000002', 'A1000000-0000-0000-0000-000000000002'),
('40000000-0000-0000-0000-000000000002', 'A1000000-0000-0000-0000-000000000004'),
('40000000-0000-0000-0000-000000000003', 'A1000000-0000-0000-0000-000000000001'),
('40000000-0000-0000-0000-000000000003', 'A1000000-0000-0000-0000-000000000005')
ON CONFLICT DO NOTHING;

-- 11. INVENTORY TRANSACTIONS
INSERT INTO "InventoryTransactions" ("TransactionId", "VariantId", "ChangeQty", "Reason", "CreatedAt", "CreatedBy")
VALUES
(gen_random_uuid(), '40000000-0000-0000-0000-000000000001', 25, 'Nhập kho khởi tạo lần đầu', CURRENT_TIMESTAMP, '10000000-0000-0000-0000-000000000001'),
(gen_random_uuid(), '40000000-0000-0000-0000-000000000002', 10, 'Nhập kho khởi tạo lần đầu', CURRENT_TIMESTAMP, '10000000-0000-0000-0000-000000000001'),
(gen_random_uuid(), '40000000-0000-0000-0000-000000000003', 100, 'Nhập kho khởi tạo lần đầu', CURRENT_TIMESTAMP, '10000000-0000-0000-0000-000000000001')
ON CONFLICT DO NOTHING;

-- 12. COUPONS
INSERT INTO "Coupons" ("CouponId", "Code", "UserId", "CouponType", "DiscountPercent", "MinOrderAmount", "MaxDiscountAmount", "StartDate", "EndDate", "UsageLimit", "UsedCount", "IsActive")
VALUES ('60000000-0000-0000-0000-000000000001', 'KHAITRUONG10', NULL, 'Promotion', 10.00, 2000000, 500000, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP + INTERVAL '30 days', 1000, 1, true)
ON CONFLICT DO NOTHING;

-- 13. ORDERS
INSERT INTO "Orders" ("OrderId", "UserId", "OrderDate", "Status", "SubTotal", "TaxAmount", "DiscountAmount", "TotalAmount", "BuyerName", "BuyerEmail", "BuyerPhone", "ShippingAddress")
VALUES
('70000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000002', CURRENT_TIMESTAMP, 'Completed', 30591000, 3059100, 500000, 33150100, 'Trần Khách Hàng', 'khachhang@nextshop.vn', '0987654321', '123 Đường Lê Lợi, Phường Bến Thành, Quận 1, Hồ Chí Minh')
ON CONFLICT DO NOTHING;

-- 14. ORDER ITEMS
INSERT INTO "OrderItems" ("OrderItemId", "OrderId", "VariantId", "ProductId", "Quantity", "UnitPrice", "DiscountAmount", "TaxAmount", "TaxRate", "TotalAmount", "ProductName", "ProductSku", "VariantSku")
VALUES
('80000000-0000-0000-0000-000000000001', '70000000-0000-0000-0000-000000000001', '40000000-0000-0000-0000-000000000001', '30000000-0000-0000-0000-000000000001', 1, 30591000, 3399000, 3059100, 0.1000, 33650100, 'Samsung Galaxy S24 Ultra', 'SS24U', 'SS24U-BLK-256')
ON CONFLICT DO NOTHING;
