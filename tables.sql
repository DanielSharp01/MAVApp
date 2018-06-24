-- --------------------------------------------------------
-- Host:                         127.0.0.1
-- Server version:               8.0.11 - MySQL Community Server - GPL
-- Server OS:                    Win64
-- HeidiSQL Version:             9.5.0.5196
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;


-- Dumping database structure for mavapp
CREATE DATABASE IF NOT EXISTS `mavapp` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */;
USE `mavapp`;

-- Dumping structure for table mavapp.line_points
CREATE TABLE IF NOT EXISTS `line_points` (
  `id` int(11) NOT NULL,
  `number` int(11) NOT NULL,
  `lat` double NOT NULL,
  `lon` double NOT NULL,
  PRIMARY KEY (`id`,`number`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Data exporting was unselected.
-- Dumping structure for table mavapp.stations
CREATE TABLE IF NOT EXISTS `stations` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL,
  `norm_name` varchar(255) NOT NULL,
  `lat` double NOT NULL,
  `lon` double NOT NULL,
  `mav_found` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `norm_name` (`norm_name`)
) ENGINE=InnoDB AUTO_INCREMENT=2050 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Data exporting was unselected.
-- Dumping structure for table mavapp.trains
CREATE TABLE IF NOT EXISTS `trains` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `elvira_id` char(13) DEFAULT NULL,
  `number` varchar(50) DEFAULT NULL,
  `name` varchar(50) DEFAULT NULL,
  `type` text,
  `number_type` varchar(50) DEFAULT NULL,
  `delay` int(11) DEFAULT NULL,
  `delay_reason` text,
  `misc_info` text COMMENT 'Every new line is a new row of information.',
  `lat` double DEFAULT NULL,
  `lon` double DEFAULT NULL,
  `last_lat` double DEFAULT NULL,
  `last_lon` double DEFAULT NULL,
  `enc_polyline` text,
  PRIMARY KEY (`id`),
  UNIQUE KEY `elvira_id` (`elvira_id`)
) ENGINE=InnoDB AUTO_INCREMENT=13642 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Data exporting was unselected.
-- Dumping structure for table mavapp.train_stations
CREATE TABLE IF NOT EXISTS `train_stations` (
  `train_id` int(11) NOT NULL,
  `number` int(11) NOT NULL,
  `station` int(11) DEFAULT NULL,
  `mav_name` varchar(255) NOT NULL,
  `int_distance` int(11) NOT NULL,
  `distance` double NOT NULL,
  `position_accuracy` tinyint(2) NOT NULL COMMENT '0 - missing, 1 - integer, 2 - precise',
  `arrive` timestamp NOT NULL,
  `depart` timestamp NOT NULL,
  `arrive_actual` timestamp NOT NULL,
  `depart_actual` timestamp NOT NULL,
  `arrived` tinyint(1) NOT NULL,
  `platform` varchar(10) DEFAULT NULL,
  PRIMARY KEY (`train_id`,`number`),
  KEY `FK_train_stations_stations` (`station`),
  CONSTRAINT `FK_train_stations_stations` FOREIGN KEY (`station`) REFERENCES `stations` (`id`),
  CONSTRAINT `FK_train_stations_trains` FOREIGN KEY (`train_id`) REFERENCES `trains` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Data exporting was unselected.
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IF(@OLD_FOREIGN_KEY_CHECKS IS NULL, 1, @OLD_FOREIGN_KEY_CHECKS) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
