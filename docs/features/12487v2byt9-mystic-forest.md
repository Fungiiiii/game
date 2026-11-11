# Biome Forêt mystique — forêt low poly jouable, relief et outil de répartition

- **Ticket ClickUp :** https://app.clickup.com/t/12487v2byt9
- **Note de design validée :** https://app.clickup.com/t/12487v2bytb — fermée par Hugo le 26/09. Le passage au low poly jouable a été validé par Hugo le 11/11, par sa demande, qui vaut validation de la note.
- **Branche :** `feat/12487v2byt9-mystic-forest`
- **Auteur :** Hugo Bernier
- **Statut :** En cours

## Docs précédents

Aucun.

## Ce que fait la feature

![La vue du spawn : la clairière, la trouée, l'arbre géant au bout](../screenshots/mystic-forest-spawn.png)

Pour le joueur :

- [x] La scène `MysticForest` est une forêt low poly jouable à la première personne. La zone jouable fait 600 × 600 m : 2 min pour la traverser en marchant (5 m/s), un peu plus d'une minute en sprintant (9 m/s).
- [x] Le joueur apparaît posé au sol, au bord sud d'une clairière de départ de 16 m de rayon, sans chute ni glissade.
- [x] Il apparaît face à l'**arbre géant**, à environ 140 m, au bout d'une trouée droite de 14 m de large. Le sol de la trouée est plus clair.
- [x] Le sol a du relief en plateaux et pentes franches, rendu en facettes. Toute pente de la zone jouable est franchissable : 40° au plus, sous la limite de 45° du joueur.
- [x] Les bords de la map sont naturels : deux falaises de 14 m en gradins, impossibles à gravir, couronnées d'une forêt très dense. Aucun mur invisible.
- [x] Trois zones, chacune avec son arbre dominant, son ambiance et sa densité :

  | Zone | Où | Arbre dominant | Ambiance | Densité |
  |---|---|---|---|---|
  | A — Lisière chaude | sud, autour du spawn | `TreeBroad`, feuillu de 14 à 18 m, feuillage dès 6 m | chaude, claire, feuillage vert-jaune | moyenne (1 feuillu tous les 12 m) |
  | B — Futaie sombre | centre | `TreePine`, conifère de 20 à 24 m | sombre, fermée, colonnes de troncs | dense (1 conifère tous les 7,5 m) |
  | C — Bois des géants | nord | `Tree` agrandi (×1,15 à ×1,4) | haute, aérée, rayons au sol | faible (1 arbre tous les 10 m) |

  ![Zone A](../screenshots/mystic-forest-zone-a.png)
  ![Zone B](../screenshots/mystic-forest-zone-b.png)
  ![Zone C](../screenshots/mystic-forest-zone-c.png)

- [x] Les points d'intérêt sont espacés d'environ 120 m. Ce sont des clairières de décor, sans gameplay : l'arbre géant, un amas de rochers à l'est, un cercle de pierres levées au nord, un creux à l'ouest.

  ![L'arbre géant](../screenshots/mystic-forest-giant.png)
  ![Le creux à l'ouest](../screenshots/mystic-forest-hollow.png)

- [x] Des chemins (trouées au sol clair) relient les points d'intérêt en boucle, sans cul-de-sac. Trois petites clairières de respiration coupent la forêt dense entre les zones.
- [x] Les arbres ont une rotation aléatoire. Deux modèles ne poussent jamais l'un dans l'autre, et aucun pied d'arbre ne flotte au-dessus d'une pente.
- [x] Quatre emplacements de Champimaison : sol plat de 7 m de rayon, sans arbre, sur un chemin ou à côté.
- [x] Brouillard vert de 30 à 220 m. Les arbres sont dessinés jusqu'à 200 m. Les ombres restent à 80 m.
- [x] En jeu, on ne traverse pas les troncs, l'arbre géant ni les gros rochers. Les petits cailloux n'ont pas de collision.

![Vue d'ensemble, sans brouillard](../screenshots/mystic-forest-overview.png)

Pour le level design :

- [x] **Relief** (`Terrain/Relief`) : **Generate relief + forest** écrit le heightmap, peint les calques de sol puis régénère la forêt. Le tout s'annule en un seul Ctrl+Z.
- [x] Le relief est déterministe : la même seed et les mêmes réglages redonnent exactement le même heightmap, chez n'importe qui.
- [x] Une zone d'exclusion avec **Flatten Ground** est aplanie à la hauteur de son centre. Toute zone d'exclusion et tout chemin sont peints en sol de clairière, et le sol de plus de 44° est peint en roche.
- [x] **Répartition** : un calque peut avoir un trou rectangulaire (anneau autour de la zone jouable) et une pente maximale.
- [x] Les calques sont prioritaires dans l'ordre de la hiérarchie : un calque n'a plus d'instance à moins de la moitié de la plus petite distance minimale des deux calques d'une instance d'un calque précédent.
- [x] Un chemin (`ScatterExclusionPath`) exclut tout un couloir avec un seul objet.
- [x] Les fonctions de la première version restent : même seed, même forêt ; New seed ; Clear ; un seul Ctrl+Z ; rien dans une zone d'exclusion.

## Hors périmètre

- Herbe, arbustes, spores (note du 11/10) : pas dans cette PR pour l'instant (Hugo, 11/11).
- Gameplay : les points d'intérêt sont du décor.
- La Champimaison elle-même : seuls ses emplacements sont prêts.
- Le contrôleur joueur du jeu : le joueur de cette scène est un joueur de test.
- Éclairage baké : tout reste en temps réel.

## Implémentation

**Scène :** `Assets/Scenes/Biomes/MysticForest.unity`.

| Objet | Rôle |
|---|---|
| `Terrain` | 1000 × 80 × 1000 m en (-500, 0, -500). Heightmap 257 (facettes de 3,9 m), splatmap 512. Données dans `MysticForestTerrainData.asset` (LFS). |
| `Terrain/Relief` | `TerrainRelief` centré en (0, 0) : seed 20716, zone jouable 600 × 600 m, base 20 m, collines ±16 m tous les 170 m, 3 octaves, paliers de 4 m dont 30 % en pente, pente max 40°, 2 falaises de 14 m tous les 30 m, raccord des zones plates sur 6 m. |
| `Terrain/<Zone>_<Arbre>`, `Border_*`, `Rocks_*` | 11 calques `ForestScatter`, dans cet ordre de priorité (tableau ci-dessous). |
| `Clearings` | `StartClearing` (spawn, r 16, aplani), clairières des points d'intérêt, de respiration, et 9 chemins `ScatterExclusionPath`. `EditorOnly`. |
| `ChampimaisonSites/ChampimaisonSite_01..04` | `ChampimaisonSite` (rayon 5 m) gardé dans le jeu, avec un enfant `Clearing` `EditorOnly` (r 7, aplani). |
| `Landmarks` | `PF_TreeGiant` (×1,25), `PF_Rock_Boulder` et `PF_Rock_Spire` posés à la main. |
| `Player` | `CharacterController` + `FirstPersonTestPlayer`, `Main Camera` en enfant à 0,7 m (œil à 1,6 m du sol). Sa position et son orientation sont le spawn. |

| Calque (ordre) | Prefab | Centre, zone | Distance min | Échelle | Pente max | Seed |
|---|---|---|---|---|---|---|
| `ZoneA_TreeBroad` | `PF_TreeBroad` | (0, -185), 600 × 230 | 12 m | 0,85–1,15 | 25° | 31001 |
| `ZoneB_TreePine` | `PF_TreePine` | (0, 25), 600 × 290 | 7,5 m | 0,85–1,15 | 25° | 31003 |
| `ZoneC_Tree` | `PF_Tree` | (0, 215), 600 × 170 | 10 m | 1,15–1,4 | 25° | 31004 |
| `ZoneA_Tree` | `PF_Tree` | (0, -185), 600 × 230 | 16 m | 0,85–1,1 | 25° | 31002 |
| `ZoneB_Tree` (ex-`Forest_Trees`) | `PF_Tree` | (0, 25), 600 × 290 | 9 m | 0,85–1,2 | 25° | 20716 |
| `ZoneC_TreePine` | `PF_TreePine` | (0, 215), 600 × 170 | 14 m | 0,9–1,2 | 25° | 31005 |
| `Border_TreePine` | `PF_TreePine` | (0, 0), 760 × 760, trou 600 × 600 | 3,5 m | 0,9–1,3 | 30° | 31006 |
| `Border_Tree` | `PF_Tree` | idem | 6 m | 0,9–1,25 | 30° | 31007 |
| `Rocks_Large` / `_Medium` / `_Small` (ex-`Forest_Rocks_*`) | `PF_Rock_*` | (0, 0), 640 × 640 | 6 / 8 / 8 m | inchangées | — | 2 / 94770 / 30709 |

Au total, 21 591 instances.

**Modèles** (`Assets/Art/Models/Environment/`, générés par `tools/blender/generate_forest_models.py`) :

| Modèle | Triangles | Taille | Cases de palette | Prefab |
|---|---|---|---|---|
| `Tree` (existant, pour comparer) | 388 | 23,4 m | Wood dark, Green dark | capsule r 0,45 |
| `TreeBroad` | 407 | ~16 m, feuillage dès 6 m | Wood, Leaf warm, Leaf warm shade | capsule r 0,55 × 7 m |
| `TreePine` | 366 | ~22 m, étages dès 4 m | Wood dark, Pine, Pine shade | capsule r 0,45 × 20 m |
| `TreeGiant` | 552 | ~29 m (×1,25 dans la scène) | Bark pale, Giant leaf, Green dark | mesh collider |
| `Rock_Boulder` | 80 | ~3,8 × 3,2 × 2,6 m | Rock base, Rock shadow, Green | mesh collider convexe |
| `Rock_Spire` | 42 | ~2,3 × 5,2 m | Rock light, base, shadow, Green | mesh collider convexe |

Tous ont l'origine au pied, l'échelle 1, un seul matériau `M_Fungi_Palette` remappé sur `Palette.mat`, un `LODGroup` à un seul LOD comme `PF_Tree`.

**Sol :** `TerrainFaceted.mat` (shader Shader Graph `Fungiiiii/TerrainFaceted`), calques `ForestFloor` (6B5436), `ClearingFloor` (8F8150), `Cliff` (7A7276).

**Scripts :**

| Fichier | Rôle |
|---|---|
| `Runtime/World/TerrainRelief.cs` | Réglages du relief (`ReliefSettings`). |
| `Editor/World/ReliefGenerator.cs` | Calcul du heightmap en C# pur, déterministe : bruit à valeur haché, paliers, falaises, zones plates, limite de pente. |
| `Editor/World/TerrainReliefTool.cs`, `TerrainReliefEditor.cs` | Écriture dans le Terrain, peinture du sol, bouton de l'Inspector. |
| `Runtime/World/ScatterExclusionPath.cs` | Chemin d'exclusion en polyligne. |
| `Runtime/World/ForestScatter.cs`, `ScatterExclusionZone.cs`, `Editor/World/TerrainForestGenerator.cs` | Trou, pente max, priorité entre calques, enfoncement sur pente, aplanissement. |
| `Runtime/Player/FirstPersonTestPlayer.cs` | Joueur de test à la première personne. |
| `Runtime/Champimaison/ChampimaisonSite.cs` | Marqueur d'emplacement. |
| `tools/blender/generate_forest_models.py` | Génère les 5 modèles : `blender -b -P tools/blender/generate_forest_models.py` |

## Décisions

- **Low poly jouable, plus clair que la charte v1.** Brouillard de 30 à 220 m au lieu de 12 à 70 m, un peu plus clair (`2C4A3C`). Lumière ambiante relevée à (0,42 ; 0,48 ; 0,42), soleil à 1,1. Il fallait voir un repère à 140 m. La zone B garde la forêt très sombre de la charte. (Hugo, 11/11)
- **Arbres dessinés jusqu'à 200 m**, juste avant la fin du brouillard. Les ombres restent à 80 m, comme avant.
- **Le Terrain de 1 km est gardé.** La zone jouable fait 600 m, entourée de 200 m de falaises et de forêt. Agrandir reste possible par tuiles.
- **Relief écrit par notre outil**, pas par Sylva : Sylva est trop réaliste pour ce jeu. (Hugo)
- **La pente max est limitée par axe à tan(40°)/√2.** C'est ce qui garantit qu'aucun triangle, quelle que soit sa diagonale, ne dépasse 40°. Les zones aplanies ne bougent pas : c'est leur voisin qui s'adapte.
- **Heightmap 257** : facettes de 3,9 m, lisibles en low poly. Le Terrain est dessiné à pleine résolution (`Pixel Error` 1), sinon les facettes changeaient avec la distance.
- **Hauteur de base 20 m.** Le sol reste au-dessus du fond du Terrain, où un arbre ne peut plus être enfoncé.
- **Sol à facettes par Shader Graph** (cible Terrain Lit) : la normale de chaque pixel est celle de son triangle (`cross(ddy, ddx)` de la position). (Hugo, 11/11)
- **L'arbre géant reprend la lecture de `Tree`** : un seul tronc, des contreforts, trois branches à 19 m, des couronnes plates. La version avec deux « jambes » et les cosses lumineuses a été refusée, et la couleur de lueur retirée de la palette. (Hugo, 11/11)
- **L'arbre géant et les gros rochers sont des objets de la scène**, pas des arbres du Terrain. Ils sont uniques, et un arbre de Terrain n'a qu'une capsule pour collider.
- **Priorité entre calques, par ordre de hiérarchie.** Les calques ne se voyaient pas : un feuillu et un `Tree` poussaient l'un dans l'autre. L'arbre dominant de chaque zone passe en premier, les cailloux en dernier. (Hugo, 11/11)
- **Enfoncement selon la pente** : `rayon du pied × échelle × tan(pente) + 5 cm`. Le rayon vient de la capsule du tronc, ou de la largeur du mesh pour un caillou. Les arbres sont aussi limités à 25° de pente. (Hugo, 11/11)
- **La pente est mesurée sur les hauteurs**, pas avec `TerrainData.GetSteepness`, qui pouvait renvoyer une valeur périmée juste après `SetHeights`.
- **`TreeBroad` n'est pas symétrique.** La rotation aléatoire était bien appliquée (vérifié en comparant un rendu avec et sans), mais trois branches à 120° faisaient un arbre identique sous tous les angles. (Hugo, 11/11)
- **Champimaison : une clairière plate exclue de la répartition, plus un petit marqueur.** La parcelle du prototype fait 4,8 m et la maison 3,4 m : 7 m de rayon plat laissent la place et un accès. La zone d'exclusion est `EditorOnly`. Le marqueur `ChampimaisonSite` reste dans le jeu pour que le code puisse retrouver les emplacements.
- **Joueur repris de `sylvatest-player`, mais posé dans la scène** : plus de code qui s'active selon le chemin de la scène. Il lit toujours le clavier et la souris directement, sans les actions de l'Input System : c'est un joueur de test, pas le contrôleur du jeu.
- **`minMoveDistance` à 0 sur le joueur.** À haut FPS, la poussée vers le bas de chaque frame passait sous 1 mm et était ignorée : le joueur n'était plus « au sol ».
- **Chemins en polyligne**, un objet par chemin. Un cercle par segment faisait 250 objets et 15 000 lignes de scène.
- Les décisions de la première version tiennent toujours : Terrain plutôt qu'objets, un calque par prototype, `LODGroup` à la racine, objets d'outil en `EditorOnly`, TerrainData en LFS, ombres à 80 m, `Palette.mat` mat, fond de caméra couleur du brouillard, pas de reflets d'environnement.

## Tests

### Automatisés (Unity Test Runner)

Lancés en batchmode avec Unity 6000.3.21f1 sur la machine de Hugo (11/11). Log et XML lus : EditMode 76/76, PlayMode 16/16.

| Test | Mode | Couvre | Résultat |
|---|---|---|---|
| `SamplerRespectsMinimumDistance`, `SamplerStaysInsideAreaAndFillsIt`, `SameSeedGivesSameResult` | EditMode | tirage de Poisson | ✅ |
| `PlacementsRespectScaleRangeRotationAndExclusions` | EditMode | échelle, rotation, zones d'exclusion | ✅ |
| `InnerHoleStaysEmptyAndLeavesTheRestUnchanged` | EditMode | anneau de bordure | ✅ |
| `PathCirclesCoverTheWholeCorridor` | EditMode | chemins sans trou | ✅ |
| `GenerateWritesEveryPlacementIntoTheTerrain`, `InstancesLandOnTheTerrainAndKeepTheirScale`, `ExclusionZonesStayEmpty` | EditMode | écriture dans le Terrain | ✅ |
| `EachLayerBecomesItsOwnPrototype` | EditMode | un prototype par calque. **Exigence modifiée :** le 2e calque perd ses instances qui chevauchent le 1er | ✅ |
| `InstancesOfDifferentLayersNeverGrowIntoEachOther` | EditMode | priorité entre calques | ✅ |
| `NoInstanceOnGroundSteeperThanTheLayerAllows` | EditMode | pente max par calque | ✅ |
| `InstancesOnASlopeSinkSoNoSideFloats` | EditMode | aucun pied ne flotte | ✅ |
| `ReseedAndGenerateIsUndoneInOneStep` | EditMode | un seul Ctrl+Z | ✅ |
| `SameSeedGivesSameRelief` | EditMode | relief déterministe | ✅ |
| `ReliefIsNotFlat` | EditMode | collines | ✅ |
| `NoWalkableTriangleIsSteeperThanTheLimit` | EditMode | pente ≤ 40° sur 4 seeds | ✅ |
| `TheBorderRisesInCliffsTooSteepToWalk` | EditMode | falaises infranchissables | ✅ |
| `FlatAreasAreLevel` | EditMode | zones aplanies | ✅ |
| `ApplyWritesTheHeightmapAndIsUndoneInOneStep` | EditMode | écriture du relief, Ctrl+Z | ✅ |
| `TreeTrunksBlockInPlayMode` | PlayMode | collision des troncs. **Modifié :** vise un arbre sur sol plat, sinon le rayon horizontal touche la pente | ✅ |
| `PlayerSpawnsOnTheGroundWithoutFalling` | PlayMode | spawn posé, ni chute ni glissade | ✅ |
| `PlayerCanWalkTowardsTheGiantTree` | PlayMode | pas de blocage au spawn | ✅ |
| `SpawnFacesTheGiantTree` | PlayMode | spawn face au géant, vue dégagée | ✅ |

### Manuels (dans Unity)

| # | Étapes | Résultat attendu | OK |
|---|---|---|---|
| 1 | Ouvrir `Assets/Scenes/Biomes/MysticForest.unity`, Play. | Le joueur est au sol, face à une trouée. L'arbre géant se voit au bout, pâle dans le brouillard. Aucune erreur dans la Console. | |
| 2 | Marcher (ZQSD / WASD, Maj pour sprinter, Espace pour sauter) jusqu'au géant en suivant le sol clair. | Environ 30 s de marche. On tourne autour du géant, on ne traverse ni le tronc ni les contreforts. | |
| 3 | Suivre les chemins clairs : amas de rochers à l'est, pierres levées au nord, creux à l'ouest, retour au spawn. | Une boucle complète sans cul-de-sac. Chaque zone a son arbre et son ambiance (A claire et chaude, B sombre et dense, C haute et aérée). | |
| 4 | Monter et descendre les pentes, partout dans la zone jouable. | Aucune pente ne bloque le joueur. | |
| 5 | Aller jusqu'à un bord de la map et essayer de grimper, en sautant. | Une falaise infranchissable, couronnée de forêt dense. Aucun mur invisible. | |
| 6 | Regarder les pieds des arbres sur les pentes, et les arbres proches les uns des autres. | Aucun pied ne flotte. Aucun arbre ne pousse dans un autre. Les arbres d'un même type n'ont pas tous la même orientation. | |
| 7 | Regarder le sol de près et de loin. | Le sol est en facettes, qui ne changent pas avec la distance. Les falaises sont grises, les clairières et les chemins plus clairs. | |
| 8 | Sélectionner `Terrain/Relief`, cliquer **Generate relief + forest**, puis Ctrl+Z. | Rien ne change (même seed). Après Ctrl+Z, toujours rien. La Console affiche « Relief generated, 21591 trees regenerated ». | |
| 9 | **New seed + Generate relief + forest**, puis Ctrl+Z une fois. | Un autre relief et une autre forêt. Un seul Ctrl+Z ramène l'ancien relief, l'ancienne forêt **et** l'ancienne seed. | |
| 10 | Sélectionner `ChampimaisonSites/ChampimaisonSite_01..04`. | Chaque site est sur un sol plat, vide d'arbres, accessible à pied. | |
| 11 | Window > Analysis > Physics Debugger, Play. | Capsules sur les troncs, mesh colliders sur le géant et les gros rochers, rien sur les petits cailloux. | |
| 12 | Play, fenêtre **Stats**, depuis le spawn, puis au centre de la zone B. | Noter les FPS dans la PR. Objectif : 120 FPS ou plus sur RX 6800 XT + i5-12400F. | |

## Limitations connues

- **Les FPS ne sont pas mesurés** (test manuel 12). Rien ne garantit encore 120 FPS avec des arbres dessinés à 200 m.
- Le `Shader Graph` du sol affiche à l'import des avertissements sur ses shaders `AddPass` et `BaseMapGen` introuvables. Sans effet visible avec 3 calques et `Basemap Distance` à 1000 m, mais non résolu.
- La pente est limitée par axe, de façon prudente : en pratique, les pentes de la zone jouable restent plutôt sous 31° que sous 40°.
- La zone B, sous les conifères serrés, est très sombre : c'est voulu, mais à confirmer en jeu.
- `MysticForestTerrainData.asset` est binaire : chaque régénération ajoute quelques Mo au stockage LFS.
- Les colliders des arbres de Terrain n'existent qu'en jeu.
- `docs/screenshots/mystic-forest.png` (l'ancienne capture) n'est plus référencé. Il est gardé : supprimer un asset demande l'accord de Hugo.

## Modifié par
