# Biome Forêt mystique — terrain, ambiance et outil de répartition

- **Ticket ClickUp :** https://app.clickup.com/t/12487v2byt9
- **Note de design validée :** https://app.clickup.com/t/12487v2bytb — fermée par Hugo le 26/09
- **Branche :** `feat/12487v2byt9-mystic-forest`
- **Auteur :** Hugo Bernier
- **Statut :** En cours

## Docs précédents

Aucun.

## Ce que fait la feature

![La forêt vue depuis la clairière de départ](../screenshots/mystic-forest.png)

Pour le joueur :

- [x] La scène `MysticForest` est une forêt sur un Terrain Unity de 1 × 1 km, au sol plat brun.
- [x] Les arbres et les cailloux sont répartis naturellement : pas de grille visible, pas d'amas, pas de trou.
- [x] Une clairière de départ de 8 m de rayon reste vide.
- [x] Aucun ciel n'est visible. Le fond de l'image se fond dans le brouillard.
- [x] Un brouillard vert sombre commence à 12 m et masque tout au-delà de 70 m.
- [x] Les arbres sont dessinés jusqu'à 90 m, donc jusqu'au-delà du brouillard.
- [x] Les arbres projettent leur ombre jusqu'à 80 m, donc jusque dans le brouillard.
- [x] Les matériaux sont mats : aucun reflet sur le sol, les troncs ni les champignons.
- [x] En jeu, on ne traverse pas les troncs.

Pour le level design, l'outil de répartition :

- [x] Une répartition (un calque) se place sous le Terrain. Chaque calque est un modèle : arbres, gros cailloux, etc.
- [x] **Generate forest** place les instances de chaque calque sur sa zone, à une distance minimale les unes des autres, avec une échelle et une rotation aléatoires. Les instances sont écrites dans les arbres du Terrain.
- [x] **Generate forest** régénère tous les calques du Terrain d'un coup.
- [x] La même seed redonne exactement la même répartition, chez n'importe qui.
- [x] **New seed + Generate forest** donne une autre répartition au calque sélectionné.
- [x] **Clear forest** retire toutes les instances du Terrain.
- [x] Chaque action s'annule en un seul Ctrl+Z, seed comprise.
- [x] Rien n'est placé dans une zone d'exclusion.
- [x] Chaque instance est posée sur le sol du Terrain.

## Hors périmètre

- Herbe, arbustes, particules de spores : sous-tâches séparées de `12487v2byt9`.
- Relief du sol : sous-tâche `12487v2bytj`. L'outil de relief n'est pas choisi.
- Chemins : leur aspect n'est pas décidé.
- Joueur, caméra de jeu et gameplay : la scène ne contient ni l'un ni l'autre.
- Éclairage baké : tout est en temps réel pour le prototype.

## Implémentation

**Scène :** `Assets/Scenes/Biomes/MysticForest.unity`, avec ses réglages de lumière dans `MysticForestLighting.lighting` à côté.

| Objet | Rôle |
|---|---|
| `Terrain` | Terrain de 1000 × 1000 m, hauteur max 50 m, en (-500, 0, -500). Données dans `MysticForestTerrainData.asset`. |
| `Terrain/Forest_Trees` | Calque de `PF_Tree`, 5 m minimum, échelle 0,85 à 1,25, seed 20716 |
| `Terrain/Forest_Rocks_Large` | Calque de `PF_Rock_Large`, 6 m minimum, échelle 0,7 à 1,5, seed 2 |
| `Terrain/Forest_Rocks_Medium` | Calque de `PF_Rock_Medium`, 8 m minimum, échelle 0,75 à 1,5, seed 94770 |
| `Terrain/Forest_Rocks_Small` | Calque de `PF_Rock_Small`, 8 m minimum, échelle 0,75 à 1,5, seed 30709 |
| `StartClearing` | Zone d'exclusion de 8 m de rayon |

Les calques et `StartClearing` portent le tag `EditorOnly` : ils servent à l'outil, Unity les retire du jeu livré.

Chaque calque couvre les 1000 × 1000 m du Terrain. Au total, 62 338 instances : 25 187 arbres, 17 437 gros cailloux, 9 849 moyens, 9 865 petits. La `Main Camera` est dans la clairière de départ, à hauteur d'œil (1,6 m).

