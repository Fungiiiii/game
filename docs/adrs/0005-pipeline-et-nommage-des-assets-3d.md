# ADR-0005 — Pipeline, nommage et organisation des assets 3D

## Statut

Proposé

Date : 2026-09-21

## Contexte

Les premiers assets arrivent : champignon, cailloux, arbre.

Aujourd'hui, `Assets/Art/` et `ArtSource/` sont vides. Ils ne contiennent que
des `.gitkeep`. C'est le bon moment pour fixer les règles. Sur un dossier vide,
une convention ne coûte rien. Sur un dossier plein, elle coûte des renommages,
des GUID et des références cassées.

[`docs/art-pipeline.md`](../art-pipeline.md) décrit déjà un pipeline. Il pose
deux problèmes.

**Ce n'est pas un ADR.** Il contient pourtant une vraie décision
d'architecture : « on exporte en FBX, jamais de `.blend` dans `Assets/` ». Elle
y est posée en prose. Sans alternatives. Sans conséquences. Modifiable sans
laisser de trace. Or `CLAUDE.md` place un ADR accepté au-dessus des conventions
du dépôt. Cette décision n'a jamais été enregistrée là où elle serait
opposable.

**Il ne décrit pas le pipeline réellement pratiqué.** Il promet « un seul
preset d'export ». En réalité les réglages changent selon que l'objet est riggé
ou non. Il ne dit rien des réglages d'import Unity, qui décident pourtant
autant de l'échelle et de l'orientation que l'export. Il ne dit rien de la
palette partagée, qui est la méthode de texturation en vigueur.

Un doc de pipeline faux ne se contente pas d'être inutile. Il fabrique de la
divergence : chacun applique la moitié qui l'arrange.

Il faut donc trancher avant le premier FBX.

## Hypothèses

- **Une seule personne modélise et exporte.** Les sources vivent sur sa
  machine. Personne d'autre n'a besoin d'ouvrir un `.blend`. **Si quelqu'un
  d'autre se met à modéliser, cette décision doit être rouverte.** C'est
  l'hypothèse qui porte tout le reste.
- Cette personne sauvegarde ses sources elle-même. Le dépôt ne les protège pas.
- Le style reste low-poly à couleurs plates. Une palette partagée n'a plus de
  sens s'il faut du détail texturé, des normal maps ou un dépliage UV par
  asset.
- Les personnages sont des créatures non-humanoïdes. Pas de retargetisation
  Humanoid prévue.
- Aucune animation n'est exportée pour l'instant.

## Contraintes

- Unity 6.3 LTS, `6000.3.21f1`. PC Windows 64-bit en V1.0. URP
  ([ADR-0002](./0002-pipeline-de-rendu-urp.md)).
- **Blender est Z-up, Unity est Y-up.** Tous les bugs « le modèle arrive couché
  à -90° sur X » viennent de là.
- GitHub est sur le plan **Free** : 1 Go de stockage LFS, 1 Go de bande
  passante par mois. Même contrainte de plan que dans
  [ADR-0004](./0004-verification-unity-hors-ci.md).
- Un `.blend` est gros, binaire, et resauvegardé des dizaines de fois.
  **Chaque sauvegarde est un blob complet de plus.** Git ne stocke pas de delta
  pour ces fichiers.
- Git ne fusionne pas un binaire. LFS non plus : il les stocke, c'est tout.
- **Un FBX est un export cuit.** Modificateurs appliqués, pile perdue, graphe
  de matériaux perdu, collections perdues. Il ne redevient jamais une source.
- **Le nom d'un objet dans Blender devient le nom du GameObject.** Le nommage
  ne peut donc pas s'arrêter au nom de fichier.
- Renommer un asset importé casse les références qui ne passent pas par le GUID
  du `.meta`.
- **Le Default Preset d'Unity est unique par type d'importer.** Pas de réglage
  par défaut différent selon le dossier sans code éditeur.

## Alternatives envisagées

### Alternative 1 — Sources `.blend` dans le dépôt, sous `ArtSource/`

Le statu quo décrit par `docs/art-pipeline.md`. Le `.blend` est suivi par LFS,
hors de `Assets/`.

- Avantages : source et export voyagent ensemble ; historique complet ; aucune
  perte possible ; un second modeleur peut arriver sans rien changer.
- Inconvénients : mange le quota LFS très vite — un `.blend` de 50 Mo
  sauvegardé vingt fois épuise le gigaoctet du plan Free ; alourdit le clone
  pour ceux qui ne touchent pas à l'art ; n'apporte aucune collaboration réelle
  tant qu'une seule personne modélise.
