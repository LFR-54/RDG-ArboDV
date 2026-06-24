# Changelog

## Version 2.0.0 · 24/06/2026

### Correctifs

- **Fin de téléversement plus claire** : le message indiquant que le dataset est temporairement occupé ne reste plus affiché après la fin du transfert. L'application attend désormais la fin complète des événements du moteur Java avant d'afficher le résultat
- **Protection contre les événements retardataires** : les anciennes lignes de sortie Java restent disponibles dans les logs, mais ne peuvent plus recolorer ou modifier l'interface après un téléversement terminé ou une réinitialisation
- **Réinitialisation visuelle complète** : le bouton Réinitialiser supprime également la sélection personnalisée de l'arbre et restaure correctement le texte ainsi que la couleur du statut
- **Aplatissement local multiple** : tous les dossiers sélectionnés sont maintenant traités, du niveau le plus profond vers la racine. La sélection de toute une arborescence permet donc de produire un dépôt entièrement aplati
- **Manifeste aligné sur l'aperçu** : les chemins virtuels sont reconstruits depuis l'arbre visible juste avant la détection des doublons et la création du manifeste. L'organisation téléversée correspond ainsi à l'organisation affichée dans l'espace de préparation
- **Renommage des dossiers distants** : l'action « Renommer sur le serveur » fonctionne désormais sur les dossiers. Elle met à jour de manière séquentielle le `directoryLabel` de tous les fichiers contenus dans le dossier et ses sous-dossiers, après validation du nouveau nom et confirmation de l'utilisateur
- **Suppression massive sécurisée** : déduplication des identifiants puis utilisation prioritaire de l'API Dataverse `deleteFiles` afin de supprimer tout le lot en une seule requête. Un mode de compatibilité espacé de trois secondes reste disponible pour les anciens serveurs
- **Protection contre le blocage réseau** : prise en charge de `Retry-After`, délai maximal par requête, arrêt immédiat sur les réponses 401, 403 ou 429 et sur les erreurs de connexion. Aucune actualisation automatique supplémentaire n'est envoyée après un blocage présumé
- **Compte rendu de suppression fiable** : distinction entre les fichiers supprimés, les échecs réellement rencontrés et les fichiers non traités après une interruption. Le bouton Annuler est disponible pendant les suppressions longues
- **Destination verrouillée pendant les opérations** : le bouton « Déposer à la racine », l'arbre distant et son actualisation sont temporairement désactivés pendant un téléversement ou une opération distante. Le chemin affiché reste ainsi cohérent avec la destination réellement utilisée

### Interface

- **Onglet renommé** : l'onglet « Fichiers » devient « Préparation du dépôt » afin de mieux distinguer l'espace de travail local du contenu déjà présent sur le serveur
- **Destination plus lisible** : l'en-tête du dossier cible est plus compact. Les chemins trop longs sont abrégés avec des points de suspension et restent consultables intégralement au survol
- **Fenêtre redimensionnable** : l'application peut maintenant être agrandie ou maximisée. Les arbres, les logs et les principaux contrôles s'adaptent à l'espace disponible tout en conservant la disposition d'origine
- **État d'attente reformulé** : le verrou temporaire du dataset est présenté comme une phase normale de finalisation ne nécessitant aucune action de l'utilisateur

### Documentation

- **README restructuré** : ajout d'un parcours d'utilisation complet, d'une présentation détaillée des onglets, d'une légende des couleurs, des prérequis, de liens de téléchargement officiels, de l'architecture et d'une section de dépannage
- **Origine du projet documentée** : ajout de l'historique de la création initiale par LFR54 pendant son stage de BTS, des limites ayant empêché la première architecture d'être mise en production et de l'évolution vers le moteur DVUploader
- **Assistance par intelligence artificielle explicitée** : documentation transparente de l'utilisation de l'IA lors de la refonte hors stage, notamment pour la partie Java et les problématiques d'intégration avancées

### Refonte Majeure du Moteur de Dépôt

Cette version marque une refonte architecturale majeure de **RDG ArboDV**. Le fichier JAR du moteur de dépôt est désormais intégré directement au binaire et n'a plus besoin d'être distribué séparément.

