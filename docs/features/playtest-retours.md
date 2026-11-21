# Retours du playtest du 20/11

- **Ticket ClickUp :** aucun, comme pour la base du playtest. Hugo (owner) a demandé de traiter les huit retours sur la même branche, sans ticket. C'est un écart volontaire au workflow, noté aussi dans la PR.
- **Note de design validée :** aucune sous-tâche de validation. Les décisions viennent de Hugo, en conversation les 20 et 21/11 ; elles sont dans « Ce que fait la feature » et « Décisions » ci-dessous. Maquettes : `docs/playtest-retours/maquettes/`.
- **Icônes d'objets :** pas faites, un humain les dessinera.
- **Branche :** `feat/playtest-base`
- **Auteur :** Hugo Bernier
- **Statut :** En cours

## Docs précédents

- [playtest-base](./playtest-base.md) — les bâtons et les champignons ne sont plus répartis sur toute la carte : ils sont gardés autour du joueur. Le contour visé devient cyan, avec la touche au-dessus. L'inventaire est refait. Le filet ne montre son arc qu'en visant.
- [12487v2byt9-mystic-forest](./12487v2byt9-mystic-forest.md) — le générateur écarte les arbres des rochers selon leur emprise, et les calques se fondent en bord de zone (`fadeSouth`, `fadeNorth`). `Border_Rocks` passe avant `Border_TreePine`. Le Terrain est regénéré. `MushroomCluster` est régénéré (même GUID) et trois variantes s'ajoutent.
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
- [ ] Toutes les 5 s, s'il en manque, 1 champignon et 1 bâton au plus apparaissent entre 20 et 50 m, **jamais dans le champ de vision** (avec 10° de marge) : on ne les voit pas surgir.
- [ ] Ce qui est à plus de 75 m du joueur disparaît. Au plus 40 champignons et 30 bâtons sur la carte.
- [ ] Les champignons poussent aussi au pied des arbres (herbe permise) et un peu partout, hors des chemins. Aucun n'est sur un bâton, dans un tronc ou dans un rocher.

**#1 Contour cyan et touche « E »**

- [ ] L'objet visé à portée porte un contour cyan `#6FD6EA` épais ; les autres gardent leur contour crème fin.
- [ ] Au-dessus de l'objet visé, une touche carrée en bois au liseré cyan affiche la touche à presser : **E** au clavier, **Y** sur une manette Xbox, **△** sur une manette PlayStation, selon le dernier appareil utilisé.
- [ ] La touche apparaît avec un petit rebond, suit la cible, et disparaît sans cible, en pause et inventaire ouvert.

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

## Hors périmètre

- Les icônes des objets : chaque objet reste un carré de sa couleur (décision de Hugo, 21/11).
- Les actions du filet ne sont pas encore dans `InputSystem_Actions`.
- `RespawningResource` n'est toujours pas utilisée.

## Implémentation

