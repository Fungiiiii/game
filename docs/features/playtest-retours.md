# Retours du playtest du 20/11

- **Ticket ClickUp :** aucun, comme pour la base du playtest. Hugo (owner) a demandé de traiter les huit retours sur la même branche, sans ticket. C'est un écart volontaire au workflow, noté aussi dans la PR.
- **Note de design validée :** aucune sous-tâche de validation. Les décisions viennent de Hugo, en conversation les 20 et 21/11 ; elles sont dans « Ce que fait la feature » et « Décisions » ci-dessous. Maquettes : `docs/playtest-retours/maquettes/`.
- **Icônes d'objets :** pas faites, un humain les dessinera.
- **Branche :** `feat/playtest-base`
- **Auteur :** Hugo Bernier
- **Statut :** En cours

## Docs précédents

- [12487v2byt9-mystic-forest-audio](./12487v2byt9-mystic-forest-audio.md) — le fond de lisière, celui du spawn, joue 20 % moins fort (0,12 au lieu de 0,15).
- [playtest-base](./playtest-base.md) — les bâtons et les champignons ne sont plus répartis sur toute la carte : ils sont gardés autour du joueur. Le contour visé devient cyan, avec la touche au-dessus. L'inventaire est refait. Le filet ne montre son arc qu'en visant.
- [12487v2byt9-mystic-forest](./12487v2byt9-mystic-forest.md) — le générateur écarte les arbres des rochers selon leur emprise, et les calques se fondent en bord de zone (`fadeSouth`, `fadeNorth`). `Border_Rocks` passe avant `Border_TreePine`. Le Terrain est regénéré. `MushroomCluster` est régénéré (même GUID) et trois variantes s'ajoutent.
- [12487v29zj5-poison-test-bench](./12487v29zj5-poison-test-bench.md) — les jauges du joueur (`VitalsHud`) deviennent `VitalGauges`, un prefab placé dans `PoisonScene`, `HUDScene` et `DemoLoopScene`. Le banc poison les reçoit dans l'Inspector.
- [12487v2nnw4-demo-boucle](./12487v2nnw4-demo-boucle.md) — le dodge et son coût d'endurance sont retirés ; Espace saute.
- [86c9mnqy5-filet-capture](./86c9mnqy5-filet-capture.md) — action principale et secondaire, visée avec zoom, points de l'arc espacés, contour de chance sur le champignon visé. `NetThrowScene` est concernée : c'est le même `NetThrower`.

## Ce que fait la feature

**#7 Arbres dans les cailloux**

- [ ] Aucun arbre n'a le pied dans un rocher, ni sur la bordure au pied des falaises, ni ailleurs.

**#8 Trop d'arbres à certains endroits**

- [ ] Là où deux zones de la forêt se rejoignent, la densité passe progressivement de l'une à l'autre : plus de « mur » d'arbres sur le chemin ouest du spawn.

**#3 Variantes des champignons à cueillir**

- [ ] Les champignons à cueillir ont quatre modèles : le groupe d'origine, trois chapeaux rouges, cinq petits violets étalés, un grand rouge et un petit violet. Chaque champignon qui apparaît en tire un au hasard.
- [ ] Les points blancs sont posés à plat sur le chapeau, le bord du chapeau est rond et sans encoche, le dessous est fermé.

**#5 Champignons et bâtons autour du joueur**

- [ ] Au départ, et après R, il y a 5 champignons et 4 bâtons à moins de 50 m du joueur, aucun à moins de 8 m.
- [ ] Toutes les 5 s, s'il en manque, 1 champignon et 1 bâton au plus apparaissent entre 20 et 50 m, **y compris dans le champ de vision**. Un emplacement droit devant le joueur est 4 fois plus probable qu'un emplacement derrière : en marchant tout droit, on en trouve devant soi.
- [ ] Rien ne surgit : un champignon pousse du sol en 1 s ; un bâton tombe du feuillage de son arbre (60 % de sa hauteur) en tournoyant. Au départ et après R, l'écran est noir : tout est déjà en place.
- [ ] Ce qui est à plus de 75 m du joueur disparaît. Au plus 40 champignons et 30 bâtons sur la carte.
- [ ] Les champignons poussent aussi au pied des arbres (herbe permise) et un peu partout, hors des chemins. Aucun n'est sur un bâton, dans un tronc ou dans un rocher.

**#1 Contour cyan et touche « E »**

- [ ] L'objet visé à portée porte un contour cyan `#6FD6EA` épais ; les autres gardent leur contour crème fin.
- [ ] Ce qui est sous le viseur est visé en premier, le plus proche d'abord : regarder un bâton le vise même avec un champignon aux pieds. Sinon, c'est l'objet le plus proche du centre de l'écran, dans un cône de 25°.
- [ ] Au-dessus de l'objet visé, une touche carrée en bois au liseré cyan affiche la touche à presser : **E** au clavier, **Y** sur une manette Xbox, **△** sur une manette PlayStation, selon le dernier appareil utilisé.
- [ ] La touche apparaît avec un petit rebond, rebondit à nouveau quand la cible change, suit la cible, et disparaît sans cible et quand un écran est ouvert. Bouger seulement un stick suffit à passer en **Y** / **△**.

**#4 Filet : actions et visée**

- [ ] Clic gauche (gâchette droite) lance le filet, en visant ou non. Sans viser, aucun aperçu.
- [ ] Clic droit (gâchette gauche) maintenu : l'arc et le cercle d'impact s'affichent, la vue se resserre de 60 à 45° en 0,15 s et tourne moins vite d'autant.
- [ ] Les points de l'arc sont espacés d'exactement 75 cm, le premier à 1 m de la main, 24 au plus : leur nombre dit la portée. Ils glissent sans sauter quand on bouge doucement la visée.
- [ ] La pause et l'inventaire annulent la visée et remettent la vue à 60°.

