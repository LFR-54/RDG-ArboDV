# RDG ArboDV

<p align="center">
  <img src="assets/AppPreviewV2.0.0.png" width="100%">
</p>

<p align="center">
  <strong>Client bureautique Windows pour le dépôt, l'organisation et la gestion de fichiers dans l'entrepôt national Recherche Data Gouv</strong><br />
  <sub>Version 2.0.0</sub>
</p>

<p align="center">
  <a href="#fonctionnalites">Fonctionnalités</a> •
  <a href="#apercu">Aperçu</a> •
  <a href="#installation">Installation</a> •
  <a href="#utilisation">Utilisation</a> •
  <a href="#parametres-requis">Paramètres requis</a> •
  <a href="#build">Build</a>
</p>

<p align="center">
  <img alt="C#" src="https://img.shields.io/badge/C%23-.NET%208.0-512bd4">
  <img alt="Platform" src="https://img.shields.io/badge/Platform-Windows-0078d4">
  <img alt="GUI" src="https://img.shields.io/badge/GUI-WinForms-0f6cbd">
  <img alt="Dataverse" src="https://img.shields.io/badge/Target-Dataverse-2d7d46">
  <img alt="Version" src="https://img.shields.io/badge/Version-2.0.0-success">
</p>

---

<a id="apercu"></a>
## Aperçu

**RDG ArboDV** est un client bureautique Windows conçu spécifiquement pour l'entrepôt national **Recherche Data Gouv** (`https://recherche.data.gouv.fr`). Il permet de simplifier, structurer et sécuriser le dépôt et la gestion de fichiers au sein de vos jeux de données (datasets)., en particulier lorsque le volume de données est important ou que l'arborescence à conserver est complexe.

L'application s'adresse aux équipes qui doivent préparer un versement propre, de façon maîtrisée et fiable, sans dépendre de l'interface web classique.

### Visualisation et gestion du serveur (Clic droit)

L'onglet **Fichiers sur le serveur** permet d'explorer l'arborescence distante en temps réel et d'interagir graphiquement avec elle (sélection simple ou multiple) :

<p align="center">
  <img src="assets/AppPreview_RightClick_file.png" alt="Menu contextuel sur un fichier" width="48%">
  <img src="assets/AppPreview_RightClick_folder.png" alt="Menu contextuel sur un dossier" width="48%">
</p>

> [!WARNING]
> Ce logiciel est destiné en priorité aux dépôts volumineux comportant de nombreux sous-dossiers.  
> Pour de petits jeux de données, il est recommandé d'utiliser l'interface web afin de limiter la charge sur les serveurs.

---

<a id="fonctionnalites"></a>
## Fonctionnalités

| Fonction | Bénéfice |
| --- | --- |
| **Upload de fichiers et dossiers** | Prépare et transfère rapidement des lots de dépôt volumineux. |
| **Conservation de l'arborescence** | Respecte rigoureusement la hiérarchie logique des données locales. |
| **Aplatissement local & distant** | Remonte récursivement tout le contenu d'un dossier dans son répertoire parent (puis supprime le dossier vide), permettant de simplifier et restructurer l'arborescence avant l'upload ou directement sur le serveur. |
| **Détection en temps réel des doublons** | Identifie les doublons exacts (verts) et les doublons existant ailleurs sur le serveur (chocolat) avec détails des chemins. |
| **Visualisation de l'arborescence serveur** | Explorez de façon interactive les fichiers déjà déposés sur le serveur. |
| **Sélection de destination graphique** | Ciblez un dossier d'upload d'un simple clic sur l'arborescence serveur. |
| **Opérations distantes (clic droit)** | Téléchargement récursif, déplacement visuel (création de dossiers), renommage et suppression. |
| **Auto-détection de la langue** | S'adapte automatiquement à la langue de votre système Windows (Français / Anglais). |
| **Statistiques intégrées** | Affiche le volume de données, la vitesse (Mo/s), le temps écoulé et le temps restant (ETA). |
| **Gestion des reprises sur erreur** | Retente automatiquement l'envoi en cas d'échec ponctuel du réseau. |

