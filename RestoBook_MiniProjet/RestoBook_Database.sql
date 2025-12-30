-- =============================================
-- RestoBook Database - Complete Setup Script
-- Updated: Added Menu & MenuCategories
-- =============================================

-- Drop existing tables if they exist (in correct order)
IF OBJECT_ID('dbo.MenuItems', 'U') IS NOT NULL DROP TABLE dbo.MenuItems;
IF OBJECT_ID('dbo.MenuCategories', 'U') IS NOT NULL DROP TABLE dbo.MenuCategories;
IF OBJECT_ID('dbo.Reservations', 'U') IS NOT NULL DROP TABLE dbo.Reservations;
IF OBJECT_ID('dbo.RestaurantTables', 'U') IS NOT NULL DROP TABLE dbo.RestaurantTables;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;

GO

-- =============================================
-- Core Tables (Users, Tables, Reservations)
-- =============================================
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(256) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    Role NVARCHAR(50) NOT NULL DEFAULT 'Customer',
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT CK_Users_Role CHECK (Role IN ('Admin', 'Customer'))
);

CREATE TABLE RestaurantTables (
    Id INT PRIMARY KEY IDENTITY(1,1),
    TableNumber INT NOT NULL UNIQUE,
    Capacity INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT CK_RestaurantTables_Capacity CHECK (Capacity > 0)
);

CREATE TABLE Reservations (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NULL,
    GuestName NVARCHAR(100) NULL,
    RestaurantTableId INT NOT NULL,
    ReservationDate DATETIME2 NOT NULL,
    NumberOfGuests INT NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Reservations_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL,
    CONSTRAINT FK_Reservations_RestaurantTables FOREIGN KEY (RestaurantTableId) REFERENCES RestaurantTables(Id) ON DELETE CASCADE,
    CONSTRAINT CK_Reservations_Status CHECK (Status IN ('Pending', 'Confirmed', 'Cancelled')),
    CONSTRAINT CK_Reservations_NumberOfGuests CHECK (NumberOfGuests > 0)
);

-- =============================================
-- Menu Tables
-- =============================================
CREATE TABLE MenuCategories (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    DisplayOrder INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1
);

CREATE TABLE MenuItems (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    Price DECIMAL(18, 2) NOT NULL,
    MenuCategoryId INT NOT NULL,
    ImageUrl NVARCHAR(MAX) NULL,
    IsAvailable BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_MenuItems_MenuCategories FOREIGN KEY (MenuCategoryId) REFERENCES MenuCategories(Id) ON DELETE CASCADE
);

GO

-- =============================================
-- Insert Sample Data
-- =============================================

-- 1. Users
SET IDENTITY_INSERT Users ON;
INSERT INTO Users (Id, FullName, Email, PasswordHash, Role, CreatedAt) VALUES
(1, 'Admin User', 'admin@restobook.com', '$2a$11$8xKJZqXhZJYvH5rKqGqE3.YvZQxYqZqZqZqZqZqZqZqZqZqZqZqZq', 'Admin', '2025-01-01 10:00:00'),
(2, 'John Doe', 'john.doe@email.com', '$2a$11$9yLKZrYiZKZwI6sLrHrF4.ZwZRyZrZrZrZrZrZrZrZrZrZrZrZrZr', 'Customer', '2025-01-05 14:30:00'),
(3, 'Jane Smith', 'jane.smith@email.com', '$2a$11$7xMNAsZjALAxJ7tMsIsG5.AxARzAsAsAsAsAsAsAsAsAsAsAsAsAs', 'Customer', '2025-01-10 09:15:00'),
(4, 'Michael Johnson', 'michael.j@email.com', '$2a$11$6wOPBtAkBMByK8uOtJtH6.ByBSABtBtBtBtBtBtBtBtBtBtBtBtBt', 'Customer', '2025-01-15 16:45:00');
SET IDENTITY_INSERT Users OFF;

-- 2. Restaurant Tables
SET IDENTITY_INSERT RestaurantTables ON;
INSERT INTO RestaurantTables (Id, TableNumber, Capacity, IsActive) VALUES
(1, 101, 2, 1), (2, 102, 2, 1), (3, 103, 4, 1), (4, 104, 4, 1), (5, 105, 6, 1),
(6, 106, 6, 1), (7, 107, 8, 1), (8, 108, 4, 1), (9, 109, 2, 1), (10, 110, 10, 1), (11, 111, 4, 0);
SET IDENTITY_INSERT RestaurantTables OFF;

