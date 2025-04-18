-- --------------------------------------------------------
-- Host:                         127.0.0.1
-- Server version:               11.7.2-MariaDB - mariadb.org binary distribution
-- Server OS:                    Win64
-- HeidiSQL Version:             12.10.0.7000
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;


-- Dumping database structure for haidilao
CREATE DATABASE IF NOT EXISTS `haidilao` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci */;
USE `haidilao`;

-- Dumping structure for table haidilao.customernoshows
CREATE TABLE IF NOT EXISTS `customernoshows` (
  `NoShowId` int(11) NOT NULL AUTO_INCREMENT,
  `OutletId` int(11) NOT NULL,
  `ContactNumber` varchar(15) NOT NULL,
  `NoShowCount` int(11) NOT NULL DEFAULT 1,
  `Status` enum('Warning','Suspended','Banned') NOT NULL DEFAULT 'Warning',
  `LastNoShowDate` datetime NOT NULL DEFAULT current_timestamp(),
  `CreatedAt` datetime NOT NULL DEFAULT current_timestamp(),
  `UpdatedAt` datetime DEFAULT NULL ON UPDATE current_timestamp(),
  `ExpiredAt` datetime GENERATED ALWAYS AS (case when `Status` = 'Warning' then `LastNoShowDate` + interval 30 day when `Status` = 'Suspended' then `LastNoShowDate` + interval 7 day when `Status` = 'Banned' then `LastNoShowDate` + interval 30 day end) STORED,
  `Reason` text DEFAULT 'No show for reservation.',
  PRIMARY KEY (`NoShowId`) USING BTREE,
  KEY `OutletId` (`OutletId`) USING BTREE,
  CONSTRAINT `customernoshows_ibfk_1` FOREIGN KEY (`OutletId`) REFERENCES `outlets` (`OutletId`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

-- Dumping data for table haidilao.customernoshows: ~5 rows (approximately)
DELETE FROM `customernoshows`;
INSERT INTO `customernoshows` (`NoShowId`, `OutletId`, `ContactNumber`, `NoShowCount`, `Status`, `LastNoShowDate`, `CreatedAt`, `UpdatedAt`, `Reason`) VALUES
	(1, 1, '0123456543', 1, 'Warning', '2025-04-18 02:25:49', '2025-04-18 02:25:50', NULL, 'No show for reservation.'),
	(2, 1, '0123456789', 1, 'Warning', '2025-04-18 02:46:51', '2025-04-18 02:46:51', NULL, 'No show for reservation.'),
	(3, 1, '9876543210', 2, 'Warning', '2025-04-18 02:46:51', '2025-04-18 02:46:51', NULL, 'No show for reservation.'),
	(4, 1, '1234567890', 3, 'Suspended', '2025-04-18 02:46:51', '2025-04-18 02:46:51', NULL, 'No show for reservation.'),
	(5, 1, '0987654321', 4, 'Banned', '2025-04-18 02:46:51', '2025-04-18 02:46:51', NULL, 'No show for reservation.');

-- Dumping structure for table haidilao.outlets
CREATE TABLE IF NOT EXISTS `outlets` (
  `OutletId` int(11) NOT NULL AUTO_INCREMENT,
  `OutletName` varchar(100) DEFAULT '',
  `Location` varchar(255) DEFAULT '',
  `OperatingHours` varchar(100) DEFAULT '',
  `Capacity` int(11) DEFAULT NULL,
  `CreatedAt` datetime DEFAULT current_timestamp(),
  `UpdatedAt` datetime DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`OutletId`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

-- Dumping data for table haidilao.outlets: ~6 rows (approximately)
DELETE FROM `outlets`;
INSERT INTO `outlets` (`OutletId`, `OutletName`, `Location`, `OperatingHours`, `Capacity`, `CreatedAt`, `UpdatedAt`) VALUES
	(1, 'Haidilao Sunway Pyramid', 'Sunway Pyramid', '11AM - 2AM', 5, '2025-04-07 22:46:12', '2025-04-08 22:30:09'),
	(2, 'Haidilao Pavilion', 'Pavilion', '11AM - 2AM', 10, '2025-04-07 22:49:45', '2025-04-07 22:49:48'),
	(3, 'Haidilao One Utama', 'One Utama', '11AM - 2AM', 10, '2025-04-07 22:57:35', '2025-04-07 22:57:36'),
	(4, 'Haidilao Sunway Velocity', 'Sunway Velocity', '11AM - 2AM', 10, '2025-04-07 22:57:51', '2025-04-07 22:57:59'),
	(5, 'Haidilao IOI City Mall', 'IOI City Mall', '11AM - 2AM', 10, '2025-04-07 22:58:12', '2025-04-07 22:58:16'),
	(6, 'Haidilao IOI Puchong Mall', 'IOI Puchong Mall', '11AM - 2AM', 10, '2025-04-07 22:58:38', '2025-04-07 22:58:38'),
	(7, 'Haidilao Bangsar Village 3', 'Bangsar Village 3', '11AM - 2AM', 10, '2025-04-07 23:00:01', '2025-04-07 23:00:02');

-- Dumping structure for table haidilao.queues
CREATE TABLE IF NOT EXISTS `queues` (
  `QueueId` int(11) NOT NULL AUTO_INCREMENT,
  `OutletId` int(11) NOT NULL,
  `CustomerName` varchar(50) DEFAULT '',
  `ContactNumber` varchar(15) DEFAULT '',
  `QueuePosition` int(11) DEFAULT NULL,
  `NumberOfGuest` int(11) DEFAULT NULL,
  `SpecialRequest` text DEFAULT NULL,
  `Status` enum('Waiting','Called','Completed','Cancelled') DEFAULT 'Waiting',
  `CreatedAt` datetime DEFAULT current_timestamp(),
  `UpdatedAt` datetime DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`QueueId`),
  KEY `QueueOutletFK` (`OutletId`),
  CONSTRAINT `QueueOutletFK` FOREIGN KEY (`OutletId`) REFERENCES `outlets` (`OutletId`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=30 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

-- Dumping data for table haidilao.queues: ~2 rows (approximately)
DELETE FROM `queues`;
INSERT INTO `queues` (`QueueId`, `OutletId`, `CustomerName`, `ContactNumber`, `QueuePosition`, `NumberOfGuest`, `SpecialRequest`, `Status`, `CreatedAt`, `UpdatedAt`) VALUES
	(28, 1, 'Lai Zhan Hong', '01161708616', 1, 1, 'sadf', 'Waiting', '2025-04-18 19:53:16', '2025-04-18 19:53:16'),
	(29, 1, 'Lim Poh Jing', '0198162823', 2, 2, 'sdf', 'Cancelled', '2025-04-18 20:06:13', '2025-04-18 20:10:21');

-- Dumping structure for table haidilao.reservations
CREATE TABLE IF NOT EXISTS `reservations` (
  `ReservationId` int(11) NOT NULL AUTO_INCREMENT,
  `OutletId` int(11) NOT NULL,
  `CustomerName` varchar(50) DEFAULT '',
  `ContactNumber` varchar(15) DEFAULT '',
  `NumberOfGuest` int(11) DEFAULT NULL,
  `Status` enum('Pending','Confirmed','Cancelled','Completed') DEFAULT 'Pending',
  `SpecialRequest` text DEFAULT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT current_timestamp(),
  `UpdatedAt` datetime NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  `ReservationDateTime` datetime NOT NULL,
  PRIMARY KEY (`ReservationId`) USING BTREE,
  KEY `FK_reservations_outlets` (`OutletId`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=105 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

-- Dumping data for table haidilao.reservations: ~1 rows (approximately)
DELETE FROM `reservations`;
INSERT INTO `reservations` (`ReservationId`, `OutletId`, `CustomerName`, `ContactNumber`, `NumberOfGuest`, `Status`, `SpecialRequest`, `CreatedAt`, `UpdatedAt`, `ReservationDateTime`) VALUES
	(104, 1, 'Lai Zhan Hong', '01161708616', 3, 'Pending', 'Baby chair x1', '2025-04-18 19:44:53', '2025-04-18 19:44:53', '2025-04-19 13:46:00');

-- Dumping structure for table haidilao.users
CREATE TABLE IF NOT EXISTS `users` (
  `UserId` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(50) DEFAULT '',
  `ContactNumber` varchar(15) DEFAULT '',
  `Email` varchar(100) DEFAULT '',
  `PasswordHash` varchar(255) DEFAULT '',
  `Role` enum('Staff','Admin') DEFAULT 'Staff',
  `OutletId` int(11) DEFAULT NULL,
  `CreatedAt` datetime DEFAULT current_timestamp(),
  `UpdatedAt` datetime DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`UserId`) USING BTREE,
  KEY `OutletId` (`OutletId`),
  CONSTRAINT `users_ibfk_1` FOREIGN KEY (`OutletId`) REFERENCES `outlets` (`OutletId`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

-- Dumping data for table haidilao.users: ~2 rows (approximately)
DELETE FROM `users`;
INSERT INTO `users` (`UserId`, `Name`, `ContactNumber`, `Email`, `PasswordHash`, `Role`, `OutletId`, `CreatedAt`, `UpdatedAt`) VALUES
	(1, 'Lai Zhan Hong', '01161708616', 'laizhanhong0119@gmail.com', '123', 'Admin', NULL, '2025-04-14 02:11:14', '2025-04-14 02:12:00'),
	(2, 'Lim Poh Jing', '01161708616', 'llgjellyfish@gmail.com', '123', 'Staff', 1, '2025-04-14 02:11:57', '2025-04-14 02:12:08'),
	(3, 'Jason', '0123456789', 'jason@gmail.com', '123', 'Staff', 1, '2025-04-18 01:48:50', '2025-04-18 01:48:50');

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
