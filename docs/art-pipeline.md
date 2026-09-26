# Pipeline art — de Blender à Unity

Comment faire passer un modèle de Blender au jeu. Référence du quotidien.

Les décisions derrière ces règles sont dans
[l'ADR-0005](./adrs/0005-pipeline-et-nommage-des-assets-3d.md). Si ce fichier
et l'ADR se contredisent, l'ADR l'emporte — et c'est ce fichier qu'il faut
corriger.

## Les cinq règles

1. **Aucun `.blend` dans ce dépôt.** Les sources restent en local.
2. **Les réglages d'export correspondent au tableau de ce fichier.** Vérifier
   avec le cube.
3. **Les modèles s'importent avec une rotation de -90° sur X.** C'est accepté.
   Placer un personnage sous un parent.
4. **Nommer l'asset, pas son état.** `Rock_Small_01`, jamais `Rock_final_v3`.
5. **Une couleur de la palette ne bouge jamais.** On ne fait qu'en ajouter.

## Où vivent les fichiers

```text
Assets/
  Art/
    Models/<Domain>/<Name>.fbx           exported models
    Textures/Palette.png                 the shared palette
    Textures/<Domain>/<Name>_<Map>.png   dedicated textures — the exception
    Materials/Palette.mat                the shared material
  Prefabs/<Domain>/<Name>.prefab
tools/palette/                           the palette generator
```

`<Domain>` vaut `Characters`, `Environment` ou `Props`. En ajouter un quatrième
impose d'amender l'ADR-0005.

### Pourquoi pas de `.blend` ici

Ce dépôt ne contient que des modèles exportés. Les sources restent en local, et
chacun sauvegarde les siennes. Un contrôle pre-commit rejette tout `.blend`.

C'est un compromis, pas un oubli. Il achète un clone léger et un quota LFS
intact. Il coûte la possibilité de retoucher un asset un jour si les sources
locales sont perdues. L'ADR-0005 l'enregistre comme une dette acceptée. Elle
nomme aussi ce qui rouvre la décision : **une deuxième personne qui modélise.**

## Exporter depuis Blender

Ce tableau fait foi. Blender retient les réglages du dernier export, donc
ils se saisissent une fois, pas avant chaque export.

**Aucun preset d'export n'est commité.** L'ADR-0005 explique pourquoi : rien
dans ce dépôt ne peut en installer un, vérifier qu'il est installé, ni
remarquer qu'il a dérivé. Le cube de référence ci-dessous est la vérification
qui marche vraiment.

| Section | Réglage | Valeur |
|---|---|---|
| Top | Path Mode | `Auto` |
| Include | Limit to → Selected Objects | on |
| Include | Object Types | `Armature`, `Mesh` |
| Include | Custom Properties | off |
| Transform | Scale | `1.00` |
| Transform | Apply Scalings | `FBX All` |
| Transform | Forward / Up | `-Z Forward` / `Y Up` |
| Transform | Apply Unit | on |
| Transform | Use Space Transform | on |
| Transform | **Apply Transform** | **off — toujours** |
| Geometry | Smoothing | `Face` |
| Geometry | Apply Modifiers | on |
| Geometry | Loose Edges | off |
| Geometry | Tangent Space | off |
| Geometry | Triangulate Faces | off |
| Armature | Primary / Secondary Bone Axis | `Y` / `X` |
| Armature | Armature FBXNode Type | `Null` |
| Armature | Only Deform Bones | off |
| Armature | Add Leaf Bones | off |
| Bake Animation | — | off |

### Ceux qu'il faut connaître

**Apply Transform — off, toujours.** L'option est expérimentale et casse les
armatures. La laisser sur off est ce qui produit les -90° sur X acceptés dans
Unity — voir *Orientation* ci-dessous.

**Smoothing — `Face`.** La valeur par défaut de Blender est `Normals Only`.
Elle perd l'information de lissage, et on obtient des artefacts d'ombrage dans
Unity.

**Apply Modifiers — attention.** Les modificateurs s'appliquent à leur niveau
*viewport*, pas à leur niveau de rendu. Un Subsurf réglé à 1 dans le viewport
et à 3 au rendu s'exporte à 1.

**Only Deform Bones — off.** L'activer supprime l'os racine, qui ne déforme
aucun vertex. La hiérarchie casse avec lui. Le prix à payer en le laissant sur
off : les os d'IK et de contrôle passent aussi.

**Triangulate Faces — off.** Unity triangule à l'import, et le mesh reste
éditable.

**Selected Objects — on.** Un asset logique par fichier. Ne jamais exporter
une scène entière pour la découper dans Unity.

## Importer dans Unity

Les réglages d'export ne sont que la moitié du travail. L'importeur de Unity
décide autant que Blender de l'échelle et de l'orientation finales.

Le côté import est donc figé par des Presets commités, pas par la mémoire.

### Props statiques — le défaut du projet

`ModelImporter_StaticProp` est le **Default Preset** du ModelImporter. Tout FBX
déposé dans `Assets/` arrive déjà correct.

| Onglet | Réglage | Valeur | Pourquoi |
|---|---|---|---|
| Model | Bake Axis Conversion | **off** | La laisser sur off est ce qui produit les -90° sur X acceptés. Voir *Orientation* ci-dessous. |
| Rig | Animation Type | `None` | Un prop statique n'a pas besoin de rig. `Generic` ici générerait un Avatar pour chaque rocher. |
| Materials | Material Creation Mode | `None` | Tout partage `Palette.mat`. |

### Personnages riggés

Appliquer `ModelImporter_RiggedCharacter` **à la main**, puis *Apply*.

Une différence : **Rig → Animation Type : `Generic`**. Les créatures ne sont
pas humanoïdes, donc `Humanoid` et son retargeting n'apportent rien.

> Cette étape ne peut pas être automatique. Le Default Preset de Unity est
> global par type d'importeur. L'ADR-0005 accepte l'étape manuelle plutôt que
> d'écrire un AssetPostprocessor.
>
> **Un personnage importé n'a pas d'Animator ? C'est cette étape qui a été
> sautée.**

### La texture de palette

Ses réglages s'appliquent à la main. Un preset par défaut pour les textures
toucherait aussi les sprites d'UI, qui veulent l'inverse.

| Réglage | Valeur | Pourquoi |
|---|---|---|
| Filter Mode | `Point` | |
| Wrap Mode | `Clamp` | |
| Generate Mip Maps | **off** | Les mips mélangent les cases voisines. La géométrie lointaine récupère des couleurs qui ne sont pas les siennes. |
| Compression | `None` | Une palette, c'est une poignée de couleurs exactes. Une compression avec perte les décale. |

## Nommage

Anglais. ASCII. PascalCase. Segments séparés par `_`.

```text
<Name>[_<Variant>][_NN]
```

`Rock_Small_01` · `MushroomCap_Large` · `TreePine_01`

La règle s'applique à quatre choses à la fois :

| Quoi | Exemple |
|---|---|
| Le fichier FBX | `Assets/Art/Models/Environment/Rock_Small_01.fbx` |
| **L'objet dans Blender** | `Rock_Small_01` |
| Le matériau | `Palette.mat` — partagé. Un matériau dédié prend le nom du modèle. |
| Le prefab | `Assets/Prefabs/Environment/Rock_Small_01.prefab` |

**Le nom de l'objet dans Blender compte.** Il devient le nom du GameObject à
l'import. Un `.blend` plein de `Cube.001` donne une hiérarchie Unity pleine de
`Cube.001`.

Autres règles :

- `NN` fait deux chiffres. Il ne sert qu'à distinguer des variantes
  interchangeables.
- Nommer ce que la chose **est**. Pas ce à quoi elle ressemble aujourd'hui.
- **Ne jamais nommer un état.** Pas de `_final`, `_v3`, `_new`, `_old`, `_test`,
  `_OK`, `_copy`. Git enregistre déjà les versions.

`scripts/check-art-assets.sh` fait respecter tout cela au commit.

### Animations

Pas encore exportées. Quand elles arriveront, chaque clip sera livré dans son
propre fichier :

```text
Assets/Art/Models/Characters/PlayerMushroom.fbx
Assets/Art/Models/Characters/PlayerMushroom@Idle.fbx
Assets/Art/Models/Characters/PlayerMushroom@Walk.fbx
```

Unity les associe automatiquement au rig du même nom.

Activer `Bake Animation` fera partie de ce changement-là, pas de celui-ci.

## La palette

Un seul `Assets/Art/Textures/Palette.png`. Un seul `Palette.mat`. Les UV de
chaque modèle pointent vers des cases de cette palette.

C'est ce qui retire le dépliage d'UV du workflow, et garde tout l'ensemble sur
un seul matériau.

**La règle qui compte : une couleur ne bouge jamais. On ne fait qu'ajouter.**

La grille est figée le jour où la palette est créée. Déplacer une case,
réordonner la grille ou redimensionner l'image invalide les UV de tous les
modèles déjà exportés.

Tous les assets du jeu changent de couleur. Et le diff responsable ne contient
qu'un seul `.png`. Ajouter une couleur dans l'espace libre est sans risque.
Tout le reste ne l'est pas.

### La grille

| | |
|---|---|
| Image | 128 × 128 px |
| Grille | 8 × 8 cases |
| Une case | 16 × 16 px |
| Utilisées | 19 cases |
| Libres | 45 cases, dessinées en damier gris |

Les lignes 0 à 4 sont prises : corps, visage, champignon et bulbe, végétation
et bois, roche. Tout ce qui est sous la ligne 4 est de l'espace libre où
ajouter.

### La générer

`Palette.png` ne s'édite pas à la main. Il est généré :

```bash
python tools/palette/generate_palette.py
```

[`tools/palette/generate_palette.py`](../tools/palette/generate_palette.py)
est la vraie source de vérité. On ajoute une couleur en y ajoutant une entrée,
puis on le relance. C'est ce qui rend « une couleur ne bouge jamais »
vérifiable dans un diff au lieu d'être une promesse.

Il écrit aussi [`docs/palette-guide.png`](./palette-guide.png), une planche de
référence qui montre chaque case avec son nom, sa valeur hexadécimale et ses
coordonnées UV.

### UV

Un UV pointe vers le **centre** d'une case :

```text
U = (col + 0.5) / 8
V = 1 - (row + 0.5) / 8
```

Viser le centre d'une case est ce qui garde les couleurs voisines hors de
l'échantillon, et c'est pour cela que le filtrage `Point` et la désactivation
des mip maps ne sont pas optionnels.

Créer les matériaux dans Unity, pour URP
([ADR-0002](./adrs/0002-pipeline-de-rendu-urp.md)). Ne pas compter sur
les matériaux venus de Blender. Les deux utilisent des modèles d'ombrage
différents, et c'est le pipeline de rendu qui décide de ce qu'est un matériau.

## Vérifier une fois, avec un cube de référence

Avant de faire confiance aux réglages, exporter un cube de 1×1×1 à l'origine.
Puis vérifier dans Unity :

- 1 unité Blender s'importe en 1 unité Unity (1 mètre)
- l'Inspector affiche une échelle de `(1, 1, 1)`

**La rotation ne fait pas partie de cette vérification.** Le cube arrive avec
une rotation de -90° sur X, comme tout le reste, et c'est attendu.

**Si l'échelle est fausse, ce sont les réglages d'export qui sont faux.
Corriger les réglages, pas l'asset.** Redimensionner dans Unity masque le
problème, et il revient à l'export suivant.

## Orientation

Les modèles s'importent avec une **rotation de -90° sur X** sur leur racine.
C'est le comportement par défaut de la chaîne Blender vers Unity, et l'ADR-0005
l'accepte plutôt que de le corriger.

Sur un prop statique, cela ne coûte rien. On le place dans une scène et on n'y
touche plus.

Sur un personnage, cela compte : le `forward` de la racine importée pointe vers
le bas, pas vers l'avant. **Placer un personnage sous un GameObject parent** et
laisser le parent porter le déplacement et la rotation. C'est la structure
qu'un personnage finit par avoir de toute façon, une fois qu'il a un
contrôleur.

## Avant de commiter un changement d'art

- [ ] Les réglages d'export correspondent au tableau ci-dessus
- [ ] `Apply Transform` était sur off
- [ ] L'Inspector affiche une échelle de `(1, 1, 1)` — les -90° sur X sont
      attendus
- [ ] Un personnage a reçu `ModelImporter_RiggedCharacter`
- [ ] Aucun `.blend` indexé
- [ ] Les noms sont en anglais, en PascalCase, et décrivent l'asset — pas son
      état
- [ ] Le nom de l'objet Blender correspond au nom du fichier
- [ ] La palette a été complétée, jamais réordonnée ni redimensionnée
- [ ] `git lfs status` montre les `.fbx` et `.png` suivis
