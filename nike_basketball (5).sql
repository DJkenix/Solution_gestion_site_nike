-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Hôte : 127.0.0.1:3306
-- Généré le : lun. 23 juin 2025 à 19:24
-- Version du serveur : 8.3.0
-- Version de PHP : 8.2.18

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Base de données : `nike_basketball`
--

DELIMITER $$
--
-- Procédures
--
DROP PROCEDURE IF EXISTS `appliquer_promotion_basique`$$
CREATE DEFINER=`root`@`localhost` PROCEDURE `appliquer_promotion_basique` (IN `nom_produit_param` VARCHAR(255), IN `pourcentage_reduction` DECIMAL(5,2))   BEGIN
    UPDATE produites
    SET 
        promotion_active = TRUE,
        prix_promo = prix_original - (prix_original * pourcentage_reduction / 100)
    WHERE nom_produit = nom_produit_param;
END$$

DELIMITER ;

-- --------------------------------------------------------

--
-- Structure de la table `avis`
--

DROP TABLE IF EXISTS `avis`;
CREATE TABLE IF NOT EXISTS `avis` (
  `id` int NOT NULL AUTO_INCREMENT,
  `user_id` int NOT NULL,
  `produit_id` int NOT NULL,
  `note` tinyint DEFAULT NULL,
  `commentaire` text,
  `date_avis` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `user_id` (`user_id`),
  KEY `produit_id` (`produit_id`)
) ENGINE=MyISAM AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Déchargement des données de la table `avis`
--

INSERT INTO `avis` (`id`, `user_id`, `produit_id`, `note`, `commentaire`, `date_avis`) VALUES
(1, 1, 12, 5, 'Super produit, je recommande !', '2025-02-22 18:24:58');

-- --------------------------------------------------------

--
-- Structure de la table `cart`
--

DROP TABLE IF EXISTS `cart`;
CREATE TABLE IF NOT EXISTS `cart` (
  `id` int NOT NULL AUTO_INCREMENT,
  `user_id` int NOT NULL,
  `product_id` int NOT NULL,
  `quantity` int NOT NULL,
  `prix` decimal(10,2) NOT NULL,
  `dateset` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `variante_id` int NOT NULL,
  `produit_taille_id` int NOT NULL,
  PRIMARY KEY (`id`),
  KEY `fkuser` (`user_id`),
  KEY `fkproduit` (`product_id`),
  KEY `variante_id` (`variante_id`),
  KEY `produit_taille_id` (`produit_taille_id`)
) ENGINE=MyISAM AUTO_INCREMENT=189 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Déchargement des données de la table `cart`
--

INSERT INTO `cart` (`id`, `user_id`, `product_id`, `quantity`, `prix`, `dateset`, `variante_id`, `produit_taille_id`) VALUES
(4, 13, 4, 1, 250.00, '2024-10-08 22:56:20', 0, 0),
(5, 13, 3, 1, 210.00, '2024-10-08 22:56:23', 0, 0),
(44, 8, 2, 1, 180.00, '2024-10-22 12:00:31', 0, 0),
(43, 8, 3, 2, 210.00, '2024-10-22 11:57:26', 0, 0),
(180, 234, 1, 1, 0.00, '2025-03-11 21:05:42', 2, 5),
(177, 232, 1, 1, 0.00, '2025-03-11 15:06:02', 1, 13);

-- --------------------------------------------------------

--
-- Structure de la table `commande`
--

DROP TABLE IF EXISTS `commande`;
CREATE TABLE IF NOT EXISTS `commande` (
  `id` int NOT NULL AUTO_INCREMENT,
  `user_id` int NOT NULL,
  `montant_total` decimal(10,2) NOT NULL,
  `statut` enum('en attente','en préparation','envoyé','livré') NOT NULL,
  `commande_le` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `user_id` (`user_id`)
) ENGINE=InnoDB AUTO_INCREMENT=24 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Déchargement des données de la table `commande`
--

INSERT INTO `commande` (`id`, `user_id`, `montant_total`, `statut`, `commande_le`) VALUES
(1, 232, 239.98, 'livré', '2025-03-09 16:54:38'),
(2, 232, 289.98, 'livré', '2025-03-09 18:40:07'),
(3, 232, 239.98, 'livré', '2025-03-09 20:28:59'),
(4, 232, 119.99, 'livré', '2025-03-11 09:14:58'),
(15, 232, 239.98, 'livré', '2025-03-11 10:59:11'),
(16, 232, 479.97, 'en attente', '2025-03-11 12:49:28'),
(17, 234, 119.99, 'livré', '2025-03-11 21:04:30'),
(18, 233, 239.98, 'en préparation', '2025-03-15 17:11:07'),
(19, 24, 389.98, 'en préparation', '2025-04-13 19:24:10'),
(20, 33, 9999.00, 'livré', '2025-04-25 14:24:44'),
(21, 33, 969.94, 'en préparation', '2025-04-27 01:23:45'),
(22, 33, 239.98, 'en préparation', '2025-06-20 23:48:01'),
(23, 33, 169.99, 'en préparation', '2025-06-20 23:48:37');

-- --------------------------------------------------------

--
-- Structure de la table `couleurs`
--

