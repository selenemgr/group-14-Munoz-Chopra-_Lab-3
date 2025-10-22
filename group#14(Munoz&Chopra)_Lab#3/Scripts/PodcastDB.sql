USE master;
GO

IF DB_ID('PodcastDB') IS NOT NULL
BEGIN
    ALTER DATABASE PodcastDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE PodcastDB;
END
GO

CREATE DATABASE PodcastDB;
GO

USE PodcastDB;
GO

CREATE TABLE dbo.Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL UNIQUE,
    Email NVARCHAR(200) NOT NULL UNIQUE,
    Role NVARCHAR(50) NOT NULL
);

CREATE TABLE dbo.Podcasts (
    PodcastID INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    CreatorID INT NOT NULL FOREIGN KEY REFERENCES Users(UserID),
    CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE dbo.Episodes (
    EpisodeID INT IDENTITY(1,1) PRIMARY KEY,
    PodcastID INT NOT NULL FOREIGN KEY REFERENCES Podcasts(PodcastID),
    Title NVARCHAR(250) NOT NULL,
    ReleaseDate DATETIME2 NOT NULL,
    DurationMinutes INT NOT NULL,
    PlayCount INT NOT NULL DEFAULT 0,
    AudioFileURL NVARCHAR(1000),
    NumberOfViews INT NOT NULL DEFAULT 0
);

CREATE TABLE dbo.Subscriptions (
    SubscriptionID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL FOREIGN KEY REFERENCES Users(UserID),
    PodcastID INT NOT NULL FOREIGN KEY REFERENCES Podcasts(PodcastID),
    SubscribedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

INSERT INTO dbo.Users (Username, Email, Role) VALUES
('alice', 'alice@example.com', 'Listener'),
('bob', 'bob@example.com', 'Podcaster'),
('selene', 'selene@example.com', 'Podcaster'),
('muskan', 'muskan@example.com', 'Admin'),
('eve', 'eve@example.com', 'Listener');

INSERT INTO dbo.Podcasts (Title, Description, CreatorID, CreatedDate) VALUES
('Bill Gates', 'Tech insights and entrepreneurial discussions', 2, '2024-01-10'),
('Making Noise', 'Research highlights', 2, '2024-05-01'),
('The Future Is', 'Exploring tomorrow''s technological possibilities', 3, '2024-02-01'),
('The Luxury', 'Premium lifestyle experiences and insights', 3, '2024-03-05'),
('Watchmen', 'Deep narrative analysis and storytelling', 3, '2024-04-12');

INSERT INTO dbo.Episodes (PodcastID, Title, ReleaseDate, DurationMinutes, PlayCount, AudioFileURL, NumberOfViews) VALUES
(1, 'Bill Gates Podcast - Ep 1', '2024-06-01', 45, 1200, 'https://lab3-podcast-bucket.s3.us-east-1.amazonaws.com/bill-gates-podcast.mp3', 1200),
(2, 'Making Noise Podcast - Ep 1', '2024-07-01', 50, 900, 'https://lab3-podcast-bucket.s3.us-east-1.amazonaws.com/making-noise-podcast.mp3', 900),
(3, 'The Future Is Podcast - Ep 1', '2024-06-15', 40, 300, 'https://lab3-podcast-bucket.s3.us-east-1.amazonaws.com/the-future-is-podcast.mp3', 300),
(4, 'The Luxury Podcast - Ep 1', '2024-08-11', 60, 450, 'https://lab3-podcast-bucket.s3.us-east-1.amazonaws.com/the-luxury-podcast.mp3', 450),
(5, 'Watchmen Podcast - Ep 1', '2024-05-20', 55, 700, 'https://lab3-podcast-bucket.s3.us-east-1.amazonaws.com/watchmen-podcast.mp3', 700);

INSERT INTO dbo.Subscriptions (UserID, PodcastID, SubscribedDate)
SELECT 
    u.UserID,
    p.PodcastID,
    DATEADD(day, -ROW_NUMBER() OVER (ORDER BY u.UserID), SYSUTCDATETIME())
FROM 
    (SELECT UserID, ROW_NUMBER() OVER (ORDER BY UserID) AS userrow FROM dbo.Users) u
JOIN 
    (SELECT PodcastID, ROW_NUMBER() OVER (ORDER BY PodcastID) AS podcastrow FROM dbo.Podcasts) p
ON u.userrow = p.podcastrow;