- Risques : dépôt lourd à cloner en quelques mois ; quota épuisé près du rendu.

### Alternative 2 — Dépôt d'art séparé

Un second dépôt, `fungiiiii-art`, avec LFS.

- Avantages : vrai historique sur les sources ; quota LFS déporté hors du dépôt
  que tout le monde clone ; mêmes outils ; la porte reste ouverte à un second
  modeleur.
- Inconvénients : un dépôt de plus à administrer et sauvegarder ; il faut une
  convention pour relier un FBX à sa source, et rien ne l'applique.
- Risques : les deux dépôts divergent en silence ; le dépôt d'art n'est cloné
  par personne. On retombe alors sur l'alternative 3, avec la cérémonie en
  plus.

### Alternative 3 — Sources locales, FBX seuls dans le dépôt (retenue)

Le dépôt ne contient que des modèles exportés. `ArtSource/` disparaît.

- Avantages : aucune consommation de quota LFS ; clone léger ; rien à mettre en
  place ; le dépôt reste un dépôt de jeu, pas une archive d'art.
- Inconvénients : **aucune source n'est protégée par le dépôt** ; aucun lien
  vérifiable entre un FBX et le `.blend` qui l'a produit ; impossible de
  revenir à l'état d'un modèle la semaine dernière.
- Risques : point de défaillance unique. Machine morte, sources perdues. Les
  assets deviennent non ré-éditables. Ils ne sont plus que re-modélisables.

### Alternative 4 — Une texture dédiée par asset

Dépliage UV et texture propre pour chaque modèle.

- Avantages : liberté visuelle totale ; aucune grille partagée à respecter.
- Inconvénients : un matériau et une texture par asset, donc un draw call par
  asset ; un dépliage UV à faire pour chaque modèle, ce qui est l'étape la plus
  lente du travail.
- Risques : incompatible avec le style et le rythme de production visés.

### Alternative 5 — Une palette par thème ou biome

`Palette_Forest.png`, `Palette_Cave.png`, un matériau chacune.

- Avantages : grilles plus petites ; une retouche reste confinée à un biome.
- Inconvénients : il faut une règle pour dire à quelle palette appartient un
  modèle, et elle devient un piège dès qu'un asset sert dans deux biomes ;
  plusieurs matériaux, donc moins de batching.
- Risques : un même caillou dupliqué en deux variantes, et ça se propage.

### Alternative 6 — Une palette unique partagée (retenue)

Un `Palette.png`, un matériau, tous les modèles mappés dessus.

- Avantages : un seul matériau pour tout le décor, donc batching maximal et une
  seule texture en mémoire ; **aucun dépliage UV à faire** ; cohérence des
  couleurs obtenue par construction, pas par discipline.
- Inconvénients : **la grille devient figée.** Déplacer une case invalide les
  UV de tous les modèles déjà exportés, d'un coup.
- Risques : une palette réorganisée sans y penser, et tout le jeu change de
  couleurs. Le bug est pénible à trouver, parce que le diff est vide.

### Alternative 7 — Nommage à préfixe de type, style Unreal

`SM_Rock_01`, `T_Palette`, `M_Palette`, `PF_Rock_01`.

- Avantages : le type se lit dans la recherche du Project window sans filtre.
  Confortable sur un gros projet.
- Inconvénients : contredit la doc existante ; redondant avec l'arborescence,
  qui porte déjà le type ; impose de connaître une table de préfixes.
- Risques : table appliquée à moitié, donc pire que pas de table.

### Alternative 8 — Nommage descriptif en PascalCase (retenue)

`Rock_Small_01`, `MushroomCap_Large`, `TreePine_01`.

- Avantages : déjà ce qu'annonce la doc, donc rien à corriger ; lisible sans
  rien décoder ; le type est porté par le dossier.
- Inconvénients : hors de son dossier, un nom ne dit plus s'il désigne un
  modèle, un matériau ou un prefab.
- Risques : faibles. L'extension et l'icône lèvent le doute presque toujours.

## Décision

### 1. Les `.blend` n'entrent jamais dans ce dépôt

Le dépôt ne contient que des modèles exportés. `ArtSource/` est supprimé.

Les sources vivent sur la machine de la personne qui modélise. C'est elle qui
les sauvegarde.

La ligne `*.blend filter=lfs` de `.gitattributes` est **conservée comme
filet** : un `.blend` committé par accident irait au moins en LFS. Un contrôle
automatique le refuse de toute façon.

### 2. Un seul preset d'export Blender

Il est committé dans [`tools/blender/`](../../tools/blender/). Un preset est de
la configuration, pas de l'art : il est versionné même si les sources ne le
sont pas.