**Terrain :**

| Réglage | Valeur |
|---|---|
| Matériau | matériau Terrain par défaut d'URP |
| Calque de texture | `Assets/Art/Materials/ForestFloor.terrainlayer`, texture unie `6B5436`, Smoothness 0 |
| Résolution de la heightmap | 513 (provisoire) |
| Distance d'affichage des arbres | 90 m |
| Colliders des arbres | activés (`Enable Tree Colliders`) |
| Draw Instanced | activé, comme Unity le recommande en URP |

**Prefabs** (`Assets/Prefabs/Environment/`) : `PF_Tree`, `PF_Rock_Large`, `PF_Rock_Medium`, `PF_Rock_Small`. Chacun a un `LODGroup` à la racine et le modèle en enfant. `PF_Tree` a une `CapsuleCollider` sur le tronc : rayon 0,45 m, hauteur 23,4 m.

**Scripts** (`Fungiiiii.Runtime.World` et `Fungiiiii.Editor.World`) :

| Fichier | Rôle |
|---|---|
| `Runtime/World/PoissonDiskSampler.cs` | Points aléatoires à distance minimale (algorithme de Bridson), déterministe par seed. `internal`. |
| `Runtime/World/ForestScatter.cs` | Composant de réglage d'un calque, et calcul des emplacements. |
| `Runtime/World/ScatterExclusionZone.cs` | Cercle où rien n'est placé. |
| `Editor/World/TerrainForestGenerator.cs` | Écrit les calques dans les arbres du Terrain : un prototype d'arbre par calque. |
| `Editor/World/ForestScatterEditor.cs` | Boutons Generate forest / New seed + Generate forest / Clear forest. |
| `Tests/EditMode/ForestScatterTests.cs` | Tests du tirage et des emplacements. |
| `Tests/EditMode/ForestScatterEditorTests.cs` | Tests de l'écriture dans le Terrain et de l'annulation. |
| `Tests/PlayMode/MysticForestPlayModeTests.cs` | Test de collision des troncs dans la vraie scène. |

**Réglages d'ambiance** (Lighting > Environment, soleil, caméra) :

| Réglage | Valeur |
|---|---|
| Skybox | aucune |
| Lumière ambiante | Color, r 0,34 · g 0,40 · b 0,345 |
| Reflets d'environnement | intensité 0 |
| Brouillard | Linear, de 12 à 70 m, `1E3A2A` |
| Fond de la caméra | Solid Color `1E3A2A`, identique au brouillard |
| Soleil | Realtime, intensité 0,8, `FFF1D6`, Soft Shadows |
| Distance des ombres | 80 m, dans `PC_RPAsset` et `Mobile_RPAsset` |
| `Palette.mat` | Smoothness 0 |

## Décisions

