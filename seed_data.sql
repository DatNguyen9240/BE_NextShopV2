-- Script chèn dữ liệu mẫu tiếng Việt chuẩn cho CSDL SQL Server của BE NextShopV2
-- Bao gồm toàn bộ các bảng quan trọng nhất trong hệ sinh thái E-Commerce

USE [NextShopDB];
GO

-- 1. USERS
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE Id = '10000000-0000-0000-0000-000000000001')
BEGIN
    INSERT INTO [Users] (Id, FullName, Email, PasswordHash, Phone, Role, Gender, EmailVerified, MfaEnabled, CreatedAt)
    VALUES 
    ('10000000-0000-0000-0000-000000000001', N'Nguyễn Quản Trị', 'admin@nextshop.vn', '$2a$11$dummyhashformw1234567890123456', '0901234567', 'Admin', 'Male', 1, 0, GETUTCDATE()),
    ('10000000-0000-0000-0000-000000000002', N'Trần Khách Hàng', 'khachhang@nextshop.vn', '$2a$11$dummyhashformw1234567890123456', '0987654321', 'User', 'Female', 1, 0, GETUTCDATE());
END
GO

-- 2. ADDRESSES
IF NOT EXISTS (SELECT 1 FROM [Addresses] WHERE AddressId = '50000000-0000-0000-0000-000000000001')
BEGIN
    INSERT INTO [Addresses] (AddressId, UserId, RecipientName, FullAddress, IsDefault)
    VALUES
    ('50000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000002', N'Trần Khách Hàng', N'123 Đường Lê Lợi, Phường Bến Thành, Quận 1, Hồ Chí Minh', 1);
END
GO

-- 3. CATEGORIES
IF NOT EXISTS (SELECT 1 FROM [Categories] WHERE CategoryId = '20000000-0000-0000-0000-000000000001')
BEGIN
    INSERT INTO [Categories] (CategoryId, Name, ParentId, CreatedAt, Icon, TaxRate)
    VALUES
    ('20000000-0000-0000-0000-000000000001', N'Điện thoại thông minh', NULL, GETUTCDATE(), 'fa-mobile', 0.1000),
    ('20000000-0000-0000-0000-000000000002', N'Thời trang Nam Cao cấp', NULL, GETUTCDATE(), 'fa-tshirt', 0.0800);
END
GO

-- 4. PRODUCT ATTRIBUTES
IF NOT EXISTS (SELECT 1 FROM [ProductAttributes] WHERE AttributeId = 'A0000000-0000-0000-0000-000000000001')
BEGIN
    INSERT INTO [ProductAttributes] (AttributeId, Name, InputType, IsActive)
    VALUES
    ('A0000000-0000-0000-0000-000000000001', N'Màu sắc', 'select', 1),
    ('A0000000-0000-0000-0000-000000000002', N'Dung lượng', 'select', 1),
    ('A0000000-0000-0000-0000-000000000003', N'Kích cỡ', 'select', 1);
END
GO

-- 5. ATTRIBUTE VALUES
IF NOT EXISTS (SELECT 1 FROM [AttributeValues] WHERE AttributeValueId = 'A1000000-0000-0000-0000-000000000001')
BEGIN
    INSERT INTO [AttributeValues] (AttributeValueId, AttributeId, Value, DisplayOrder, IsActive)
    VALUES
    ('A1000000-0000-0000-0000-000000000001', 'A0000000-0000-0000-0000-000000000001', N'Đen Titan', 1, 1),
    ('A1000000-0000-0000-0000-000000000002', 'A0000000-0000-0000-0000-000000000001', N'Bạc', 2, 1),
    ('A1000000-0000-0000-0000-000000000003', 'A0000000-0000-0000-0000-000000000002', N'256GB', 1, 1),
    ('A1000000-0000-0000-0000-000000000004', 'A0000000-0000-0000-0000-000000000002', N'512GB', 2, 1),
    ('A1000000-0000-0000-0000-000000000005', 'A0000000-0000-0000-0000-000000000003', N'Size M', 1, 1),
    ('A1000000-0000-0000-0000-000000000006', 'A0000000-0000-0000-0000-000000000003', N'Size L', 2, 1);
END
GO