DROP TABLE IF EXISTS `couleurs`;
CREATE TABLE IF NOT EXISTS `couleurs` (
  `id` int NOT NULL AUTO_INCREMENT,
  `nom` varchar(50) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `nom` (`nom`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Déchargement des données de la table `couleurs`
--

INSERT INTO `couleurs` (`id`, `nom`) VALUES
(1, 'Blanc'),
(4, 'Bleu'),
(3, 'Gris'),
(2, 'Noir'),
(5, 'Rouge'),
(7, 'Unique'),
(6, 'Vert');

-- --------------------------------------------------------

--
-- Structure de la table `details_commande`
--

DROP TABLE IF EXISTS `details_commande`;
CREATE TABLE IF NOT EXISTS `details_commande` (
  `id` int NOT NULL AUTO_INCREMENT,
  `commande_id` int NOT NULL,
  `product_id` int NOT NULL,
  `quantite` int NOT NULL,
  `prix_achat` decimal(10,2) NOT NULL,
  `variante_id` int NOT NULL,
  `produit_taille_id` int NOT NULL,
  PRIMARY KEY (`id`),
  KEY `commande_id` (`commande_id`),
  KEY `product_id` (`product_id`),
  KEY `variante_id` (`variante_id`),
  KEY `produit_taille_id` (`produit_taille_id`)
) ENGINE=MyISAM AUTO_INCREMENT=28 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Déchargement des données de la table `details_commande`
--

INSERT INTO `details_commande` (`id`, `commande_id`, `product_id`, `quantite`, `prix_achat`, `variante_id`, `produit_taille_id`) VALUES
(1, 1, 1, 1, 119.99, 1, 2),
(2, 1, 1, 1, 119.99, 1, 1),
(3, 2, 1, 1, 119.99, 1, 3),
(4, 2, 2, 1, 169.99, 15, 10),
(5, 3, 1, 1, 119.99, 2, 4),
(6, 3, 1, 1, 119.99, 1, 13),
(7, 4, 1, 1, 119.99, 1, 2),
(13, 15, 1, 2, 119.99, 1, 1),
(14, 16, 2, 2, 169.99, 15, 10),
(15, 17, 1, 1, 119.99, 1, 13),
(16, 18, 1, 2, 119.99, 1, 1),
(17, 19, 1, 1, 119.99, 1, 3),
(18, 19, 17, 1, 269.99, 17, 24),
(19, 20, 78, 1, 9999.00, 46, 25),
(20, 21, 79, 4, 169.99, 47, 28),
(21, 21, 2, 1, 169.99, 3, 9),
(22, 21, 1, 1, 119.99, 1, 1),
(24, 22, 1, 1, 119.99, 2, 6),
(25, 22, 1, 1, 119.99, 1, 1),
(26, 23, 2, 1, 169.99, 15, 12),
(27, 16, 18, 1, 139.99, 18, 21);

-- --------------------------------------------------------

--
-- Structure de la table `order_modifications`
--

DROP TABLE IF EXISTS `order_modifications`;
CREATE TABLE IF NOT EXISTS `order_modifications` (
  `id` int NOT NULL AUTO_INCREMENT,
  `order_id` int NOT NULL,
  `admin_username` varchar(50) NOT NULL,
  `modification_date` datetime NOT NULL,
  `reason` varchar(255) NOT NULL,
  `details` text,
  PRIMARY KEY (`id`),
  KEY `order_id` (`order_id`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Déchargement des données de la table `order_modifications`
--

INSERT INTO `order_modifications` (`id`, `order_id`, `admin_username`, `modification_date`, `reason`, `details`) VALUES
(1, 21, '34', '2025-04-27 03:29:16', 'Demande client', 'Articles ajoutés:\r\n- Air Force 1, Quantité: 1, Prix: 119,99 €\r\n'),
(2, 21, '34', '2025-04-27 14:09:10', 'Erreur du site', 'Articles modifiés:\r\n- Nike Shox TL (Noir, taille 40), Nouvelle quantité: 2 (précédemment: 2)\r\n- ID: -1, Nouvelle quantité: 2 (précédemment: 0)\r\n\r\nArticles ajoutés:\r\n- Barkev Shoes (Standard, taille 44), Quantité: 1, Prix: 9 999,00 €\r\n'),
(3, 21, '34', '2025-05-14 17:38:50', 'Produit indisponible', 'Articles supprimés:\r\n- Barkev Shoes (Standard, taille 44)\r\n\r\nArticles modifiés:\r\n- Nike Shox TL (Noir, taille 40), Nouvelle quantité: 4 (précédemment: 4)\r\n\r\n'),
(4, 16, '232', '2025-06-21 02:18:32', 'Erreur de commande', 'Articles modifiés:\r\n- Nike Air Max Dn (Noir, taille 41), Nouvelle quantité: 2 (précédemment: 2)\r\n\r\nArticles ajoutés:\r\n- Air Jordan 1 Mid (Standard, taille 42), Quantité: 1, Prix: 139,99 €\r\n');

-- --------------------------------------------------------

--
-- Structure de la table `paiements`
--

DROP TABLE IF EXISTS `paiements`;
CREATE TABLE IF NOT EXISTS `paiements` (
  `paiement_id` int NOT NULL AUTO_INCREMENT,
  `user_id` int NOT NULL,
  `commande_id` int NOT NULL,
  `montant` decimal(10,0) NOT NULL,
  `mode_paiement` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `date_paiement` datetime NOT NULL,
  `statut_paiement` varchar(50) NOT NULL,
  `transaction_id` varchar(100) NOT NULL,
  PRIMARY KEY (`paiement_id`),
  KEY `FK_user_id` (`user_id`),
  KEY `FK_commande_id` (`commande_id`)
) ENGINE=MyISAM AUTO_INCREMENT=78 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Déchargement des données de la table `paiements`
--

INSERT INTO `paiements` (`paiement_id`, `user_id`, `commande_id`, `montant`, `mode_paiement`, `date_paiement`, `statut_paiement`, `transaction_id`) VALUES
(1, 1, 1, 5000, 'carte', '2024-11-05 16:17:15', 'requires_payment_method', 'pi_3QHoVBG8NKYv2Bjh0PShrrZw'),
(2, 1, 1, 5000, 'carte', '2024-11-27 14:30:07', 'requires_payment_method', 'pi_3QPlJbG8NKYv2Bjh0lUlI9YS'),
(3, 1, 1, 5000, 'carte', '2024-11-27 14:30:08', 'requires_payment_method', 'pi_3QPlJkG8NKYv2Bjh0Yx8Zyi4'),
(4, 1, 1, 5000, 'carte', '2024-11-27 14:30:09', 'requires_payment_method', 'pi_3QPlJlG8NKYv2Bjh0KJs8gKg'),
(5, 1, 1, 5000, 'carte', '2024-11-27 14:30:10', 'requires_payment_method', 'pi_3QPlJmG8NKYv2Bjh1r47yOFm'),
(6, 1, 1, 5000, 'carte', '2024-11-27 14:30:11', 'requires_payment_method', 'pi_3QPlJnG8NKYv2Bjh0Pd4in9y'),
(7, 1, 1, 5000, 'carte', '2024-11-27 14:30:12', 'requires_payment_method', 'pi_3QPlJoG8NKYv2Bjh1eEgS4aM'),
(8, 1, 1, 5000, 'carte', '2024-11-27 14:30:13', 'requires_payment_method', 'pi_3QPlJpG8NKYv2Bjh0iHR9OFS'),
(9, 1, 1, 5000, 'carte', '2024-11-27 14:30:14', 'requires_payment_method', 'pi_3QPlJqG8NKYv2Bjh0aFKIaoU'),
(10, 1, 1, 5000, 'carte', '2024-11-27 14:30:15', 'requires_payment_method', 'pi_3QPlJrG8NKYv2Bjh0bhGXruD'),
(11, 1, 1, 5000, 'carte', '2024-11-27 14:30:16', 'requires_payment_method', 'pi_3QPlJsG8NKYv2Bjh1DVavuT3'),
(12, 1, 1, 5000, 'carte', '2024-11-27 14:30:17', 'requires_payment_method', 'pi_3QPlJtG8NKYv2Bjh0xkuKYAd'),
(13, 1, 1, 5000, 'carte', '2024-11-27 14:30:18', 'requires_payment_method', 'pi_3QPlJuG8NKYv2Bjh14ZGYci0'),
(14, 1, 1, 5000, 'carte', '2024-11-27 14:30:19', 'requires_payment_method', 'pi_3QPlJvG8NKYv2Bjh1WBpIkGv'),
(15, 1, 1, 5000, 'carte', '2024-11-27 14:30:20', 'requires_payment_method', 'pi_3QPlJwG8NKYv2Bjh0tgWE8VK'),
(16, 1, 1, 5000, 'carte', '2024-11-27 14:30:21', 'requires_payment_method', 'pi_3QPlJxG8NKYv2Bjh0qO0nlLL'),
(17, 1, 1, 5000, 'carte', '2024-11-27 14:30:22', 'requires_payment_method', 'pi_3QPlJyG8NKYv2Bjh14QOp4ZM'),
(18, 1, 1, 5000, 'carte', '2024-11-27 14:30:23', 'requires_payment_method', 'pi_3QPlJzG8NKYv2Bjh0VquTmSh'),
(19, 1, 1, 5000, 'carte', '2024-11-27 14:30:24', 'requires_payment_method', 'pi_3QPlK0G8NKYv2Bjh0HATXzWk'),
(20, 1, 1, 5000, 'carte', '2024-11-27 14:30:25', 'requires_payment_method', 'pi_3QPlK1G8NKYv2Bjh0Y2ASLmQ'),
(21, 1, 1, 5000, 'carte', '2024-11-27 16:17:37', 'requires_payment_method', 'pi_3QPmzlG8NKYv2Bjh1mLpHxuF'),
(22, 1, 1, 5000, 'carte', '2024-11-27 16:17:38', 'requires_payment_method', 'pi_3QPmzmG8NKYv2Bjh1LOAuUVn'),
(23, 1, 1, 5000, 'carte', '2024-11-27 16:27:09', 'requires_payment_method', 'pi_3QPn8zG8NKYv2Bjh1XeoOiVX'),
(24, 1, 1, 5000, 'carte', '2024-11-27 16:29:11', 'requires_payment_method', 'pi_3QPnAxG8NKYv2Bjh1U93i5VG'),
(25, 1, 1, 5000, 'carte', '2024-12-10 14:00:29', 'requires_payment_method', 'pi_3QUT38G8NKYv2Bjh0TOREOd3'),
(26, 1, 1, 5000, 'carte', '2024-12-10 14:00:29', 'requires_payment_method', 'pi_3QUT3CG8NKYv2Bjh1WgRHSWn'),
(27, 1, 1, 5000, 'carte', '2024-12-10 14:00:30', 'requires_payment_method', 'pi_3QUT3CG8NKYv2Bjh06dHdgN1'),
(28, 1, 1, 5000, 'carte', '2025-02-16 14:59:28', 'requires_payment_method', 'pi_3Qt8NIG8NKYv2Bjh0SS47kkU'),
(29, 1, 1, 5000, 'carte', '2025-02-16 14:59:29', 'requires_payment_method', 'pi_3Qt8NRG8NKYv2Bjh1aloZr0J'),
(30, 1, 1, 5000, 'carte', '2025-02-16 14:59:30', 'requires_payment_method', 'pi_3Qt8NSG8NKYv2Bjh0SvFXDe5'),
(48, 1, 1, 5000, 'carte', '2025-03-11 10:14:57', 'requires_payment_method', 'pi_3R1OtpG8NKYv2Bjh1vOebtRf'),
(47, 1, 1, 5000, 'carte', '2025-03-11 10:14:44', 'requires_payment_method', 'pi_3R1OtcG8NKYv2Bjh1GoZxsz4'),
(46, 1, 1, 5000, 'carte', '2025-03-09 21:28:58', 'requires_payment_method', 'pi_3R0qT0G8NKYv2Bjh1CkhVE6y'),
(45, 1, 1, 5000, 'carte', '2025-03-09 19:40:06', 'requires_payment_method', 'pi_3R0oldG8NKYv2Bjh1sBNIyZM'),
(44, 1, 1, 5000, 'carte', '2025-03-09 17:54:37', 'requires_payment_method', 'pi_3R0n7ZG8NKYv2Bjh0JUNR5ar'),
(43, 1, 1, 5000, 'carte', '2025-03-09 17:54:34', 'requires_payment_method', 'pi_3R0n7VG8NKYv2Bjh0PbctRQp'),
(42, 1, 1, 5000, 'carte', '2025-03-05 16:42:09', 'requires_payment_method', 'pi_3QzK5FG8NKYv2Bjh1qpgLGVS'),
(41, 1, 1, 5000, 'carte', '2025-03-04 23:31:48', 'requires_payment_method', 'pi_3Qz407G8NKYv2Bjh18GnneZe'),
(40, 1, 1, 5000, 'carte', '2025-03-04 16:26:48', 'requires_payment_method', 'pi_3QyxMpG8NKYv2Bjh1rD8Sw5N'),
(49, 1, 1, 5000, 'carte', '2025-03-11 11:03:24', 'requires_payment_method', 'pi_3R1PejG8NKYv2Bjh0LmSi85F'),
(50, 1, 1, 5000, 'carte', '2025-03-11 11:05:30', 'requires_payment_method', 'pi_3R1PgkG8NKYv2Bjh0Vl737hf'),
(51, 1, 1, 5000, 'carte', '2025-03-11 11:19:40', 'requires_payment_method', 'pi_3R1PuTG8NKYv2Bjh0BH0sVMM'),
(52, 1, 1, 5000, 'carte', '2025-03-11 11:26:59', 'requires_payment_method', 'pi_3R1Q1YG8NKYv2Bjh0764QIGW'),
(53, 1, 1, 5000, 'carte', '2025-03-11 11:27:44', 'requires_payment_method', 'pi_3R1Q2HG8NKYv2Bjh10V6sZTt'),
(54, 1, 1, 5000, 'carte', '2025-03-11 11:27:59', 'requires_payment_method', 'pi_3R1Q2WG8NKYv2Bjh1M49LyHR'),
(55, 1, 1, 5000, 'carte', '2025-03-11 11:34:59', 'requires_payment_method', 'pi_3R1Q9IG8NKYv2Bjh0aIMpWkU'),
(56, 1, 1, 5000, 'carte', '2025-03-11 11:35:04', 'requires_payment_method', 'pi_3R1Q9NG8NKYv2Bjh1x6WJciO'),
(57, 1, 1, 5000, 'carte', '2025-03-11 11:35:25', 'requires_payment_method', 'pi_3R1Q9iG8NKYv2Bjh0AJTeQsb'),
(58, 1, 1, 5000, 'carte', '2025-03-11 11:36:18', 'requires_payment_method', 'pi_3R1QAYG8NKYv2Bjh0szYhFPx'),
(59, 1, 1, 5000, 'carte', '2025-03-11 11:37:33', 'requires_payment_method', 'pi_3R1QBlG8NKYv2Bjh0JPzsfaT'),
(60, 1, 1, 5000, 'carte', '2025-03-11 11:41:56', 'requires_payment_method', 'pi_3R1QG1G8NKYv2Bjh1oYoEt6x'),
(61, 1, 1, 5000, 'carte', '2025-03-11 11:41:57', 'requires_payment_method', 'pi_3R1QG1G8NKYv2Bjh1fPrBWBP'),
(62, 1, 1, 5000, 'carte', '2025-03-11 11:42:23', 'requires_payment_method', 'pi_3R1QGSG8NKYv2Bjh1z0XbGdE'),
(63, 1, 1, 5000, 'carte', '2025-03-11 11:52:59', 'requires_payment_method', 'pi_3R1QQiG8NKYv2Bjh0uHqEir7'),
(64, 1, 1, 5000, 'carte', '2025-03-11 11:54:21', 'requires_payment_method', 'pi_3R1QS2G8NKYv2Bjh1dQlWTxW'),
(65, 1, 1, 5000, 'carte', '2025-03-11 11:54:58', 'requires_payment_method', 'pi_3R1QSdG8NKYv2Bjh0KiWPHPB'),
(66, 1, 1, 5000, 'carte', '2025-03-11 11:59:10', 'requires_payment_method', 'pi_3R1QWhG8NKYv2Bjh0KQAAonw'),
(67, 1, 1, 5000, 'carte', '2025-03-11 13:49:27', 'requires_payment_method', 'pi_3R1SFPG8NKYv2Bjh1lAIylJS'),
(68, 1, 1, 5000, 'carte', '2025-03-11 22:04:29', 'requires_payment_method', 'pi_3R1ZyTG8NKYv2Bjh08klOxBI'),
(69, 1, 1, 5000, 'carte', '2025-03-15 18:11:06', 'requires_payment_method', 'pi_3R2yEmG8NKYv2Bjh0Jct0XJk'),
(70, 1, 1, 5000, 'carte', '2025-03-15 18:11:06', 'requires_payment_method', 'pi_3R2yEoG8NKYv2Bjh0MItk9Uy'),
(71, 1, 1, 5000, 'carte', '2025-04-13 21:24:09', 'requires_payment_method', 'pi_3RDW8QG8NKYv2Bjh0lnrLaPv'),
(72, 1, 1, 5000, 'carte', '2025-04-25 16:24:43', 'requires_payment_method', 'pi_3RHnBFG8NKYv2Bjh1KlqZZzZ'),
(73, 1, 1, 5000, 'carte', '2025-04-27 03:23:44', 'requires_payment_method', 'pi_3RIJwYG8NKYv2Bjh10qEagRq'),
(74, 1, 1, 5000, 'carte', '2025-06-21 01:48:00', 'requires_payment_method', 'pi_3RcEf4G8NKYv2Bjh0s0F0GKO'),
(75, 1, 1, 5000, 'carte', '2025-06-21 01:48:00', 'requires_payment_method', 'pi_3RcEf7G8NKYv2Bjh13oV899m'),
(76, 1, 1, 5000, 'carte', '2025-06-21 01:48:01', 'requires_payment_method', 'pi_3RcEf7G8NKYv2Bjh0ntjiJuB'),
(77, 1, 1, 5000, 'carte', '2025-06-21 01:48:36', 'requires_payment_method', 'pi_3RcEfhG8NKYv2Bjh0X5OfgtL');

-- --------------------------------------------------------

--
-- Structure de la table `produits`
--

DROP TABLE IF EXISTS `produits`;
CREATE TABLE IF NOT EXISTS `produits` (
  `id` int NOT NULL AUTO_INCREMENT,
  `nom` varchar(50) NOT NULL,
  `description` text,
  `prix` decimal(10,2) NOT NULL,
  `date_sortie` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `tendance` tinyint(1) DEFAULT '0',
  `type_produit` enum('Football','Running','Jordan','Lifestyle','Basketball','Marche à pied') NOT NULL DEFAULT 'Lifestyle',
  `genre` enum('Homme','Femme') NOT NULL DEFAULT 'Homme',
  `stock_total` int DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=95 DEFAULT CHARSET=utf8mb3;

--
-- Déchargement des données de la table `produits`
--

INSERT INTO `produits` (`id`, `nom`, `description`, `prix`, `date_sortie`, `tendance`, `type_produit`, `genre`, `stock_total`) VALUES
(1, 'Air Force 1', 'Confortable, résistante et intemporelle : elle n\'est pas numéro 1 pour rien. La conception culte des années 80, avec du cuir lisse et des détails originaux. Un plaisir sur le terrain comme au quotidien.', 119.99, '2024-10-09 07:45:18', 0, 'Lifestyle', 'Homme', 2213),
(2, 'Nike Air Max Dn', 'Découvre la technologie Air nouvelle génération. La Air Max Dn intègre l’unité Dynamic Air (composée de quatre cylindres) qui te propulse à chaque pas, pour une sensation de fluidité incroyable. Résultat ? Un look futuriste hyper confortable, à porter de jour comme de nuit. Et des sensations irréelles.', 169.99, '2024-10-09 07:45:18', 0, 'Lifestyle', 'Homme', 48),
(3, 'Nike Motiva GORE-TEX', 'Avec son empeigne imperméable en GORE-TEX et ses motifs réfléchissants, cette Nike Motiva est idéale par temps pluvieux. Épaisse couche de mousse. Technologie innovante Comfortgroove sur la semelle extérieure. C\'est notre niveau d\'amorti le plus élevé pour absorber les chocs. Et son balancier oversize confortable te propulse vers l\'avant.', 129.00, '2024-10-09 07:45:18', 0, 'Marche à pied', 'Homme', 0),
(4, 'Kobe 9 Protro Elite « Masterpiece »', 'La quête de perfection se joue dans les détails. Sortie pour la première fois en 2014, la Kobe 9 « Masterpiece » revient dans la forme de la Protro. Cette silhouette culte a été revisitée avec un amorti React et un nouveau motif d\'adhérence. Avec son Flyknit multicolore, cette chaussure star des parquets s\'impose aussi comme un modèle premium au quotidien.', 239.00, '2024-10-09 07:45:18', 0, 'Basketball', 'Homme', 25),
(6, 'Nike Alphafly 3', 'Une vitesse digne des marathons pour repousser toujours plus tes limites : découvre la Alphafly 3. Elle intègre trois technologies innovantes pour booster ton run. Les deux unités Air Zoom te poussent vers l\'avant. La plaque en fibre de carbone sur toute la longueur crée un effet de propulsion. Et avec la semelle intermédiaire en mousse ZoomX du talon à la pointe, tu restes au top de ta forme jusqu\'à la ligne d\'arrivée.', 319.99, '2025-02-18 10:43:40', 0, 'Running', 'Homme', 0),
(7, 'Nike Pegasus Premium', 'La Pegasus Premium est conçue pour offrir une réactivité maximale avec trois de nos technologies running les plus performantes : mousse ZoomX, unité Air Zoom sculptée et mousse ReactX. Cette Pegasus est la plus performante à ce jour, avec un retour d\'énergie élevé unique. Son empeigne plus légère que l\'air, réduit le poids et augmente la respirabilité pour que tu voles plus rapidement.', 209.99, '2025-02-18 10:54:53', 0, 'Running', 'Homme', 0),
(8, 'Nike Air Max Dn8', 'Plus d\'Air, moins de volume. La Dn8 reprend notre système Dynamic Air et le condense dans une chaussure basse et épurée. Avec ses huit tubes Air pressurisés, elle offre une sensation de réactivité à chaque pas. Vivez une expérience de mouvement incroyable.', 189.99, '2025-02-18 11:03:01', 0, 'Lifestyle', 'Homme', 10),
(9, 'Nike Zoom Vomero 5', 'Résistance, profondeur et style facile à porter : la Vomero 5 bouscule tous les codes. Son design alterne tissus, cuir synthétique et détails en plastique. Le résultat ? Un hommage à l\'esthétique des années 2000.', 159.99, '2025-02-18 12:42:33', 0, 'Lifestyle', 'Femme', 0),
(10, 'Nike United Mercurial Vapor 16 Elite', 'La Nike United Vapor 16 Elite révolutionne le monde du foot. Conçu pour les athlètes les plus rapides sur le terrain, comme Lauren James et Salma Paralluelo, ce nouveau modèle au coloris audacieux intègre un amorti Air Zoom dynamique et une plaque d\'adhérence agressive pour une vitesse révolutionnaire.', 279.99, '2025-02-18 12:44:14', 0, 'Football', 'Homme', 0),
(11, 'Nike Metcon 9', 'Quelle que soit ta raison de t\'entraîner, la Metcon 9 t\'aide à aller plus loin. Avec sa nouvelle plaque Hyperlift plus large et un renfort en caoutchouc pour monter à la corde, elle est encore meilleure que la Metcon 8. Validée par les plus célèbres athlètes du monde, idéale pour les fans de muscu, de cross-training et les personnes qui visent haut, c\'est la référence absolue pour les performances quotidiennes.', 139.99, '2023-06-14 22:00:00', 1, 'Running', 'Homme', 0),
(12, 'Nike Vomero 18', 'Amorti maximal. Découvre notre sneaker confortable pour tes runs quotidiens. Elle est plus souple et garantit un meilleur amorti avec une mousse ZoomX légère suprposée et une mousse ReactX réactive dans la semelle intermédiaire. De plus, le motif d\'adhérence repensé assure une transition fluide de la cheville aux orteils.', 149.99, '2023-03-09 23:00:00', 1, 'Running', 'Homme', 0),
(13, 'Air Jordan 1 Low SE', 'Cette nouvelle version de la AJ1 apporte une nouvelle dynamique à un coloris neutre culte. Le cuir lisse premium et l\'amorti Nike Air garantissent la qualité et le confort attendus d\'une Jordan.', 139.99, '2023-06-14 22:00:00', 1, 'Jordan', 'Homme', 34),
(14, 'Jordan Spizike Low', 'La Spizike reprend et combine des éléments de cinq Jordan cultes pour créer une sneaker iconique. Elle rend hommage à Spike Lee qui a fait entrer le basket à Hollywood par la grande porte. Le résultat ? Une sneaker stylée chargée d\'histoire. Que demander de plus ? Ça te tente ?', 169.99, '2023-03-09 23:00:00', 1, 'Jordan', 'Homme', 20),
(16, 'Nike Air Monarch IV', 'La Nike Air Monarch IV est la chaussure qu\'il te faut pour t\'entraîner. Son cuir résistant sur le dessus du pied crée un maintien parfait. Côté confort, la mousse légère s\'allie à un amorti Nike Air pour plus de confort à chaque foulée.', 74.99, '2023-08-24 22:00:00', 0, 'Marche à pied', 'Homme', 0),
(17, 'Nike Phantom GX 2 Elite « Erling Haaland »', 'Impossible d\'éviter Erling Haaland. Sa force surnaturelle sur le terrain en fait un joueur inarrêtable quand il fonce en direction du but. C\'est pour cette raison qu\'il lui faut la Phantom GX 2 Elite. Avec sa technologie Gripknit adhérente, cette chaussure est là pour aller droit au but, avec précision. Avec des motifs inspirés de phénomènes naturels comme les supernovas et les météorites, cette version spéciale illustre la force imprévisible d\'Erling sur le terrain.', 269.99, '2023-08-24 22:00:00', 0, 'Football', 'Homme', 9),
(18, 'Air Jordan 1 Mid', 'Inspiré de la AJ1 originale, ce modèle mi-montant conserve le look emblématique que tu aimes tant, tandis que le choix des couleurs et le cuir impeccable lui confèrent une identité unique.', 139.99, '2023-08-24 22:00:00', 0, 'Jordan', 'Homme', 100),
(19, 'Jumpman MVP', 'Pour cette version remixée, on a eu l\'embarras du choix. On a repris des éléments des AJ6, 7 et 8 pour créer une toute nouvelle chaussure qui rend hommage aux trois titres consécutifs de Michael Jordan. Avec leurs détails en cuir, tissu et nubuck, ces sneakers rendent hommage à un héritage et t\'invitent à tracer ta propre légende.', 169.99, '2023-08-24 22:00:00', 0, 'Jordan', 'Homme', 0),
(20, 'Jordan Heir Series « Paolo »', 'Quand Paolo entre sur le terrain avec ses légendaires baskets bleues, le match est terminé. Capte la même énergie avec ce coloris de la série Jordan Heir inspiré de ses débuts dans le basket et de sa première équipe. Elle regorge de technologies près du sol. Tu trouveras une semelle intermédiaire étudiée pour plus de mobilité, une cage intégrée pour plus de stabilité et des motifs à chevrons pour plus d\'adhérence. Avec elle, la victoire t\'appartient.', 109.99, '2023-08-24 22:00:00', 0, 'Jordan', 'Homme', 9),
(21, 'Luka 3 « Speedway »', 'Quoi de plus classique qu\'un coloris noir et blanc ? Avec sa mousse légère et réactive et sa technologie prête pour le terrain, la Luka 3 décuple tes performances : tes adversaires ne verront rien venir. Pas besoin de grands discours, ces sneakers ont tout dit.', 129.99, '2023-08-24 22:00:00', 0, 'Jordan', 'Homme', 0),
(22, 'LeBron NXXT Genisus', 'Mesh léger offrant un bon maintien et amorti Air Zoom souple pour une vitesse exceptionnelle tout au long du match : la LeBron NXXT Genisus a tout ce dont tu as besoin pour révolutionner le monde du basket.', 149.99, '2023-08-24 22:00:00', 0, 'Basketball', 'Homme', 0),
(23, 'Nike Vomero 18', 'Amorti maximal. Découvre notre sneaker confortable pour tes runs quotidiens. Elle est plus souple et garantit un meilleur amorti avec une mousse ZoomX légère suprposée et une mousse ReactX réactive dans la semelle intermédiaire. De plus, le motif d\'adhérence repensé assure une transition fluide du talon à la pointe.', 149.99, '2023-08-24 22:00:00', 0, 'Lifestyle', 'Femme', 11),
(24, 'Nike Rival Multi', 'Avec son amorti supplémentaire au talon et son empeigne légère et résistante, la Zoom Rival t\'accompagne pendant tes longues sessions d\'entraînement de la saison et t\'aide à te démarquer les jours de compétition. Tu ne sais toujours pas quelles épreuves te conviennent le mieux ? Cette chaussure à pointes polyvalente est conçue pour les sprints, les courses d\'obstacles, les sauts et le saut à la perche. Serre tes lacets et vise l\'or.', 84.99, '2023-08-24 22:00:00', 0, 'Running', 'Femme', 0),
(78, 'Barkev Shoes', 'C les chaussures de barkev apartian', 9999.00, '2025-04-25 13:20:49', 0, 'Lifestyle', 'Homme', 24),
(79, 'Nike Shox TL', 'La Nike Shox TL repousse les limites de l\'amorti mécanique. Cette version revisitée du modèle iconique de 2003 intègre du mesh respirant sur l\'empeigne et la technologie Nike Shox sur toute la longueur. Résultat : une absorption optimale des chocs et un look sublimé.', 169.99, '2025-04-26 20:53:03', 0, 'Lifestyle', 'Homme', 52),
(80, 'Nike Air Max 270', 'La première Air Max lifestyle de Nike vous offre style et confort à travers la Nike Air Max 270. Ce modèle s\'inspire des chaussures Air Max emblématiques en intégrant les meilleures innovations de Nike, avec sa grande fenêtre et une nouvelle gamme de couleurs.', 159.99, '2025-04-26 22:43:33', 0, 'Lifestyle', 'Homme', 0),
(81, 'Nike Air Max 90', 'Il n\'existe rien d’aussi aérien, confortable et inégalé. La Nike Air Max 90 reste fidèle au modèle de running d\'origine, avec sa semelle à motif gaufré emblématique, ses renforts cousus et ses détails classiques en TPU. Ses couleurs offrent un look plein de fraîcheur, tandis que l\'amorti Max Air assure un confort optimal pendant vos runs.', 149.99, '2025-04-27 00:25:40', 0, 'Lifestyle', 'Homme', 0),
(93, 'jnr', 'jnr', 15.00, '2025-06-21 00:40:51', 0, 'Lifestyle', 'Homme', 0),
(94, 'puff', '8k', 10.00, '2025-06-21 00:55:50', 0, 'Lifestyle', 'Homme', 0);

-- --------------------------------------------------------

--
-- Structure de la table `produit_images`
--

DROP TABLE IF EXISTS `produit_images`;
CREATE TABLE IF NOT EXISTS `produit_images` (
  `id` int NOT NULL AUTO_INCREMENT,
  `variante_id` int NOT NULL,
  `url_image` varchar(255) NOT NULL,
  `is_main` tinyint(1) DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `variante_id` (`variante_id`)
) ENGINE=MyISAM AUTO_INCREMENT=116 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Déchargement des données de la table `produit_images`
--

INSERT INTO `produit_images` (`id`, `variante_id`, `url_image`, `is_main`) VALUES
(1, 1, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/b7d9211c-26e7-431a-ac24-b0540fb3c00f/AIR+FORCE+1+%2707.png', 1),
(2, 1, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/33533fe2-1157-4001-896e-1803b30659c8/AIR+FORCE+1+%2707.png', 0),
(3, 1, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/82aa97ed-98bf-4b6f-9d0b-31a9f907077b/AIR+FORCE+1+%2707.png', 0),
(4, 2, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/fc4622c4-2769-4665-aa6e-42c974a7705e/AIR+FORCE+1+%2707.png', 1),
(5, 2, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/7cd0845e-4eba-4ccf-8237-bc47f1e31f3e/AIR+FORCE+1+%2707.png', 0),
(6, 2, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/900c2ac8-8a3e-45f7-aac7-c92ccce8505a/AIR+FORCE+1+%2707.png', 0),
(7, 3, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/5bc5bc81-9e76-444f-93ce-c7e1446f17af/AIR+MAX+DN.png', 1),
(8, 3, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/216c0de9-45dd-45b5-ae3d-4ab9d6066401/AIR+MAX+DN.png', 0),
(9, 3, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/6e292019-1a9f-4ba5-9b3e-7d1fddf0f7dd/AIR+MAX+DN.png', 0),
(10, 15, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/0083d873-114c-4a76-9dd3-82aa3c1b168f/AIR+MAX+DN.png', 1),
(11, 15, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/7e60be0b-786b-4b47-a60c-08b48117238c/AIR+MAX+DN.png', 0),
(12, 15, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/5abb32bd-b6ad-4b4f-824e-21a7b29bea02/AIR+MAX+DN.png', 0),
(13, 4, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/0556d65a-587e-4083-b05a-d12a33194778/WMNS+NIKE+MOTIVA+GTX.png', 1),
(14, 4, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/4b3fece4-5208-4873-8a43-3130ed957c93/WMNS+NIKE+MOTIVA+GTX.png', 0),
(15, 4, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/57ece726-5d12-42c6-a0e4-f2a8430f9a56/WMNS+NIKE+MOTIVA+GTX.png', 0),
(16, 5, 'https://static.nike.com/a/images/w_1280,q_auto,f_auto/e3794a58-1583-44c0-be6c-c352df69c0d2/date-de-sortie-de-la-kobe%C2%A09-elite-protro-%C2%AB%C2%A0masterpiece%C2%A0%C2%BB-%C2%AB%C2%A0black-and-metallic-silver%C2%A0%C2%BB-fz7335-001.jpg', 1),
(17, 5, 'https://static.nike.com/a/images/w_1280,q_auto,f_auto/9739923e-e954-42d6-8c95-5d2839993b3d/date-de-sortie-de-la-kobe%C2%A09-elite-protro-%C2%AB%C2%A0masterpiece%C2%A0%C2%BB-%C2%AB%C2%A0black-and-metallic-silver%C2%A0%C2%BB-fz7335-001.jpg', 0),
(18, 5, 'https://static.nike.com/a/images/w_1280,q_auto,f_auto/fbe4cd40-eb29-4d31-abc3-6329ecdb68be/date-de-sortie-de-la-kobe%C2%A09-elite-protro-%C2%AB%C2%A0masterpiece%C2%A0%C2%BB-%C2%AB%C2%A0black-and-metallic-silver%C2%A0%C2%BB-fz7335-001.jpg', 0),
(19, 6, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/f8e7b14b-5d9b-4488-ac9a-d3cdba36118c/AIR+ZOOM+ALPHAFLY+NEXT%25+3.png', 1),
(20, 6, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/2f588f99-1286-4540-a49c-ee8fb94cea9d/AIR+ZOOM+ALPHAFLY+NEXT%25+3.png', 0),
(21, 6, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/9909f279-ea27-4430-aca7-d5e057d423d9/AIR+ZOOM+ALPHAFLY+NEXT%25+3.png', 0),
(22, 7, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/e69e78d9-f9bc-41dd-a436-e4a89f8f0d0b/NIKE+AIR+MAX+1+%2786+PRM.png', 1),
(23, 7, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/caaa5920-f0b1-42f1-b0f9-7425c256c9c6/NIKE+AIR+MAX+1+%2786+PRM.png', 0),
(24, 7, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/1446a199-bf3e-4bfa-8021-8a5c9e6bc27a/NIKE+AIR+MAX+1+%2786+PRM.png', 0),
(25, 8, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/b4cdd269-25d6-4fdd-aaaf-a13b65a76c71/AIR+MAX+DN8.png', 1),
(26, 8, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/70466fe0-c158-4103-bf4a-041c5d464604/AIR+MAX+DN8.png', 0),
(27, 8, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/62e4b234-4b7e-4a8f-b00b-1c79159a4bab/AIR+MAX+DN8.png', 0),
(28, 9, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/5d9bfeb8-75af-41bf-a4bd-7788d84d6a9f/NIKE+ZOOM+VOMERO+5.png', 1),
(29, 9, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/e1efdce1-0012-4adf-bb32-4850ef34f2e6/NIKE+ZOOM+VOMERO+5.png', 0),
(30, 9, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/ea8fa5c6-1d0f-4fe4-a270-36040c2f5e96/NIKE+ZOOM+VOMERO+5.png', 0),
(31, 10, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/edebecd5-2575-49df-b73e-ee11d9bb8107/ZM+VAPOR+16+ELITE+FG+NU1.png', 1),
(32, 10, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/42074612-3bf2-4631-b7a1-9db6aa1e2454/ZM+VAPOR+16+ELITE+FG+NU1.png', 0),
(33, 10, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/148a92fc-b7bd-44f5-85fa-b8460a0777a0/ZM+VAPOR+16+ELITE+FG+NU1.png', 0),
(34, 11, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/2a14bf14-2d2c-49a3-b168-2a8e150acb4c/NIKE+METCON+9.png', 1),
(35, 11, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/80e79f89-b583-4e8d-bfaa-5d2ecb28a4be/NIKE+METCON+9.png', 0),
(36, 11, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/e98dd904-3912-4c3b-a6f9-9d88f3d9036b/NIKE+METCON+9.png', 0),
(37, 12, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/1983f2e1-0271-479c-bada-6176a571fa4f/NIKE+VOMERO+18.png', 1),
(38, 12, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/884352f9-97a7-4367-9cb4-cf10d77561c7/NIKE+VOMERO+18.png', 0),
(39, 12, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/884352f9-97a7-4367-9cb4-cf10d77561c7/NIKE+VOMERO+18.png', 0),
(40, 13, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco,u_126ab356-44d8-4a06-89b4-fcdcc8df0245,c_scale,fl_relative,w_1.0,h_1.0,fl_layer_apply/a18da738-1447-4320-b209-1cef25d9e8a7/AIR+JORDAN+1+LOW+SE.png', 1),
(41, 13, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco,u_126ab356-44d8-4a06-89b4-fcdcc8df0245,c_scale,fl_relative,w_1.0,h_1.0,fl_layer_apply/b7965df2-a566-416f-ba7d-058813e80b3a/AIR+JORDAN+1+LOW+SE.png', 0),
(42, 13, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco,u_126ab356-44d8-4a06-89b4-fcdcc8df0245,c_scale,fl_relative,w_1.0,h_1.0,fl_layer_apply/4a4cda02-047e-4cc0-afcf-3777c58b89eb/AIR+JORDAN+1+LOW+SE.png', 0),
(43, 14, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco,u_126ab356-44d8-4a06-89b4-fcdcc8df0245,c_scale,fl_relative,w_1.0,h_1.0,fl_layer_apply/f014625e-1a1d-4944-8395-46a07841a794/JORDAN+SPIZIKE+LOW.png', 1),
(44, 14, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco,u_126ab356-44d8-4a06-89b4-fcdcc8df0245,c_scale,fl_relative,w_1.0,h_1.0,fl_layer_apply/85479a61-a6ce-46b7-a53a-c958b936c7b8/JORDAN+SPIZIKE+LOW.png', 0),
(45, 14, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco,u_126ab356-44d8-4a06-89b4-fcdcc8df0245,c_scale,fl_relative,w_1.0,h_1.0,fl_layer_apply/3bc93a5f-e131-47f4-b722-8808f54c0087/JORDAN+SPIZIKE+LOW.png', 0),
(46, 16, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/f9a8deaa-87f2-4191-92b8-c7661aae48de/AIR+MONARCH+IV.png', 1),
(47, 16, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/3d9f59cf-cf57-4d60-8873-51889795f70e/AIR+MONARCH+IV.png', 0),
(48, 16, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/a3ce45dc-21a3-4b34-ad29-cc09ec142136/AIR+MONARCH+IV.png', 0),
(49, 17, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/e0c25dae-4835-4f9a-88f8-042761fe8ebe/PHANTOM+GX+II+ELITE+FG+EH.png', 1),
(50, 17, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/be43df0b-3474-4e1b-bcc5-a43e5eb3bdc2/PHANTOM+GX+II+ELITE+FG+EH.png', 0),
(51, 17, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/f9478fc0-47c6-4e3e-afa5-6a919f69cbc1/PHANTOM+GX+II+ELITE+FG+EH.png', 0),
(52, 18, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco,u_126ab356-44d8-4a06-89b4-fcdcc8df0245,c_scale,fl_relative,w_1.0,h_1.0,fl_layer_apply/ad4f9c72-6b83-461a-9a01-806bb0bfce95/AIR+JORDAN+1+MID.png', 1),
(53, 18, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco,u_126ab356-44d8-4a06-89b4-fcdcc8df0245,c_scale,fl_relative,w_1.0,h_1.0,fl_layer_apply/b860a316-22ad-47d4-84fd-d858278741ff/AIR+JORDAN+1+MID.png', 0),
(54, 18, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco,u_126ab356-44d8-4a06-89b4-fcdcc8df0245,c_scale,fl_relative,w_1.0,h_1.0,fl_layer_apply/c36135b0-8383-439f-a099-99f8188d8e45/AIR+JORDAN+1+MID.png', 0),
(55, 19, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco,u_126ab356-44d8-4a06-89b4-fcdcc8df0245,c_scale,fl_relative,w_1.0,h_1.0,fl_layer_apply/1f4b2c79-d877-4114-bfa5-2ebb0d804f8e/JORDAN+MVP.png', 1),
(56, 19, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco,u_126ab356-44d8-4a06-89b4-fcdcc8df0245,c_scale,fl_relative,w_1.0,h_1.0,fl_layer_apply/2065f898-87d9-4730-8d0f-7aee2c35e369/JORDAN+MVP.png', 0),
(57, 19, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco,u_126ab356-44d8-4a06-89b4-fcdcc8df0245,c_scale,fl_relative,w_1.0,h_1.0,fl_layer_apply/925f6435-f8b5-48ea-9e5e-3b7764b98eca/JORDAN+MVP.png', 0),
(58, 20, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco,u_126ab356-44d8-4a06-89b4-fcdcc8df0245,c_scale,fl_relative,w_1.0,h_1.0,fl_layer_apply/da5d1d87-7e89-4e95-aa79-24910ca914c4/JORDAN+HEIR.png', 1),
(59, 20, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco,u_126ab356-44d8-4a06-89b4-fcdcc8df0245,c_scale,fl_relative,w_1.0,h_1.0,fl_layer_apply/a4587ee5-91c0-4038-b296-daa0332bab6f/JORDAN+HEIR.png', 0),
(60, 20, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco,u_126ab356-44d8-4a06-89b4-fcdcc8df0245,c_scale,fl_relative,w_1.0,h_1.0,fl_layer_apply/281a6ebc-d5f5-4b2f-87ad-6ae3972d41b0/JORDAN+HEIR.png', 0),
(61, 21, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco,u_126ab356-44d8-4a06-89b4-fcdcc8df0245,c_scale,fl_relative,w_1.0,h_1.0,fl_layer_apply/2fd71a36-68fc-4d19-9fa4-2ed034d4e747/JORDAN+LUKA+3.png', 1),
(62, 21, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco,u_126ab356-44d8-4a06-89b4-fcdcc8df0245,c_scale,fl_relative,w_1.0,h_1.0,fl_layer_apply/933f7dff-27c3-477a-9af6-2de1c3ed9fcc/JORDAN+LUKA+3.png', 0),
(63, 21, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco,u_126ab356-44d8-4a06-89b4-fcdcc8df0245,c_scale,fl_relative,w_1.0,h_1.0,fl_layer_apply/7a3aaafb-9ad8-4c84-b20e-7eba5d7c85b9/JORDAN+LUKA+3.png', 0),
(64, 22, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/d4cf7e51-d6ef-4545-bbb6-92038987cc4f/LBJ+NXXT+GENISUS.png', 1),
(65, 22, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/8779d64c-39eb-4592-aebd-10abd7def621/LBJ+NXXT+GENISUS.png', 0),
(66, 22, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/0878a12f-3f06-40ce-a56e-07fc6124df66/LBJ+NXXT+GENISUS.png', 0),
(67, 23, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/1983f2e1-0271-479c-bada-6176a571fa4f/NIKE+VOMERO+18.png', 1),
(68, 23, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/31f2f33c-5cb2-4799-88d4-08823b6596bc/NIKE+VOMERO+18.png', 0),
(69, 23, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/884352f9-97a7-4367-9cb4-cf10d77561c7/NIKE+VOMERO+18.png', 0),
(70, 24, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/1b43762b-01d6-4efb-85dd-99a1c11d8c2f/NIKE+ZOOM+RIVAL+MULTI.png', 1),
(71, 24, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/126a7efc-8a17-40bc-9ccc-9ce34c643c75/NIKE+ZOOM+RIVAL+MULTI.png', 0),
(72, 24, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/20612ba7-a77d-4515-8e77-f5838ba30a98/NIKE+ZOOM+RIVAL+MULTI.png', 0),
(73, 2, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/364d6a11-fad9-4a21-a763-59e458e41463/AIR+FORCE+1+%2707.png', 0),
(74, 2, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/f430f78b-6107-4c30-a4b2-f6df4fd228be/AIR+FORCE+1+%2707.png', 0),
(76, 2, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/89a2cdd6-992b-4224-aab3-e49681f01e00/AIR+FORCE+1+%2707.png', 0),
(75, 2, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/ddec7bcc-6100-4d27-ac7c-d15771e1780e/AIR+FORCE+1+%2707.png', 0),
(83, 16, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/2013aa36-3cf6-46a0-ad9a-cd5ff7927a11/AIR+MONARCH+IV.png', 0),
(82, 16, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/bc3d0920-1f5a-4d02-8c6e-95139818f5b7/AIR+MONARCH+IV.png', 0),
(84, 16, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/072a26c8-39e8-43d9-a9ef-d4b59fd27495/AIR+MONARCH+IV.png', 0),
(85, 16, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/eda69520-2307-4daa-a9df-759c0461bb3d/AIR+MONARCH+IV.png', 0),
(86, 16, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/05c4d7ab-961f-4047-9557-ad19db51b6f6/AIR+MONARCH+IV.png', 0),
(87, 46, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco,u_126ab356-44d8-4a06-89b4-fcdcc8df0245,c_scale,fl_relative,w_1.0,h_1.0,fl_layer_apply/80675a3a-d5d3-4543-9635-eb6243c31e94/AIR+JORDAN+1+LOW.png', 1),
(88, 46, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco,u_126ab356-44d8-4a06-89b4-fcdcc8df0245,c_scale,fl_relative,w_1.0,h_1.0,fl_layer_apply/ca8b917a-540f-45f2-b97b-a11d946569d8/AIR+JORDAN+1+LOW.png', 0),
(89, 47, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/tjkr8ecmktw7qooy9d0h/NIKE+SHOX+TL.png', 1),
(90, 47, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/u1ekclgdmmgtvfvybxd1/NIKE+SHOX+TL.png', 0),
(91, 48, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/ggph3qhx524pkw9jqfek/NIKE+SHOX+TL.png', 1),
(92, 48, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/ycmgssxdcamldzlwngig/NIKE+SHOX+TL.png', 0),
(93, 48, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/p0ymekm58ut9atnpd32o/NIKE+SHOX+TL.png', 0),
(94, 49, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/awjogtdnqxniqqk0wpgf/AIR+MAX+270.png', 1),
(95, 49, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/zohr1uagxkvngypyrsg6/AIR+MAX+270.png', 0),
(96, 50, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/gorfwjchoasrrzr1fggt/AIR+MAX+270.png', 1),
(97, 50, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/dl2krirthxihbhgtkdv5/AIR+MAX+270.png', 0),
(98, 51, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/zwxes8uud05rkuei1mpt/AIR+MAX+90.png', 1),
(99, 51, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/q8og2dec1ethdbcehbdu/AIR+MAX+90.png', 0),
(100, 52, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/m55is6buar3k4isirw0k/AIR+MAX+90.png', 1),
(101, 52, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/gjgi7wknzuckkcd4ar9p/AIR+MAX+90.png', 0),
(115, 66, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/3f63d83c-5d3e-4e89-8cfc-0f9b04c57cec/NIKE+VICTORI+ONE+SLIDE.png', 1),
(114, 66, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/2753a772-37df-496a-95e2-2fad5c2b31b8/NIKE+VICTORI+ONE+SLIDE.png', 0),
(113, 65, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/fa2e7371-d725-4414-a58f-c76d16494449/NIKE+VICTORI+ONE+SLIDE.png', 1),
(112, 64, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/4c4097c1-b584-41c1-ae2c-c3da0db3d69c/T90+SP.png', 0),
(111, 64, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/cef63d35-7221-4309-a027-bbe0f82bebc3/T90+SP.png', 1),
(110, 63, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/9daf942a-55e1-40fb-9f42-1ad7ca3a7664/T90.png', 1),
(109, 63, 'https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/b141ed29-4f80-4e55-a7a6-cc01e16ae0b6/T90.png', 0);

-- --------------------------------------------------------

--
-- Structure de la table `produit_tailles`
--

DROP TABLE IF EXISTS `produit_tailles`;
CREATE TABLE IF NOT EXISTS `produit_tailles` (
  `id` int NOT NULL AUTO_INCREMENT,
  `variante_id` int NOT NULL,
  `taille` int DEFAULT NULL,
  `stock` int DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `variante_id` (`variante_id`)
) ENGINE=InnoDB AUTO_INCREMENT=170 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Déchargement des données de la table `produit_tailles`
--

INSERT INTO `produit_tailles` (`id`, `variante_id`, `taille`, `stock`) VALUES
(1, 1, 40, 3),
(2, 1, 42, 20),
(3, 1, 44, 7),
(4, 2, 40, 100),
(5, 2, 42, 69),
(6, 2, 44, 8),
(7, 3, 40, 10),
(8, 3, 42, 5),
(9, 3, 44, 8),
(10, 15, 41, 11),
(11, 15, 43, 6),
(12, 15, 45, 8),
(13, 1, 38, 6),
(14, 13, 44, 34),
(15, 18, 35, 10),
(16, 13, 45, 5),
(17, 2, 47, 2000),
(18, 14, 42, 10),
(19, 14, 46, 10),
(20, 23, 42, 11),
(21, 18, 42, 80),
(22, 20, 39, 9),
(23, 18, 43, 10),
(24, 17, 43, 9),
(25, 46, 42, 9),
(26, 46, 44, 15),
(27, 47, 39, 0),
(28, 47, 40, 2),
(29, 47, 41, 0),
(30, 47, 42, 0),
(31, 47, 43, 0),
(32, 47, 44, 0),
(33, 47, 45, 0),
(34, 48, 39, 0),
(35, 48, 40, 50),
(36, 48, 41, 0),
(37, 48, 42, 0),
(38, 48, 43, 0),
(39, 48, 44, 0),
(40, 48, 45, 0),
(41, 49, 39, 0),
(42, 49, 40, 0),
(43, 49, 41, 0),
(44, 49, 42, 0),
(45, 49, 43, 0),
(46, 49, 44, 0),
(47, 49, 45, 0),
(48, 50, 39, 0),
(49, 50, 40, 0),
(50, 50, 41, 0),
(51, 50, 42, 0),
(52, 50, 43, 0),
(53, 50, 44, 0),
(54, 50, 45, 0),
(55, 51, 39, 0),
(56, 51, 40, 0),
(57, 51, 41, 0),
(58, 51, 42, 0),
(59, 51, 43, 0),
(60, 51, 44, 0),
(61, 51, 45, 0),
(62, 52, 39, 0),
(63, 52, 40, 0),
(64, 52, 41, 0),
(65, 52, 42, 0),
(66, 52, 43, 0),
(67, 52, 44, 0),
(68, 52, 45, 0),
(69, 8, 42, 10),
(70, 5, 43, 15),
(71, 5, 40, 10),
(142, 63, 39, 0),
(143, 63, 40, 0),
(144, 63, 41, 0),
(145, 63, 42, 0),
(146, 63, 43, 0),
(147, 63, 44, 0),
(148, 63, 45, 0),
(149, 64, 39, 0),
(150, 64, 40, 0),
(151, 64, 41, 0),
(152, 64, 42, 0),
(153, 64, 43, 0),
(154, 64, 44, 0),
(155, 64, 45, 0),
(156, 65, 39, 0),
(157, 65, 40, 0),
(158, 65, 41, 0),
(159, 65, 42, 0),
(160, 65, 43, 0),
(161, 65, 44, 0),
(162, 65, 45, 0),
(163, 66, 39, 0),
(164, 66, 40, 0),
(165, 66, 41, 0),
(166, 66, 42, 0),
(167, 66, 43, 0),
(168, 66, 44, 0),
(169, 66, 45, 0);

--
-- Déclencheurs `produit_tailles`
--
DROP TRIGGER IF EXISTS `update_variante_stock`;
DELIMITER $$
CREATE TRIGGER `update_variante_stock` AFTER UPDATE ON `produit_tailles` FOR EACH ROW BEGIN
    UPDATE produit_variantes
    SET stock = (
        SELECT SUM(stock) FROM produit_tailles WHERE variante_id = NEW.variante_id
    )
    WHERE id = NEW.variante_id;
END
$$
DELIMITER ;
DROP TRIGGER IF EXISTS `update_variante_stock_after_insert`;
DELIMITER $$
CREATE TRIGGER `update_variante_stock_after_insert` AFTER INSERT ON `produit_tailles` FOR EACH ROW BEGIN
    UPDATE produit_variantes
    SET stock = (
        SELECT SUM(stock) FROM produit_tailles WHERE variante_id = NEW.variante_id
    )
    WHERE id = NEW.variante_id;
END
$$
DELIMITER ;

-- --------------------------------------------------------

--
-- Structure de la table `produit_variantes`
--

DROP TABLE IF EXISTS `produit_variantes`;
CREATE TABLE IF NOT EXISTS `produit_variantes` (
  `id` int NOT NULL AUTO_INCREMENT,
  `produit_id` int NOT NULL,
  `couleur` varchar(50) NOT NULL,
  `stock` int DEFAULT '0',
  `is_main` tinyint(1) DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `produit_id` (`produit_id`)
) ENGINE=InnoDB AUTO_INCREMENT=67 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Déchargement des données de la table `produit_variantes`
--

INSERT INTO `produit_variantes` (`id`, `produit_id`, `couleur`, `stock`, `is_main`) VALUES
(1, 1, 'Blanc', 36, 1),
(2, 1, 'Noir', 2177, 0),
(3, 2, 'Blanc', 23, 1),
(4, 3, 'Standard', 0, 1),
(5, 4, 'Standard', 25, 1),
(6, 6, 'Standard', 0, 1),
(7, 7, 'Standard', 0, 1),
(8, 8, 'Standard', 10, 1),
(9, 9, 'Standard', 0, 1),
(10, 10, 'Standard', 0, 1),
(11, 11, 'Standard', 0, 1),
(12, 12, 'Standard', 0, 1),
(13, 13, 'Standard', 34, 1),
(14, 14, 'Standard', 20, 1),
(15, 2, 'Noir', 25, 0),
(16, 16, 'Standard', 0, 1),
(17, 17, 'Standard', 9, 1),
(18, 18, 'Standard', 100, 1),
(19, 19, 'Standard', 0, 1),
(20, 20, 'Standard', 9, 1),
(21, 21, 'Standard', 0, 1),
(22, 22, 'Standard', 0, 1),
(23, 23, 'Standard', 11, 1),
(24, 24, 'Standard', 0, 1),
(45, 24, 'Standard', 0, 0),
(46, 78, 'Standard', 24, 1),
(47, 79, 'Noir', 2, 1),
(48, 79, 'Blanc', 50, 0),
(49, 80, 'Blanc', 0, 1),
(50, 80, 'Noir', 0, 0),
(51, 81, 'Blanc', 0, 1),
(52, 81, 'Noir', 0, 0),
(63, 93, 'Gris', 0, 1),
(64, 93, 'Rose', 0, 0),
(65, 94, 'Bleu', 0, 1),
(66, 94, 'Rose', 0, 0);

--
-- Déclencheurs `produit_variantes`
--
DROP TRIGGER IF EXISTS `update_stock_total_after_insert`;
DELIMITER $$
CREATE TRIGGER `update_stock_total_after_insert` AFTER INSERT ON `produit_variantes` FOR EACH ROW BEGIN
    UPDATE produits 
    SET stock_total = (
        SELECT SUM(stock) 
        FROM produit_variantes 
        WHERE produit_id = NEW.produit_id
    ) 
    WHERE id = NEW.produit_id;
END
$$
DELIMITER ;
DROP TRIGGER IF EXISTS `update_stock_total_after_update`;
DELIMITER $$
CREATE TRIGGER `update_stock_total_after_update` AFTER UPDATE ON `produit_variantes` FOR EACH ROW BEGIN
    UPDATE produits 
    SET stock_total = (
        SELECT SUM(stock) 
        FROM produit_variantes 
        WHERE produit_id = NEW.produit_id
    ) 
    WHERE id = NEW.produit_id;
END
$$
DELIMITER ;

-- --------------------------------------------------------

--
-- Structure de la table `role_change_logs`
--

DROP TABLE IF EXISTS `role_change_logs`;
CREATE TABLE IF NOT EXISTS `role_change_logs` (
  `log_id` int NOT NULL AUTO_INCREMENT,
  `admin_username` varchar(50) NOT NULL COMMENT 'Identifiant de l''administrateur qui a effectué la modification',
  `user_modified` varchar(50) NOT NULL COMMENT 'Identifiant de l''utilisateur modifié',
  `new_role` varchar(20) NOT NULL COMMENT 'Nouveau rôle attribué',
  `change_date` datetime NOT NULL COMMENT 'Date et heure de la modification',
  PRIMARY KEY (`log_id`)
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Journal des modifications de rôle';

--
-- Déchargement des données de la table `role_change_logs`
--

INSERT INTO `role_change_logs` (`log_id`, `admin_username`, `user_modified`, `new_role`, `change_date`) VALUES
(4, '34', 'Bernard', 'employee', '2025-04-06 16:39:01'),
(5, '34', 'aa', 'employee', '2025-04-06 16:53:21'),
(6, '236', 'Bernard', 'client', '2025-04-06 16:55:13'),
(7, '34', 'aa', 'client', '2025-04-06 17:03:12'),
(8, '34', 'aa', 'employee', '2025-04-06 17:11:10'),
(9, '34', 'Dupont', 'employee', '2025-04-06 17:12:22'),
(10, '34', 'Nike', 'admin', '2025-04-06 18:49:04'),
(11, '34', 'Nike', 'client', '2025-04-06 18:49:37'),
(12, '34', 'salim', 'client', '2025-05-14 17:28:38'),
(13, '34', 'salim', 'admin', '2025-05-14 17:28:47'),
(14, '34', 'Vatche', 'employee', '2025-05-14 17:35:31'),
(15, '34', 'Vatche', 'admin', '2025-05-14 17:35:37');

-- --------------------------------------------------------

--
-- Structure de la table `users`
--

DROP TABLE IF EXISTS `users`;
CREATE TABLE IF NOT EXISTS `users` (
  `id_client` int NOT NULL AUTO_INCREMENT,
  `identifiant` varchar(50) NOT NULL,
  `email` varchar(50) NOT NULL,
  `mdp` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `role` enum('client','admin','employee') NOT NULL DEFAULT 'client',
  `created at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `secret` varchar(32) NOT NULL,
  PRIMARY KEY (`id_client`)
) ENGINE=InnoDB AUTO_INCREMENT=238 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Déchargement des données de la table `users`
--

INSERT INTO `users` (`id_client`, `identifiant`, `email`, `mdp`, `role`, `created at`, `secret`) VALUES
(7, 'kevin', 'kevin@kevina.fr', '$2y$10$Ifj05yJrAAi6cAnuTGUacuqiGpg7ZpJJU3x0XH4xkvGVxt63H0V5.', 'client', '2024-10-08 09:43:16', ''),
(8, 'admin', 'admin@admin.com', '$2y$10$dtDVk3jjewyJwUa5dZxgxORnd6iKBysytLHP5p9Ga1RUuRs.j8XBG', 'admin', '2024-10-08 09:47:15', ''),
(11, 'Manoukian', 'mnk@mnk.fr', '$2y$10$pkM299K5e6eky2tyWJdlwOpYsJQK.lWbrzkMOQalB/1PPinQtFlOq', 'admin', '2024-10-08 13:08:41', ''),
(13, 'Vatche', 'vatche1@live.f', '$2y$10$40GTasZgPzkZShzdibtSH.2ZlZZ0CPJ0cuLX2EQ7f.He9f2uDkARO', 'admin', '2024-10-08 22:56:09', ''),
(16, 'Nabil', 'nabil@nabil.fr', '070aa66550916626673f492bdbdb655f', 'admin', '2024-10-29 09:28:34', ''),
(19, 'salim', 'salim@salIm.com', 'ca6b147b8fbdd688d8ebcaa3b803c22a', 'admin', '2024-10-29 10:08:42', ''),
(20, 'adamm', 'adam@adam.fr', '$2y$10$s9z9mcVrY5p2a.ApDulgXOi1.wHJy/Qc5s5Hj0uzMTLDDMF6YGkbC', 'admin', '2024-10-29 10:09:57', ''),
(21, 'antoine', 'antoine@antoine.fr', '0e5091a25295e44fea9957638527301f', 'employee', '2024-10-29 10:49:40', ''),
(22, 'Tanguy', 'tanguy@tanguy.fr', 'd2a550fe0b4de921fc0b371413b3dbdc', 'employee', '2024-10-29 14:16:33', ''),
(23, 'Momo', 'momo@momo.fr', '$2y$10$vSg1d3mEeA5ATAHMgdwUfeKL5WvSOyUOPIdzcEY40LqiG4zb1a6gO', 'client', '2024-11-12 13:21:53', 'YENYEK5KWVXTXJJ3'),
(24, 'Joco', 'joco@joco.fr', '$2y$10$s9z9mcVrY5p2a.ApDulgXOi1.wHJy/Qc5s5Hj0uzMTLDDMF6YGkbC', 'client', '2024-11-12 13:34:01', 'ID5ZTYQDVMU7FUZ5'),
(26, 'Master', 'master@master.fr', '$2y$10$l9WrvcqkHKmnA0rbZOcUV.JDhEiE2vAew3Cjct1vqcBDA2E3Xc5vS', 'client', '2024-11-12 15:21:09', '5PE43WFSAA5EUKU7'),
(27, 'Zidane', 'zidane@zidane.fr', '$2y$10$3SPaEDR99XyrMO.3k.nQCexux4cs5tPQRsemifeN0VcJfpWv/0yGu', 'client', '2024-11-12 15:23:13', 'KBC4KRJWHMFUHQ2R'),
(30, 'Niro', 'niro@niro.fr', '$2y$10$vfVKwPlBo7OwJJ/JzJRJ5ObCVdloX8u45.z.iLLnofkRBGA.2MIv2', 'client', '2024-11-12 15:38:13', 'SHR6SALF3VMLDB3V'),
(33, 'client', 'client@client.fr', '$2y$10$37bDfISStzrce4pbkuw8cODDKM/pwEsxzbG.rLYhPuBhe13CZXXbW', 'client', '2024-11-26 13:43:33', 'VBSLD4V7ROU6M6MJ'),
(34, 'Bouboule', 'kk', '$2y$10$Qpxc6aqBFAwvPT.O8MBF9u4xzfXm6jPcZYrpqO8ENzTztD4CEk5Va', 'admin', '2025-02-05 14:23:08', 'SAKML5U44UAKU3FA'),
(206, 'Dupont', 'dupont@example.com', '$2y$10$abc123...', 'employee', '2025-02-05 15:21:00', ''),
(208, 'Durand', 'durand@example.com', '$2y$10$ghi789...', 'client', '2025-02-05 15:21:00', ''),
(209, 'Bernard', 'bernardarnaud@example.com', '$2y$10$jkl012...', 'client', '2025-02-05 15:21:00', ''),
(210, 'Simon', 'simonthere@example.com', '$2y$10$mno345...', 'client', '2025-02-05 15:21:00', ''),
(219, 'Nike', 'niketester@client.fr', '$2y$10$fRaa1/ryKJYzqbwkRaEUJer/cCW0IK25VWEINioWBhLH2OV/iAFFm', 'client', '2025-03-03 22:37:14', 'T4PE52IFQ5SXYNR6'),
(220, 'gouloum', 'gouloum@client.fr', '$2y$10$8r7mipX4IbkvUeeokTRneOdj35c9nq9GLqJrQnGhKvdcJb8dJfw2O', 'client', '2025-03-03 22:38:44', '233TFO552GANXH4Y'),
(232, 'mm', 'mm@mm.com', '$2y$10$uHyugMkGAwZerzhZrgOtWemEJCbJpZoY4fYBUVfgCqsM2G48Z4NA2', 'client', '2025-03-04 12:50:53', 'QSZ343DYVTYNLXJK'),
(233, 'tt', 'tt@tt.com', '$2y$10$gAHMCzhTbpIGh3VNpk2mre4cxezdKDQto3igq61kVjw/RBfA14hYa', 'client', '2025-03-11 15:31:03', 'Z7CSPA3HAGOX5R7W'),
(234, 'aa', 'aa@aa.fr', '$2y$10$kObTgPubaZtdSEvgl7IvJeOdX1UjIOuInXQs8R9YtjX2SC6hn6iSC', 'employee', '2025-03-11 21:01:07', 'SXNQGVHO2IX7DYZE'),
(235, 'Miguel', 'miguel@miguel.fr', '$2a$11$yyMWBEtp6zx6iF.VfZeSw.bR9zblB.h5CRaOKgz2eJIkUMUHN.ySK', 'client', '2025-04-05 12:01:38', 'I9HNSTHE3MRFKUU2'),
(236, 'qq', 'qq@qq.fr', '$2a$11$d4nR7bWUeJ3iivpdOYbnSuGBv4aKoKK1C8HNW65On.SyEvmvd5vYW', 'admin', '2025-04-06 14:54:09', 'B8GQ5ODJMF8I6JXZ');

-- --------------------------------------------------------

--
-- Structure de la table `user_details`
--

DROP TABLE IF EXISTS `user_details`;
CREATE TABLE IF NOT EXISTS `user_details` (
  `id` int NOT NULL AUTO_INCREMENT,
  `user_id` int NOT NULL,
  `prenom` varchar(100) DEFAULT NULL,
  `nom` varchar(100) DEFAULT NULL,
  `adresse` varchar(255) DEFAULT NULL,
  `ville` varchar(100) DEFAULT NULL,
  `code_postal` varchar(20) DEFAULT NULL,
  `pays` varchar(100) DEFAULT NULL,
  `telephone` varchar(20) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_user_details_user` (`user_id`)
) ENGINE=MyISAM AUTO_INCREMENT=21 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Déchargement des données de la table `user_details`
--

INSERT INTO `user_details` (`id`, `user_id`, `prenom`, `nom`, `adresse`, `ville`, `code_postal`, `pays`, `telephone`) VALUES
(4, 219, 'TEST', 'Nike', 'tester 4 bis tes', 'testteeer', '00000', 'TESTING', '0908070401'),
(11, 232, 'Skibidi', 'Manoukian', '1 rue jules guesde', 'Alfortville', '94140', 'France', '0651343107'),
(15, 233, 'mon', 'code', 'html888', 'dansle', '95555', 'HTML', '0I935U35908'),
(16, 234, 'aa', 'aa', 'amena nisrine', 'Alfortville', '94140', 'France', '0651344444'),
(17, 19, 'michou', 'miguel', 'miguelharr', 'harr', '55555', 'france', ''),
(18, 24, 'Jonathan', 'Coco', '7 rue de Fleury Noisy', 'Le Sec', '75024', 'France', '06777777777'),
(19, 33, 'Kevin', 'Manoukian', '1 Rue Jules Guesde', 'Alfortville', '94140', 'BV', '0634625644'),
(20, 7, 'mm', 'mm', 'mmm', 'alfort', '94500', 'France', '066666666');

--
-- Contraintes pour les tables déchargées
--

--
-- Contraintes pour la table `commande`
--
ALTER TABLE `commande`
  ADD CONSTRAINT `commande_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id_client`) ON DELETE CASCADE;

--
-- Contraintes pour la table `order_modifications`
--
ALTER TABLE `order_modifications`
  ADD CONSTRAINT `order_modifications_ibfk_1` FOREIGN KEY (`order_id`) REFERENCES `commande` (`id`) ON DELETE CASCADE;

DELIMITER $$
--
-- Évènements
--
DROP EVENT IF EXISTS `add_missing_variantes`$$
CREATE DEFINER=`root`@`localhost` EVENT `add_missing_variantes` ON SCHEDULE EVERY 5 SECOND STARTS '2025-02-19 19:44:46' ON COMPLETION NOT PRESERVE ENABLE DO BEGIN
    INSERT INTO produit_variantes (produit_id, couleur, stock)
    SELECT p.id, 'Standard', p.stock_total  -- Prend le stock réel du produit
    FROM produits p
    LEFT JOIN produit_variantes pv ON p.id = pv.produit_id
    WHERE pv.produit_id IS NULL;
END$$

DELIMITER ;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