**#6 Chance de capture**

- [ ] En visant, le champignon que le filet attraperait porte un contour de la couleur de sa chance, le même dégradé que le cercle (rouge, orange, jaune, vert). Aucun autre n'a de contour.

**#2 Inventaire**

- [ ] Tout se fait à l'appui, sans délai. Clic gauche : prendre la pile, la poser, compléter une pile du même objet (le reste reste en main), échanger avec un autre objet.
- [ ] Double clic (deux appuis sur la même case en moins de 250 ms, objet en main) : toutes les piles de cet objet viennent sur la souris, jusqu'à une pile pleine, les incomplètes d'abord.
- [ ] Clic droit : prendre la moitié (arrondie au-dessus), poser un seul objet.
- [ ] Fermer l'inventaire avec un objet en main le remet dans sa case, ou ailleurs ; ce qui ne rentre nulle part reste en main.
- [ ] Aucun nombre sur une case qui ne contient qu'un objet.
- [ ] Le panneau en bois de l'artboard Inventaire : voile, titre « Inventaire », grille 3 × 4, séparateur pointillé, « Barre rapide », aides en bas de l'écran. Les textes suivent la langue.
- [ ] À la manette : le premier mouvement du stick ou de la croix sélectionne la première case, puis on va de case en case (la grille, puis la barre rapide dessous). **A** = clic gauche, **X** = clic droit, **Y** = tout ramasser (le double clic), **B** ferme. L'objet tenu se pose sur la case sélectionnée.
- [ ] La souris et la manette partagent une seule case en surbrillance ; aucune ne reste éclairée à la réouverture.

**Composants réutilisables (21/11)**

- [ ] **Échap ferme l'écran ouvert** (l'inventaire) et n'ouvre la pause que si aucun écran n'est ouvert, comme dans Minecraft. Échap ou **Reprendre** ferme la pause. **Tab** n'ouvre pas l'inventaire par-dessus la pause.
- [ ] Tant qu'un écran est ouvert, le joueur ne marche plus, la vue ne tourne plus, ni le filet, ni la barre rapide, ni E ne répondent. **R** ne réinitialise pas.
- [ ] La main tient l'objet de la case sélectionnée de la barre rapide : le filet en main seulement sur sa case ; changer de case le range, revenir le reprend.
- [ ] `NetThrowScene` marche comme avant : le filet y est toujours en main.

**Aides de touches (21/11, artboard « Aides · 2 · textes collés »)**

- [ ] En bas à droite, sans fond : une ligne par aide, la touche dessinée pour l'appareil en main (touche, souris, bouton Xbox ou forme PlayStation), puis le texte collé à 8 px.
- [ ] En jeu, avec le filet : « Lancer le filet », « Viser », « Changer d'objet », « Inventaire ». En visant : « Lancer », « Relâcher : ne plus viser ». Pendant que le filet est parti, ses aides disparaissent.
- [ ] Un objet visé ajoute « Ramasser » en tête ; au-delà de 4 lignes, « Changer d'objet » laisse sa place.
- [ ] Inventaire ouvert : « Prendre ou poser », « La moitié, ou un seul », « Jeter » (Q ou RB), « Fermer » ; « Tout ramasser » (souris ×2 ou Y) laisse sa place à « Jeter ». Les aides centrées en bas de l'inventaire sont retirées. En pause, aucune aide.
- [ ] Les touches viennent des actions : une touche réassignée s'affiche réassignée. Les textes suivent la langue.

**Retours du 21/11, deuxième vague**

- [ ] **Menus** : à l'accueil comme en pause, aucune planche n'est allumée tant que la souris n'en survole pas une ; la souris qui quitte une planche l'éteint. À la manette ou aux flèches, le premier mouvement allume la première planche.
- [ ] **Jeter** (comme Minecraft) : dans l'inventaire, un clic hors du panneau jette ce que la souris tient, toute la pile au clic gauche, un seul objet au clic droit ; Q ou RB jette un objet de la case sous la souris ou sélectionnée à la manette. Comme dans Minecraft ou Terraria, l'image de l'objet dans l'inventaire tombe de la main devant le joueur et flotte au-dessus du sol, tournée vers la caméra, entourée comme ce qui se ramasse ; E la reprend.
- [ ] **Objets ramassés** : au-dessus de la barre rapide, une ligne courte par objet, « +1 Bâton », « +2 Champignon », sans fond. Ramasser le même objet ajoute à sa ligne ; une ligne s'efface après 2 s, et trois au plus restent à l'écran. Cueillir, capturer et reprendre un objet jeté s'affichent ; ranger l'inventaire non.
- [ ] **Saut** : Espace, ou A / ✕ à la manette, saute d'environ 1,2 m, au sol seulement et pas accroupi. Le dodge n'existe plus.
- [ ] **Zoom** : ouvrir l'inventaire ou la pause en visant élargit la vue comme le relâchement du clic droit, sur 0,15 s, au lieu d'un coup.

**Retours du 21/11, troisième vague**

