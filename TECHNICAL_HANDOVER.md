# Document de Transfert Technique - RDG ArboDV (v2.0.0)

Ce document résume l'intégralité des modifications architecturales et techniques apportées à l'application **RDG ArboDV** (version C# .NET 8.0 WinForms + moteur Java personnalisé). Il est conçu pour permettre à un autre agent d'intelligence artificielle de comprendre instantanément l'état du code, les choix d'implémentation et les mécanismes réseau.

---

## 1. Architecture Générale

Le projet est composé de deux briques étroitement liées :
1. **Client C# WinForms (`RDG_Uploader_GUI`)** : L'interface utilisateur écrite en C# sous .NET 8.0 Windows.
2. **Moteur d'envoi Java (`DVUploader-v1.3.0-RDGengine.jar`)** : Une version modifiée de l'outil officiel `DVUploader` de Harvard Dataverse.

---

## 2. Refonte Majeure : Embarquement du Moteur Java (v2.0.0)

Dans la version v2.0.0, l'application est devenue 100% autonome. Elle ne nécessite plus la présence physique du fichier JAR à côté de l'exécutable lors de la distribution.

### Intégration MSBuild
Le fichier JAR et le logo de l'application ont été déclarés directement dans le fichier projet [RDG_Uploader_GUI.csproj](file:///d:/projets_perso/RDG-ArboDV/rdg_arbodv_v2/RDG_Uploader_GUI.csproj) :
```xml
<PropertyGroup>
  ...
  <ApplicationIcon>Logo_RDG_ArboDV.ico</ApplicationIcon>
</PropertyGroup>
<ItemGroup>
  <EmbeddedResource Include="DVUploader-v1.3.0-RDGengine.jar" />
</ItemGroup>
```

### Extraction Dynamique au Démarrage
Dans [Form1.cs](file:///d:/projets_perso/RDG-ArboDV/rdg_arbodv_v2/Form1.cs), la méthode privée `ExtractJarFromResources()` a été implémentée et est appelée dès le constructeur de la fenêtre principale :
1. **Destination principale** : Elle tente d'extraire la ressource `RDG_Uploader_GUI.DVUploader-v1.3.0-RDGengine.jar` vers le dossier de base de l'application (`AppDomain.CurrentDomain.BaseDirectory`).
2. **Destination secondaire (Fallback)** : Si les droits d'écriture manquent (ex: installation dans `Program Files`), elle extrait le JAR dans le profil utilisateur local : `%LocalAppData%\RDG_ArboDV\`.
3. **Optimisation des performances** : Elle compare la taille du fichier physique existant avec la longueur du flux de la ressource. S'ils sont identiques, elle n'effectue aucune écriture pour éviter des blocages de fichiers lorsque plusieurs instances s'exécutent.
4. **Sécurité (Search Fallback)** : Si les deux méthodes d'extraction échouent, le système recherche le JAR dans les dossiers parents ou la structure du projet à des fins de rétrocompatibilité.

---

## 3. Système d'Upload Virtuel (Manifeste JSON)

Pour éviter de dupliquer ou réorganiser physiquement les fichiers de l'utilisateur sur son disque lors des opérations d'aplatissement ou de dépôt dans un sous-dossier virtuel :
1. L'application C# écrit un fichier temporaire contenant un manifeste au format JSON.
2. Ce manifeste liste chaque fichier réel avec son chemin local (`source`) et son chemin virtuel cible sur le serveur Dataverse (`directoryLabel`).
3. Le JAR Java personnalisé a été modifié pour accepter l'argument `-manifest=<chemin_du_manifeste>`. Il lit le JSON, construit un arbre logique en mémoire (`ManifestDirectory`) et téléverse les fichiers en appliquant les bons chemins virtuels.

---

## 4. Exploration et Gestion Distante du Serveur

L'onglet **Fichiers sur le serveur** a été entièrement implémenté et s'appuie sur le contrôle `MultiSelectTreeView` avec un nœud virtuel **Racine (/)**.

### Opérations Graphiques Distantes (Clic Droit)
Des fonctionnalités asynchrones de gestion de fichiers ont été ajoutées via l'API Dataverse :
* **Déplacement (Move)** : Ouvre un `FolderPickerDialog` visuel affichant l'arborescence des répertoires du serveur. L'utilisateur peut créer un nouveau dossier dynamiquement et y déplacer les éléments.
* **Suppression (Delete)** : Collecte récursivement tous les identifiants de fichiers du dossier ciblé, demande une confirmation unique avec décompte, puis effectue la suppression séquentielle.
* **Aplatissement (Flatten)** : Décale l'arborescence sélectionnée d'un niveau vers le haut dans le répertoire parent de chaque élément.
* **Téléchargement Asynchrone en Lot** : Télécharge par morceaux (Chunks de 8 Ko) les fichiers/dossiers en conservant leur arborescence locale. Calcule dynamiquement le débit, l'ETA et gère l'annulation propre (avec suppression du fichier temporaire incomplet).

### Préservation de l'État de l'Arbre distant
Les méthodes `GetExpandedPaths()` et `RestoreExpandedPaths()` mémorisent la liste des dossiers ouverts/fermés. L'état d'expansion de l'arbre est restauré après chaque rafraîchissement manuel ou automatique.

---

## 5. Contournements Réseau & Stabilité de l'API Dataverse

### Limitation du Débit (Rate Limiting) et Séquençage
* **Problème** : Les suppressions ou déplacements distants en masse provoquaient des erreurs de transaction (verrous concurrents) sur le serveur Dataverse et menaient parfois à un bannissement temporaire de l'IP de l'utilisateur.
* **Solution** : Les traitements en masse ont été sérialisés en utilisant un sémaphore global (`SemaphoreSlim(1)`). Un délai d'attente de **350 ms** (`await Task.Delay(350)`) a été intercalé entre chaque requête HTTP.

### Déplacement vers la Racine
* **Problème** : L'API Dataverse ignore les chaînes vides ou `null` pour le champ `directoryLabel` lors de l'édition des métadonnées, rendant impossible le déplacement de fichiers vers la racine.
* **Solution** : Lors d'un déplacement vers la racine, l'application envoie explicitement la valeur `"/"`. Le serveur Dataverse reçoit cette valeur non vide, valide la requête et applique le déplacement, puis son algorithme interne de nettoyage (sanitization) convertit le `"/"` en chemin vide (root), déplaçant ainsi correctement le fichier ou le dossier à la racine.

### Gestion du Verrouillage du Jeu de Données (Dataset Lock)
* **Problème** : Dataverse verrouille parfois le dataset lors d'indexations volumineuses. Le moteur Java affiche alors en boucle `Dataset locked - waiting...`, ce qui pouvait faire croire à un gel de l'application C#.
* **Solution** : La Regex de capture de log identifie cette ligne, affiche le message en orange `"Jeu de données verrouillé - attente de la libération par le serveur..."` et fait basculer la barre de progression en mode *Marquee* (animation continue).

### Décapsulage du Checksum
* **Problème** : crash de désérialisation sur l'API Dataverse car le champ `checksum` est renvoyé sous la forme d'un objet `{ type, value }` et non d'une chaîne brute.
* **Solution** : Correction des classes DTO internes en ajoutant la sous-classe `DataverseChecksumInfo`.

---

## 6. Interface Graphique et Thread-Safety

### MultiSelectTreeView & Ghost Selection
* Pour corriger les sélections fantômes (le dessin bleu persistant imposé par Windows sur les nœuds non sélectionnés de l'arbre), la méthode `OnDrawNode` dessine manuellement le fond de tous les éléments à l'aide de `this.BackColor` et `this.ForeColor` lorsqu'ils ne sont pas présents dans la collection personnalisée `SelectedNodes`.
* La sélection multiple est conservée lors du clic droit pour permettre d'appliquer des opérations de lot depuis le menu contextuel.
* Les clics droits sont testés via `HitTest(e.Location)` au lieu de `GetNodeAt(e.X, e.Y)` pour garantir une sélection exacte même en cliquant dans l'espace vide à droite de l'intitulé du nœud.

### Sécurisation Inter-Threads
Toutes les variables de configuration (`api`, `srv`) sont lues depuis le thread principal de l'interface utilisateur (UI Thread) et transmises comme arguments aux tâches d'arrière-plan, éliminant les exceptions `InvalidOperationException` d'accès inter-threads non autorisés.

### Réinitialisation Propre
Le bouton **Réinitialiser** vide les listes locales de fichiers, remet à zéro l'arbre visuel local et réinitialise de manière thread-safe la progression, la vitesse à `0.0 Mo/s` et les chronomètres de transfert (sans toucher à l'API Key ni au DOI).

### Détection Linguistique
Lors du tout premier lancement, la langue de l'interface est initialisée automatiquement en français ou en anglais en lisant `CultureInfo.CurrentUICulture` du système hôte.

---

## 7. Encodage des Fichiers Source
* **IMPORTANT** : Le fichier source [Form1.cs](file:///d:/projets_perso/RDG-ArboDV/rdg_arbodv_v2/Form1.cs) est encodé en **UTF-8 (sans BOM)**. 
* Toute insertion de caractères accentués (comme les commentaires français ou les libellés traduits) doit impérativement être effectuée en préservant ce format UTF-8 (double octet pour les accents, ex: `é` stocké en `\xc3\xa9`).
* *Rappel historique* : L'ouverture ou la sauvegarde du fichier sous encodage `Windows-1252` corrompt l'interprétation par le compilateur C# et casse l'affichage de tous les accents de l'interface graphique (ex: `SÃ©lectionner`).

---

## 8. Commandes de Compilation & Publication

### Nettoyer le projet (Clean)
```powershell
dotnet clean RDG_Uploader_GUI.sln
dotnet clean -c Release RDG_Uploader_GUI.sln
```

### Compiler la version Debug (avec fichiers de sortie séparés)
```powershell
dotnet build RDG_Uploader_GUI.sln
```

### Générer le binaire de production Unique (Single File)
Cette commande génère un exécutable autonome `.exe` optimisé contenant toutes ses dépendances (Newtonsoft.Json, le JAR Java, le logo et les configurations) dans le dossier de publication : `bin\Release\net8.0-windows\win-x64\publish\`.
```powershell
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -p:PublishReadyToRun=true RDG_Uploader_GUI.csproj
```