---

<a id="parametres-requis"></a>
## Paramètres requis

Le logiciel repose sur trois informations simples :

- **API Key** : la clé personnelle qui autorise le dépôt sécurisé sans utiliser le mot de passe du compte.
- **DOI** : l'identifiant pérenne (PID) du dataset cible.
- **Serveur** : l'instance Dataverse cible.

Le logiciel peut être utilisé avec les deux environnements proposés par défaut :

- **Entrepôt de production** : `https://entrepot.recherche.data.gouv.fr`
- **Instance de démo** : `https://demo.recherche.data.gouv.fr`

Le DOI attendu par l'application suit le format `doi:10.xxxx/xxxxx`.  
Si vous collez une URL complète (ex: `https://doi.org/...`), le logiciel la normalise automatiquement dans le bon format.

---

<a id="installation"></a>
## Installation

### Utilisation du binaire

- Utiliser un poste Windows.
- Disposer du runtime **.NET 8.0**.
- Lancer l'exécutable autonome **`RDG_Uploader_GUI.exe`** (le moteur Java y est intégré et s'extrait automatiquement au démarrage, aucun autre fichier n'est requis sur le disque).

### Utilisation depuis le code source

- Installer le SDK **.NET 8.0** sur votre machine.
- Ouvrir une invite de commandes ou PowerShell dans le dossier `rdg_arbodv_v2`.
- Lancer la compilation de la solution :
  ```powershell
  dotnet build RDG_Uploader_GUI.sln
  ```
- L'exécutable et ses fichiers de sortie seront générés dans `bin/Debug/net8.0-windows/`. Le moteur Java y est directement intégré en tant que ressource embarquée, l'application est donc immédiatement opérationnelle sans aucune autre action.

---

<a id="utilisation"></a>
## Utilisation

### 🔧 Préparation à son utilisation

1. **Récupération du Jeton API** :
   Cliquez en haut à droite sur votre profil Recherche Data Gouv, puis sur **Jeton API**.
   ![Capture d’écran de RDG ArboDV](assets/Step1.png)

   Cliquez sur **"Créer le jeton"**, puis copiez-le dans le champ **API Key** du logiciel.
   ![Capture d’écran de RDG ArboDV](assets/Step2.png)

2. **DOI du Dataset** :
   Récupérez le DOI (surligné en jaune dans l'interface de votre jeu de données) et copiez-le dans le champ **DOI** du logiciel.
   ![Capture d’écran de RDG ArboDV](assets/Step3.png)

3. **Ciblage de destination** :
   Sélectionnez un dossier dans l'onglet **Fichiers sur le serveur** pour y envoyer vos fichiers locaux. Si aucun dossier n'est sélectionné, l'envoi s'effectue à la racine du dataset.

4. **Lancement** :
   Ajoutez vos fichiers ou dossiers locaux dans le premier onglet, vérifiez la détection des doublons en temps réel, puis cliquez sur **Téléverser**.

---

<a id="build"></a>
## Build

Le projet est conçu sous **.NET 8.0**.

1. Ouvrir la solution `RDG_Uploader_GUI.sln` dans Visual Studio 2022 ou utiliser le CLI .NET.
2. Compiler la solution en configuration `Release` ou `Debug` :
   ```powershell
   dotnet build -c Release
   ```
3. Le moteur Java (`DVUploader-v1.3.0-RDGengine.jar`) est embarqué directement dans l'application en tant que ressource. Pour générer un fichier unique (Single File) indépendant qui contient toutes ses dépendances, utilisez la commande :
   ```powershell
   dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -p:PublishReadyToRun=true RDG_Uploader_GUI.csproj
   ```
   L'exécutable autonome sera généré dans `bin\Release\net8.0-windows\win-x64\publish\`.

---

<p align="center">
  <sub>RDG ArboDV • Présentation logicielle • Version 2.0.0</sub>
</p>