- **Forêt** : `TerrainForestGenerator` (emprise des calques), `ForestScatter` (`fadeSouth`, `fadeNorth`), menu **Fungiiiii > Playtest > 6. Regenerate Mystic Forest** ; `MysticForest.unity` et `MysticForestTerrainData.asset` regénérés.
- **Champignons** : `generate_forest_models.py`, `MushroomCluster_02` à `_04` (FBX, `.meta`, prefabs) ; `DemoLoopBootstrap.sporeMushroomPrefabs`, renseigné par **3. Wire Demo Loop Scene**.
- **Ressources** : `ResourceSpawner` réécrit (bulles par joueur, `ResourceViewer`) ; `GatherableField` (liste de joueurs `FieldPlayer`, variantes avec leur réserve, disparition) ; `ForestResourceSpots` (`FindTreeFootMushroomSpots`, `ScatterMushroomSpots`, `RemoveOverlaps`, `MushroomPose`) ; réglages dans `DemoLoopBootstrap` et `DemoLoopScene.unity`.
- **Touche E** : `IInteractable.PromptPoint`, `PlayerInteractor` (cible, libellé du binding), `KeyPrompt`, `KeyCapGraphic`, `TriangleGlyphGraphic`, `Polygon` (partagé avec `PlankGraphic`).
- **Filet** : `NetThrower` (actions `NetPrimary` / `NetSecondary`, `HoldAim`, zoom, `PreviewTarget`, `PreviewChance`), `NetPreviewDots`, `PrototypeFirstPersonCamera.LookScale`, `NetTargetOutline`, `ItemOutlineFactory.CreateChanceOutline`, `ItemOutline.SetVisible` ; `Spongeus.fbx` et `Champoule.fbx` en Read/Write.
- **Inventaire** : `InventoryHand` (logique des clics, C# pur), `InventoryDemoController` (écran refait), `CutCornerGraphic` ; dix textes dans la table `UI` (`UiText`, `PlaytestLocalization`).

## Décisions

- **Jamais d'apparition dans le champ de vision, jusqu'à la fin du brouillard.** La passation autorisait l'apparition en vue au-delà de 35 m (« le brouillard le cache ») ; le brouillard linéaire va de 15 à 185 m et ne cache qu'environ 12 % à 35 m. Hugo a choisi : jamais dans le champ. La vue est bornée à la fin du brouillard (185 m), sinon un second joueur au loin bloquerait les apparitions devant lui.
- **Les champignons de décor gardent la priorité** sur les nouveaux emplacements quand ils se chevauchent : ils sont listés en premier.
- **La logique des clics de l'inventaire est en C# pur** (`InventoryHand`), testée en EditMode ; l'écran ne fait que l'afficher.
- **Le zoom et la sensibilité sont dans `NetThrower`**, qui les applique à sa caméra : `DemoLoopScene` et `NetThrowScene` en profitent toutes les deux.
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
| `InventoryDemoControllerPlayModeTests` (appui, double appui, 300 ms, clic droit, fermeture, nombre, langue) | PlayMode | #2 | ✅ |
| `PlaytestLoopPlayModeTests.FromTheHomeScreen_...` (9 objets au départ au lieu de plus de 50 : l'exigence a changé) | PlayMode | #5 | ✅ |

### Manuels (dans Unity)

| # | Étapes | Résultat attendu | OK |
|---|---|---|---|
| 1 | Accueil, Jouer. Longer le bord sud du spawn | Aucun pin dans un rocher | ⬜ |
| 2 | Suivre le chemin ouest jusqu'à (−80, −95) | La forêt s'épaissit peu à peu, sans mur | ⬜ |
| 3 | Au départ, tourner sur soi | Quelques champignons et bâtons à portée de vue, aucun collé au joueur | ⬜ |
| 4 | Cueillir un champignon, regarder devant soi 10 s, puis se retourner | Rien n'est apparu devant ; un champignon a pu apparaître derrière | ⬜ |
| 5 | Marcher 2 minutes en ligne droite | On trouve toujours des champignons et des bâtons autour de soi, de modèles différents | ⬜ |
| 6 | Viser un objet à moins de 2 m | Contour cyan, touche **E** au-dessus avec un petit rebond. Toucher la manette : **Y** ou **△** | ⬜ |
| 7 | Échap, puis Tab | La touche disparaît dans les deux cas | ⬜ |
| 8 | Clic gauche sans viser | Le filet part, sans arc avant | ⬜ |
| 9 | Clic droit maintenu, regarder en bas puis en haut | Zoom ; l'arc a peu de points vers le bas, beaucoup vers le haut | ⬜ |
| 10 | En visant, placer le cercle sur un Spongeus | Le Spongeus a un contour de la couleur du cercle ; les autres non | ⬜ |
| 11 | Tab, cliquer une pile | Elle est en main tout de suite, penchée, avec une ombre | ⬜ |
| 12 | Double clic sur une pile de spores | Toutes les spores viennent sur la souris, jusqu'à 16 | ⬜ |
| 13 | Prendre une pile, Tab | Elle revient dans sa case | ⬜ |
| 14 | Changer la langue en anglais | « Inventory », « Quick bar », aides en anglais | ⬜ |

## Limitations connues

- La police Fredoka Medium n'est pas dans le projet : « Barre rapide » et les aides sont en Fredoka SemiBold.
- L'appareil courant est celui du dernier bouton pressé : bouger seulement un stick de manette ne fait pas passer la touche en **Y**.
- Rien n'a été testé dans un exécutable : les tests ont tourné dans l'Editor en batch.

## Modifié par