Le fichier fait autorité. `docs/art-pipeline.md` en donne la lecture humaine.

### 3. `Apply Transform` reste décoché, pour tous les objets

Riggés comme statiques.

La conversion d'axes se fait **une seule fois, côté Unity**, par
`Bake Axis Conversion`. Empiler les deux mécanismes expose à une double
conversion.

N'en garder qu'un supprime la classe de bug entière. Et rend enfin vraie la
promesse d'un preset unique.

### 4. Les réglages d'import Unity sont portés par des Presets committés

Pas par la mémoire de celui qui importe. Il y en a deux :

- `ModelImporter_StaticProp`, déclaré **Default Preset** du ModelImporter. Il
  couvre le cas majoritaire.
- `ModelImporter_RiggedCharacter`, appliqué **à la main** sur les personnages.

Leurs valeurs vivent dans [`docs/art-pipeline.md`](../art-pipeline.md).

Deux choses relèvent en revanche de la décision, et pas du réglage :

- **Un prop statique n'a pas de rig.** Un `Generic` appliqué par défaut
  générerait un Avatar pour chaque caillou et chaque arbre.
- **Le second preset reste manuel.** Le Default Preset est unique par type
  d'importer. C'est le prix assumé de ne pas écrire d'AssetPostprocessor.

### 5. Arborescence

Par type, puis par domaine.

```text
Assets/
  Art/
    Models/<Domaine>/<Nom>.fbx
    Textures/Palette.png                 la palette partagée
    Textures/<Domaine>/<Nom>_<Map>.png   textures dédiées, l'exception
    Materials/Palette.mat                le matériau partagé
  Prefabs/<Domaine>/<Nom>.prefab
tools/blender/                           preset d'export
```

Les domaines sont `Characters`, `Environment` et `Props`. Il n'y en a pas
d'autres sans modifier cet ADR.

### 6. Nommage

Anglais, ASCII, PascalCase. Segments séparés par `_` :

```text
<Nom>[_<Variante>][_NN]
```

La règle s'applique à quatre choses : le fichier FBX, **l'objet à l'intérieur
de Blender**, le matériau, le prefab. L'objet Blender compte parce que c'est
lui qui devient le GameObject.

`NN` tient sur deux chiffres. Il ne sert qu'à distinguer des variations
interchangeables.

**Aucun nom ne porte de version, d'état ou de date.** Pas de `_final`, `_v3`,
`_new`, `_old`, `_test`, `_OK`. Git est le système de versions.

Les animations, quand elles viendront, suivront la convention Unity
`<Personnage>@<Clip>.fbx`.

### 7. Palette

Il n'existe qu'un `Assets/Art/Textures/Palette.png`.

**Règle non négociable : une couleur ne se déplace jamais. On ne fait
qu'ajouter.**

La grille est figée à la création. Sa taille est consignée dans
`docs/art-pipeline.md` dès que la palette entre dans le dépôt.

Déplacer une case, réordonner la grille ou redimensionner l'image invalide les
UV de tous les modèles déjà exportés.

Ses réglages d'import sont appliqués **à la main**. Ils ne peuvent pas passer
par un Default Preset, qui toucherait aussi les sprites d'UI. Les valeurs
vivent dans [`docs/art-pipeline.md`](../art-pipeline.md).

### 8. La convention est vérifiée mécaniquement

`scripts/check-art-assets.sh` refuse tout `.blend`. Il valide le nommage et
l'emplacement des assets.

Il est branché dans `.githooks/pre-commit` et dans
`.github/workflows/conventions.yml`, comme les autres contrôles du dépôt.

## Justification

**Sur les sources.** L'alternative 1 achète l'historique des sources. Cette
garantie est aujourd'hui presque sans objet : une seule personne modélise, et
le format ne fusionne pas. Il n'y a aucune collaboration à arbitrer. Le prix,
lui, est immédiat : le gigaoctet de LFS part en quelques dizaines de
sauvegardes.

L'alternative 2 est la bonne réponse le jour où un second modeleur arrive.
D'ici là, elle ajoute un dépôt que personne ne clone. Ce qui la ramène à
l'alternative 3, avec la cérémonie en plus.

L'alternative 3 est retenue **en connaissance de son défaut**. Elle échange une
garantie de récupération contre de la simplicité. Ce n'est défendable que parce
que l'hypothèse « une seule personne détient les sources » est vraie
aujourd'hui. C'est pour ça qu'elle est écrite en tête de cet ADR plutôt que
sous-entendue. Le jour où elle devient fausse, la décision est caduque.

