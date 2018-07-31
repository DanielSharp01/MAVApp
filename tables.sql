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
) ENGINE=InnoDB AUTO_INCREMENT=3003 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Dumping structure for table mavapp.trace
CREATE TABLE IF NOT EXISTS `trace` (
  `id` int(11) NOT NULL,
  `train_instance_id` int(11) NOT NULL,
  `lat` double NOT NULL,
  `lon` double NOT NULL,
  `updated` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `FK_trace_train_instances` (`train_instance_id`),
  CONSTRAINT `FK_trace_train_instances` FOREIGN KEY (`train_instance_id`) REFERENCES `train_instances` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Dumping data for table mavapp.trace: ~0 rows (approximately)
/*!40000 ALTER TABLE `trace` DISABLE KEYS */;
/*!40000 ALTER TABLE `trace` ENABLE KEYS */;

-- Dumping structure for table mavapp.trains
CREATE TABLE IF NOT EXISTS `trains` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(50) DEFAULT NULL,
  `type` varchar(50) DEFAULT NULL,
  `polyline` text,
  `expiry_date` date DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=9434 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Dumping data for table mavapp.trains: ~0 rows (approximately)
/*!40000 ALTER TABLE `trains` DISABLE KEYS */;
/*!40000 ALTER TABLE `trains` ENABLE KEYS */;

-- Dumping structure for table mavapp.train_instances
CREATE TABLE IF NOT EXISTS `train_instances` (
  `id` int(11) NOT NULL,
  `train_id` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `FK_train_instances_trains` (`train_id`),
  CONSTRAINT `FK_train_instances_trains` FOREIGN KEY (`train_id`) REFERENCES `trains` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Dumping data for table mavapp.train_instances: ~0 rows (approximately)
/*!40000 ALTER TABLE `train_instances` DISABLE KEYS */;
/*!40000 ALTER TABLE `train_instances` ENABLE KEYS */;

-- Dumping structure for table mavapp.train_instance_stations
CREATE TABLE IF NOT EXISTS `train_instance_stations` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `train_instance_id` int(11) NOT NULL,
  `train_station_id` int(11) NOT NULL,
  `actual_arrival` datetime DEFAULT NULL,
  `actual_departure` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `FK_train_instance_stations_train_stations` (`train_station_id`),
  KEY `FK_train_instance_stations_train_instances` (`train_instance_id`),
  CONSTRAINT `FK_train_instance_stations_train_instances` FOREIGN KEY (`train_instance_id`) REFERENCES `train_instances` (`id`),
  CONSTRAINT `FK_train_instance_stations_train_stations` FOREIGN KEY (`train_station_id`) REFERENCES `train_stations` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Dumping data for table mavapp.train_instance_stations: ~0 rows (approximately)
/*!40000 ALTER TABLE `train_instance_stations` DISABLE KEYS */;
/*!40000 ALTER TABLE `train_instance_stations` ENABLE KEYS */;

-- Dumping structure for table mavapp.train_stations
CREATE TABLE IF NOT EXISTS `train_stations` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `train_id` int(11) NOT NULL,
  `ordinal` int(11) NOT NULL,
  `station_id` int(11) NOT NULL,
  `arrival` datetime DEFAULT NULL,
  `departure` datetime DEFAULT NULL,
  `rel_distance` double DEFAULT NULL,
  `platform` varchar(5) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `FK_train_stations_trains` (`train_id`),
  KEY `FK_train_stations_stations` (`station_id`),
  CONSTRAINT `FK_train_stations_stations` FOREIGN KEY (`station_id`) REFERENCES `stations` (`id`),
  CONSTRAINT `FK_train_stations_trains` FOREIGN KEY (`train_id`) REFERENCES `trains` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Dumping data for table mavapp.train_stations: ~0 rows (approximately)
/*!40000 ALTER TABLE `train_stations` DISABLE KEYS */;
/*!40000 ALTER TABLE `train_stations` ENABLE KEYS */;

/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IF(@OLD_FOREIGN_KEY_CHECKS IS NULL, 1, @OLD_FOREIGN_KEY_CHECKS) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