-- 3. Reservations
SET IDENTITY_INSERT Reservations ON;
INSERT INTO Reservations (Id, UserId, GuestName, RestaurantTableId, ReservationDate, NumberOfGuests, Status, CreatedAt) VALUES
(1, 2, NULL, 1, '2025-12-15 19:00:00', 2, 'Confirmed', '2025-12-10 10:30:00'),
(2, 3, NULL, 3, '2025-12-16 20:00:00', 4, 'Confirmed', '2025-12-11 14:20:00'),
(3, 4, NULL, 5, '2025-12-17 18:30:00', 6, 'Cancelled', '2025-12-12 09:45:00'),
(4, 2, NULL, 2, '2025-12-21 19:30:00', 2, 'Confirmed', '2025-12-18 11:00:00'),
(5, 3, NULL, 4, '2025-12-22 20:00:00', 4, 'Pending', '2025-12-19 15:30:00'),
(6, 4, NULL, 6, '2025-12-23 19:00:00', 5, 'Confirmed', '2025-12-19 16:45:00'),
(7, 2, NULL, 7, '2025-12-24 18:00:00', 8, 'Pending', '2025-12-20 08:30:00'),
(8, 3, NULL, 1, '2025-12-25 20:30:00', 2, 'Confirmed', '2025-12-20 09:15:00'),
(9, 4, NULL, 3, '2025-12-26 19:30:00', 4, 'Pending', '2025-12-20 10:00:00'),
(10, 2, NULL, 5, '2025-12-27 18:30:00', 6, 'Confirmed', '2025-12-20 11:20:00'),
(11, NULL, 'Robert Williams', 8, '2025-12-28 19:00:00', 4, 'Confirmed', '2025-12-20 12:00:00'),
(12, NULL, 'Sarah Davis', 9, '2025-12-29 20:00:00', 2, 'Pending', '2025-12-20 12:30:00'),
(13, 3, NULL, 10, '2025-12-31 21:00:00', 10, 'Confirmed', '2025-12-15 10:00:00'),
(14, 2, NULL, 4, '2025-12-31 19:00:00', 4, 'Confirmed', '2025-12-16 14:30:00'),
(15, 4, NULL, 2, '2026-01-03 19:30:00', 2, 'Pending', '2025-12-20 13:00:00');
SET IDENTITY_INSERT Reservations OFF;

-- 4. Menu Categories
SET IDENTITY_INSERT MenuCategories ON;
INSERT INTO MenuCategories (Id, Name, DisplayOrder, IsActive) VALUES
(1, 'Appetizers', 1, 1),
(2, 'Salads', 2, 1),
(3, 'Main Course', 3, 1),
(4, 'Pasta & Risotto', 4, 1),
(5, 'Seafood', 5, 1),
(6, 'Desserts', 6, 1),
(7, 'Beverages', 7, 1);
SET IDENTITY_INSERT MenuCategories OFF;

