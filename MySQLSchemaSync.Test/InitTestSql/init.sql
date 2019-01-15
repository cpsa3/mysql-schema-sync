DROP TABLE IF EXISTS `event_received_template`;
DROP TABLE IF EXISTS `event_received_local`;

CREATE TABLE `event_received_template` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `GroupName` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `EventId` varchar(32) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Content` longtext COLLATE utf8mb4_unicode_ci NOT NULL,
  `Retry` int(11) NOT NULL,
  `StatusName` varchar(10) COLLATE utf8mb4_unicode_ci NOT NULL,
  `NextRetryTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `db_created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `db_updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `Remark` varchar(45) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '' COMMENT '备注',
  PRIMARY KEY (`Id`),
  KEY `idx_statusname` (`StatusName`),
  KEY `idx_statusname_retry` (`StatusName`,`Retry`),
  KEY `idx_eventid` (`EventId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `event_received_local` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `GroupName` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `EventId` varchar(32) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Content` longtext COLLATE utf8mb4_unicode_ci NOT NULL,
  `Retry` int(11) NOT NULL,
  `StatusName` varchar(10) COLLATE utf8mb4_unicode_ci NOT NULL,
  `NextRetryTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `db_created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `db_updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  KEY `idx_statusname` (`StatusName`),
  KEY `idx_eventid` (`EventId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;