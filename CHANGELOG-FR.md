# Changelog

## Version 1.3.0 · 23/06/2026

### Nouveautés

- **Visualisation de l'arborescence serveur** : Ajout d'un nouvel onglet interactif "Fichiers sur le serveur" affichant l'intégralité de la structure distante sous une racine virtuelle commode (supportant la sélection multiple avec Ctrl/Shift).
- **Sélection graphique de la destination** : Définition simplifiée de l'emplacement cible d'upload par simple clic dans l'arborescence distante, avec bouton rapide de retour à la racine.
- **Téléchargement récursif distant en lot** : Option au clic droit pour télécharger plusieurs fichiers et dossiers complets du serveur simultanément (sélection multiple), avec reconstruction de la structure locale, débit en Mo/s (ou MB/s) et calcul du temps restant (ETA).
- **Opérations à distance (sélection multiple au clic droit)** :
  - *Déplacer* : Déplacement simultané de plusieurs fichiers ou dossiers vers un autre répertoire via le sélecteur `FolderPickerDialog`.
  - *Aplatir* : Aplatissement en lot de plusieurs dossiers sélectionnés (remontée d'un cran du contenu).
  - *Supprimer* : Suppression récursive et en lot de répertoires ou fichiers sélectionnés avec confirmation globale.
  - *Renommer* : Renommage à la volée (réservé à une sélection unique).
- **Affichage de la taille et de la date des fichiers distants** : L'arborescence serveur affiche la taille (convertie dynamiquement) et la date de dépôt pour chaque fichier dans la langue sélectionnée (ex: `(4.5 Ko — Déposé le...)`), avec un stylisme allégé et discret (gris-ardoise adouci hors sélection, bleu-gris clair sous sélection) pour ne pas encombrer l'affichage.
- **Auto-détection de la langue** : Détection automatique de la langue Windows au tout premier lancement et initialisation en français ou anglais.
- **Avertissement de démarrage à affichage unique** : L'avertissement de démarrage n'apparaît plus qu'une seule fois et est mémorisé pour les sessions futures.

### Améliorations

- **Traitement séquentiel ordonné et sécurisé** : Les actions en lot à distance (suppression, déplacement, aplatissement) s'exécutent de manière séquentielle ordonnée (`SemaphoreSlim(1)`) pour éviter les conflits de verrous de transactions simultanées sur la base de données du serveur Dataverse.
- **Préservation de la sélection au clic droit** : Correction d'un bug de sélection ; le clic droit sur un élément faisant déjà partie d'une sélection multiple (Ctrl/Shift) conserve désormais la totalité des éléments sélectionnés au lieu de la réduire à un seul élément, facilitant les actions groupées.
- **Détection des doublons en temps réel** : Analyse réactive des doublons locaux et distants avec coloration (Vert pour doublon exact ignoré, Chocolat pour doublon existant dans un autre dossier avec détails des chemins).
- **Chronomètre intelligent** : Arrêt instantané du temps écoulé dès la fin des transferts pour éviter d'incrémenter le temps pendant la lecture des messages de confirmation.
- **Annulation et sécurité** : Support fluide du bouton ANNULER pour l'interruption des téléchargements et suppression des fichiers temporaires ou incomplets.
- **Réinitialisation complète** : Remise à zéro de la barre de progression, des statistiques de transfert, du temps écoulé, de l'ETA et du libellé de vitesse dynamique au clic sur "Réinitialiser" (sans effacer l'API Key ni le DOI).
- **Rafraîchissement automatique intelligent** : Synchronisation automatique en arrière-plan du serveur après transfert, avec sauvegarde et restauration des dossiers dépliés/pliés de l'arbre.
- **Mise à niveau globale** : Alignement de la version client GUI et du moteur Java (`DVUploader-v1.3.0-RDGengine.jar`).

### Correctifs

- **Déplacement vers la racine (/)** : Résolution du problème où l'API Dataverse ignorait les valeurs vides pour `directoryLabel` empêchant de replacer les fichiers à la racine (utilisation d'un contournement intelligent avec `"/"` géré et nettoyé par le serveur).
- **Sécurisation d'accès aux threads (Thread-safety)** : Capture des informations de connexion sur le thread UI pour éviter les exceptions de type `InvalidOperationException` lors des appels réseau asynchrones en lot.
- Correctifs et optimisations d'affichage généraux (gestion du défilement automatique des logs, correction d'un bug de désérialisation JSON sur l'API Dataverse, détection correcte de clic dans la zone à droite des nœuds).

---

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