-- 5. Menu Items
INSERT INTO MenuItems (Name, Description, Price, MenuCategoryId, ImageUrl, IsAvailable, CreatedAt) VALUES
('Bruschetta', 'Toasted bread topped with fresh tomatoes, garlic, basil and olive oil', 8.99, 1, 'https://images.unsplash.com/photo-1572695157366-5e585ab2b69f?w=400', 1, GETDATE()),
('Mozzarella Sticks', 'Golden fried mozzarella with marinara sauce', 9.99, 1, 'https://images.unsplash.com/photo-1531749668029-2db88e4276c7?w=400', 1, GETDATE()),
('Calamari Fritti', 'Crispy fried squid rings with spicy aioli', 12.99, 1, 'https://images.unsplash.com/photo-1599487488170-d11ec9c172f0?w=400', 1, GETDATE()),
('Caesar Salad', 'Romaine lettuce, parmesan, croutons, Caesar dressing', 10.99, 2, 'https://images.unsplash.com/photo-1546793665-c74683f339c1?w=400', 1, GETDATE()),
('Greek Salad', 'Cucumbers, tomatoes, olives, feta cheese, red onions', 11.99, 2, 'https://images.unsplash.com/photo-1540189549336-e6e99c3679fe?w=400', 1, GETDATE()),
('Caprese Salad', 'Fresh mozzarella, tomatoes, basil, balsamic glaze', 12.99, 2, 'https://images.unsplash.com/photo-1592417817038-d13fd7ab00e8?w=400', 1, GETDATE()),
('Grilled Ribeye Steak', '12oz ribeye with roasted vegetables and mashed potatoes', 32.99, 3, 'https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=400', 1, GETDATE()),
('Chicken Parmesan', 'Breaded chicken breast, marinara sauce, melted mozzarella', 19.99, 3, 'https://images.unsplash.com/photo-1632778149955-e80f8ceca2e8?w=400', 1, GETDATE()),
('Lamb Chops', 'Grilled lamb chops with herb butter and seasonal vegetables', 28.99, 3, 'https://images.unsplash.com/photo-1529692236671-f1f6cf9683ba?w=400', 1, GETDATE()),
('Spaghetti Carbonara', 'Creamy sauce with bacon, eggs, parmesan cheese', 16.99, 4, 'https://images.unsplash.com/photo-1612874742237-6526221588e3?w=400', 1, GETDATE()),
('Fettuccine Alfredo', 'Rich cream sauce with butter and parmesan', 15.99, 4, 'https://images.unsplash.com/photo-1645112411341-6c4fd023714a?w=400', 1, GETDATE()),
('Mushroom Risotto', 'Creamy arborio rice with porcini mushrooms', 18.99, 4, 'https://images.unsplash.com/photo-1476124369491-f56c5ab8f354?w=400', 1, GETDATE()),
('Grilled Salmon', 'Atlantic salmon with lemon butter sauce and asparagus', 24.99, 5, 'https://images.unsplash.com/photo-1467003909585-2f8a72700288?w=400', 1, GETDATE()),
('Lobster Tail', 'Butter-poached lobster tail with drawn butter', 38.99, 5, 'https://images.unsplash.com/photo-1625943553852-781c6dd46faa?w=400', 1, GETDATE()),
('Shrimp Scampi', 'Sautéed shrimp in garlic white wine butter sauce', 22.99, 5, 'https://images.unsplash.com/photo-1565680018434-b513d5e5fd47?w=400', 1, GETDATE()),
('Tiramisu', 'Classic Italian dessert with espresso and mascarpone', 8.99, 6, 'https://images.unsplash.com/photo-1571877227200-a0d98ea607e9?w=400', 1, GETDATE()),
('Chocolate Lava Cake', 'Warm chocolate cake with molten center, vanilla ice cream', 9.99, 6, 'https://images.unsplash.com/photo-1624353365286-3f8d62daad51?w=400', 1, GETDATE()),
('Cheesecake', 'New York style cheesecake with berry compote', 8.99, 6, 'https://images.unsplash.com/photo-1533134242820-4f85520993a3?w=400', 1, GETDATE()),
('Freshly Squeezed Orange Juice', 'Pure orange juice, no added sugar', 4.99, 7, NULL, 1, GETDATE()),
('Iced Tea', 'Freshly brewed sweet or unsweet', 2.99, 7, NULL, 1, GETDATE()),
('Espresso', 'Double shot Italian espresso', 3.99, 7, NULL, 1, GETDATE()),
('San Pellegrino', 'Italian sparkling water', 3.99, 7, NULL, 1, GETDATE());

GO

-- =============================================
-- Final Verification Summary
-- =============================================
PRINT '=== RestoBook Database Setup Complete ===';
SELECT 'Users' AS TableName, COUNT(*) AS RecordCount FROM Users
UNION ALL
SELECT 'RestaurantTables', COUNT(*) FROM RestaurantTables
UNION ALL
SELECT 'Reservations', COUNT(*) FROM Reservations
UNION ALL
SELECT 'MenuCategories', COUNT(*) FROM MenuCategories
UNION ALL
SELECT 'MenuItems', COUNT(*) FROM MenuItems;
GO

ALTER TABLE Reservations
ADD SpecialRequest NVARCHAR(500) NULL;
GO