-- 6. CATEGORY ATTRIBUTES
IF NOT EXISTS (SELECT 1 FROM [CategoryAttributes])
BEGIN
    INSERT INTO [CategoryAttributes] (CategoryId, AttributeId)
    VALUES
    ('20000000-0000-0000-0000-000000000001', 'A0000000-0000-0000-0000-000000000001'), -- Điện thoại -> Màu sắc
    ('20000000-0000-0000-0000-000000000001', 'A0000000-0000-0000-0000-000000000002'), -- Điện thoại -> Dung lượng
    ('20000000-0000-0000-0000-000000000002', 'A0000000-0000-0000-0000-000000000001'), -- Quần áo -> Màu sắc
    ('20000000-0000-0000-0000-000000000002', 'A0000000-0000-0000-0000-000000000003'); -- Quần áo -> Kích cỡ
END
GO

-- 7. PRODUCTS
IF NOT EXISTS (SELECT 1 FROM [Products] WHERE ProductId = '30000000-0000-0000-0000-000000000001')
BEGIN
    INSERT INTO [Products] (ProductId, Name, Description, AdditionalInfo, GenderTarget, Brand, AverageRating, TotalReviews, TotalLikes, CreatedAt, IsActive, TaxRate, TagsJson)
    VALUES
    ('30000000-0000-0000-0000-000000000001', N'Samsung Galaxy S24 Ultra', N'Điện thoại thông minh cao cấp nhất của Samsung năm 2024 tích hợp Galaxy AI. Chụp ảnh đêm vượt trội.', N'Bảo hành chính hãng 12 tháng tại các trung tâm trên toàn quốc', 'Unisex', 'Samsung', 4.9, 1500, 5000, GETUTCDATE(), 1, 0.1000, '["Samsung", "Smartphone", "AI"]'),
    ('30000000-0000-0000-0000-000000000002', N'Áo Polo Nam Aristino Có Cổ', N'Áo polo chất liệu cotton pima cao cấp, thấm hút mồ hôi tốt, chống nhăn hiệu quả.', N'Form dáng slimfit tôn dáng', 'Male', 'Aristino', 4.8, 450, 1200, GETUTCDATE(), 1, 0.0800, '["Polo", "Aristino", "Thoitrangnam"]');
END
GO

-- 8. PRODUCT CATEGORIES
IF NOT EXISTS (SELECT 1 FROM [ProductCategories] WHERE ProductId = '30000000-0000-0000-0000-000000000001')
BEGIN
    INSERT INTO [ProductCategories] (ProductId, CategoryId, AssignedAt)
    VALUES
    ('30000000-0000-0000-0000-000000000001', '20000000-0000-0000-0000-000000000001', GETUTCDATE()),
    ('30000000-0000-0000-0000-000000000002', '20000000-0000-0000-0000-000000000002', GETUTCDATE());
END
GO

-- 9. PRODUCT VARIANTS
IF NOT EXISTS (SELECT 1 FROM [ProductVariants] WHERE VariantId = '40000000-0000-0000-0000-000000000001')
BEGIN
    INSERT INTO [ProductVariants] (VariantId, ProductId, SKU, BasePrice, DiscountPercent, DiscountAmount, PriceAfterDiscount, StockQuantity, IsDefault, DisplayOrder, ImageUrl, IsActive)
    VALUES
    ('40000000-0000-0000-0000-000000000001', '30000000-0000-0000-0000-000000000001', 'SS24U-BLK-256', 33990000, 10.00, 3399000, 30591000, 25, 1, 1, 'https://example.com/s24u-black.jpg', 1),
    ('40000000-0000-0000-0000-000000000002', '30000000-0000-0000-0000-000000000001', 'SS24U-SLV-512', 37990000, 5.00, 1899500, 36090500, 10, 0, 2, 'https://example.com/s24u-silver.jpg', 1),
    ('40000000-0000-0000-0000-000000000003', '30000000-0000-0000-0000-000000000002', 'POLO-BLK-M', 450000, 0.00, 0, 450000, 100, 1, 1, 'https://example.com/polo-m.jpg', 1);
END
GO

