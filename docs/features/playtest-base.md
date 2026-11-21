# Base du playtest : accueil, pause, vent, ressources par tick, contour, barre d'objets

- **Ticket ClickUp :** aucun. Hugo (owner) a demandé le 20/11 de travailler « directement sur GitHub, une branche », sans ticket. C'est un écart volontaire au workflow, noté aussi dans la PR.
- **Note de design validée :** aucune sous-tâche de validation. Les choix ont été pris par Hugo en conversation le 20/11 : ils sont listés dans « Décisions ».
- **Branche :** `feat/playtest-base`
- **Auteur :** Hugo Bernier
- **Statut :** En cours

## Docs précédents

- [12487v2nnw4-demo-boucle](./12487v2nnw4-demo-boucle.md) — la boucle devient la base du playtest. L'anneau cyan disparaît au profit d'un contour du modèle. Les bâtons et les champignons à cueillir ne sont plus posés à la main près du spawn : ils apparaissent sur toute la map, par tick, et ne reviennent plus au même endroit. Le trou de la Champimaison n'est plus construit. Échap ouvre un menu pause.
- [12487v2byt9-mystic-forest-tree-wind](./12487v2byt9-mystic-forest-tree-wind.md) — le vent change : tout l'arbre penche depuis le pied et rien ne tremble. La décision « seul le feuillage bouge » est remplacée. Les rafales entendues gardent leur effet.
- [12487v2byt9-mystic-forest](./12487v2byt9-mystic-forest.md) — les buissons et les fougères bougent avec le vent. Les modèles `Stick` et `MushroomCluster` deviennent lisibles (Read/Write). La scène `MysticForest` n'est pas modifiée.

## Ce que fait la feature

**Accueil**

