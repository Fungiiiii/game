# Biome Forêt mystique — forêt low poly jouable, relief, ambiance et outil de répartition

- **Ticket ClickUp :** https://app.clickup.com/t/12487v2byt9
- **Note de design validée :** https://app.clickup.com/t/12487v2bytb — fermée par Hugo le 26/09. Le passage au low poly jouable (11/11) puis l'ambiance (13/11) ont été validés par Hugo : ses demandes valent validation de la note.
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
- [x] Il apparaît face à l'**arbre géant** (environ 52 m de haut), à environ 140 m, au bout d'une trouée droite de 14 m de large. Le sol de la trouée est plus clair.
- [x] **Ambiance pesante et magique** : un ciel en dégradé gris-bleu, un brouillard bleu-violet qui se referme entre 15 et 185 m, et un soleil chaud et bas aux ombres nettes. L'étalonnage rend les ombres froides et les lumières chaudes, avec un vignettage. Des spores bleu clair et violet clair tombent lentement autour du joueur.
- [x] Le sol a un relief doux en plateaux et pentes franches, rendu en petites facettes de 2 m, sans bosses au hasard. Toute pente de la zone jouable est franchissable : 40° au plus, sous la limite de 45° du joueur.
- [x] Les bords de la map sont naturels : une ligne de falaises irrégulière, en gradins de hauteurs variées, aux faces rocheuses accidentées et semées de gros rochers au pied, couronnée d'une forêt très dense. Impossible à gravir, et aucun mur invisible.
- [x] **Sous-bois** : herbe par plaques (jamais sur les chemins, dans les clairières ni sur les pentes raides), buissons, fougères et groupes de champignons rouges et violets.
- [x] Les arbres poussent **en bosquets**, avec des trouées entre eux, au lieu d'une plantation régulière.
- [x] Trois zones, chacune avec son arbre dominant, son ambiance et sa densité :

  | Zone | Où | Arbre dominant | Sous-bois | Ambiance |
  |---|---|---|---|---|
  | A — Lisière chaude | sud, autour du spawn | `TreeBroad`, feuillu de 14 à 18 m, en bosquets | buissons, herbe | chaude, feuillage vert-jaune au soleil |
  | B — Futaie sombre | centre | `TreePine`, conifère de 20 à 30 m, serré, avec des trouées | fougères, herbe | sombre et fermée, colonnes de troncs, taches de lumière |
  | C — Bois des géants | nord | `Tree` agrandi (×1,1 à ×1,5), en grands bosquets | fougères, buissons | haute et aérée |

  ![Zone A](../screenshots/mystic-forest-zone-a.png)
  ![Zone B](../screenshots/mystic-forest-zone-b.png)
  ![Zone C](../screenshots/mystic-forest-zone-c.png)