-- 10. VARIANT ATTRIBUTE VALUES
IF NOT EXISTS (SELECT 1 FROM [VariantAttributeValues])
BEGIN
    INSERT INTO [VariantAttributeValues] (VariantId, AttributeValueId)
    VALUES
    ('40000000-0000-0000-0000-000000000001', 'A1000000-0000-0000-0000-000000000001'), -- S24U Đen
    ('40000000-0000-0000-0000-000000000001', 'A1000000-0000-0000-0000-000000000003'), -- S24U 256GB
    ('40000000-0000-0000-0000-000000000002', 'A1000000-0000-0000-0000-000000000002'), -- S24U Bạc
    ('40000000-0000-0000-0000-000000000002', 'A1000000-0000-0000-0000-000000000004'), -- S24U 512GB
    ('40000000-0000-0000-0000-000000000003', 'A1000000-0000-0000-0000-000000000001'), -- Polo Đen
    ('40000000-0000-0000-0000-000000000003', 'A1000000-0000-0000-0000-000000000005'); -- Polo Size M
END
GO

-- 11. INVENTORY TRANSACTIONS
IF NOT EXISTS (SELECT 1 FROM [InventoryTransactions])
BEGIN
    INSERT INTO [InventoryTransactions] (TransactionId, VariantId, ChangeQty, Reason, CreatedAt, CreatedBy)
    VALUES
    (NEWID(), '40000000-0000-0000-0000-000000000001', 25, N'Nhập kho khởi tạo lần đầu', GETUTCDATE(), '10000000-0000-0000-0000-000000000001'),
    (NEWID(), '40000000-0000-0000-0000-000000000002', 10, N'Nhập kho khởi tạo lần đầu', GETUTCDATE(), '10000000-0000-0000-0000-000000000001'),
    (NEWID(), '40000000-0000-0000-0000-000000000003', 100, N'Nhập kho khởi tạo lần đầu', GETUTCDATE(), '10000000-0000-0000-0000-000000000001');
END
GO

-- 12. COUPONS (Mã giảm giá Khai Trương)
IF NOT EXISTS (SELECT 1 FROM [Coupons] WHERE CouponId = '60000000-0000-0000-0000-000000000001')
BEGIN
    -- Mừng khai trương giảm 10% tối đa 500k cho đơn từ 2 triệu
    INSERT INTO [Coupons] (CouponId, Code, UserId, CouponType, DiscountPercent, MinOrderAmount, MaxDiscountAmount, StartDate, EndDate, UsageLimit, UsedCount, IsActive)
    VALUES ('60000000-0000-0000-0000-000000000001', 'KHAITRUONG10', NULL, 'Promotion', 10.00, 2000000, 500000, GETUTCDATE(), DATEADD(day, 30, GETUTCDATE()), 1000, 1, 1);
END
GO

-- 13. ORDERS
IF NOT EXISTS (SELECT 1 FROM [Orders] WHERE OrderId = '70000000-0000-0000-0000-000000000001')
BEGIN
    INSERT INTO [Orders] (OrderId, UserId, OrderDate, Status, SubTotal, TaxAmount, DiscountAmount, TotalAmount, BuyerName, BuyerEmail, BuyerPhone, ShippingAddress)
    VALUES
    ('70000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000002', GETUTCDATE(), 'Completed', 30591000, 3059100, 500000, 33150100, N'Trần Khách Hàng', 'khachhang@nextshop.vn', '0987654321', N'123 Đường Lê Lợi, Phường Bến Thành, Quận 1, Hồ Chí Minh');
END
GO

-- 14. ORDER ITEMS
IF NOT EXISTS (SELECT 1 FROM [OrderItems] WHERE OrderItemId = '80000000-0000-0000-0000-000000000001')
BEGIN
    INSERT INTO [OrderItems] (OrderItemId, OrderId, VariantId, ProductId, Quantity, UnitPrice, DiscountAmount, TaxAmount, TaxRate, TotalAmount, ProductName, ProductSku, VariantSku)
    VALUES
    ('80000000-0000-0000-0000-000000000001', '70000000-0000-0000-0000-000000000001', '40000000-0000-0000-0000-000000000001', '30000000-0000-0000-0000-000000000001', 1, 30591000, 3399000, 3059100, 0.1000, 33650100, N'Samsung Galaxy S24 Ultra', 'SS24U', 'SS24U-BLK-256');
END
GO

PRINT N'Thêm dữ liệu mẫu Tiếng Việt toàn diện thành công!';