**Sur la palette.** Elle supprime le dépliage UV, l'étape la plus lente de la
production d'un asset. Et elle ramène tout le décor à un seul matériau. Sa
contrepartie — la grille figée — est sévère. Mais elle est contenable, à
condition d'être énoncée comme une règle et non découverte comme un bug. C'est
le rôle du point 7.

**Sur le nommage.** L'alternative 8 gagne surtout parce qu'elle est déjà écrite
dans la doc. Passer aux préfixes de type imposerait de corriger l'existant,
pour un gain qui n'apparaît qu'à une échelle que le projet n'a pas.

**Sur le point 3.** Il ne vient d'aucune alternative. Il vient d'un constat :
deux mécanismes de conversion d'axes coexistaient dans la pratique. En
supprimer un ne coûte rien. Et ça fait disparaître une classe de bugs
difficiles à diagnostiquer, parce qu'ils ressemblent à un problème d'art alors
que ce sont des réglages.

## Dépendances

Aucune.

Les Presets et le système de Default Preset sont natifs à Unity. Aucun package
n'est ajouté à [`Packages/manifest.json`](../../Packages/manifest.json). Aucun
code runtime n'est introduit.

Le preset Blender est un fichier de configuration. Sa seule dépendance est
Blender, déjà outil d'autorité du pipeline.

## Conséquences

- **Positives :** dépôt léger et clonable ; quota LFS préservé ; un FBX déposé
  dans `Assets/` arrive déjà bien réglé ; la conversion d'axes n'a plus qu'un
  seul point d'application ; le nommage est appliqué par un script, pas par la
  relecture ; `docs/art-pipeline.md` redevient exact.
- **Négatives :** aucune source protégée par le dépôt ; aucun lien vérifiable
  entre un FBX et sa source ; les personnages exigent un preset appliqué à la
  main, que rien ne rappelle ; la palette figée interdit une réorganisation
  tardive des couleurs.
- **Dette technique acceptée : les sources 3D reposent sur une seule machine,
  et sur une sauvegarde qu'aucun contrôle ne vérifie.** Si cette machine est
  perdue, les assets ne sont plus ré-éditables. Il faudra les remodéliser
  depuis zéro. C'est le coût assumé de la décision. La condition de réouverture
  est nommée dans les hypothèses. S'y ajoute l'absence d'AssetPostprocessor :
  le second preset d'import ne s'applique pas tout seul.
- **Impact migration :** `ArtSource/` et ses `.gitkeep` sont supprimés.
  `docs/art-pipeline.md` est réécrit. `CLAUDE.md`,
  [`docs/unity-init.md`](../unity-init.md), `README.md` et le skill
  `unity-serialization` cessent de désigner `ArtSource/`. **Aucun asset n'est
  touché**, puisque aucun n'est encore committé. C'est toute la raison d'être
  du calendrier de cet ADR.
- **Impact workflow éditeur :** l'export passe par le preset committé, jamais
  par des réglages saisis à la main. À l'import d'un personnage, appliquer
  `ModelImporter_RiggedCharacter`. Une correction nécessaire dans l'Inspector
  signifie que le preset est faux — et c'est le preset qu'on corrige, pas
  l'asset. La procédure de contrôle et la checklist avant commit sont dans
  [`docs/art-pipeline.md`](../art-pipeline.md).
- **Impact runtime :** favorable, mais non mesuré. Un matériau unique autorise
  le batching et ne charge qu'une texture. Aucun profilage n'a été fait. Ce
  n'est pas la justification de la décision, seulement un effet attendu.

## Références

- [`docs/art-pipeline.md`](../art-pipeline.md) — le mode d'emploi, réécrit par
  cet ADR. **Le partage est net : cet ADR porte les décisions et le pourquoi,
  le mode d'emploi porte les valeurs et les procédures.** Une valeur qui change
  se corrige là-bas, sans toucher à cet ADR. Une décision qui change demande un
  nouvel ADR.
- [`tools/blender/`](../../tools/blender/) — le preset d'export, qui fait
  autorité sur les réglages.
- [`scripts/check-art-assets.sh`](../../scripts/check-art-assets.sh) et
  [`.github/workflows/conventions.yml`](../../.github/workflows/conventions.yml)
  — l'application mécanique.
- [`.gitattributes`](../../.gitattributes) — LFS et fusion Unity.
- [ADR-0002](./0002-pipeline-de-rendu-urp.md) — URP, dont dépend le shader du
  matériau de palette.
- [ADR-0004](./0004-verification-unity-hors-ci.md) — plan GitHub Free, d'où
  vient la contrainte LFS, et vérification hors CI.
- `CLAUDE.md`, sections « Language » et « Scope ».
- Skills `unity-serialization` et `unity-project-config`.
