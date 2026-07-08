# Modifications apportées à DVUploader (Moteur Java)

Ce document récapitule l'ensemble des modifications apportées au code source d'origine de **DVUploader (v1.3.0-beta)** pour créer la version personnalisée **DVUploader-ArboDV** utilisée par **RDG ArboDV v2**.

Le projet d'origine est disponible dans le dépôt [GlobalDataverseCommunityConsortium/dataverse-uploader](https://github.com/GlobalDataverseCommunityConsortium/dataverse-uploader). Ce document couvre uniquement les modifications du moteur Java. Les évolutions de l'interface C# sont décrites dans les changelogs français et anglais du projet.

---

## 1. Renommage du Projet
* **Fichier** : [`pom.xml`](dataverse_uploader_1.3.0_beta_sourcecode/pom.xml)
* **Modification** : Modification de l'identifiant du package (`artifactId`) en **`DVUploader`** et de la version en **`1.3.0-RDGengine`** pour générer un fichier JAR final nommé **`DVUploader-v1.3.0-RDGengine.jar`**. Cela permet d'identifier immédiatement le moteur personnalisé et de le distinguer du JAR amont.

---

## 2. Intégration de l'argument `-manifest` (Upload Virtuel)
* **Fichier** : [`AbstractUploader.java`](dataverse_uploader_1.3.0_beta_sourcecode/src/main/java/org/sead/uploader/AbstractUploader.java)
* **Modification** :
  * Ajout du parsing du paramètre `-manifest=<chemin_du_fichier_json>` dans la méthode `parseArgs`.
  * Ajout du lancement de la méthode `uploadManifest(manifestPath)` dans `processRequests` si ce paramètre est détecté.

---

## 3. Modélisation de l'Arborescence Virtuelle en Mémoire
* **Fichier** : [`AbstractUploader.java`](dataverse_uploader_1.3.0_beta_sourcecode/src/main/java/org/sead/uploader/AbstractUploader.java)
* **Modification** :
  * **Création de la classe interne `ManifestDirectory`** (héritant de `Resource`) : Cette classe implémente virtuellement un dossier racine qui contient une liste de sous-ressources en mémoire. Cela permet de simuler un répertoire disque à partir du JSON sans altérer la structure physique des fichiers sur la machine de l'utilisateur.
  * **Méthode `uploadManifest(String manifestPath)`** :
    * Lit le manifeste JSON temporaire écrit par l'application C#.
    * Analyse les couples `source` (chemin réel) et `directoryLabel` (dossier virtuel sur Dataverse).
    * Reconstruit dynamiquement l'arbre de répertoires `ManifestDirectory` en mémoire.
    * Démarre l'upload récursif classique en appelant la méthode standard `uploadCollection` sur le répertoire virtuel racine.

---

## 4. Uniformisation des flux de logs pour l'intégration C#
* **Fichier** : [`AbstractUploader.java`](dataverse_uploader_1.3.0_beta_sourcecode/src/main/java/org/sead/uploader/AbstractUploader.java)
* **Modification** :
  * Par défaut, DVUploader écrivait la progression de l'upload en utilisant des retours chariots (`\r`), ce qui écrasait la ligne en console mais perturbait la capture des logs du processus par l'application C#.
  * **Modification de la méthode `printStatus`** : utilisation de `System.out.println` pour générer des lignes complètes avec des sauts de ligne réels (`\n`).
  * Les messages critiques de progression tels que `PROCESSING(F):`, `Progress:`, `UPLOADED as:` et les réponses d'erreur sont exposés sous une forme stable afin que le parseur du client C# puisse mettre à jour la progression et les états visuels.

---

## 5. Corrections et Imports requis pour la compilation
* **Fichier** : [`AbstractUploader.java`](dataverse_uploader_1.3.0_beta_sourcecode/src/main/java/org/sead/uploader/AbstractUploader.java)
* **Modification** :
  * Ajout des imports requis par la classe `ManifestDirectory` qui n'étaient pas importés à l'origine dans cette classe abstraite :
    * `import java.io.InputStream;`
    * `import org.apache.http.entity.mime.content.ContentBody;`

---

## 6. Alignement des arguments d'entrée du point d'entrée Java
* **Fichier** : [`DVUploader.java`](dataverse_uploader_1.3.0_beta_sourcecode/src/main/java/org/sead/uploader/dataverse/DVUploader.java)
* **Modification** : Ajustement de la validation du nombre et de la nature des arguments dans la méthode principale `main()` afin de tolérer l'absence de chemin de fichier direct sur la ligne de commande quand le paramètre `-manifest` est présent.

---

## 7. Compatibilité et format du manifeste

* Le moteur est compilé avec un niveau de compatibilité **Java 8** (`source` et `target` à `1.8`). Il peut être exécuté avec Java 8 ou une version ultérieure.
* Le manifeste attendu est un tableau JSON. Chaque entrée contient :
  * `source` : chemin absolu du fichier local
  * `directoryLabel` : chemin virtuel du dossier cible dans Dataverse, ou une chaîne vide pour la racine
* Les séparateurs de chemin sont normalisés en `/` et les barres placées au début ou à la fin du `directoryLabel` sont supprimées avant la reconstruction de l'arbre virtuel.

---

## 8. Renforcement de la détection des fichiers tabulaires ingérés
* **Fichier** : [`DVUploader.java`](dataverse_uploader_1.3.0_beta_sourcecode/src/main/java/org/sead/uploader/dataverse/DVUploader.java)
* **Modification** :
  * Ajout d'un index distinct des fichiers `.tab` créés par l'ingest Dataverse.
  * Conservation de la détection existante via `originalFileName` lorsque l'API expose le nom du fichier source.
  * Ajout d'un repli sur le chemin `.tab` dérivé pour les fichiers tabulaires déjà convertis lorsque le checksum du fichier original n'est pas disponible côté Dataverse.
  * Comparaison du checksum lorsque le fichier local est déjà comparable au `.tab` serveur, notamment pour les sources `.tab` ou `.tsv`.

Cette évolution évite de redéposer un fichier tabulaire source lorsque Dataverse en expose déjà la version convertie, tout en évitant de traiter tous les fichiers `.tab` du serveur comme des équivalents d'ingest.

---

## 9. Documentation des arguments en ligne de commande
* **Fichier** : [`DVUploader.java`](dataverse_uploader_1.3.0_beta_sourcecode/src/main/java/org/sead/uploader/dataverse/DVUploader.java)
* **Modification** : Correction du texte d'aide associé à `-failOnInvalidNames`, qui décrivait par erreur le comportement de `-noIngest`.

---

## 10. Note de portée pour RDG ArboDV 2.1.0

Les ajustements suivants relèvent de l'interface C# et ne modifient pas le moteur Java :

* serveur de démonstration sélectionné par défaut dans l'interface
* commandes de préparation du dépôt grisées tant que la clé API, le DOI et le serveur ne permettent pas un dépôt
* consultation et téléchargement publics possibles avec DOI et serveur seuls

Le moteur Java reste concerné par `-noIngest` et par la détection des fichiers tabulaires déjà convertis décrites dans les sections précédentes.