- **La forêt est un Terrain Unity, pas des objets dans la scène.** Avec 100 m de côté, la scène stockait déjà 3 000 objets sur 48 900 lignes. À 1 km, il en faudrait plus de 60 000. Le Terrain stocke chaque arbre comme une position, une échelle et une rotation, dans un seul fichier de données. La scène redescend à environ 750 lignes. (Hugo, 26/09)
- **1 km de côté directement.** Si c'est trop petit, on agrandit en ajoutant des Terrains voisins (tuiles). (Hugo)
- **L'outil de répartition est gardé** : même tirage, mêmes seeds, mêmes zones d'exclusion. Seule la sortie change : les arbres du Terrain au lieu d'objets. Une forêt faite à la main sur 1 km prendrait trop de temps. Le résultat est figé dans le Terrain : il est identique à chaque partie.
- **Arbres dessinés jusqu'à 90 m.** Le brouillard masque tout à 70 m : au-delà de 90 m, les dessiner ne se verrait pas et coûterait. (Hugo)
- **Un calque = un prototype d'arbre du Terrain.** Generate réécrit tous les calques d'un coup, parce qu'un Terrain n'a qu'une seule liste d'arbres.
- **Les prefabs ont un `LODGroup` à la racine.** Le Terrain l'exige pour les dessiner comme arbres.
- **La capsule du tronc est mesurée sur le mesh** de `Tree.fbx` : 0,45 m de rayon, 23,4 m de haut. Elle n'a pas de collider sur le feuillage : on passe sous les branches. (Hugo : collisions dans cette PR)
- **Les objets de l'outil sont en `EditorOnly`.** Ils ne font rien en jeu : Unity les retire du jeu livré.
- **Les cailloux n'ont pas de collision.** Ce sont des petits cailloux : 22 cm de large et 8 cm de haut au plus pour `Rock_Large`, avant l'échelle aléatoire (×1,5 au maximum). Buter dessus gênerait le joueur plus qu'autre chose. (Hugo, 26/09)
- **Les colliders des arbres n'existent qu'en jeu.** C'est le fonctionnement du Terrain d'Unity : en mode édition, un rayon traverse les troncs. C'est pour ça que le test de collision est un test PlayMode.
- **`MysticForestTerrainData.asset` passe par Git LFS**, avec la règle `*TerrainData.asset` dans `.gitattributes`. Unity l'enregistre toujours en binaire, même en Force Text : la règle `*.asset eol=lf` l'aurait corrompu. Toute donnée de Terrain doit donc s'appeler `<Nom>TerrainData.asset`.
- **Matériau Terrain par défaut d'URP, sans shader maison.** Le sol est plat, un calque de texture uni suffit. Un shader à facettes (Shader Graph) viendra avec le relief, si on le garde.
- **Résolution de heightmap 513 et hauteur 50 m, provisoires.** Elles seront fixées avec l'outil de relief.
- **Les tirages d'échelle et de rotation sont faits même pour les points exclus.** Ajouter ou déplacer une zone d'exclusion ne rebat pas le reste de la forêt.
- **Prefabs nommés `PF_[Name]`, ScriptableObjects `SO_[Name]`**, comme la STD §6. **Remplace la décision 6 d'ADR-0005 sur ce point.** Un prefab porte toujours le nom du modèle qu'il enveloppe : sans préfixe, `Tree.fbx` et son prefab sont identiques dans les sélecteurs, et placer le FBX par erreur ferait disparaître les colliders sans que personne ne le voie. Les modèles, textures et matériaux restent sans préfixe. `scripts/check-art-assets.sh` exige `PF_` sur les `.prefab`. (Hugo, 26/09)
- **Ombres à 80 m**, au-delà de la fin du brouillard (70 m). À 50 m, la valeur par défaut, les arbres lointains perdaient leur ombre et des bandes éclairées apparaissaient à leur pied. Le réglage vaut pour tout le projet. (Hugo)
- **`Palette.mat` en Smoothness 0.** Sur du low poly, la brillance ajoutait des reflets qui abîmaient les couleurs. Touche tous les modèles. (Hugo)
- **L'obscurité vient de la lumière, pas des couleurs.** Les matériaux restent en tons moyens, la lumière est sombre. Une couleur sombre sous une lumière sombre devient un noir illisible.
- **Fond de caméra de la même couleur que le brouillard.** Le brouillard d'Unity ne s'applique jamais au fond de la caméra : sans ça, le lointain ne se fond pas dans la brume.
- **Reflets d'environnement coupés.** Sans skybox, Unity reflétait quand même son ciel par défaut, ce qui donnait au sol et aux troncs une teinte gris-bleu.
- **Une seule caméra dans la scène.** Une deuxième caméra de priorité plus haute se dessinait par-dessus la Main Camera et affichait le ciel.
- **Pas de caméra de prototype.** Celle qu'avait écrite l'agent de départ n'était utilisée nulle part et aurait été embarquée dans les builds : retirée.
- **Cette forêt est le biome de départ.** La scène `TheClearing` prévue pour le 30/10 ne sera probablement pas faite. (Hugo)

## Tests

### Automatisés (Unity Test Runner)

