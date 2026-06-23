# Changelog

## Version 2.0.0 · 23/06/2026

### Refonte Majeure du Moteur de Dépôt

Cette version marque une refonte architecturale majeure de **RDG ArboDV** qui s'affranchit désormais de toute dépendance de fichier sur le disque en intégrant directement son moteur de dépôt au binaire.

- **Intégration du moteur Java (JAR embarqué)** : Le moteur Java `DVUploader-v1.3.0-RDGengine.jar` est dorénavant directement inclus dans les ressources de l'exécutable. Il est extrait dynamiquement sur le disque à l'exécution (dans le répertoire applicatif ou dans `%LocalAppData%\RDG_ArboDV` en cas de droits d'écriture limités), rendant l'exécutable C# 100% autonome et distribuable via un fichier unique `.exe`.
- **Intégration de l'icône de marque** : L'icône de l'application (`Logo_RDG_ArboDV.ico`) est directement intégrée aux métadonnées de l'exécutable pour un rendu professionnel sous l'Explorateur Windows.
- **Liaison intime avec le moteur de dépôt (DVUploader)** : Alignement avec la version personnalisée du JAR Java pour :
  - Supporter le paramètre `-manifest` et l'analyse d'arbre virtuel en mémoire pour des dépôts locaux asynchrones.
  - Uniformiser les sorties de progression avec des retours chariots réels (`
`) pour une capture et un affichage en temps réel dans l'interface graphique.

### Nouvelles Fonctionnalités Visuelles et Distantes

- **Visualisation de l'arborescence serveur** : Nouvel onglet interactif "Fichiers sur le serveur" pour naviguer en temps réel dans la structure du jeu de données distant (supportant la sélection multiple avec Ctrl/Shift).
- **Sélection graphique de la destination** : Choix simplifié du répertoire cible d'upload par clic direct dans l'arborescence serveur.
- **Téléchargement récursif distant en lot** : Récupération asynchrone de répertoires ou fichiers distants complets, avec reconstruction de la structure locale et affichage dynamique de la vitesse (Mo/s) et de l'ETA.
- **Gestion distante des fichiers (Clic droit)** :
  - *Déplacer* : Déplacement graphique de fichiers/dossiers distants (avec support de création dynamique de dossiers).
  - *Aplatir* : Aplatissement de dossiers distants à la volée.
  - *Supprimer* : Nettoyage séquentiel et récursif de fichiers et dossiers du serveur.
  - *Renommer* : Renommage à chaud sur le serveur.
- **Enrichissement des métadonnées distantes** : Affichage discret et stylisé de la taille du fichier et de la date de dépôt dans la langue sélectionnée (ex: `(4.5 Ko — Déposé le...)`).

### Améliorations Architecturales

- **Traitement séquentiel ordonné** : Sécurisation des requêtes asynchrones en lot via sémaphore pour respecter la politique de verrous transactionnels de l'API Dataverse.
- **Prévention des bannissements IP (Rate Limiting)** : Temporisation automatique de 350 ms entre chaque requête réseau sur les opérations de masse.
- **Détection des doublons en temps réel** : Marquage coloré immédiat des fichiers locaux (Vert pour doublon exact à la cible, Chocolat pour doublon existant dans un autre dossier).
- **Indication explicite des verrous (Dataset Lock)** : Interception du statut de verrouillage du serveur pour afficher un message clair et basculer la barre de progression en animation continue, évitant de faire croire à un blocage du logiciel.
- **Auto-détection de la langue de l'hôte** : Initialisation automatique de l'interface en français ou en anglais selon la culture de l'OS.
- **Gestion des sessions** : Avertissement de démarrage unique et sauvegarde automatique de la configuration.
- **Réinitialisation globale** : Remise à zéro complète et thread-safe de la progression et de la vitesse lors d'un Reset.

## Version 1.2.0 · 15/04/2026

### Nouveautés

- Ajout d'un choix de langue dans l'interface avec anglais par défaut et français disponible.
- Traduction de l'interface principale, des statistiques, des messages, des aides contextuelles et de la fenêtre About.
- Normalisation automatique du DOI au format `doi:...` à partir d'une URL `https://doi.org/...`.

### Améliorations

- Suppression du collage automatique dans les champs API Key et DOI.
- Ajout de boutons d'information pour expliquer l'API Key et le DOI de manière plus claire.
- Mise à jour de la présentation du projet dans le README.

## Version 1.0.1 · 18/06/2025

### Nouveautés

- Lors d'une erreur, le logiciel retentera l'envoi.

## Version 1.0.0 · 17/06/2025

### Nouveautés

- Possibilité d'aplatir un dossier, c'est-à-dire de récupérer tout son contenu, de le déplacer à l'emplacement du dossier, puis de supprimer ce dernier.

### Correctifs

- Résolution d'un problème où l'encodage UTF-8 ajoutait des caractères indésirables devant les noms de fichiers lors de l'upload.

## Version 0.9.0 · 11/06/2025

- Version stable initiale.