- [x] Les points d'intérêt sont espacés d'environ 120 m. Ce sont des clairières de décor, sans gameplay : l'arbre géant entouré de champignons, un amas de rochers à l'est, un cercle de pierres levées au nord, un creux à l'ouest.

  ![L'arbre géant](../screenshots/mystic-forest-giant.png)
  ![Le creux à l'ouest](../screenshots/mystic-forest-hollow.png)

- [x] **Couleurs du sol** : terre brune sous les arbres, mousse verte dans les clairières et sous les plaques d'herbe, terre de chemin sableuse, roche grise sur les falaises.
- [x] Des chemins relient les points d'intérêt en boucle, sans cul-de-sac. Ce sont des courbes lisses, sur un sol aplani : un sentier de terre sableuse au milieu d'une trouée bordée de mousse. Trois petites clairières de respiration coupent la forêt dense entre les zones.
- [x] Les arbres ont une rotation aléatoire et une taille bien différente d'un arbre à l'autre : leur hauteur varie aussi indépendamment de leur largeur, certains plus élancés, d'autres plus trapus. Deux modèles ne poussent jamais l'un dans l'autre, et aucun pied d'arbre ne flotte au-dessus d'une pente.
- [x] Quatre emplacements de Champimaison : sol plat de 7 m de rayon, sans arbre ni herbe, sur un chemin ou à côté.
- [x] Les arbres sont dessinés jusqu'à 200 m, l'herbe jusqu'à 75 m. Les ombres restent à 80 m.
- [x] **Vent** : l'herbe penche et ondule, par rafales qui traversent les plaques. Le pied des touffes ne bouge pas.
- [x] **Brume au sol** : une brume basse bleu-violet dérive lentement dans le creux et dans les trois clairières de respiration.

  ![Brume au sol près du creux](../screenshots/mystic-forest-mist.png)
- [x] En jeu, on ne traverse pas les troncs, l'arbre géant ni les gros rochers posés à la main. Les petits cailloux, l'herbe, les buissons, les fougères, les champignons et les rochers du pied des falaises n'ont pas de collision.

![Vue d'ensemble, sans brouillard](../screenshots/mystic-forest-overview.png)

Pour le level design :

- [x] **Relief** (`Terrain/Relief`) : **Generate relief + forest** écrit le heightmap, peint les calques de sol puis régénère la forêt et l'herbe. Le tout s'annule en un seul Ctrl+Z.
- [x] Le relief est déterministe : la même seed et les mêmes réglages redonnent exactement le même heightmap, chez n'importe qui.
- [x] Une zone d'exclusion avec **Flatten Ground** est aplanie à la hauteur de son centre. Toute zone d'exclusion et tout chemin sont peints en sol de clairière, à bord net, et le sol de plus de 44° est peint en roche.
- [x] **Répartition** : un calque peut avoir un trou rectangulaire (anneau autour de la zone jouable), une pente maximale et des **bosquets** (taille, part de la zone couverte).
- [x] **Herbe** (`Terrain/Grass`) : écrite par **Generate forest** comme couche de détail du Terrain, par plaques, de façon reproductible (seed). **Clear forest** l'efface aussi.
- [x] Les calques sont prioritaires dans l'ordre de la hiérarchie : un calque n'a plus d'instance à moins de la moitié de la plus petite distance minimale des deux calques d'une instance d'un calque précédent.
- [x] Un chemin (`ScatterExclusionPath`) exclut tout un couloir avec un seul objet.
- [x] Les fonctions de la première version restent : même seed, même forêt ; New seed ; Clear ; un seul Ctrl+Z ; rien dans une zone d'exclusion.

## Hors périmètre

- Le vent sur les buissons, les fougères et les arbres : seul l'herbe bouge.
- Le son : aucun son n'est dans la scène. La liste des sons possibles a été remise à Hugo le 17/11.
- L'eau et les rais de lumière.
- Gameplay : les points d'intérêt sont du décor.
- La Champimaison elle-même : seuls ses emplacements sont prêts.
- Le contrôleur joueur du jeu : le joueur de cette scène est un joueur de test.
- Éclairage baké : tout reste en temps réel.

## Implémentation

**Scène :** `Assets/Scenes/Biomes/MysticForest.unity`.

| Objet | Rôle |
|---|---|
| `Terrain` | 1000 × 80 × 1000 m en (-500, 0, -500). Heightmap 513 (facettes de 1,95 m), splatmap 1024, détails 512 × 512 (cases de 2 m). Données dans `MysticForestTerrainData.asset` (LFS). |
| `Terrain/Relief` | `TerrainRelief` centré en (0, 0) : seed 20716, zone jouable 600 × 600 m, base 20 m, collines ±11 m tous les 220 m, 2 octaves, paliers de 4 m dont 30 % en pente, pente max 40°. Falaises : 2 gradins de 14 m (±25 %) tous les 30 m, ligne qui ondule de ±12 m, rugosité de ±3 m. Sol jouable : bruit doux de ±0,3 m (longueur d'onde 9 m). Chemins lissés sur 6 m de part et d'autre de leur axe. Raccord des zones plates sur 6 m. |
| `Terrain/Grass` | `TerrainGrass` : touffe `GrassTuft`, seed 31201, plaques de 14 m, 68 % du sol, 12 touffes au plus par case de 2 m, pente max 30°. Touffes à l'échelle 0,9–1,3 en largeur, 0,7–1,3 en hauteur. |
| `Terrain/<calques>` | 17 calques `ForestScatter`, dans l'ordre de priorité du tableau ci-dessous. |
| `Clearings` | `StartClearing` (spawn, r 16, aplani), clairières des points d'intérêt, de respiration, et 9 chemins `ScatterExclusionPath` (trouée de 10 m, 14 m vers le géant ; sentier peint sur 45 % de la largeur). `EditorOnly`. |
| `ChampimaisonSites/ChampimaisonSite_01..04` | `ChampimaisonSite` (rayon 5 m) gardé dans le jeu, avec un enfant `Clearing` `EditorOnly` (r 7, aplani). |
| `Landmarks` | `PF_TreeGiant` (×1,8), gros rochers, pierres levées, champignons et fougères posés à la main au spawn et autour du géant. |
| `Player` | `CharacterController` + `FirstPersonTestPlayer`, `Main Camera` en enfant à 0,7 m (œil à 1,6 m du sol), post-traitement et SMAA activés. Sa position et son orientation sont le spawn. |
| `Player/Main Camera/Spores` | `ParticleSystem` simulé dans le monde : boîte de 36 × 10 × 36 m autour de la caméra, 90 spores par seconde, vie de 7 à 11 s, entre bleu clair et violet clair, qui tombent lentement. Matériau `ParticlesUnlit` d'URP. |
| `Mist/PF_GroundMist` ×4 | `ParticleSystem` simulé dans le monde : boîte de 50 × 1,2 × 50 m à 0,8 m du sol, 5 particules par seconde de 10 à 18 m, vie de 18 à 28 s, opacité 22 %, dérive dans le sens du vent. Matériau `GroundMist.mat` (`ParticlesUnlit` d'URP, particules douces). Au creux et aux trois clairières de respiration. |
| `PostProcess` | `Volume` global, profil `SO_MysticForestVolume.asset` : tonemapping Neutral, exposition +0,25, contraste +16, saturation +4, split toning (ombres bleu-violet, lumières chaudes), bloom léger, vignettage 0,36. |

| Calque (ordre) | Prefab | Centre, zone | Distance min | Échelle | Pente max | Bosquets (taille, part) | Seed |
|---|---|---|---|---|---|---|---|
| `ZoneA_TreeBroad` | `PF_TreeBroad` | (0, -185), 600 × 230 | 8,5 m | 0,7–1,35 | 25° | 45 m, 55 % | 31001 |
| `ZoneB_TreePine` | `PF_TreePine` | (0, 25), 600 × 290 | 6,5 m | 0,7–1,45 | 25° | 32 m, 70 % | 31003 |
| `ZoneC_Tree` | `PF_Tree` | (0, 215), 600 × 170 | 9 m | 1–1,65 | 25° | 60 m, 55 % | 31004 |
| `ZoneA_Tree` | `PF_Tree` | (0, -185), 600 × 230 | 16 m | 0,75–1,25 | 25° | — | 31002 |
| `ZoneB_Tree` (ex-`Forest_Trees`) | `PF_Tree` | (0, 25), 600 × 290 | 9 m | 0,75–1,35 | 25° | — | 20716 |
| `ZoneC_TreePine` | `PF_TreePine` | (0, 215), 600 × 170 | 14 m | 0,75–1,45 | 25° | — | 31005 |
| `Border_TreePine` | `PF_TreePine` | (0, 0), 760 × 760, trou 600 × 600 | 3,5 m | 0,8–1,45 | 30° | — | 31006 |
| `Border_Tree` | `PF_Tree` | idem | 6 m | 0,8–1,4 | 30° | — | 31007 |
| `Border_Rocks` | `PF_Rock_Boulder` | (0, 0), 700 × 700, trou 580 × 580 | 13 m | 1–2,4 | — | — | 31106 |
| `ZoneA_Bush` | `PF_Bush` | zone A | 6 m | 0,7–1,3 | 30° | 25 m, 50 % | 31101 |
| `ZoneC_Bush` | `PF_Bush` | zone C | 9 m | 0,8–1,4 | 30° | 35 m, 40 % | 31104 |
| `Mushrooms` | `PF_MushroomCluster` | (0, 0), 600 × 600 | 7 m | 0,8–1,7 | 30° | 28 m, 30 % | 31105 |
| `ZoneB_Fern` | `PF_Fern` | zone B | 2,6 m | 0,8–1,6 | 30° | 22 m, 55 % | 31102 |
| `ZoneC_Fern` | `PF_Fern` | zone C | 4 m | 0,8–1,5 | 30° | 30 m, 40 % | 31103 |
| `Rocks_Large` / `_Medium` / `_Small` (ex-`Forest_Rocks_*`) | `PF_Rock_*` | (0, 0), 640 × 640 | 6 / 8 / 8 m | inchangées | — | — | 2 / 94770 / 30709 |

Les 8 calques d'arbres ont en plus une variation de hauteur de ±15 %, tirée indépendamment de l'échelle. Au total, 26 765 instances d'arbres de Terrain, plus l'herbe.

**Modèles** (`Assets/Art/Models/Environment/`, générés par `tools/blender/generate_forest_models.py`) :

| Modèle | Triangles | Taille | Cases de palette | Prefab |
|---|---|---|---|---|
| `Tree` (existant, pour comparer) | 388 | 23,4 m | Wood dark, Green dark | capsule r 0,45 |
| `TreeBroad` | 407 | ~16 m, feuillage dès 6 m | Wood, Leaf warm, Leaf warm shade | capsule r 0,55 × 7 m |
| `TreePine` | 366 | ~22 m, étages dès 4 m | Wood dark, Pine, Pine shade | capsule r 0,45 × 20 m |
| `TreeGiant` | 562 | ~29 m (×1,8 dans la scène) | Bark pale, Giant leaf, Green dark | mesh collider |
| `Rock_Boulder` | 80 | ~3,8 × 3,2 × 2,6 m | Rock base, Rock shadow, Green | mesh collider convexe |
| `Rock_Spire` | 42 | ~2,3 × 5,2 m | Rock light, base, shadow, Green | mesh collider convexe |
| `GrassTuft` | 120 | 22 à 45 cm, 12 brins plats et courbes en trois tons (pied couleur mousse, milieu vert, pointe claire), normales vers le haut | Grass shade, Grass, Grass tip | — (détail du Terrain, pas de prefab) |
| `Bush` | 120 | ~1,3 m | Green, Green dark | sans collider |
| `Fern` | 56 | ~0,5 m, 7 frondes fermées avec une nervure | Green dark, Pine shade | sans collider |
| `MushroomCluster` | 200 | ~0,6 m, 4 champignons à chapeau rond et points clairs | Mushroom red, Mushroom violet, Body light, Body base, White | sans collider |

Tous ont l'origine au pied, l'échelle 1, un seul matériau `M_Fungi_Palette` remappé sur `Palette.mat`, un `LODGroup` à un seul LOD.

**Sol et ciel :** `TerrainFaceted.mat` (Shader Graph `Fungiiiii/TerrainFaceted`), 4 calques dans cet ordre : `ForestFloor` (terre, 6B5436), `MossFloor` (mousse, 5E7036), `Cliff` (roche, 7A7276), `PathSoil` (chemin, A38D5E). Ciel `SkyGradient.mat` (shader `Fungiiiii/SkyGradient`) : haut 748093, horizon = brouillard 344758, bas 1E2630. Lumière ambiante Trilight : ciel 5E6E8C, horizon 4C5852, sol 504230. Soleil FFDFAE, intensité 1,3, ombres dures, force 0,85, à 34° de hauteur.

**Scripts :**

| Fichier | Rôle |
|---|---|
| `Runtime/World/ValueNoise.cs` | Bruit à valeur haché par seed, partagé par le relief, les bosquets et l'herbe. |
| `Runtime/World/TerrainRelief.cs` | Réglages du relief (`ReliefSettings`). |
| `Editor/World/ReliefGenerator.cs` | Calcul du heightmap en C# pur, déterministe : paliers, falaises irrégulières, rugosité, zones plates, limite de pente. |
| `Editor/World/TerrainReliefTool.cs`, `TerrainReliefEditor.cs` | Écriture dans le Terrain, peinture du sol, bouton de l'Inspector. |
| `Runtime/World/TerrainGrass.cs`, `Editor/World/GrassDensity.cs` | Réglages et calcul déterministe de l'herbe. |
| `Runtime/World/ScatterExclusionPath.cs` | Chemin d'exclusion en courbe (Catmull-Rom). |
| `Runtime/World/ForestScatter.cs`, `ScatterExclusionZone.cs`, `Editor/World/TerrainForestGenerator.cs` | Trou, pente max, bosquets, priorité entre calques, enfoncement sur pente, aplanissement, écriture de l'herbe. |
| `Runtime/Player/FirstPersonTestPlayer.cs` | Joueur de test à la première personne. |
| `Runtime/Champimaison/ChampimaisonSite.cs` | Marqueur d'emplacement. |
| `Art/Shaders/SkyGradient.shader` | Ciel en dégradé à trois couleurs pour URP. |
| `tools/blender/generate_forest_models.py` | Génère les 9 modèles : `blender -b -P tools/blender/generate_forest_models.py` |

## Décisions

- **Ambiance pesante et magique.** Brouillard bleu-violet de 15 à 185 m, ciel gris-bleu, lumière ambiante plus basse, ombres froides, vignettage. Les seules couleurs vives sont les champignons et les spores. (Hugo, 13/11)
- **Premier plan chaud, fond froid**, comme le demande la charte d'ambiance. Le soleil est chaud et la lumière ambiante du sol aussi ; le ciel, le brouillard et les ombres sont froids.
- **Ciel en dégradé dont l'horizon a la couleur du brouillard.** Le brouillard d'Unity ne s'applique pas au ciel : avec un horizon de la même couleur, les arbres lointains se fondent dedans. Remplace « Aucun ciel » de la première version.
- **Ombres dures.** Les ombres douces brouillaient les arêtes du low poly.
- **Post-traitement par un Volume de la scène**, pas par le profil par défaut du projet : les autres scènes ne changent pas. L'étalonnage reste léger, sinon le feuillage de la zone A virait au jaune fluo et le ciel au bleu nuit.
- **Vent : un seul shader, sur l'herbe seulement** (Hugo, 17/11 : « le choix le plus simple »). `Fungiiiii/GrassWind` déplace les sommets selon leur hauteur au-dessus du pied de la touffe ; l'éclairage est un Lambert simple, proche du matériau palette. `GrassTuft.fbx` pointe sur `GrassWind.mat` au lieu de `Palette.mat`. Les buissons, fougères et arbres gardent `Palette.mat`.
- **Brume : un seul prefab de particules** (`PF_GroundMist`), posé à la main à 4 endroits sous l'objet `Mist`, plutôt qu'un brouillard de hauteur (URP n'en a pas). 140 grandes particules au plus par endroit, en fondu doux contre le sol : la caméra de la scène demande donc la texture de profondeur.
- **Spores bleu clair et violet clair** (Hugo, 13/11), simulées dans le monde (elles ne tournent pas avec la caméra), qui tombent lentement : l'air paraît lourd. Coût fixe partout (900 au plus).
- **Arbres en bosquets** : un bruit par calque garde les arbres là où il dépasse un seuil. Les tirages de position, d'échelle et de rotation ne changent pas : on retire des arbres, on n'en rebat aucun.
- **Herbe en détails du Terrain**, en mode « nombre d'instances ». En mode « couverture », celui par défaut, les valeurs 1 à 5 étaient lues comme une part de 255 et presque rien ne s'affichait.
- **La touffe d'herbe est exportée avec Apply Transform.** Les détails du Terrain ignorent la rotation de -90° sur X de la racine importée : l'herbe s'affichait couchée. C'est la seule exception à la règle d'export.
- **`Palette.mat` en GPU instancing** : les détails instanciés l'exigent.
- **Heightmap 513 et bruit doux de ±0,3 m sur le sol** : des facettes de 2 m qui accrochent la lumière. Une valeur au hasard par échantillon rendait le sol cabossé : remplacée par un bruit continu. Collines moins hautes (±11 m) et plus larges (220 m). (Hugo, 13/11)
- **Chemins en courbes lisses, sur un sol aplani** : un chemin se lit par sa régularité. Le sentier sableux n'occupe que le milieu de la trouée, bordé de mousse. (Hugo, 13/11)
- **Herbe stylisée, réécrite de zéro** (Hugo, 13/11), sur la technique courante des jeux stylisés : toutes les normales des brins pointent vers le haut, donc l'herbe est éclairée comme le sol au lieu de faire une mosaïque de facettes claires et sombres. Brins plats et courbés vers l'extérieur, dégradé en trois tons du pied (couleur de la mousse) à la pointe claire. Chaque brin a un recto et un verso à quelques millimètres : plein des deux côtés. Plaques denses (12 touffes au plus par case de 2 m), dessinées jusqu'à 75 m pour compenser.
- **Quatre couleurs de sol** : terre, mousse, chemin, roche. Mousse sous les plaques d'herbe (le même bruit, un peu plus large) : l'herbe pousse sur du vert, pas sur de la terre nue. Quatre calques, c'est le maximum du shader du sol en une passe. (Hugo, 13/11)
- **Modèles fermés** : Unity ne dessine qu'un côté des faces. Les brins d'herbe sont des pointes à trois faces, les frondes de fougère des losanges fermés, et les contreforts du géant ont toutes leurs faces tournées vers l'extérieur (certaines étaient retournées : on voyait à travers). (Hugo, 13/11)
- **La limite de pente a changé d'algorithme.** L'ancienne relaxation par paires oscillait sans fin à 513 échantillons, entre les clairières plates et la rugosité. La nouvelle propage d'abord un plancher depuis les clairières, puis ne fait que descendre le sol, jamais sous ce plancher. Les deux étapes sont monotones : elle s'arrête toujours.
- **Falaises irrégulières** : la ligne ondule, les gradins varient de ±25 %, les faces sont rugueuses et des rochers sont semés à leur pied. Le bord ne se lit plus comme un mur droit.
- **Géant à ×1,8** (environ 52 m) : à ×1,25, il ne dépassait pas assez la canopée.
- **Bords nets des zones peintes** (0,75 m) : le low poly se lit avec des arêtes, pas des dégradés.
- **Arbres dessinés jusqu'à 200 m**, juste avant la fin du brouillard. Les ombres restent à 80 m.
- **Le Terrain de 1 km est gardé.** La zone jouable fait 600 m, entourée de 200 m de falaises et de forêt.
- **Relief écrit par notre outil** : un générateur de terrain réaliste ne colle pas au style low poly du jeu. (Hugo)
- **La pente max est limitée par axe à tan(40°)/√2.** C'est ce qui garantit qu'aucun triangle, quelle que soit sa diagonale, ne dépasse 40°.
- **Hauteur de base 20 m.** Le sol reste au-dessus du fond du Terrain, où un arbre ne peut plus être enfoncé.
- **Sol à facettes par Shader Graph** (cible Terrain Lit) : la normale de chaque pixel est celle de son triangle. (Hugo, 11/11)
- **L'arbre géant reprend la lecture de `Tree`** : un seul tronc, des contreforts, trois branches, des couronnes plates. Pas de « jambes » ni de cristaux lumineux. (Hugo, 11/11)
- **L'arbre géant et les gros rochers posés à la main sont des objets de la scène**, pas des arbres du Terrain : ils sont uniques, et un arbre de Terrain n'a qu'une capsule pour collider.
- **Tailles bien différentes entre arbres d'un même modèle** (Hugo, 13/11) : fourchettes d'échelle élargies (environ ×0,7 à ×1,45) et hauteur tirée à part (±15 %). L'enfoncement sur pente suit la largeur, pas la hauteur. Ajouter ce tirage a rebattu les échelles et les rotations, pas les positions.
- **Priorité entre calques, par ordre de hiérarchie** : arbres dominants d'abord, sous-bois ensuite, cailloux en dernier. (Hugo, 11/11)
- **Enfoncement selon la pente** : `rayon du pied × échelle × tan(pente) + 5 cm`. Les arbres sont limités à 25° de pente. (Hugo, 11/11)
- **La pente est mesurée sur les hauteurs**, pas avec `TerrainData.GetSteepness`, qui pouvait renvoyer une valeur périmée juste après `SetHeights`.
- **`TreeBroad` n'est pas symétrique**, sinon la rotation aléatoire ne se voyait pas. (Hugo, 11/11)
- **Champimaison : une clairière plate exclue de la répartition, plus un petit marqueur** `ChampimaisonSite` gardé dans le jeu.
- **Joueur de test à la première personne, posé dans la scène** : sa position et son orientation sont le spawn. Il lit le clavier et la souris directement : c'est un joueur de test, pas le contrôleur du jeu. `minMoveDistance` à 0, sinon il n'était plus « au sol » à haut FPS.
- **Un objet par chemin**, au lieu d'un cercle d'exclusion par segment.
- Les décisions de la première version tiennent toujours : Terrain plutôt qu'objets, un calque par prototype, `LODGroup` à la racine, objets d'outil en `EditorOnly`, TerrainData en LFS, ombres à 80 m, `Palette.mat` mat, pas de reflets d'environnement.

## Tests

### Automatisés (Unity Test Runner)

Lancés en batchmode avec Unity 6000.3.21f1 sur la machine de Hugo (17/11, après le vent et la brume). Le vent et la brume n'ont pas de test automatisé : ce sont des réglages visuels. Log et XML lus : **EditMode 82/82, PlayMode 17/17.**

| Test | Mode | Couvre | Résultat |
|---|---|---|---|
| `SamplerRespectsMinimumDistance`, `SamplerStaysInsideAreaAndFillsIt`, `SameSeedGivesSameResult` | EditMode | tirage de Poisson | ✅ |
| `PlacementsRespectScaleRangeRotationAndExclusions` | EditMode | échelle, rotation, zones d'exclusion | ✅ |
| `InnerHoleStaysEmptyAndLeavesTheRestUnchanged` | EditMode | anneau de bordure | ✅ |
| `GrovesLeaveOpenGroundWithoutReshufflingTheTrees` | EditMode | bosquets | ✅ |
| `HeightVariesApartFromWidthWithinItsShare` | EditMode | hauteur variée à part de la largeur | ✅ |
| `PathCirclesCoverTheWholeCorridor` | EditMode | chemins sans trou. **Exigence modifiée :** courbes lisses passant par chaque point, au lieu d'une polyligne | ✅ |
| `PathGroundIsSmootherThanTheGroundAround` | EditMode | sol aplani le long des chemins | ✅ |
| `SameSeedGivesSameGrass` | EditMode | herbe déterministe | ✅ |
| `GrassGrowsInPatchesWithBareGroundBetween` | EditMode | herbe par plaques | ✅ |
| `NoGrassInClearingsPathsOrOnSteepGround` | EditMode | herbe absente des clairières, chemins, pentes | ✅ |
| `GenerateWritesEveryPlacementIntoTheTerrain`, `InstancesLandOnTheTerrainAndKeepTheirScale`, `ExclusionZonesStayEmpty` | EditMode | écriture dans le Terrain | ✅ |
| `EachLayerBecomesItsOwnPrototype` | EditMode | un prototype par calque. **Exigence modifiée :** le 2e calque perd ses instances qui chevauchent le 1er | ✅ |
| `InstancesOfDifferentLayersNeverGrowIntoEachOther` | EditMode | priorité entre calques | ✅ |
| `NoInstanceOnGroundSteeperThanTheLayerAllows` | EditMode | pente max par calque | ✅ |
| `InstancesOnASlopeSinkSoNoSideFloats` | EditMode | aucun pied ne flotte | ✅ |
| `ReseedAndGenerateIsUndoneInOneStep` | EditMode | un seul Ctrl+Z | ✅ |
| `SameSeedGivesSameRelief` | EditMode | relief déterministe | ✅ |
| `ReliefIsNotFlat` | EditMode | collines | ✅ |
| `NoWalkableTriangleIsSteeperThanTheLimit` | EditMode | pente ≤ 40° sur 4 seeds, rugosité comprise | ✅ |
| `TheBorderRisesInCliffsTooSteepToWalk` | EditMode | falaises infranchissables | ✅ |
| `FlatAreasAreLevel` | EditMode | zones aplanies | ✅ |
| `ApplyWritesTheHeightmapAndIsUndoneInOneStep` | EditMode | écriture du relief, Ctrl+Z | ✅ |
| `TreeTrunksBlockInPlayMode` | PlayMode | collision des troncs. **Modifié :** vise un arbre sur sol plat | ✅ |
| `PlayerSpawnsOnTheGroundWithoutFalling` | PlayMode | spawn posé, ni chute ni glissade | ✅ |
| `PlayerCanWalkTowardsTheGiantTree` | PlayMode | pas de blocage au spawn | ✅ |
| `SpawnFacesTheGiantTree` | PlayMode | spawn face au géant, vue dégagée | ✅ |
| `SporesFloatAroundTheCameraInWorldSpace` | PlayMode | spores sur la caméra, simulées dans le monde, présentes en jeu | ✅ |

### Manuels (dans Unity)

| # | Étapes | Résultat attendu | OK |
|---|---|---|---|
| 1 | Ouvrir `Assets/Scenes/Biomes/MysticForest.unity`, Play. | Le joueur est au sol, face à une trouée. L'arbre géant se voit au bout, pâle dans le brouillard bleu-violet. Des champignons au pied des deux rochers. Aucune erreur dans la Console. | |
| 2 | Regarder autour de soi sans bouger. | Ciel gris-bleu en dégradé, horizon de la couleur du brouillard. Spores bleu clair et violet clair qui tombent lentement ; en tournant la tête, elles restent en place dans l'air. | |
| 3 | Marcher (ZQSD / WASD, Maj pour sprinter, Espace pour sauter) jusqu'au géant en suivant le sol clair. | Environ 30 s de marche. Le géant dépasse nettement la canopée, des champignons à ses racines. On ne traverse ni le tronc ni les contreforts. | |
| 4 | Suivre les chemins clairs : amas de rochers à l'est, pierres levées au nord, creux à l'ouest, retour au spawn. | Une boucle complète sans cul-de-sac. Chaque zone a son arbre, son sous-bois et son ambiance. Les arbres forment des bosquets avec des trouées. | |
| 5 | Regarder le sol en marchant, de près, en tournant autour des touffes, des fougères et du pied du géant. | Herbe en plaques sur de la mousse, jamais sur les chemins ni dans les clairières. Fougères en zone B, buissons en zones A et C. On ne voit à travers aucun modèle, sous aucun angle. Sol en facettes douces : terre, mousse, sentier sableux, roche. | |
| 6 | Monter et descendre les pentes, partout dans la zone jouable. | Aucune pente ne bloque le joueur. | |
| 7 | Aller jusqu'à un bord de la map et essayer de grimper, en sautant. | Une falaise irrégulière et rocheuse, de gros rochers au pied, une forêt dense au-dessus. Infranchissable, aucun mur invisible. | |
| 8 | Regarder les pieds des arbres sur les pentes, et les arbres proches les uns des autres. | Aucun pied ne flotte. Aucun arbre ne pousse dans un autre. Orientations variées. | |
| 9 | Sélectionner `Terrain/Relief`, cliquer **Generate relief + forest**, puis Ctrl+Z. | Rien ne change (même seed), herbe comprise. Après Ctrl+Z, toujours rien. La Console affiche « Relief generated, 26765 trees regenerated ». | |
| 10 | **New seed + Generate relief + forest**, puis Ctrl+Z une fois. | Un autre relief, une autre forêt, une autre herbe. Un seul Ctrl+Z ramène l'ancien relief, l'ancienne forêt **et** l'ancienne seed. | |
| 11 | Sélectionner `ChampimaisonSites/ChampimaisonSite_01..04`. | Chaque site est sur un sol plat, vide d'arbres et d'herbe, accessible à pied. | |
| 12 | Window > Analysis > Physics Debugger, Play. | Capsules sur les troncs, mesh colliders sur le géant et les gros rochers posés à la main, rien sur le sous-bois ni les petits cailloux. | |
| 13 | Play, fenêtre **Stats**, depuis le spawn, puis au centre de la zone B. | Noter les FPS dans la PR. Objectif : 120 FPS ou plus sur RX 6800 XT + i5-12400F. | |
| 14 | Play, regarder une plaque d'herbe sans bouger. | L'herbe ondule par rafales, le pied des touffes reste en place, aucune touffe ne clignote. | |
| 15 | Play, aller au creux (ouest) puis dans une clairière de respiration. | Une brume basse bleu-violet dérive lentement, sans bord net contre le sol ni les troncs. Noter les FPS dans la brume. | |

## Limitations connues

- **Le vent et la brume n'ont pas été vus en mouvement** : seules des captures fixes ont été rendues (17/11). Les FPS dans la brume n'ont pas été mesurés.

- **FPS** : mesurés par Hugo le 13/11 sur RX 6800 XT + i5-12400F, jusqu'à 220 FPS. Le minimum n'a pas été relevé, ni une machine plus modeste.
- Les captures sont rendues en batchmode : les spores y sont simulées à la main. L'ambiance en mouvement a été vue et validée en jeu par Hugo le 13/11.
- Le Shader Graph du sol affiche à l'import des avertissements sur ses shaders `AddPass` et `BaseMapGen` introuvables. Sans effet visible avec 3 calques et `Basemap Distance` à 1000 m, mais non résolu.
- La pente est limitée par axe, de façon prudente : en pratique, les pentes de la zone jouable restent plutôt sous 31° que sous 40°.
- Le diff de la scène est gros : le `ParticleSystem` des spores occupe à lui seul des milliers de lignes de YAML, et les groupes `Clearings` et `Landmarks` ont été recréés.
- `MysticForestTerrainData.asset` est binaire : chaque régénération ajoute quelques Mo au stockage LFS.
- Les colliders des arbres de Terrain n'existent qu'en jeu.
- `docs/screenshots/mystic-forest.png` (l'ancienne capture) n'est plus référencé. Il est gardé : supprimer un asset demande l'accord de Hugo.

## Modifié par