| Test | Mode | Couvre | Résultat |
|---|---|---|---|
| `SamplerRespectsMinimumDistance` | EditMode | distance minimale | ✅ 26/09, headless |
| `SamplerStaysInsideAreaAndFillsIt` | EditMode | répartition sans trou, dans la zone | ✅ 26/09, headless |
| `SameSeedGivesSameResult` | EditMode | même seed, même répartition | ✅ 26/09, headless |
| `PlacementsRespectScaleRangeRotationAndExclusions` | EditMode | échelle, rotation, zones d'exclusion | ✅ 26/09, headless |
| `GenerateWritesEveryPlacementIntoTheTerrain` | EditMode | chaque emplacement devient un arbre du Terrain | ✅ 26/09, headless |
| `InstancesLandOnTheTerrainAndKeepTheirScale` | EditMode | positions dans le Terrain, échelle gardée | ✅ 26/09, headless |
| `EachLayerBecomesItsOwnPrototype` | EditMode | un prototype par calque | ✅ 26/09, headless |
| `ExclusionZonesStayEmpty` | EditMode | aucun arbre dans une zone d'exclusion | ✅ 26/09, headless |
| `ReseedAndGenerateIsUndoneInOneStep` | EditMode | New seed + Generate forest s'annule en un seul Ctrl+Z, seed comprise | ✅ 26/09, headless |
| `TreeTrunksBlockInPlayMode` | PlayMode | un rayon tiré sur un tronc de la vraie scène s'arrête à la surface du tronc | ✅ 26/09, headless |

### Manuels (dans Unity)

Tout est à rejouer : la forêt a changé de structure.

| # | Étapes | Résultat attendu | OK |
|---|---|---|---|
| 1 | Ouvrir `Assets/Scenes/Biomes/MysticForest.unity`, regarder la Game view. | Aucun ciel. Le fond est vert sombre et se confond avec le brouillard. | |
| 2 | En Scene view, se placer dans la clairière et regarder au loin. | Les arbres disparaissent progressivement dans le brouillard entre 12 et 70 m. Leurs ombres restent visibles jusqu'au brouillard. Aucun arbre n'apparaît ni ne disparaît d'un coup devant la caméra. | |
| 3 | Regarder le sol et les troncs de près. | Couleurs mates, aucun reflet gris-bleu. | |
| 4 | Sélectionner `Terrain/Forest_Trees`, cliquer **Generate forest**. | La forêt se régénère à l'identique (même seed). La Console affiche « Generated 62338 trees on Terrain ». | |
| 5 | Cliquer **New seed + Generate forest**, puis Ctrl+Z une fois. | Une autre répartition apparaît. Un seul Ctrl+Z ramène l'ancienne répartition **et** l'ancienne seed dans l'Inspector. | |
| 6 | Déplacer `StartClearing`, puis **Generate forest**. | Aucun arbre ni caillou dans le cercle rouge, à sa nouvelle position. Remettre `StartClearing` en place et regénérer, ou annuler. | |
| 7 | Cliquer **Clear forest**, puis Ctrl+Z. | Le Terrain est vide, puis la forêt revient telle quelle. | |
| 8 | Ouvrir **Window > Analysis > Physics Debugger**, entrer en Play Mode, regarder les arbres autour de la clairière dans la Scene view. | Chaque tronc a une capsule de collision dessinée dessus, de la même largeur que le tronc. Le feuillage n'en a pas. Hors Play Mode, aucune capsule. | |
| 9 | En Play Mode, ouvrir la fenêtre **Stats** de la Game view, depuis la clairière. | Noter les FPS et la machine dans la PR. Pas de seuil fixé pour l'instant. | |

## Limitations connues

- Le sol est plat : sous-tâche `12487v2bytj`. L'outil de relief et la taille des facettes ne sont pas choisis.
- `MysticForestTerrainData.asset` est binaire : deux personnes ne peuvent pas modifier le Terrain en même temps, Git ne sait pas fusionner. Chaque régénération ajoute environ 4 Mo au stockage LFS du dépôt.
- Les arbres du Terrain ne sont pas des objets : pas de script par arbre, pas d'arbre qu'on abat. Il faudrait les remplacer par un objet au besoin.
- Les colliders des arbres n'existent qu'en jeu. Un outil d'éditeur qui tire un rayon ne les voit pas.
- Les performances à 1 km ne sont pas mesurées (test manuel 9). Le GPU Resident Drawer n'est activé que si c'est nécessaire.
- `MysticForestLighting` a encore les lightmaps bakées activées, alors que toute la lumière est en temps réel. C'est sans effet tant que rien n'est baké, mais ce réglage devra être tranché avant de baker quoi que ce soit.

## Modifié par
