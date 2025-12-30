-- =============================================
-- RestoBook Database - Complete Setup Script
-- =============================================
-- This script creates tables and inserts sample data
-- Includes: 1 Admin + 3 Customers, 11 Tables, 15 Reservations
-- =============================================

-- Drop existing tables if they exist (in correct order to respect foreign keys)
IF OBJECT_ID('dbo.Reservations', 'U') IS NOT NULL DROP TABLE dbo.Reservations;
IF OBJECT_ID('dbo.RestaurantTables', 'U') IS NOT NULL DROP TABLE dbo.RestaurantTables;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;

GO

-- =============================================
-- Create Users Table
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

GO

-- =============================================
-- Create RestaurantTables Table
-- =============================================
CREATE TABLE RestaurantTables (
    Id INT PRIMARY KEY IDENTITY(1,1),
    TableNumber INT NOT NULL UNIQUE,
    Capacity INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT CK_RestaurantTables_Capacity CHECK (Capacity > 0)
);

GO

-- =============================================
-- Create Reservations Table
-- =============================================
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

GO

-- =============================================
-- Insert Sample Users
-- =============================================
-- Note: Password hashes are placeholders - replace with actual PBKDF2 hashes
-- Suggested passwords for testing:
-- Admin: Admin@123
-- Customers: Customer1@123, Customer2@123, Customer3@123

SET IDENTITY_INSERT Users ON;

INSERT INTO Users (Id, FullName, Email, PasswordHash, Role, CreatedAt) VALUES
(1, 'Admin User', 'admin@restobook.com', '$2a$11$8xKJZqXhZJYvH5rKqGqE3.YvZQxYqZqZqZqZqZqZqZqZqZqZqZqZq', 'Admin', '2025-01-01 10:00:00'),
(2, 'John Doe', 'john.doe@email.com', '$2a$11$9yLKZrYiZKZwI6sLrHrF4.ZwZRyZrZrZrZrZrZrZrZrZrZrZrZrZr', 'Customer', '2025-01-05 14:30:00'),
(3, 'Jane Smith', 'jane.smith@email.com', '$2a$11$7xMNAsZjALAxJ7tMsIsG5.AxARzAsAsAsAsAsAsAsAsAsAsAsAsAs', 'Customer', '2025-01-10 09:15:00'),
(4, 'Michael Johnson', 'michael.j@email.com', '$2a$11$6wOPBtAkBMByK8uOtJtH6.ByBSABtBtBtBtBtBtBtBtBtBtBtBtBt', 'Customer', '2025-01-15 16:45:00');

SET IDENTITY_INSERT Users OFF;

GO

-- =============================================
-- Insert Sample Restaurant Tables
-- =============================================
SET IDENTITY_INSERT RestaurantTables ON;

INSERT INTO RestaurantTables (Id, TableNumber, Capacity, IsActive) VALUES
(1, 101, 2, 1),  -- Table for 2 people
(2, 102, 2, 1),  -- Table for 2 people
(3, 103, 4, 1),  -- Table for 4 people
(4, 104, 4, 1),  -- Table for 4 people
(5, 105, 6, 1),  -- Table for 6 people
(6, 106, 6, 1),  -- Table for 6 people
(7, 107, 8, 1),  -- Table for 8 people
(8, 108, 4, 1),  -- Table for 4 people
(9, 109, 2, 1),  -- Table for 2 people
(10, 110, 10, 1), -- Large table for 10 people
(11, 111, 4, 0);  -- Inactive table (under maintenance)

SET IDENTITY_INSERT RestaurantTables OFF;

GO

-- =============================================
-- Insert Sample Reservations
-- =============================================
SET IDENTITY_INSERT Reservations ON;

-- Past reservations
INSERT INTO Reservations (Id, UserId, GuestName, RestaurantTableId, ReservationDate, NumberOfGuests, Status, CreatedAt) VALUES
(1, 2, NULL, 1, '2025-12-15 19:00:00', 2, 'Confirmed', '2025-12-10 10:30:00'),
(2, 3, NULL, 3, '2025-12-16 20:00:00', 4, 'Confirmed', '2025-12-11 14:20:00'),
(3, 4, NULL, 5, '2025-12-17 18:30:00', 6, 'Cancelled', '2025-12-12 09:45:00'),

-- Current/Future reservations
(4, 2, NULL, 2, '2025-12-21 19:30:00', 2, 'Confirmed', '2025-12-18 11:00:00'),
(5, 3, NULL, 4, '2025-12-22 20:00:00', 4, 'Pending', '2025-12-19 15:30:00'),
(6, 4, NULL, 6, '2025-12-23 19:00:00', 5, 'Confirmed', '2025-12-19 16:45:00'),
(7, 2, NULL, 7, '2025-12-24 18:00:00', 8, 'Pending', '2025-12-20 08:30:00'),
(8, 3, NULL, 1, '2025-12-25 20:30:00', 2, 'Confirmed', '2025-12-20 09:15:00'),
(9, 4, NULL, 3, '2025-12-26 19:30:00', 4, 'Pending', '2025-12-20 10:00:00'),
(10, 2, NULL, 5, '2025-12-27 18:30:00', 6, 'Confirmed', '2025-12-20 11:20:00'),

-- Walk-in reservations (no user account)
(11, NULL, 'Robert Williams', 8, '2025-12-28 19:00:00', 4, 'Confirmed', '2025-12-20 12:00:00'),
(12, NULL, 'Sarah Davis', 9, '2025-12-29 20:00:00', 2, 'Pending', '2025-12-20 12:30:00'),

-- New Year's Eve reservations
(13, 3, NULL, 10, '2025-12-31 21:00:00', 10, 'Confirmed', '2025-12-15 10:00:00'),
(14, 2, NULL, 4, '2025-12-31 19:00:00', 4, 'Confirmed', '2025-12-16 14:30:00'),

-- Future reservations in January
(15, 4, NULL, 2, '2026-01-03 19:30:00', 2, 'Pending', '2025-12-20 13:00:00');

SET IDENTITY_INSERT Reservations OFF;

GO

-- =============================================
-- Verification Queries
-- =============================================
DECLARE @UserCount INT = (SELECT COUNT(*) FROM Users);
DECLARE @TableCount INT = (SELECT COUNT(*) FROM RestaurantTables);
DECLARE @ReservationCount INT = (SELECT COUNT(*) FROM Reservations);

PRINT '=== Database Setup Complete ===';
PRINT '';
PRINT 'Total Users Inserted: ' + CAST(@UserCount AS NVARCHAR(10));
PRINT 'Total Tables Inserted: ' + CAST(@TableCount AS NVARCHAR(10));
PRINT 'Total Reservations Inserted: ' + CAST(@ReservationCount AS NVARCHAR(10));
PRINT '';

-- Display summary
SELECT 'Users' AS TableName, COUNT(*) AS RecordCount FROM Users
UNION ALL
SELECT 'RestaurantTables', COUNT(*) FROM RestaurantTables
UNION ALL
SELECT 'Reservations', COUNT(*) FROM Reservations;

GO