- [ ] **Les objets ramassés vont d'abord dans la barre rapide** (Hugo, 21/11), comme dans Minecraft : d'abord compléter une pile du même objet où qu'elle soit, puis les cases libres de la barre rapide, puis le sac. `InventoryLayout.FillOrder`, que `Backpack` donne à `PlayerInventory`.
- [ ] **Le cercle de l'appui long n'apparaît qu'après le temps d'un appui court** (0,3 s), puis se remplit : voir [spongeus-fougere](./spongeus-fougere.md).
- [ ] **Le nom de l'objet en main s'affiche au-dessus de la barre rapide** (Hugo, 21/11 ; artboard « HUD · noms et ramassage » du canevas de maquettes), comme dans Minecraft : quand un autre objet arrive en main (molette, touches 1 à 3, ramassage dans la case sélectionnée), son nom s'affiche 2 s puis s'efface en 0,4 s. Mains vides : rien. Il se cache avec la barre quand l'inventaire est ouvert. `HeldItemName`, enfant de la barre (`Hotbar`).
- [ ] **Dans l'inventaire, une plaquette en bois donne le nom de l'objet de la case visée** (souris ou manette), juste en bas à droite de la case. Rien sur une case vide, ni quand un objet est en main (il cache la case). `ItemNamePlate`, dans le prefab `Inventory`.
- [ ] **Le fil des objets ramassés passe en bas à gauche** (36 px des bords), lignes alignées à gauche : au-dessus de la barre rapide, c'est la place du nom de l'objet en main, et le bas droit est aux aides de touches. C'est le placement le plus courant dans les jeux.
- [ ] **Le vent est moins fort** (Hugo, 21/11) : à l'accueil, la boucle de la lisière passe de 0,6 à 0,24 (−60 %) ; au spawn, le fond de lisière (`southVolume` de `Audio/Beds`, scène `MysticForest`) passe de 0,15 à 0,12 (−20 %). Les rafales ne changent pas.

**Nettoyage du 21/11 (refactor, aucun comportement ne change)**

Hugo : « tout ce qui est dans le playtest devrait être quelque chose de non expérimental », et les textes au bon endroit seulement.

- **Les textes du joueur ne vivent que dans les tables de localisation** (`UI_fr`, `UI_en`). `PlaytestLocalization.cs`, qui les portait dans le code en français et en anglais, est supprimé, avec son menu « 1. Set Up Localization » : les tables existent, un nouveau texte s'ajoute dans la fenêtre *Localization Tables* de Unity. Les commentaires citent la clé (`hint.gather`) au lieu du texte français.
- **Plus de « Demo » ni de « Prototype » dans le code du jeu.** Le dossier `Runtime/Demo` (espace de noms `Fungiiiii.Demo`) est réparti par domaine :

| Avant | Après |
|---|---|
| `Demo/PlayerInteractor`, `IInteractable`, `IHoldInteractable`, `ILabelledInteractable`, `Gathering/InteractionTargeting` | `Interaction/` (`Fungiiiii.Interaction`) |
| `Demo/ForestGathering`, `ForestResourceSpots`, `Gatherable`, `GatherableField`, `RefillingPlant(s)` | `Gathering/` |
| `Demo/DroppedItem(s)` | `Items/` |
| `Demo/SessionTerrain`, `Capture/Net/Demo/NetDemoPrimitives` | `World/SessionTerrain`, `World/Primitives` |
| `Demo/Companion`, `CompanionKeeper`, `CompanionInHand` | `Companions/` |
| `Demo/ItemOutline`, `OutlineSettings` | `Outlines/` (`Fungiiiii.Outlines`) |
| `Demo/NetTargetOutline`, `Capture/Net/Demo/NetThrower` | `Capture/Net/` |
| `Capture/Net/Demo/NetDemoMushroom` | `Capture/WildMushroom` |
| `Capture/Net/Demo/PrototypeFirstPersonCamera` | `Player/FirstPersonCamera` |
| `Demo/StaminaDriver` | `Survival/` |
| `Demo/FungalHole`, `FungalHoleSpawner`, `DemoGroundRing` | `Champimaison/` (`GroundRing`) |
| `Demo/DemoLoopBootstrap`, `Scenes/Prototype/DemoLoopScene.unity` | `Game/GameBootstrap` (`Fungiiiii.Game`), `Scenes/Game/Game.unity` |
| Action `ResetDemo` (R) | `ResetGame`, toujours un raccourci de développement |

- **Les bancs prototypes ont leur assembly**, `Fungiiiii.Prototypes` (`Assets/Scripts/Prototypes/`) : `MushroomCaptureDemo`, `NetThrowDemo`, `GenericPatrolDemoBootstrap`, `PlayerMovementDemoBootstrap`, `PoisonTestBenchController`, `PoisonPrototypeView`, `VitalGaugesPrototype*`, `InventoryPrototypeSeed`, `ChampimaisonPrototype` et son économie `ChampimaisonState`. Le jeu (`Fungiiiii.Runtime`) ne peut pas en dépendre ; eux s'appuient sur lui (`InternalsVisibleTo`). Leurs scènes restent dans `Scenes/Prototype/`. Rien n'est supprimé. Leurs textes affichés sont marqués `i18n-exempt` : ce sont des relevés de banc, pas le jeu.
- Chaque fichier a été déplacé avec son `.meta` : les scènes et les prefabs gardent leurs références (GUID). Les `m_EditorClassIdentifier` des scènes et prefabs suivent les nouveaux noms.

## Hors périmètre