- [ ] Le jeu démarre sur un écran d'accueil : une petite forêt low poly sur une pente douce, avec le même ciel, le même brouillard, les mêmes spores et le même vent que la Forêt mystique. Les arbres sont répartis en bosquets irréguliers, jamais en ligne.
- [ ] La caméra est presque fixe : elle flotte de quelques centimètres. C'est le vent dans les arbres qui anime l'image. On entend l'ambiance de la lisière.
- [ ] En haut à gauche, le mot « Fungiiiii » en crème, police Fredoka. Dessous, deux planches de bois à pans coupés : **Jouer** et **Quitter**. Pas de logo.
- [ ] La planche survolée ou sélectionnée s'éclaircit et avance de quelques pixels. Une seule planche ressort à la fois, à la souris, au clavier (flèches, Entrée) comme à la manette. « Jouer » est sélectionnée au départ.
- [ ] **Jouer** : fondu au noir, puis la partie se charge. **Quitter** : ferme le jeu (dans l'Editor, arrête le Play).
- [ ] Les textes sont en français ou en anglais selon la langue du système. Toute autre langue donne le français.

**Pause**

- [ ] En jeu, Échap (ou Start à la manette) fige la partie sous un voile sombre et affiche trois planches : **Reprendre**, **Accueil**, **Quitter**. Échap de nouveau, ou Reprendre, relance la partie.
- [ ] En pause, la vue ne tourne plus, le filet ne part pas, E, Tab et R ne font rien, la souris est libre.
- [ ] **Accueil** ramène à l'écran d'accueil. La partie en cours est perdue.

**Vent**

- [ ] Tout l'arbre penche dans le sens du vent depuis son pied : le pied ne bouge pas, le haut du tronc et la couronne penchent le plus. Les branches restent attachées au tronc et le feuillage aux branches.
- [ ] La couronne dérive lentement en plus de ce penché, davantage au bout des branches. Rien ne tremble.
- [ ] Le vent traverse la forêt : on voit un coup de vent passer d'un arbre au suivant, dans le sens du vent. Il a des accalmies et des reprises ; deux arbres voisins ne bougent pas en même temps.
- [ ] Le haut d'un arbre se déplace d'environ 50 cm par vent établi, et jusqu'à 80 cm de plus pendant une rafale entendue. L'arbre géant bouge un peu moins.
- [ ] Les buissons et les fougères ondulent avec le même vent. Les champignons et les rochers restent immobiles.
- [ ] L'herbe ne change pas.

**Bâtons et champignons à cueillir**

- [ ] Il y en a sur toute la zone jouable (600 × 600 m), dès le début de la partie : 80 bâtons et 120 groupes de champignons au plus.
- [ ] Toutes les 10 s, 1 bâton et 2 groupes de champignons au plus apparaissent sur des emplacements libres tirés au hasard, jamais à moins de 20 m du joueur, tant que le maximum n'est pas atteint.
- [ ] Un objet cueilli disparaît. Celui qui le remplace apparaît ailleurs, pas forcément au même endroit.
- [ ] Les bâtons sont couchés au pied des arbres, entre 1 et 2,5 m d'un tronc, sur un sol sans herbe, et suivent la pente.
- [ ] Les champignons apparaissent là où la forêt avait des champignons de décor, avec la même taille et la même orientation. Il ne reste aucun champignon qu'on ne peut pas cueillir.
- [ ] Les Spongeus et les Champoule ne changent pas : 4, dans la clairière du spawn.
- [ ] R remet la map dans un état de départ : les objets sont tirés de nouveau.

**Contour**

- [ ] L'anneau cyan au sol disparaît. Ce qui peut être cueilli porte un contour crème fin qui suit la silhouette du modèle.
- [ ] Le contour est visible jusqu'à 30 m : net jusqu'à 16 m, puis de plus en plus effacé. Il garde la même épaisseur à l'écran quelle que soit la distance.
- [ ] Quand l'objet est visé à portée de E, le contour devient plus épais et plus clair.
- [ ] Le contour ne se voit pas à travers un tronc, un rocher ou une colline.

**Retiré pour l'instant**

- [ ] Le trou de la Champimaison et son réseau fongique ne sont plus dans la partie. Le bois et les spores s'accumulent dans l'inventaire.

**Barre d'objets et filet**

- [ ] En bas de l'écran, au centre, trois cases en planches de bois, comme les boutons des menus. Ce sont les trois cases de la barre rapide de l'inventaire.
- [ ] Le filet est un objet : il est dans la première case au début de la partie. L'inventaire ne contient rien d'autre.
- [ ] Une case est sélectionnée : sa planche est plus claire, monte un peu et n'est pas transparente. La première l'est au départ.
- [ ] La molette change de case : vers le bas, la suivante ; vers le haut, la précédente. Après la troisième vient la première. Les touches 1, 2, 3 choisissent une case ; à la manette, LB et RB.
- [ ] La main avec le filet, l'arc de visée et le cercle d'impact ne se voient que si la case sélectionnée contient le filet. Sinon le clic gauche ne lance rien.
- [ ] Un filet déjà lancé finit sa course et revient avec sa prise, même si on change de case entre-temps.
- [ ] Une case montre la couleur de son objet, et le nombre s'il y en a plus d'un.
- [ ] Les cases 2 et 3 sont vides au départ. Ce qu'on cueille ou capture va d'abord dans la grille de l'inventaire, comme avant. Avec Tab, on peut déplacer n'importe quel objet dans la barre, filet compris.
- [ ] Inventaire ouvert, la barre du bas disparaît (l'inventaire montre la sienne). En pause, elle ne répond plus.
- [ ] R remet le filet dans la première case et la sélectionne.

**Exécutable**

- [ ] Le menu **Fungiiiii > Playtest > Build Windows Player** produit `Builds/Playtest/Fungiiiii.exe`, qui démarre sur l'accueil.

## Hors périmètre

- Un écran d'options (son, graphismes, touches, langue).
- Un écran ou un texte de chargement : l'écran reste noir quelques secondes pendant le chargement de la forêt.
- Une sauvegarde : revenir à l'accueil perd la partie.
- Des sons d'interface.
- L'apparition par tick des Spongeus et des Champoule.
- Le vent sur l'herbe, qui garde son mouvement.
- La traduction de l'inventaire de Pol-Mattis, dont les textes restent en dur.
- Un modèle de filet ou une icône : la case montre un carré de couleur, comme l'inventaire.
- Le nom de l'objet affiché quand on change de case.
- Tenir en main un autre objet que le filet (bois, spores, champignons).
- Le multijoueur.
- Toute modification de la scène `MysticForest` et de ses données de Terrain.

## Implémentation

**Scènes**

| Scène | Rôle |
|---|---|
| `Assets/Scenes/Menu/MainMenu.unity` (nouvelle) | L'accueil. Terrain de 256 × 256 m (`MainMenuTerrainData.asset`, LFS), 7 calques `ForestScatter`, herbe, caméra, lumière, post-traitement, objet `Main Menu`. Écrite par l'API de l'Editor. |
| `Assets/Scenes/Prototype/DemoLoopScene.unity` | La partie. Nouvelles références sur `DemoLoopBootstrap` : shader du contour, arbres à bâtons, police, scène d'accueil. |
| `Assets/Scenes/Biomes/MysticForest.unity` | Inchangée. |

Les trois sont dans les Build Settings, dans cet ordre. `SampleScene` n'y est plus (le fichier reste).

**Outils d'Editor** (`Assets/Scripts/Editor/Playtest/`, menu **Fungiiiii > Playtest**)

| Fichier | Rôle |
|---|---|
| `PlaytestSetup.cs` | Câble `DemoLoopScene`, écrit les Build Settings, ajoute le shader des placeholders aux shaders toujours inclus, lance le build Windows. **Set Up Everything** rejoue tout sauf le build. |
| `MainMenuSceneBuilder.cs` | Génère `MainMenu` : relief (pente + collines douces, `ValueNoise`), sol peint, calques d'arbres puis `TerrainForestGenerator`, caméra. Le ciel, le brouillard, la lumière, le post-traitement et les spores sont copiés depuis `MysticForest`. |
| `PlaytestLocalization.cs` | Crée les réglages de localisation, les langues `fr` et `en`, la table `UI` et ses 6 entrées. N'écrase jamais un texte existant. |

**Interface** (`Assets/Scripts/Runtime/UI/`)

| Fichier | Rôle |
|---|---|
| `MainMenu.cs` | L'accueil : titre, Jouer, Quitter, fondu, chargement de la partie. |
| `PauseMenu.cs` | La pause : Échap, voile, trois planches, retour à l'accueil. |
| `WoodMenu.cs` | Construit canvas, titre, planches et voile. |
| `PlankGraphic.cs` | Dessine une planche en facettes de couleur unie, sans texture (couleurs « Wood », « Bark pale », « Wood dark » de la palette). |
| `PlankButton.cs` | Planche sélectionnée : éclaircie et glissement. |
| `LocalizedLabel.cs`, `UiText.cs` | Affichent une entrée de la table `UI` ; clés des textes. |
| `MenuCameraDrift.cs` | Flottement de la caméra de l'accueil. |
| `GameExit.cs` | Quitter. |
| `Hotbar.cs` | La barre d'objets du bas : trois planches, la case sélectionnée, molette, 1/2/3, LB/RB. |

**Cueillette**

| Fichier | Rôle |
|---|---|
| `Runtime/Gathering/ResourceSpawner.cs` | C# pur : tick, maximum, distance au joueur, emplacements libres. Générateur injecté. |
| `Runtime/Demo/GatherableField.cs` | Une sorte de ressource sur la map : fait tourner son `ResourceSpawner` et montre un `Gatherable` par emplacement rempli. Les objets cueillis sont réutilisés. |
| `Runtime/Demo/ForestResourceSpots.cs` | Lit dans le Terrain les emplacements : champignons de décor, pieds d'arbres. |
| `Runtime/Demo/Gatherable.cs` | Réécrit : apparaît sur un emplacement, disparaît une fois cueilli. Ne réapparaît plus seul. |
| `Runtime/Demo/ItemOutline.cs` | Le contour et sa fabrique. |
| `Art/Shaders/ItemOutline.shader` | `Fungiiiii/ItemOutline`. |

**Vent**

| Fichier | Rôle |
|---|---|
| `Art/Shaders/Wind.hlsl` | `WindStrengthAt` : force du vent en un point du sol, de 0 à 1, en vagues qui avancent. |
| `Art/Shaders/TreeWind.shader` | Réécrit : penché depuis le pied, dérive de la couronne. Plus de masque bois / feuillage. |
| `Art/Materials/TreeWind.mat`, `TreeWindGiant.mat` | Nouveaux réglages (tableau ci-dessous). |
| `Art/Materials/BushWind.mat`, `FernWind.mat` (nouveaux) | Même shader, pour `Bush.fbx` et `Fern.fbx`, dont le `.meta` pointe dessus au lieu de `Palette.mat`. |

| Matériau | Hauteur | Rayon de couronne | Penché | Rafale | Dérive | Vitesse de dérive |
|---|---|---|---|---|---|---|
| `TreeWind` | 20 m | 6 m | 0,5 m | 0,8 m | 0,15 m | 0,6 |
| `TreeWindGiant` | 29 m | 12 m | 0,4 m | 0,6 m | 0,12 m | 0,5 |
| `BushWind` | 1,3 m | 0,8 m | 0,12 m | 0,16 m | 0,04 m | 1 |
| `FernWind` | 0,5 m | 0,5 m | 0,07 m | 0,09 m | 0,025 m | 1,2 |

**Autres**

- `Packages/manifest.json` : `com.unity.localization` 1.5.14. Unity a ajouté Addressables 2.9.1 et ses dépendances.
- `Assets/Localization/` : réglages, langues, table `UI`. `Assets/AddressableAssetsData/` : créé par le package.
- `Assets/Art/Fonts/Fredoka_SemiBold.ttf` (LFS) et `Fredoka_OFL.txt`, sa licence.
- `Stick.fbx.meta`, `MushroomCluster.fbx.meta` : `isReadable: 1`.
- `ProjectSettings/EditorBuildSettings.asset` : scènes du build, réglages de localisation et d'Addressables.
- `ProjectSettings/GraphicsSettings.asset` : `Universal Render Pipeline/Unlit` ajouté aux *Always Included Shaders*.
- `DemoLoopBootstrap` : champs de cueillette, de contour et de menus ; `buildHole` (faux) ; `SetGameplayInput` ; le filet posé dans la barre, et équipé selon la case sélectionnée.
- `DemoItems.Net` : le filet comme objet d'inventaire (un seul, couleur du filet provisoire).
- `NetThrower.IsEquipped` : rangé, le filet en main et la visée sont cachés et il ne part pas. Vrai par défaut : `NetThrowScene` ne change pas.
- `Runtime/AssemblyInfo.cs` : les types internes sont visibles de `Fungiiiii.Editor`.

Réglages de la partie, sur `DemoLoopBootstrap` dans `DemoLoopScene` : tick (10 s), distance au joueur (20 m), maximums (80 et 120), objets par tick (1 et 2), distance au tronc (1 à 2,5 m), pente max des bâtons (30°), zone jouable, couleur, épaisseur et portée du contour.

## Décisions

Prises par Hugo le 20/11, en conversation :

- **Pas de ticket ni de note validée**, une seule branche.
- **Exécutable Windows.** Les testeurs n'ouvrent pas Unity.
- **Localisation : package Unity Localization**, donc Addressables, pour deux textes de menu. C'est la décision d'architecture que la règle « aucun texte en dur » attendait. Français et anglais.
- **L'accueil a sa propre petite map**, générée, plutôt que la Forêt mystique : elle s'affiche tout de suite, la forêt de 1 km met plusieurs secondes à charger.
- **Caméra fixe à léger flottement**, planches low poly, pas de logo, police Fredoka (choisie sur maquette parmi trois).
- **Échap ouvre un menu pause** à trois choix.
- **Vent « pas réaliste, comme les autres jeux low poly » : simple, tout reste attaché, moins « les arbres ont peur »**, rafales qui traversent, mouvement irrégulier, plus d'amplitude, buissons et fougères aussi.
- **Tick global avec plafond** ; seuls les champignons à cueillir et les bâtons ; aucun champignon non cueillable ; un bâton toutes les 30 s de marche environ.
- **Contour crème « Body light »**, assez visible pour que le joueur repère les objets.
- **Le trou est retiré pour l'instant.**

Prises pendant l'implémentation :

- **Le vent ne distingue plus le bois du feuillage.** Le déplacement dépend seulement de la position d'un point : sa hauteur dans l'arbre et sa distance au tronc. Deux points qui se touchent bougent ensemble, donc rien ne se détache. L'ancien shader arrêtait le bois et déplaçait le feuillage : c'est ce qui faisait glisser les feuilles autour des branches.
- **Le penché suit le carré de la hauteur** : le pied reste planté, le tronc plie surtout en haut.
- **Le frémissement par sommet est supprimé** : c'était lui, le tremblement.
- **Un vent par arbre, lu au pied**, pour le penché : l'arbre penche d'un bloc. La dérive de la couronne, elle, est lue à chaque point.
- **Les vagues de vent sont des sinus qui avancent**, pas une texture de bruit : rien à charger, même résultat partout. Deux longueurs : 46 m (ce qu'on voit passer) et 170 m (accalmies et reprises), à environ 9 m/s. Les fronts sont courbés pour ne pas balayer la forêt en ligne droite.
- **La rafale entendue s'ajoute aux vagues** au lieu de pencher tous les arbres au même instant : le son reste global, mais on voit la rafale courir.
- **Les champignons de décor sont retirés d'une copie des données du Terrain**, faite au lancement de la partie. L'asset `MysticForestTerrainData` n'est jamais modifié. Leurs emplacements deviennent les emplacements d'apparition : la répartition en bosquets de la forêt est gardée.
- **Les champignons posés à la main** (spawn, arbre géant) sont masqués et deviennent des emplacements comme les autres.
- **Bâtons sur sol sans herbe** : couchés dans l'herbe, haute de 20 à 45 cm, ils ne se verraient pas. La carte de l'herbe est lue une fois dans le Terrain.
- **Les emplacements des bâtons sont calculés au lancement**, un par arbre de la zone jouable, à partir des arbres du Terrain. Rien n'est enregistré dans la scène.
- **Un tirage au hasard parmi les emplacements libres**, 24 essais au plus par apparition pour en trouver un assez loin du joueur. Sinon, rien n'apparaît à ce tick.
- **Les objets cueillis sont réutilisés**, pas détruits : la liste de ce que E peut viser ne contient jamais d'objet détruit.
- **Contour par coque inversée** : une copie du maillage, dont on ne dessine que les faces arrière, repoussées à l'écran d'un nombre fixe de pixels. Pas de passe de rendu ajoutée, pas de changement des réglages URP. Le maillage de la copie porte des normales lissées, calculées au lancement : avec les normales à facettes du low poly, le contour s'ouvrirait à chaque arête. D'où `isReadable` sur les deux modèles.
- **Le contour passe le test de profondeur** : il ne se voit pas à travers le décor.
- **Deux matériaux de contour** (repos, visé) partagés par tous les objets, plutôt qu'un réglage par objet.
- **Le shader du contour est une référence sérialisée**, pas un `Shader.Find` : il est sûr d'être dans le build.
- **Le shader Unlit d'URP est ajouté aux shaders toujours inclus.** Les placeholders du filet et des disques d'état créent leurs matériaux en le cherchant par nom. Aucun asset ne l'utilise : le premier build le laissait de côté, et le cercle d'impact et la corde du filet sortaient gris, éclairés par la scène, au lieu de blancs. Constaté dans l'exécutable, corrigé, revérifié.
- **Interface en uGUI**, comme le HUD et l'inventaire. Les planches sont un `Graphic` qui génère sa géométrie : aucune image.
- **Texte uGUI avec la police dynamique**, pas TextMeshPro : pas d'atlas à générer. La police est le fichier statique SemiBold (600) : Unity ne sait pas choisir une graisse dans la police variable distribuée par Google Fonts.
- **Les textes sont lus par clé** (`UiText`) avec `LocalizedString`, et se mettent à jour quand la langue change. Le titre « Fungiiiii » est aussi une entrée de la table.
- **La sélection pilote la mise en avant** : survoler une planche la sélectionne. Souris, clavier et manette montrent la même chose.
- **La pause met `Time.timeScale` à 0.** Les menus s'animent en temps réel.
- **Les menus chargent les scènes par chemin**, d'où les trois scènes dans les Build Settings.
- **`buildHole` est un réglage sérialisé, faux** : le code du trou reste, pour quand il reviendra.
- **`RespawningResource` n'est plus utilisée** par la démo. Elle et ses tests sont laissés en place : supprimer un fichier demande l'accord de Hugo.
- **Les scènes sont écrites par des outils d'Editor**, rejouables depuis le menu, plutôt qu'à la main.

Barre d'objets, décidée par Hugo le 20/11 en conversation (même branche, pas de ticket) :

- **Le filet est un objet**, dans la première case ; les deux autres cases sont vides au départ.
- **Molette, 1/2/3 et LB/RB.** La molette boucle.
- **Un filet lancé revient** même si on change de case.
- **Faire simple, comme les autres jeux** : un index de case sélectionnée ; à chaque changement, ce qui est tenu en main est montré ou caché, comme le `SetActive` habituel sur l'objet tenu.

Prises pendant l'implémentation :

- **La barre affiche les cases de la barre rapide existante** de l'inventaire (`InventoryDemoController`), pas une deuxième liste : déplacer le filet avec Tab change ce qu'on a en main.
- **La molette est liée comme un bouton** (`<Mouse>/scroll/down` et `/up`, Input System 1.20) : un cran, une case, sans seuil à régler.
- **Molette vers le bas = case suivante**, comme dans la plupart des jeux à barre rapide.
- **`TryThrow` refuse un filet rangé**, en plus de l'entrée : un filet qu'on ne tient pas ne part pas.
- **Inventaire ouvert, la barre du bas est cachée** : le panneau de l'inventaire la recouvrirait, et il montre déjà la barre rapide.
- **Un seul objet ne montre pas de nombre** dans la barre (le filet).
- **Un creux sombre (« Wood dark ») sous l'objet** : sans lui, le carré du bois, de la couleur des planches, ne se voyait pas. Constaté sur une capture, corrigé, revérifié.

## Tests

### Automatisés (Unity Test Runner)

Lancés en batchmode avec Unity 6000.3.21f1 le 20/11, sur la machine de Hugo, après le dernier changement de code et la dernière génération des scènes. Log et XML lus : **EditMode 216/216, PlayMode 46/46**, tous les tests du projet compris. Aucune erreur de compilation. EditMode compte un test de plus que les nôtres : `AddressableAssets.DocExampleCode.TestStub.RequiredTest`, apporté par le package Addressables. PlayMode a tourné sans `-nographics`. La branche part de `origin/develop` (faef7e0) : il n'y a pas eu de rebase à refaire.

**Exécutable.** Construit par `PlaytestSetup.BuildWindowsPlayer` (113 Mo), puis lancé en fenêtre 1280 × 720 avec un script temporaire, retiré depuis, qui jouait le parcours seul et écrivait dans le log du player : accueil affiché avec « Jouer » et « Quitter », 3 170 arbres sur la map d'accueil, partie prête, 80 bâtons et 120 champignons sur la map, contour actif, shaders des placeholders présents, 4 champignons à capturer, pas de trou, pause (temps à 0, « Reprendre »), retour à l'accueil, fermeture. Aucune erreur ni exception dans le log. Trois captures de l'exécutable ont été regardées : accueil, partie, pause. Environ 430 images par seconde au spawn dans cette fenêtre : une seule mesure, à l'arrêt, pas un test de performance.

L'exécutable final, reconstruit sans ce script, démarre sans erreur dans son log. Son parcours n'a pas été rejoué, et personne n'y a encore joué à la main.

**Barre d'objets.** Après son ajout, relancés en batchmode le 20/11 sur la machine de Hugo, sur le code final : **EditMode 216/216, PlayMode 52/52**, log et XML lus, aucune erreur de compilation. Deux captures de la partie, rendues par un test temporaire retiré depuis, ont été regardées : filet sélectionné (main, arc et cercle visibles) et case 2 sélectionnée (rien en main). L'exécutable n'a pas été reconstruit avec la barre.

| Test | Mode | Couvre | Résultat |
|---|---|---|---|
| `ResourceSpawnerTests` (12 tests) | EditMode | Remplissage jusqu'au maximum sur des emplacements distincts ; arrêt quand tout est pris ; rien avant un tick ; nombre par tick, ticks cumulés sur plusieurs images ; plafond jamais dépassé ; rien près du joueur ; distance mesurée au sol ; un objet cueilli libère son emplacement, un autre apparaît au tick suivant ; libérer deux fois ne change rien ; même graine, mêmes emplacements ; remise à zéro ; tick nul refusé | ✅ |
| `DemoLoopScenePlayModeTests.SticksAndMushrooms_AreSpreadOverTheWholeMap` | PlayMode | Au moins 40 bâtons et 40 champignons, étalés sur plus de 300 m dans les deux sens, posés au sol | ✅ |
| `DemoLoopScenePlayModeTests.EveryStick_LiesAtTheFootOfATree` | PlayMode | Chaque bâton est à 2,6 m au plus d'un tronc | ✅ |
| `DemoLoopScenePlayModeTests.NoDecorMushroomIsLeft_ThatCannotBeGathered` | PlayMode | Le Terrain ne dessine plus de champignon de décor ; tout groupe de champignons affiché est cueillable ; l'asset du Terrain de la forêt garde les siens | ✅ |
| `DemoLoopScenePlayModeTests.AGatheredStick_IsReplacedOnALaterTick_AwayFromThePlayer` | PlayMode | Un bâton cueilli est remplacé au tick suivant, ailleurs, à 20 m au moins du joueur | ✅ |
| `DemoLoopScenePlayModeTests.WhatCanBeGathered_IsOutlined_AndTheOutlineChangesWhenAimedAt` | PlayMode | Plus d'anneau au sol ; bâtons et champignons portent un contour (`Fungiiiii/ItemOutline`) qui change quand ils sont visés | ✅ |
| `DemoLoopScenePlayModeTests.Pausing_FreezesTheGame_AndResumingGivesItBack` | PlayMode | Pause : temps figé, trois planches, vue et filet coupés, souris libre. Reprendre rend tout. À la deuxième pause, Reprendre est de nouveau mise en avant | ✅ |
| `DemoLoopScenePlayModeTests.ResumingWithTheInventoryOpen_LeavesTheInventoryInCharge` | PlayMode | Reprendre avec l'inventaire ouvert ne rend pas la vue | ✅ |
| `DemoLoopScenePlayModeTests.Home_GoesBackToTheHomeScreen_WithTimeRunning` | PlayMode | Accueil recharge l'écran d'accueil, le temps tourne, la partie est déchargée | ✅ |
| `MainMenuPlayModeTests.TheHomeScreen_ShowsTheTitleAndTwoButtons_InFrench` | PlayMode | « Fungiiiii », « Jouer », « Quitter » | ✅ |
| `MainMenuPlayModeTests.ChangingTheLanguage_ChangesTheButtonsAtOnce` | PlayMode | Passage à l'anglais : « Play », « Quit » | ✅ |
| `MainMenuPlayModeTests.PlayIsSelectedFirst_AndTheSelectedPlankStandsOut` | PlayMode | Jouer sélectionnée au départ ; une seule planche mise en avant, qui suit la sélection | ✅ |
| `MainMenuPlayModeTests.TheHomeScreen_ShowsASmallForestOnASlope` | PlayMode | Terrain avec plus de 100 arbres, sol qui monte de plus de 10 m, caméra posée au-dessus du sol et flottante | ✅ |
| `MainMenuPlayModeTests.Play_LoadsTheGame` | PlayMode | Jouer charge la partie jusqu'à ce qu'elle soit prête, l'accueil est déchargé | ✅ |
| `PlaytestLoopPlayModeTests.FromTheHomeScreen_PlayReachesTheGame_AndPauseComesBackHome` | PlayMode | Le parcours entier, scènes chargées comme dans un build : accueil, textes chargés, Jouer, partie prête avec ses ressources et leur contour, pause, retour à l'accueil | ✅ |

| `DemoLoopScenePlayModeTests.TheNet_StartsInTheFirstSlotOfTheQuickBar_InHand` | PlayMode | Un seul filet, dans la première case de la barre ; barre visible, 3 cases, la première sélectionnée ; filet en main | ✅ |
| `DemoLoopScenePlayModeTests.SelectingAnotherSlot_PutsTheNetAway_AndSelectingItAgainGivesItBack` | PlayMode | Autre case : filet rangé, main cachée, lancer refusé. Retour sur la case : filet en main, lancer possible | ✅ |
| `DemoLoopScenePlayModeTests.ANetThrown_ComesBack_EvenWhenAnotherSlotIsSelected` | PlayMode | Un filet lancé puis rangé revient ; la main reste cachée | ✅ |
| `DemoLoopScenePlayModeTests.MovingTheNetOutOfTheSelectedSlot_PutsItAway` | PlayMode | Filet déplacé dans une autre case : rangé ; cette case sélectionnée : en main | ✅ |
| `DemoLoopScenePlayModeTests.TheInventory_HidesTheQuickBar_AndThePause_FreezesIt` | PlayMode | Inventaire ouvert : barre cachée et sans effet ; pause : barre sans effet | ✅ |
| `DemoLoopScenePlayModeTests.Reset_PutsTheNetBackInTheFirstSlot_InHand` | PlayMode | R : un seul filet, dans la première case, sélectionnée, en main | ✅ |

**Tests existants modifiés, exigence changée :** `DemoLoopScenePlayModeTests.SceneLoad_LoadsTheForest_WithOnePlayerAndOneCamera` vérifiait que le trou existe ; il vérifie maintenant qu'il n'y en a pas, puisque le trou est retiré. Il vérifiait aussi que l'inventaire démarre vide ; il démarre maintenant avec le filet seul, dans la première case de la barre. Les autres tests existants ne sont pas modifiés.

`GatheringTests` garde ses 7 tests de `RespawningResource`, classe que la démo n'utilise plus (voir « Limitations connues »).

Aucun test automatisé ne vérifie ce qu'on voit du vent, du contour ou des planches : ce sont des réglages visuels. Des images fixes ont été rendues en batchmode (accueil, pause, contour à 2 et 9 m) et regardées pendant le développement.

### Manuels (dans Unity)

Aucun de ces tests n'a encore été joué par un humain : à dérouler par Hugo, puis par le reviewer.

**Dans l'Editor**

| # | Étapes | Résultat attendu | OK |
|---|---|---|---|
| 1 | Ouvrir `Assets/Scenes/Menu/MainMenu.unity`, Play | Fondu depuis le noir. Une forêt sur une pente qui monte devant soi, brouillard bleu-violet, spores. « Fungiiiii » en haut à gauche, planches **Jouer** et **Quitter** dessous. Son d'ambiance. Aucune erreur rouge dans la Console | ⬜ |
| 2 | Regarder l'image 30 s sans toucher à rien | La caméra flotte à peine. Les arbres penchent et reviennent, pas tous en même temps ; on voit des coups de vent passer d'un arbre à l'autre. Aucun arbre ne tremble, aucun feuillage ne se détache de ses branches | ⬜ |
| 3 | Regarder la répartition des arbres | Des bosquets et des trouées, aucun alignement. Le titre et les planches restent lisibles | ⬜ |
| 4 | Passer la souris sur Quitter, puis sur Jouer ; flèches haut et bas | Une seule planche ressort à la fois : elle s'éclaircit et avance un peu vers la droite | ⬜ |
| 5 | Dans la Game View, menu de langue en haut à droite : English, puis French | Play / Quit, puis Jouer / Quitter. Le titre ne change pas | ⬜ |
| 6 | Cliquer **Jouer** | Fondu au noir, quelques secondes de chargement, puis la Forêt mystique en vue à la première personne, au spawn, barres de vie et d'endurance en haut à gauche | ⬜ |
| 7 | Regarder les arbres du spawn 30 s, au casque, en attendant une rafale (70 s au plus) | Même vent que sur l'accueil. Pendant la rafale entendue, les arbres penchent nettement plus, puis reviennent. Les buissons et les fougères ondulent. Les troncs ne se déracinent pas : le pied reste fixe | ⬜ |
| 8 | Regarder la jonction tronc / branches / feuillage de près, pendant une rafale | Rien ne se décolle, rien ne s'étire de façon visible | ⬜ |
| 9 | Marcher en forêt 2 minutes, sous les arbres | On croise des bâtons couchés au pied des troncs, sur la terre nue, environ un toutes les 30 s. Ils portent un contour crème fin | ⬜ |
| 10 | Chercher des champignons | Des groupes rouges et violets, partout, avec un contour crème. Aucun groupe sans contour à moins de 15 m | ⬜ |
| 11 | S'éloigner d'un champignon à reculons | Le contour s'efface peu à peu et disparaît vers 30 m. Il garde la même épaisseur tant qu'il est visible | ⬜ |
| 12 | Se placer derrière un tronc ou un rocher par rapport à un objet à cueillir | Le contour ne se voit pas à travers | ⬜ |
| 13 | S'approcher à moins de 2 m d'un bâton, le viser | Le contour devient plus épais et plus clair. E : le bâton disparaît. Tab : un carré brun | ⬜ |
| 14 | Idem sur un champignon | Contour plus épais visé, E le cueille, un carré violet dans l'inventaire | ⬜ |
| 15 | Rester près d'un objet cueilli 1 minute | Il ne revient pas à cet endroit | ⬜ |
| 16 | Chercher l'anneau cyan et le trou de la Champimaison (47 m au nord-est du spawn) | Aucun anneau cyan nulle part. Pas de trou, pas d'anneau crème | ⬜ |
| 17 | Capturer un Spongeus au filet (clic gauche) | Comme avant : il entre dans l'inventaire | ⬜ |
| 18 | Échap | La partie se fige, voile sombre, planches **Reprendre**, **Accueil**, **Quitter**, Reprendre mise en avant, souris libre. La souris ne fait plus tourner la vue | ⬜ |
| 19 | En pause : clic gauche dans le vide, E, Tab, R | Rien ne se passe | ⬜ |
| 20 | Reprendre, puis Échap, puis Échap | La partie reprend, se remet en pause (Reprendre de nouveau mise en avant), reprend | ⬜ |
| 21 | Tab (inventaire ouvert), Échap, Reprendre | L'inventaire est toujours ouvert et la vue ne tourne pas. Tab le ferme, la vue tourne de nouveau | ⬜ |
| 22 | Échap, **Accueil** | L'écran d'accueil revient, animé (le temps n'est pas figé) | ⬜ |
| 23 | **Jouer** de nouveau, puis Échap, **Quitter** | Une nouvelle partie démarre. Quitter arrête le Play | ⬜ |
| 24 | Pendant le Play, fenêtre **Stats**, au spawn puis en zone B | Noter les FPS dans la PR. Objectif : 120 FPS ou plus sur RX 6800 XT + i5-12400F | ⬜ |
| 25 | Ouvrir `MysticForest` seule, Play. Puis `git status` | La forêt marche avec son joueur de test, ses champignons de décor sont tous là. La scène et `MysticForestTerrainData.asset` ne sont pas modifiés | ⬜ |
| 26 | Ouvrir `NetThrowScene`, `InventaireScene` et `MouvementJoueurScene`, Play | Comme avant | ⬜ |
| 31 | `DemoLoopScene`, Play, regarder le bas de l'écran | Trois planches au centre ; la première, plus claire et plus haute, montre un carré beige (le filet). Le filet est en main, l'arc de visée se voit | ⬜ |
| 32 | Molette vers le bas, un cran à la fois, quatre fois | La sélection passe à la 2, la 3, revient à la 1, puis la 2. Sur les cases 2 et 3 : plus de filet en main, ni arc ni cercle, le clic gauche ne lance rien | ⬜ |
| 33 | Molette vers le haut ; puis touches 3, 1 ; puis LB et RB à la manette | Même chose dans l'autre sens ; 3 et 1 choisissent directement ; LB précédente, RB suivante | ⬜ |
| 34 | Case 1, lancer le filet sur un Spongeus, puis tout de suite touche 2 | Le filet finit sa course et revient avec sa prise ; il ne réapparaît pas en main. Touche 1 : il est de nouveau en main | ⬜ |
| 35 | Cueillir trois bâtons | Ils vont dans la grille de l'inventaire, pas dans la barre | ⬜ |
| 36 | Tab, déplacer les bâtons dans la case 2 de la barre, Tab | Inventaire ouvert, la barre du bas a disparu. Fermé : la case 2 montre un carré brun et « 3 » | ⬜ |
| 37 | Tab, déplacer le filet dans la grille, Tab | Plus de filet en main, sur aucune case. Le remettre dans la barre le rend | ⬜ |
| 38 | Échap, molette | La sélection ne bouge pas en pause | ⬜ |
| 39 | Sélectionner la case 3, R | Le filet revient dans la case 1, sélectionnée, en main | ⬜ |

**Exécutable**

| # | Étapes | Résultat attendu | OK |
|---|---|---|---|
| 27 | Menu **Fungiiiii > Playtest > Build Windows Player** | La Console affiche « Playtest built ». `Builds/Playtest/Fungiiiii.exe` existe | ⬜ |
| 28 | Lancer `Fungiiiii.exe` | L'écran d'accueil, en français sur un Windows français. Mêmes vérifications que #1 à #4 | ⬜ |
| 29 | **Jouer**, puis rejouer #9, #13, #14, #17 et #18 | Comme dans l'Editor. Les disques sous les Spongeus et le filet sont bien dessinés (ni roses, ni absents) | ⬜ |
| 30 | Échap, **Quitter** | Le jeu se ferme | ⬜ |

## Limitations connues

- **Rien n'a été vu en mouvement par un humain.** Le vent, le flottement de la caméra et l'animation des planches ont été réglés sur des images fixes rendues en batchmode. Les amplitudes du vent sont à régler en jeu, sur les matériaux.
- **FPS non mesurés.** Le shader des arbres fait un peu plus de calcul par sommet qu'avant, et jusqu'à 200 objets à cueillir portent un contour.
- **Écran noir pendant le chargement** de la forêt, sans texte ni indicateur.
- **Le contour suit chaque volume du modèle** : entre deux chapeaux de champignons qui se chevauchent, un trait intérieur peut apparaître.
- **Les bâtons évitent l'herbe, pas les champignons** : dans l'herbe haute, un groupe de champignons peut être en partie caché, contour compris.
- **Un arbre dont tout le pourtour est en herbe ou en pente n'a pas de bâton.** Le nombre d'emplacements dépend donc de la forêt ; s'il passe sous le maximum, il y a moins de bâtons, sans erreur.
- **Les emplacements des champignons sont ceux du décor** : là où la forêt n'en avait pas, il n'y en aura jamais.
- **La copie des données du Terrain** est faite à chaque lancement de partie : quelques mégaoctets de plus en mémoire pendant la partie.
- **Sur l'accueil, la rafale entendue n'existe pas** : seul le vent de fond anime les arbres.
- **L'accueil garde les champignons de décor** de sa petite forêt : on n'y joue pas.
- **Un build modifie cinq fichiers de réglages** sans qu'on le demande (`ProjectSettings.asset`, `UnityConnectSettings.asset`, et dans `Assets/Settings/` : `PC_RPAsset`, `DefaultVolumeProfile`, `UniversalRenderPipelineGlobalSettings`). Ce sont des écritures d'Unity au premier build : elles ne sont pas dans cette branche. Après un build, `git status` les montre modifiés : ne pas les commiter avec autre chose.
- **Les anciens tests PlayMode ne compilent pas pour un player** (`EditorSceneManager` sans garde) : `-testPlatform StandaloneWindows64` échoue à la compilation. L'exécutable a donc été vérifié autrement (voir « Tests »).
- **Le titre n'a pas de crénage** : le texte uGUI ignore les paires de crénage de la police. L'espace après le « F » est un peu large.
- **L'inventaire de Pol-Mattis** garde ses textes anglais en dur.
- **`RespawningResource` et ses 7 tests** ne servent plus à la démo. À supprimer avec l'accord de Hugo.
- **Noms de fichiers avec espaces** dans `Assets/Localization/` et `Assets/AddressableAssetsData/` : ce sont les noms que donnent les packages.
- **Langue** : pas de choix en jeu. Elle suit le système, ou l'argument `-language=en` au lancement.
- **La barre n'a été vue que sur des images fixes** : la molette, les touches et la manette n'ont été essayées par personne ; les tests appellent la sélection directement.
- **Lancer les tests PlayMode supprime `Assets/AddressableAssetsData/link.xml`** (et son `.meta`) : c'est le package Addressables. `git restore` le remet ; ne pas commiter cette suppression.

## Modifié par