- **Intégration du moteur Java (JAR embarqué)** : Le moteur Java `DVUploader-v1.3.0-RDGengine.jar` est dorénavant directement inclus dans les ressources de l'exécutable. Il est extrait dynamiquement sur le disque à l'exécution, dans le répertoire applicatif ou dans `%LocalAppData%\RDG_ArboDV` en cas de droits d'écriture limités. L'application peut ainsi être distribuée sous la forme d'un fichier `.exe` unique, tout en nécessitant toujours le runtime .NET 8 Desktop et Java 8 ou une version ultérieure sur le poste cible.
- **Intégration de l'icône de marque** : L'icône de l'application (`Logo_RDG_ArboDV.ico`) est directement intégrée aux métadonnées de l'exécutable pour un rendu professionnel sous l'Explorateur Windows.
- **Liaison intime avec le moteur de dépôt (DVUploader)** : Alignement avec la version personnalisée du JAR Java pour :
  - Supporter le paramètre `-manifest` et la reconstruction d'une arborescence virtuelle en mémoire pour appliquer les chemins préparés dans l'interface
  - Uniformiser les sorties de progression avec de véritables sauts de ligne (`\n`) pour permettre leur capture et leur affichage en temps réel dans l'interface graphique

### Nouvelles Fonctionnalités Visuelles et Distantes

- **Visualisation de l'arborescence serveur** : Nouvel onglet interactif "Fichiers sur le serveur" pour naviguer en temps réel dans la structure du jeu de données distant (supportant la sélection multiple avec Ctrl/Shift).
- **Sélection graphique de la destination** : Choix simplifié du répertoire cible d'upload par clic direct dans l'arborescence serveur.
- **Téléchargement récursif distant en lot** : Récupération asynchrone de répertoires ou fichiers distants complets, avec reconstruction de la structure locale et affichage dynamique de la vitesse (Mo/s) et de l'ETA.
- **Annulation propre des téléchargements** : Arrêt contrôlé des transferts distants et suppression du fichier temporaire incomplet en cas d'annulation.
- **Gestion distante des fichiers (Clic droit)** :
  - *Déplacer* : Déplacement graphique de fichiers/dossiers distants (avec support de création dynamique de dossiers).
  - *Aplatir* : Aplatissement de dossiers distants à la volée.
  - *Supprimer* : Nettoyage séquentiel et récursif de fichiers et dossiers du serveur.
  - *Renommer* : Renommage à chaud sur le serveur.
- **Enrichissement des métadonnées distantes** : Affichage discret et stylisé de la taille du fichier et de la date de dépôt dans la langue sélectionnée (ex: `(4.5 Ko — Déposé le...)`).
- **Conservation de l'état de navigation** : Restauration automatique des dossiers ouverts et du chemin sélectionné après une actualisation ou une opération distante.
- **Sélection multiple fiabilisée** : Correction des sélections visuelles fantômes et maintien de la sélection groupée lors de l'ouverture du menu contextuel.

### Améliorations Architecturales

- **Traitement séquentiel ordonné** : Sécurisation des requêtes asynchrones en lot via sémaphore pour respecter la politique de verrous transactionnels de l'API Dataverse.
- **Prévention des bannissements IP (Rate Limiting)** : Temporisation automatique de 350 ms entre chaque requête réseau sur les opérations de masse.
- **Déplacement vers la racine** : Contournement de la limitation de l'API Dataverse en envoyant explicitement `/` lorsque la destination est la racine du dataset.
- **Compatibilité des checksums Dataverse** : Prise en charge du checksum renvoyé sous la forme d'un objet `{ type, value }`, évitant les erreurs de désérialisation lors du chargement des fichiers distants.
- **Sécurisation inter-threads** : Lecture des valeurs de configuration depuis le thread de l'interface puis transmission explicite aux tâches asynchrones, afin d'éviter les accès WinForms non autorisés.
- **Détection des doublons en temps réel** : Marquage coloré immédiat des fichiers locaux (Vert pour doublon exact à la cible, Chocolat pour doublon existant dans un autre dossier).
- **Indication explicite des verrous (Dataset Lock)** : Interception du statut de verrouillage du serveur pour afficher un message clair et basculer la barre de progression en animation continue, évitant de faire croire à un blocage du logiciel.
- **Auto-détection de la langue de l'hôte** : Initialisation automatique de l'interface en français ou en anglais selon la culture de l'OS.
- **Gestion des préférences** : Avertissement de démarrage unique et sauvegarde de la langue choisie.
- **Réinitialisation globale** : Remise à zéro complète et thread-safe de la progression et de la vitesse lors d'un Reset.
- **Extraction optimisée du moteur** : Réutilisation du JAR déjà extrait lorsque sa taille correspond à la ressource embarquée, avec repli vers `%LocalAppData%\RDG_ArboDV` si le dossier de l'application n'est pas accessible en écriture.

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