- Les icônes des objets : chaque objet reste un carré de sa couleur (décision de Hugo, 21/11).
- Une option pour masquer les aides de touches, comme le « guide des commandes » de Monster Hunter : proposée, pas décidée.
- Les scènes prototypes et bancs de test gardent leurs touches dans le code : `MushroomCaptureDemo`, F1/F2 de `PlayerMovementDemoBootstrap`, R de `NetThrowDemo`, le banc poison, le banc HUD, `FirstPersonTestPlayer`.
- Les noms traduits des objets : les fiches ont la clé, vide tant qu'aucun écran n'affiche un nom.
- `RespawningResource` n'est toujours pas utilisée.

## Implémentation

- **Forêt** : `TerrainForestGenerator` (emprise des calques), `ForestScatter` (`fadeSouth`, `fadeNorth`), menu **Fungiiiii > Playtest > 6. Regenerate Mystic Forest** ; `MysticForest.unity` et `MysticForestTerrainData.asset` regénérés.
- **Champignons** : `generate_forest_models.py`, `MushroomCluster_02` à `_04` (FBX, `.meta`, prefabs) ; leurs quatre modèles dans la fiche `Assets/Data/Gathering/Spore Mushrooms.asset`.
- **Ressources** : `ResourceSpawner` réécrit (bulles par joueur, `ResourceViewer`) ; `GatherableField` (liste de joueurs `FieldPlayer`, variantes avec leur réserve, disparition) ; `ForestResourceSpots` (`FindTreeFootMushroomSpots`, `ScatterMushroomSpots`, `RemoveOverlaps`, `MushroomPose`) ; réglages dans les fiches de `Assets/Data/Gathering/`.
- **Touche E** : `IInteractable.PromptPoint`, `PlayerInteractor` (cible, libellé du binding), `KeyPrompt`, `KeyCapGraphic`, `TriangleGlyphGraphic`, `Polygon` (partagé avec `PlankGraphic`).
- **Filet** : `NetThrower` (actions `NetPrimary` / `NetSecondary`, `HoldAim`, zoom, `PreviewTarget`, `PreviewChance`), `NetPreviewDots`, `PrototypeFirstPersonCamera.LookScale`, `NetTargetOutline`, `ItemOutlineFactory.CreateChanceOutline`, `ItemOutline.SetVisible` ; `Spongeus.fbx` et `Champoule.fbx` en Read/Write.
- **Inventaire** : `InventoryHand` (logique des clics, C# pur), `InventoryScreen` (écran refait), `CutCornerGraphic` ; dix textes dans la table `UI` (`UiText`, `PlaytestLocalization`).
- **Commandes** : toutes les actions du jeu dans `InputSystem_Actions` (cartes `Player`, `Screens`, `UI`), classe `GameControls` générée ; `ControlMaps` (cartes actives, schéma du dernier appareil, C# pur) et `PlayerControls` sur le prefab Player.
- **Objets** : `ItemDefinition` (ScriptableObject) et `ItemCatalog`, dans `Assets/Data/` ; `PlayerInventory` stocke une fiche et une quantité ; `Backpack` sur le prefab Player porte l'inventaire, que l'écran reçoit par `Bind`.
- **Main** : `IHeldItem`, `HeldItemSwitch` (C# pur), `PlayerHands`, `IItemSelection` (la barre rapide), `AimZoom` (zoom partagé de la caméra) ; `NetThrower` est le premier `IHeldItem`.
- **Écrans** : `IGameScreen`, `ScreenStack` (C# pur) ; `PauseMenu` et `InventoryScreen` sont des écrans.
- **Interaction** : `InteractionTargeting` (boîte du modèle sous le viseur d'abord), `IInteractable` (ex-`IDemoInteractable`), `KeyPrompt.PopIn`.
- **Apparitions** : `ArrivalCurves`, `GrowIn`, `FallIn` ; `GatherableSpot.DropHeight`, `ResourceSpawner.Settings.AheadWeight`.
- **Icône d'objet** : `ItemIcon`, seul endroit qui dessine un objet dans l'interface (barre rapide, cases, objet tenu) : l'icône de la fiche (`ItemDefinition.Icon`), sinon le carré de sa couleur.
- **Aides de touches** : `InputGlyphs` (contrôle → dessin, C# pur), `ControlHint`, `HintInput`, `IHintSource`, `FixedHints`, `ControlHintsPanel`, `GlyphShapeGraphic` ; `IHeldItemHints` (le filet) ; sources : `PlayerInteractor`, `PlayerHands`, `InventoryScreen`, `IInteractable.HintKey` ; douze textes `hint.*` dans la table `UI`.
- **Architecture de la scène** : `DemoLoopBootstrap` passe d'environ 1 100 à 490 lignes. Il charge la forêt, fait apparaître le joueur et le donne aux composants de la scène :
  - monde : `ForestGathering` (bâtons et champignons à cueillir), `CreaturePopulation` (champignons à capturer), `FungalHoleSpawner` (trou du Champimaison, inactif) ;
  - fiches : `GatherableDefinition`, `GatheringSettings`, `CreatureDefinition`, `NetSettings`, `StaminaSettings`, `OutlineSettings`, dans `Assets/Data/{Gathering,Creatures,Settings}` ;
  - interface : `GameScreens` (pile d'écrans, Échap, Tab, aides) et les prefabs de `Assets/Prefabs/UI/` (`KeyPrompt`, `ControlHints` et ses lignes et touches dans `Hints/`, `Hotbar` et `HotbarSlot`, `Inventory` et ses deux cases, `PauseMenu`, `MainMenu`, `PlankButton`), avec `GlyphView`, `HintRow`, `HotbarSlot`, `InventorySlotView`, `StackLabel` ;
  - joueur : le variant `PlaytestPlayer` du prefab Player porte `PlayerHands`, `PlayerInteractor`, `NetThrower`, `NetTargetOutline` et `StaminaDriver` ; Netcode l'a ajouté à `DefaultNetworkPrefabs`.
  - jauges : `VitalsHud` devient `VitalGauges` (prefab `VitalGauges`, trois instances du prefab `Gauge`), dans `DemoLoopScene`, `HUDScene` (`VitalGaugesPrototypeBootstrap`, `VitalGaugesPrototypeControls`) et `PoisonScene` ;
  - Retirés : `WoodMenu`, `DemoItems`, le menu **3. Wire Demo Loop Scene**, `VitalsHudPrototypeInstaller` (doublon du bootstrap de `HUDScene`). `PrototypeFirstPersonCamera.Moved` place la touche E.
- **Deuxième vague du 21/11** : `SelectFirstOnMove` (première planche au premier mouvement), `PlankButton.OnPointerExit` ; `PlayerInventory.Collect` et son événement `Collected`, `PickupFeed` et `PickupLine` (prefabs `PickupFeed`, `PickupLine`) ; `InventoryHand.Drop`, `InventoryScreen.Dropped`, `InventoryDropZone` sur le voile, action `UI/Drop` (Q, RB), `DroppedItems` (objet de scène), `DroppedItem` et `FacesCamera` (l'image au sol) ; action `Player/Jump` à la place de `Dodge`, `PlayerMotor.TryJump`, `PlayerMovementCalculator.JumpSpeed`, pas vertical intégré à la vitesse moyenne (hauteur indépendante de la cadence), `PlayerDodge` supprimé ; `AimZoom` sans `Drop`, en temps réel. Les objets ont leur nom (`item.*` dans la table `UI`).

## Décisions

- **Apparitions permises dans le champ de vision, plus probables devant** (Hugo, 21/11, remplace « jamais dans le champ ») : en marchant tout droit, on ne trouvait rien devant soi. Un champignon pousse, un bâton tombe du feuillage, pour ne rien voir surgir. Poids 4 devant (`spawnAheadWeight`), tirage par échantillonnage pondéré.
- **Les champignons de décor gardent la priorité** sur les nouveaux emplacements quand ils se chevauchent : ils sont listés en premier.
- **La logique des clics de l'inventaire est en C# pur** (`InventoryHand`), testée en EditMode ; l'écran ne fait que l'afficher.
- **Composants réutilisables dès maintenant** (Hugo, 21/11) : la priorité est donnée à la propreté pour les prochains objets, écrans et aides, plutôt qu'au code minimal du cas présent.
- **Toutes les commandes dans l'asset `InputSystem_Actions`**, comme le demande l'ADR-0003, avec la classe générée. Une carte `Screens` reste active pour ouvrir et fermer les écrans ; un écran ouvert coupe `Player` et active `UI`. Le schéma suit le dernier appareil qui a déclenché une action (sticks compris) ; `FindControlSchemeForDevice` n'est pas utilisé, car il exige tous les appareils du schéma et ne rendait jamais la main au clavier seul.
- **Les objets sont des fiches (ScriptableObject) trouvées par id dans un catalogue** : deux piles sont le même objet quand elles pointent vers la même fiche. L'id sert aux sauvegardes et au réseau. La démo refuse de démarrer si le catalogue n'est pas sain.
- **L'inventaire appartient au joueur** (`Backpack`), pas à l'écran qui l'affiche.
- **`PlayerHands` est le seul lecteur des actions d'usage** et les donne à l'objet en main (`IHeldItem`) : deux objets ne répondent jamais au même clic. Un objet agit en main par un comportement que sa scène enregistre (le filet, qui a besoin du monde) ou par le prefab de sa fiche. Le zoom est dans `AimZoom`, partagé par tout objet qui vise.
- **Écrans en pile, règle de Minecraft** : Échap ferme l'écran du dessus et n'ouvre la pause que si rien n'est ouvert ; un écran ouvert depuis un autre (une confirmation) se ferme d'abord. Une table de craft n'aura qu'à être un `IGameScreen` ouvert depuis son interaction.
- **Visée de l'interaction** : ce qui est sous le viseur gagne (boîte du modèle, sans physique : les objets à ramasser n'ont pas de collider) ; sinon le plus proche du centre dans le cône.
- **La mention de l'exception à la Charte IA est retirée** de la branche, sur décision de Hugo.
- **Aides de touches en bas à droite, sans fond, textes collés à leur touche** (Hugo, 21/11, sur cinq essais du canvas) : la proximité dit à quelle touche va un texte ; aligner les textes sur la touche la plus large les éloignait des touches courtes. Comme le guide des commandes de Monster Hunter : icône puis texte court, contextuel, sans plaque. Texte crème avec contour sombre pour se lire sur la forêt.
- **Chaque source déclare ses aides** (`IHintSource`) ; le panneau ne change jamais pour une nouvelle aide. Les touches sont lues dans les actions (réassignation comprise) ; seuls les boutons que lit l'event system (clics, A) sont des chemins fixes. La touche au-dessus de la cible passe par le même `InputGlyphs`.
- **Les icônes d'objets se posent dans les fiches** : un `Sprite` dans `ItemDefinition.Icon` s'affiche partout via `ItemIcon` ; sans icône, le carré de couleur reste.
- **`PlaytestLocalization` retire les textes qu'il avait ajoutés et que le jeu n'utilise plus** (`RetiredKeys`), en plus d'ajouter les manquants.
- **Ce qui se règle dans l'Editor se fait dans l'Editor** (Hugo, 21/11, « on respecte les règles de Unity ») : l'interface est en prefabs, le monde en objets de scène, les réglages et le contenu en fiches ; le point d'entrée reste petit. Une nouvelle espèce ou ressource est une fiche, pas du code. Les prefabs et les scènes font foi : les scripts qui les ont écrits ont été supprimés une fois lancés. La règle est dans `CLAUDE.md` (*Scenes, prefabs and data*).
- **Les jauges ont le style des menus** (Hugo, 21/11) : sans plaque, comme les aides ; chacune est une petite planche (arête claire, dessous sombre, coins coupés) autour d'un creux sombre que remplit une couleur de la palette des maquettes : rouge chapeau `#D94F3D` pour la vie, doré `#E8CF7E` pour l'endurance, violet `#8E6CB8` pour le poison. En haut à gauche, à 36 px du bord comme la barre rapide.
- **Chaque graphisme maison exige son `CanvasRenderer`** (`[RequireComponent]`, comme `CutCornerGraphic`) : sans lui, Unity ne dessine rien et ne dit rien. Les prefabs des planches, de la touche E et de trois aides avaient été enregistrés sans ; un rendu de contrôle l'a montré.
- **`VitalsHud` devient `VitalGauges`** (proposé le 21/11, Hugo ayant demandé de critiquer le nom ; à confirmer) : « Vitals » reste, c'est le nom du modèle (`PlayerVitals`) et le terme usuel pour la vie et l'endurance ; « valeurs de personnage » désignerait plutôt des caractéristiques permanentes (force, vitesse). « Hud » était trop large : la barre rapide, les aides et la touche E font aussi partie du HUD. Ce composant n'en montre que les jauges, le mot des docs.
- **Les composants du joueur de la partie sont sur un variant** (`PlaytestPlayer`) : le prefab Player de base reste celui des autres scènes et du réseau.
- **La touche E se place quand la caméra a suivi le joueur** (`PrototypeFirstPersonCamera.Moved`), pas dans son propre `LateUpdate` : Unity ne garantit pas l'ordre de deux `LateUpdate`, et la touche affichait la position de l'image précédente quand l'interaction passait avant la caméra.
- **Le dodge est retiré, le saut le remplace** (Hugo, 21/11) : à la première personne, le dodge ne convient pas ; il reviendra si le besoin se montre. Saut de 1,2 m, sans coût d'endurance, refusé en l'air et accroupi. Espace et A, libres une fois le dodge parti.
- **La hauteur du saut ne dépend pas de la cadence** : `PlayerMotor` déplace le joueur à la moyenne des vitesses verticales avant et après chaque pas, exacte sous une gravité constante ; avec la seule vitesse d'après, un saut perdait environ v₀·dt/2, 6 cm à 60 images/s, plus de 30 cm à 10.
- **Jeter comme dans Minecraft** (Hugo, 21/11) : clic hors du panneau, gauche pour la pile, droit pour un objet ; Q (et RB à la manette) pour un objet de la case visée. Le panneau arrête les clics, pour qu'un clic entre deux cases ne jette rien. Un objet jeté est une pile au sol qu'E reprend, autant qu'il en tient.
- **Un objet jeté est son image d'inventaire au sol** (Hugo, 21/11, comme Minecraft et Terraria) : un bâton jeté qui ressemblerait à un bâton de la forêt se confondrait avec une ressource ; l'image se lit tout de suite comme un objet à reprendre, et vaut pour tout objet, filet compris. Tant que les icônes ne sont pas dessinées, c'est le carré de couleur de l'inventaire. Une icône avec de la transparence demandera un matériau découpé.
- **« Ramasser » se distingue d'« ajouter »** : `PlayerInventory.Collect` prévient l'interface, `Add` non, car remettre en place ce que tient la souris passe aussi par `Add` et ne doit rien afficher.
- **Fil des objets ramassés léger** (Hugo, 21/11, « sans être trop lourd ») : pas de fond, une ligne par objet qui se cumule, 2 s, trois lignes au plus, juste au-dessus de la barre rapide, texte crème à contour sombre comme les aides.
- **Le fil des objets ramassés quitte le dessus de la barre rapide pour le bas gauche** (Hugo, 21/11 : « peut-être un anti-pattern ») : au-dessus de la barre, la plupart des jeux mettent le nom de l'objet en main ; les ramassages vont sur le côté. Les noms restent dans les tables (clés `item.*` des fiches), aucun texte dans le code.
- **Les menus s'ouvrent sans planche allumée** (Hugo, 21/11) : la sélection suit la souris ; la manette prend la première planche à son premier mouvement, comme la première case de l'inventaire. Même règle pour la pause, par cohérence.
- **Quitter la visée pour un menu élargit la vue en douceur** (Hugo, 21/11), en temps réel pour que la transition joue aussi sous la pause.
- **Pas de note validée dans ClickUp** : voir l'en-tête.

## Tests

### Automatisés (Unity Test Runner)

Lancés en batch sur le PC de Hugo (Unity 6000.3.21f1) à chaque étape.

| Test | Mode | Couvre | Résultat |
|---|---|---|---|
| `MysticForestDataTests.NoTreeTrunkStandsInsideABoulder` | EditMode | #7 sur le Terrain réel | ✅ |
| `ForestScatterEditorTests.*`, `GrassAndGroveTests.FadeThinsTheLayerTowardsItsEdgeWithoutMovingTheOthers` | EditMode | #7, #8 | ✅ |
| `ResourceSpawnerTests` (réécrit : l'exigence a changé) | EditMode | #5 : nombre visé, distances, champ de vision, disparition, maximum, deux joueurs, graine | ✅ |
| `ForestResourceSpotsTests` | EditMode | #5 : chevauchements | ✅ |
| `NetPreviewDotsTests` | EditMode | #4 : points de l'arc | ✅ |
| `InventoryHandTests` | EditMode | #2 : clics | ✅ |
| `DemoLoopScenePlayModeTests.SticksAndMushrooms_AreAroundThePlayer` (remplace `..._AreSpreadOverTheWholeMap`) | PlayMode | #5 | ✅ |
| `DemoLoopScenePlayModeTests.NoMushroomSpot_IsCloseToAStickSpot` | PlayMode | #5 | ✅ |
| `DemoLoopScenePlayModeTests.MushroomsAppear_InSeveralModels_AllOutlined` | PlayMode | #3 | ✅ |
| `DemoLoopScenePlayModeTests.TheTarget_IsOutlinedInCyan_...`, `TheKey_ShowsEAboveTheTarget_AndFollowsIt`, `TheKey_IsHidden_...` | PlayMode | #1 | ✅ |
| `DemoLoopScenePlayModeTests.TheArc_ShowsOnlyWhileAiming`, `Aiming_NarrowsTheViewTo45_...`, `ThePauseAndTheInventory_DropTheAim_...`, `TheNet_IsThrownWhileAiming_...` ; `NetThrowScenePlayModeTests.TheArc_ShowsOnlyWhileAiming` | PlayMode | #4 | ✅ |
| `DemoLoopScenePlayModeTests.OnlyTheMushroomAimedAt_IsOutlined_InTheColourOfItsChance` | PlayMode | #6 | ✅ |
| `InventoryScreenPlayModeTests` (appui, double appui, 300 ms, clic droit, fermeture, nombre, langue) | PlayMode | #2 | ✅ |
| `PlaytestLoopPlayModeTests.FromTheHomeScreen_...` (9 objets au départ au lieu de plus de 50 : l'exigence a changé) | PlayMode | #5 | ✅ |
| `ControlMapsTests` | EditMode | Cartes actives, schéma clavier ↔ manette, sticks | ✅ |
| `ItemCatalogTests`, `PlayerInventoryTests` (fiches) | EditMode | Fiches, catalogue, piles | ✅ |
| `HeldItemSwitchTests` | EditMode | Objet en main, prefab, commandes | ✅ |
| `ScreenStackTests` | EditMode | Échap, Tab, pile, demande de fermeture | ✅ |
| `GatheringTests.Targeting_*` | EditMode | Viseur d'abord, puis le cône | ✅ |
| `ResourceSpawnerTests` (champ de vision remplacé par le poids devant) | EditMode | Apparitions devant | ✅ |
| `ArrivalCurvesTests`, `ArrivalPlayModeTests` | EditMode, PlayMode | Pousse et chute | ✅ |
| `InventoryHandTests.GatherAll_*` | EditMode | Y à la manette | ✅ |
| `KeyPromptPlayModeTests` | PlayMode | Rebond de la touche | ✅ |
| `DemoLoopScenePlayModeTests.TheInventory_IsTheOneThePlayerCarries_...`, `Escape_*` | PlayMode | Sac, écrans | ✅ |
| `InventoryScreenPlayModeTests` (navigation, focus, A, X, Y) | PlayMode | Inventaire à la manette | ✅ |
| `InputGlyphsTests`, `ControlHintTests` | EditMode | Dessin des touches, schémas, réassignation | ✅ |
| `DemoLoopScenePlayModeTests.InGame_TheHints_*`, `WhileAiming_*`, `WithATarget_*`, `TheInventory_ShowsItsOwnHints_*` | PlayMode | Aides de touches | ✅ |
| `PlayerInventoryTests.Collect_*`, `InventoryHandTests.Drop_*`, `PlayerMovementCalculatorTests.JumpSpeed_*`, `PlayerJumpPlayModeTests` (sol nu, 60 et 10 images/s), `StaminaRegulatorTests` (sans dodge) | EditMode | Ramasser, jeter, sauter, endurance | ✅ |
| `DemoLoopScenePlayModeTests.Gathering_ShowsWhatWasCollected_*`, `AnItemDroppedFromTheInventory_*`, `Space_Jumps_*`, `ThePauseAndTheInventory_DropTheAim_AndWidenTheViewAsWhenReleased` ; `InventoryScreenPlayModeTests.APressOutOfThePanel_*` ; `MainMenuPlayModeTests.NoPlankStandsOutFirst_*` | PlayMode | Deuxième vague du 21/11 | ✅ |
| `UiPrefabTests.EveryGraphicOfTheInterface_IsSavedWithTheCanvasRendererThatDrawsIt` (lit les fichiers : en mémoire, Unity ajoute le composant exigé et masque l'oubli) | EditMode | Chaque graphisme des prefabs d'interface est dessiné | ✅ |
| `DemoLoopScenePlayModeTests`, `PlaytestLoopPlayModeTests`, `MainMenuPlayModeTests`, `KeyPromptPlayModeTests`, `InventoryScreenPlayModeTests`, `VitalGaugesPlayModeTests`, `PoisonTestBenchTests` (prefabs et objets de scène) | PlayMode | Architecture : même comportement après le passage en prefabs, objets de scène et fiches | ✅ |

### Manuels (dans Unity)

| # | Étapes | Résultat attendu | OK |
|---|---|---|---|
| 1 | Accueil, Jouer. Longer le bord sud du spawn | Aucun pin dans un rocher | ⬜ |
| 2 | Suivre le chemin ouest jusqu'à (−80, −95) | La forêt s'épaissit peu à peu, sans mur | ⬜ |
| 3 | Au départ, tourner sur soi | Quelques champignons et bâtons à portée de vue, aucun collé au joueur | ⬜ |
| 4 | Cueillir un champignon, regarder devant soi une dizaine de secondes | Un champignon peut apparaître devant, entre 20 et 50 m : il pousse du sol, il ne surgit pas | ⬜ |
| 5 | Marcher 2 minutes en ligne droite | On trouve des champignons et des bâtons devant soi, de modèles différents ; les bâtons tombent des arbres | ⬜ |
| 6 | Viser un objet à moins de 2 m | Contour cyan, touche **E** au-dessus avec un petit rebond. Toucher la manette : **Y** ou **△** | ⬜ |
| 7 | Échap, puis Tab | La touche disparaît dans les deux cas | ⬜ |
| 7b | Regarder un bâton avec un champignon aux pieds | Le bâton est visé ; passer à l'autre fait rebondir la touche | ⬜ |
| 7c | Tab, puis Échap | L'inventaire se ferme, la pause ne s'ouvre pas ; Échap ensuite ouvre la pause | ⬜ |
| 7d | Échap (pause), puis Tab | L'inventaire ne s'ouvre pas ; Reprendre ferme la pause | ⬜ |
| 7e | Inventaire ouvert, appuyer sur ZQSD / WASD | Le joueur ne bouge pas | ⬜ |
| 7f | Molette jusqu'à une case vide, puis revenir au filet | Le filet disparaît de la main, puis revient | ⬜ |
| 8 | Clic gauche sans viser | Le filet part, sans arc avant | ⬜ |
| 9 | Clic droit maintenu, regarder en bas puis en haut | Zoom ; l'arc a peu de points vers le bas, beaucoup vers le haut | ⬜ |
| 10 | En visant, placer le cercle sur un Spongeus | Le Spongeus a un contour de la couleur du cercle ; les autres non | ⬜ |
| 11 | Tab, cliquer une pile | Elle est en main tout de suite, penchée, avec une ombre | ⬜ |
| 12 | Double clic sur une pile de spores | Toutes les spores viennent sur la souris, jusqu'à 16 | ⬜ |
| 13 | Prendre une pile, Tab | Elle revient dans sa case | ⬜ |
| 14 | Changer la langue en anglais | « Inventory », « Quick bar », aides en anglais | ⬜ |
| 15 | À la manette : Select, stick, A sur une pile, déplacer, A | La pile se prend, suit la case sélectionnée, se pose | ⬜ |
| 16 | À la manette : X sur une pile, Y sur une autre, B | Moitié prise ; tout ramassé ; l'inventaire se ferme | ⬜ |
| 17 | À la manette, en jeu : bouger seulement le stick droit, viser un objet | La touche affiche **Y** / **△** | ⬜ |
| 18 | Ouvrir `NetThrowScene` | Le filet est en main, se lance et vise comme avant | ⬜ |
| 19 | Deux instances (hôte et client) dans `MouvementJoueurScene` | Chaque joueur ne bouge qu'avec sa propre instance | ⬜ |
| 20 | En jeu, regarder en bas à droite, puis viser, puis lancer | Aides du filet, puis « Lancer / Relâcher », puis plus d'aide du filet tant qu'il est parti | ⬜ |
| 21 | Viser un bâton | « Ramasser » en tête avec **E** ; « Changer d'objet » disparaît | ⬜ |
| 22 | Tab ; puis manette Xbox ; puis manette PlayStation | Aides de l'inventaire ; souris et Tab, puis A X Y B, puis ✕ □ △ ○ | ⬜ |
| 23 | Le filet en main | Le filet en bas à droite de l'écran ne passe pas sous les aides | ⬜ |
| 24 | Accueil, souris hors des planches ; puis manette, stick vers le bas | Aucune planche allumée ; puis « Jouer » s'allume | ⬜ |
| 25 | Inventaire : prendre une pile, clic droit hors du panneau, puis clic gauche | L'image de l'objet tombe devant et flotte, tournée vers soi, puis le reste ; E les reprend | ⬜ |
| 26 | Inventaire : souris sur une case, Q ; à la manette, RB | Un objet de la case tombe devant | ⬜ |
| 27 | Ramasser deux bâtons, puis un champignon | « +2 Bâton » puis « +1 Champignon » au-dessus de la barre rapide, qui s'effacent | ⬜ |
| 28 | Espace, puis A à la manette ; accroupi, Espace | Saut ; pas de saut accroupi | ⬜ |
| 29 | Clic droit maintenu, Tab | La vue s'élargit en douceur, comme au relâchement | ⬜ |

## Limitations connues

- La police Fredoka Medium n'est pas dans le projet : « Barre rapide » et les aides sont en Fredoka SemiBold.
- Rien n'a été testé dans un exécutable : les tests ont tourné dans l'Editor en batch.
- L'apparence des prefabs n'a été vue que sur un rendu de contrôle dans l'Editor, sans textes traduits : jauges, barre rapide, touche E, planches de la pause. Les aides, l'inventaire et l'accueil restent à regarder en jeu.

## Modifié par

- [spongeus-fougere](./spongeus-fougere.md) — E peut aussi être maintenu (cercle autour de la touche, appui court décidé au relâchement) sur ce qui le demande ; la touche peut laisser place à un court texte ; trois aides de touches. Le contrôle « tronc ou rocher » des champignons passe dans `ForestResourceSpots.TouchesTrunkOrRock`, partagé avec les fougères à eau.
