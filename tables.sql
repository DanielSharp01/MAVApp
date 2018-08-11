-- --------------------------------------------------------
-- Host:                         127.0.0.1
-- Server version:               8.0.11 - MySQL Community Server - GPL
-- Server OS:                    Win64
-- HeidiSQL Version:             9.5.0.5280
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;

-- Dumping structure for table mavapp.stations
CREATE TABLE IF NOT EXISTS `stations` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL,
  `norm_name` varchar(255) NOT NULL,
  `lat` double DEFAULT NULL,
  `lon` double DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `norm_name` (`norm_name`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Data exporting was unselected.
-- Dumping structure for table mavapp.trace
CREATE TABLE IF NOT EXISTS `trace` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `train_instance_id` bigint(20) NOT NULL,
  `lat` double NOT NULL,
  `lon` double NOT NULL,
  `updated` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `FK_trace_train_instances` (`train_instance_id`),
  CONSTRAINT `FK_trace_train_instances` FOREIGN KEY (`train_instance_id`) REFERENCES `train_instances` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Data exporting was unselected.
-- Dumping structure for table mavapp.trains
CREATE TABLE IF NOT EXISTS `trains` (
  `id` int(11) NOT NULL,
  `name` varchar(50) DEFAULT NULL,
  `type` varchar(50) DEFAULT NULL,
  `polyline` text,
  `expiry_date` date DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Data exporting was unselected.
-- Dumping structure for table mavapp.train_instances
CREATE TABLE IF NOT EXISTS `train_instances` (
  `id` bigint(20) NOT NULL,
  `train_id` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `FK_train_instances_trains` (`train_id`),
  CONSTRAINT `FK_train_instances_trains` FOREIGN KEY (`train_id`) REFERENCES `trains` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Data exporting was unselected.
-- Dumping structure for table mavapp.train_instance_stations
CREATE TABLE IF NOT EXISTS `train_instance_stations` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `train_instance_id` bigint(20) NOT NULL,
  `train_station_id` int(11) NOT NULL,
  `actual_arrival` time DEFAULT NULL,
  `actual_departure` time DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `train_instance_id_train_station_id` (`train_instance_id`,`train_station_id`),
  KEY `FK_train_instance_stations_train_stations` (`train_station_id`),
  CONSTRAINT `FK_train_instance_stations_train_instances` FOREIGN KEY (`train_instance_id`) REFERENCES `train_instances` (`id`),
  CONSTRAINT `FK_train_instance_stations_train_stations` FOREIGN KEY (`train_station_id`) REFERENCES `train_stations` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Data exporting was unselected.
-- Dumping structure for table mavapp.train_stations
CREATE TABLE IF NOT EXISTS `train_stations` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `train_id` int(11) NOT NULL,
  `ordinal` int(11) NOT NULL,
  `station_id` int(11) NOT NULL,
  `arrival` time DEFAULT NULL,
  `departure` time DEFAULT NULL,
  `rel_distance` double DEFAULT NULL,
  `platform` varchar(5) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `train_id_ordinal` (`train_id`,`ordinal`),
  KEY `FK_train_stations_stations` (`station_id`),
  CONSTRAINT `FK_train_stations_stations` FOREIGN KEY (`station_id`) REFERENCES `stations` (`id`),
  CONSTRAINT `FK_train_stations_trains` FOREIGN KEY (`train_id`) REFERENCES `trains` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Data exporting was unselected.
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IF(@OLD_FOREIGN_KEY_CHECKS IS NULL, 1, @OLD_FOREIGN_KEY_CHECKS) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
