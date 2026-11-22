# Boucle du premier playtest : graine, Champimaison, fabrication, lance-pierre

- **Ticket ClickUp :** aucun. Hugo (owner) a demandé de travailler sans ticket, sur la branche du playtest. Écart volontaire au workflow, noté aussi dans la PR.
- **Note de design validée :** aucune sous-tâche de validation. La note a été validée par Hugo le 22/11, en conversation ; son contenu est dans « Ce que fait la feature » et « Décisions ». Maquette : [`Fabrication.html`](../playtest-retours/maquettes/Fabrication.html).
- **Branche :** `feat/playtest-base`
- **Auteur :** Hugo Bernier
- **Statut :** En cours. Codé sans Editor Unity : rien n'a encore été compilé ni lancé. Le script de mise en place à usage unique reste à lancer (voir « Mise en place »).

## Docs précédents

- [playtest-base](./playtest-base.md) — l'inventaire de départ est vide : le filet n'est plus donné, il se fabrique. Le trou de la Champimaison revient, planté par le joueur. R ne remet plus la partie à zéro.
- [playtest-retours](./playtest-retours.md) — un panneau « Fabrication » s'ouvre à côté de l'inventaire. Les cailloux rejoignent les bâtons et les champignons autour du joueur. En haut à droite : l'objectif et les recettes traquées.
- [spongeus-fougere](./spongeus-fougere.md) — les Spongeus sauvages vivent dans la brume et vont boire aux fougères mouillées. L'aspect mouillé est partagé entre le compagnon et le Spongeus sauvage.
- [12487v2nnw4-demo-boucle](./12487v2nnw4-demo-boucle.md) — la Champimaison se plante dans un trou de taupe avec la graine trouvée au pied de l'arbre géant ; ses paliers se paient en dépôt progressif (eau, spores, bois). Plus de champignons dans la clairière du spawn. Le filet n'est plus en main au départ. R ne remet plus la partie à zéro.

## Ce que fait la feature

**La boucle**

- [ ] Le joueur apparaît les mains vides : inventaire vide, rien en main.
- [ ] Il trouve la graine au pied de l'arbre géant, la plante dans un trou de taupe, réveille la graine (1 Eau + 3 Spores), fabrique le filet avec le fil de la Champimaison, capture un Spongeus, paie le palier 2 (5 Eaux + 5 Spores + 5 Bois) et entre dans la Champimaison : fin de la démo.
- [ ] Tuer un Spongeus mouillé donne 1 Eau, une fois. Un Spongeus capturé donne de l'eau autant de fois qu'il boit.

**Règles générales**

- [ ] Planter, tirer et capturer demandent l'objet en main (case sélectionnée de la barre rapide). Déposer et fabriquer prennent les ressources n'importe où dans l'inventaire.
- [ ] Ce qui porte un contour peut servir. Le texte au-dessus de la cible dit ce qui manque.

**Intro : la graine qui tombe**

- [ ] Au lancement d'une partie (Jouer), la caméra est devant l'arbre géant, côté départ, et regarde son feuillage. La graine en tombe en tournant, la caméra la suit et la vue se resserre sur elle. Une fois la graine au sol, la vue revient au joueur, au départ, qui prend la main. Environ 4 s.
- [ ] Pendant l'intro, aucune interface (barre rapide, objectif, jauges, aides, touche) et aucune commande. Deux appuis la passent, comme dans les autres jeux, pour ne pas la perdre par erreur : au premier appui de n'importe quel bouton (clavier, souris, manette), « Appuie encore pour passer » s'affiche en bas à droite pendant 3 s ; un second appui pendant ce temps passe l'intro. La graine est alors posée au sol, le joueur a la main.

**Graine et trous de taupe**

- [ ] La graine est posée au pied de l'arbre géant, avec le contour de ce qui se ramasse. E la ramasse.
- [ ] De petits trous de terre sont placés dans la forêt, sur les 9 emplacements de Champimaison : chacun dans une petite clairière, à 12–25 m d'un chemin, du départ (à 20 m, sur la droite) jusqu'aux pierres levées. Placeholder : des cubes de terre en rond, posés dans le sol, autour d'un petit trou de quelques centimètres qui s'enfonce. Tant qu'aucune graine n'est plantée, ils portent un contour crème fin.
- [ ] Visé, un trou passe au contour cyan et affiche, sur un petit panneau en bois, « Nécessite une graine » sans graine ou « Prends la graine en main » si elle est dans le sac ; graine en main, la touche E avec le triangle de l'appui simple (« Planter » dans les aides en bas à droite).
- [ ] Planter consomme la graine. Ce trou devient la Champimaison. Les autres trous restent, sans contour ni interaction.

**Champimaison**

- [ ] Au-dessus de la Champimaison, un panneau en bois avec sa pointe vers elle : le titre (« Réveiller la graine », « Agrandir la Champimaison ») et le palier à droite (« 1/2 ») ; une ligne pointillée ; une ligne par ressource : icône dans un creux sombre, nom, « N dans le sac » tant qu'elle n'est pas complète, « déposé/requis » en corail quand le sac n'en a pas, en or quand il en a, en vert avec une coche une fois complète.
- [ ] Quand la Champimaison est visée, le pied du panneau montre ses touches, et aucune touche ne flotte au-dessus d'elle : E avec le triangle et « Récupérer le fil » ou « Entrer » pour l'appui court ; E avec son bord, « Maintenir » et « Déposer », « Réveiller » ou « Améliorer » pour l'appui long. Le bord se remplit pendant l'appui. Une touche qui ne ferait rien n'est pas montrée ; sans touche, le pied et sa ligne pointillée disparaissent.
- [ ] Une fois le dernier palier atteint, le panneau ne garde que la touche « Entrer ».
- [ ] E maintenu dépose ce que le joueur porte et qui manque encore, jamais plus. La Champimaison garde ce qui a été déposé. Le dernier dépôt déclenche le palier.
- [ ] Palier 1 (1 Eau + 3 Spores) : la graine se réveille, le réseau fongique pousse. La Champimaison produit du fil de mycélium, qu'un appui court sur E récupère.
- [ ] Palier 2 (5 Eaux + 5 Spores + 5 Bois) : la Champimaison devient une entrée. Appui court sur E « Entrer » : un intérieur simple, puis « Fin de la démo, merci d'avoir joué » et la planche **Accueil**.

**Cailloux**

- [ ] Des cailloux ramassables apparaissent autour du joueur, comme les bâtons : 4 à moins de 50 m au départ, aucun à moins de 8 m ; toutes les 5 s, s'il en manque, 1 au plus entre 20 et 50 m, plus souvent devant le joueur ; disparus au-delà de 75 m ; 30 au plus.
- [ ] Ils sortent du sol, hors des chemins, des troncs et des rochers, avec le contour de ce qui se ramasse. Placeholder : un cube gris. Les petits cailloux du décor restent.

**Fabrication**

- [ ] Tab ouvre l'inventaire et, à sa droite, le panneau « Fabrication ».
- [ ] En haut du panneau, sur toute la largeur : toutes les recettes, même impossibles, avec « Possible » ou « Il manque ». Aucune n'est ouverte à l'ouverture.
- [ ] Cliquer une recette ouvre ses détails en bas du panneau ; à la manette, la recette sélectionnée s'ouvre : icône, nom, description, ingrédients « possédé / requis ». Un ingrédient qui manque est en corail, avec sa provenance ; un ingrédient en quantité suffisante est en vert, avec une coche.
- [ ] En bas du panneau : **E Fabriquer** (absent tant qu'il manque un ingrédient) et **R Traquer** / **Ne plus traquer**.
- [ ] Fabriquer utilise la même touche que déplacer un objet : deuxième clic sur la recette ouverte, ou E ; **A** à la manette. Traquer : R ; **Y** à la manette. Sur une case de l'inventaire, A, X et Y gardent leur rôle.
- [ ] Fabriquer prend tous les ingrédients d'un coup. L'objet va d'abord dans la barre rapide et s'affiche dans le fil des objets ramassés. Si l'objet ne rentre pas, rien n'est pris.
- [ ] Recettes : Lance-pierre = 2 Bois + 1 Caillou ; Filet = 3 Fils de mycélium + 2 Bois.

**Haut à droite**

- [ ] L'objectif en cours : « Découvre ce qui se passe sous le grand arbre », « Plante la graine », « Réveille la graine », « Fabrique un filet », « Améliore la Champimaison », « Entre dans la Champimaison ».
- [ ] Sous l'objectif, les recettes traquées, avec leurs ingrédients mis à jour pendant le jeu. Une recette fabriquée n'est plus traquée.

**Spongeus sauvages**

- [ ] Plus aucun champignon dans la clairière du spawn. Les Spongeus vivent dans la brume, et seulement à côté des fougères à eau : un seul, à 2,5 m, sur environ 1 fougère sur 3. Les 3 fougères les plus proches de l'arbre géant (les premières qu'on croise en entrant dans la brume) ont toujours leur Spongeus et sont pleines au départ.
- [ ] Calme et sec, un Spongeus va boire à une fougère mouillée proche, mais seulement quand le joueur est à moins de 15 m et le regarde (à moins de 40° du centre de la vue) : il boit sous ses yeux, jamais à l'autre bout de la forêt ni dans son dos. Parti boire, il continue tant que le joueur reste à moins de 15 m. Il s'imbibe en 3 s, son chapeau fonce et brille peu à peu. La fougère sèche.

**Lance-pierre**

- [ ] Lance-pierre en main : clic droit maintenu (gâchette gauche) pour viser, avec l'arc et le zoom du filet, y compris après avoir ouvert l'inventaire ou la pause ; clic gauche (gâchette droite) pour tirer un caillou. Chaque tir consomme un caillou ; sans caillou, rien ne part.
- [ ] Un champignon touché perd des points de vie et fuit. Abattu, il disparaît ; un Spongeus mouillé lâche 1 Eau au sol, un Spongeus sec ne lâche rien. E ramasse ce qui est au sol.

**Panneaux en bois et touches (design system)**

Le canevas Claude Design « Panneaux en bois » (https://claude.ai/artifact/6QKkEEsLLfLJGbdY9hCTdV) est la référence : anatomie, blocs, cas en jeu.

- [ ] Toutes les touches (au-dessus des cibles, dans les panneaux, dans les aides en bas de l'écran) sont des carrés de bois clair à coins coupés avec un dessous sombre, sans liseré cyan.
- [ ] Une touche qui fait quelque chose sur un appui simple porte un petit triangle cyan qui pointe vers le bas, posé à cheval sur son bord haut.
- [ ] Une touche qui peut être maintenue porte un bord collé autour d'elle, aux coins coupés comme elle, visible dès que la cible est visée : piste crème transparente, remplie de cyan pendant l'appui, depuis le coin haut gauche et dans le sens des aiguilles d'une montre. Une touche qui fait les deux (Spongeus plein : presser ou reprendre) porte les deux.
- [ ] Un panneau en bois a des coins coupés, une arête claire en haut, un dessous sombre, une facette plus sombre sur sa partie droite, et une pointe sous lui vers ce qu'il désigne.
- [ ] Les textes qui remplacent la touche (« Nécessite une graine », « Il faut un Spongeus ») sont sur un petit panneau en bois.
- [ ] Les aides de touches en bas de l'écran ne portent ni triangle ni bord : elles listent, elles ne désignent pas une cible.

**Plus de remise à zéro**

- [ ] R ne remet plus la partie à zéro : l'action de développement `ResetGame` est retirée. Recommencer une partie passe par Pause → Accueil → Jouer, qui recharge tout.

## Hors périmètre

- Plusieurs Champimaisons, la téléportation entre elles, les paliers au-delà du 2.
- La Champoule.
- Une IA des champignons au-delà de : se promener, fuir, boire.
- Le réseau, la faim, la toxicité, la mort du joueur, la sauvegarde.
- Les modèles définitifs : graine, caillou, trou de taupe, lance-pierre, intérieur, icônes (faits par un humain, Charte IA). Placeholders en attendant.
- Toute modification de la scène `MysticForest` et de ses données de Terrain, hormis les emplacements de Champimaison (Hugo, 22/11).

## Implémentation

Chemins relatifs à `Assets/Scripts/`.

| Domaine | Fichiers |
|---|---|
| Dépôt progressif | `Runtime/Core/ItemAmount.cs`, `Runtime/Champimaison/RequirementProgress.cs`, `ChampimaisonDefinition.cs`, `ThreadProduction.cs` (nouveaux) ; `FungalNetwork.cs` (coût en liste, `Deposit`), `FungalHoleSpawner.cs` (définition, position donnée), `FungalHole.cs` (appui long = dépôt, appui court = fil ou entrée) ; `Assets/Data/Champimaison/Champimaison.asset` |
| Graine et trous de taupe | `Runtime/Champimaison/MoleHole.cs`, `MoleHoles.cs`, `PlantingRules.cs` (nouveaux) ; `Runtime/Items/DroppedItems.cs` (`Place`) ; `Runtime/Outlines/ItemOutline.cs` (`IsVisible`) ; `Assets/Data/Items/Seed.asset` ; `Game.unity` (`Mole Holes`, `Seed Spot`, spawner actif) |
| Panneau de la Champimaison | `Runtime/UI/RequirementPanel.cs`, `RequirementRow.cs` ; `Runtime/UI/LocalizedLabel.cs` (valeurs pour les entrées à trous `{0}`) |
| Cailloux | `Runtime/Gathering/ForestGathering.cs` ; `Assets/Data/Items/Pebble.asset`, `Assets/Data/Gathering/Pebbles.asset` |
| Fabrication | `Runtime/Crafting/CraftingRecipe.cs`, `RecipeBook.cs`, `Crafter.cs`, `RecipeTracker.cs` ; `Runtime/UI/CraftingPanel.cs`, `RecipeRow.cs`, `IngredientRow.cs` ; `Runtime/UI/InventoryScreen.cs` (panneau à droite, navigation manette) ; `Runtime/Core/ItemDefinition.cs` (`descriptionKey`, `sourceKey`) ; `Assets/Data/Crafting/` (recettes, livre) ; `Assets/Data/Items/MyceliumThread.asset`, `Slingshot.asset` |
| Haut à droite | `Runtime/Game/ObjectiveRules.cs`, `GameObjectives.cs` ; `Runtime/UI/ObjectiveHud.cs`, `TrackedRecipeView.cs` |
| Spongeus sauvages | `Runtime/Capture/WildSoak.cs`, `Runtime/Companions/WetLook.cs` (nouveaux) ; `WildMushroom.cs` (imbibition, `Hit`, `Felled`), `CreatureDefinition.cs` (santé, butin, imbibition), `CreaturePopulation.cs`, `Companion.cs` (utilise `WetLook`) ; `Assets/Data/Creatures/Spongeus.asset` ; `Game.unity` (Spongeus à côté d'une fougère sur trois, Champoule retirée) ; `RefillingPlants.cs` (premières fougères croisées) |
| Lance-pierre | `Runtime/Combat/CreatureHealth.cs`, `Slingshot.cs`, `SlingshotSettings.cs` ; `Assets/Data/Settings/SlingshotSettings.asset` |
| Fin de démo | `Runtime/UI/DemoEnd.cs` ; `GameBootstrap` charge `ChampimaisonInterior.unity` ; `Editor/Playtest/PlaytestSetup.cs` (scène ajoutée au build) |
| Reset retiré | action `ResetGame` retirée de `InputSystem_Actions.inputactions` et `GameControls.cs` ; `GameBootstrap.ResetGame` et les méthodes de reset sans autre appelant retirées |
| Entrées | actions `UI/Craft` (E) et `UI/Track` (R, Y) |
| Textes | 42 entrées dans la table `UI` (fr/en), dont `pickup.line` |
| Panneaux en bois et touches | `Runtime/UI/TriangleMarkGraphic.cs`, `DashedLineGraphic.cs`, `TickGraphic.cs`, `ActionLine.cs`, `Runtime/Interaction/IPanelledInteractable.cs` (nouveaux) ; `KeyCapGraphic.cs` (sans liseré, bois clair) ; `HoldRingGraphic.cs` renommé `HoldBorderGraphic.cs` (même GUID : bord en octogone autour de la touche) ; `KeyPrompt.cs` (triangle, bord, texte sur panneau, `SetMarks`) ; `PlayerInteractor.cs` (bord dès le ciblage, pas de touche sur un `IPanelledInteractable`, `HoldProgress` et `InteractGlyph` publics) ; `RequirementPanel.cs`, `RequirementRow.cs`, `IngredientRow.cs` (couleurs d'état, coche) ; `FungalHole.cs` (`IPanelledInteractable`, panneau plus bas) ; prefabs `Hints/HintKey`, `HintFace`, `HintPill`, `HintView` (face bois clair) ; entrée `panel.hold` |
| Mise en place | `Editor/Playtest/PlaytestLoopSetup.cs`, à usage unique |

### Mise en place (à faire une fois, dans l'Editor)

1. Ouvrir le projet, attendre la compilation, lire la Console.
2. Menu **Fungiiiii > Playtest > One-off: Set Up the Playtest Loop**. Il écrit :
   - `Assets/Prefabs/Environment/PF_MoleHole.prefab` et `Assets/Prefabs/Props/PF_Pebble.prefab` (placeholders), leurs matériaux `Assets/Art/Materials/Placeholder*.mat` ;
   - `KeyPrompt.prefab` au style des panneaux en bois : `PressMark` (triangle) sur la touche, `HoldRing` renommé `HoldBorder` et ramené autour de la touche, le texte dans un `TextPanel` en bois avec sa pointe ;
   - les prefabs d'interface `RequirementPanel`, `RequirementRow`, `RecipeRow`, `IngredientRow`, `TrackedIngredientRow`, `TrackedRecipe`, `ObjectiveHud`, `DemoEnd` dans `Assets/Prefabs/UI/` ;
   - le panneau « Fabrication » dans `Inventory.prefab` (l'inventaire se décale à gauche) ;
   - le composant `Slingshot` sur `PlaytestPlayer.prefab` (renommé `PF_PlaytestPlayer.prefab` le 22/11) ;
   - dans `Game.unity` : le modèle des trous de taupe, le panneau et l'objectif ;
   - la scène `Assets/Scenes/Game/ChampimaisonInterior.unity` et les scènes du build.
3. Relire le diff (prefabs, scène, `EditorBuildSettings.asset`), régler la mise en page dans l'Inspector si besoin, commiter, puis **supprimer `PlaytestLoopSetup.cs`**.

**Fait le 22/11/2026** en batchmode (`-executeMethod Fungiiiii.Editor.Playtest.PlaytestLoopSetup.SetUp`), puis le script a été supprimé. La mise en page des prefabs d'interface n'a pas encore été regardée dans l'Editor.

## Décisions

Validées par Hugo le 22/11 :

- **L'intro de la graine** (Hugo, 22/11 : « on voit un zoom sur la graine qui tombe de l'arbre géant ») : `SeedFallIntro`, dans `Game.unity`, emprunte la caméra du joueur (la scène garde une seule caméra) et coupe `FirstPersonCamera` et `PlayerControls` le temps du plan. La graine tombe avec `FallIn`, l'animation des bâtons, de `fallHeight` (18 m). La caméra se place à `cameraDistance` (7 m) de la graine, du côté du départ, l'arbre derrière. Durée : 0,6 s sur le feuillage, la chute (1,9 s), 1,2 s sur la graine au sol ; la vue passe de 60° à 30°. Les canvas de l'interface sont listés sur le composant et cachés pendant le plan. Deux appuis passent l'intro (Hugo, 22/11 : un seul appui, comme un clic droit, la passait trop vite) : le premier affiche le prefab `IntroSkipHint` (entrée `intro.skip_again`, taille 26) pendant `secondsToPressAgain` (3 s), le second passe l'intro (`InputSystem.onAnyButtonPress`). Les tests chargent la partie et passent l'intro (`game.Intro.Skip()`), sauf ceux de l'intro. `FirstPersonCamera` revient sur le joueur dès qu'elle est réactivée (`OnEnable`), pas une image plus tard : sinon, juste après l'intro passée, la vue est encore devant l'arbre.
- **La graine se trouve au pied de l'arbre géant** et se plante dans un trou de taupe, pas n'importe où. Les autres trous serviront aux Champimaisons suivantes.
- **Réveiller la graine coûte 1 Eau + 3 Spores**, le palier 2 coûte 5 Eaux + 5 Spores + 5 Bois. Le fil vient de la Champimaison réveillée : le filet arrive après le palier 1, la capture devient rentable au palier 2.
- **Tuer un Spongeus ne donne de l'eau que s'il est mouillé** : le joueur observe avant de tirer.
- **Les outils en main, les ressources dans le sac.**
- **Panneau de coût au-dessus de la Champimaison**, sur le modèle de beaucoup de jeux : la liste des ressources, puis ses touches.
- **Design system des panneaux en bois** (canevas Claude Design « Panneaux en bois ») : un cadre qui prend la taille de son contenu, des blocs empilés (en-tête, lignes de ressource, pied d'actions). Appui simple : triangle posé sur le bord haut de la touche. Appui long : bord collé à la touche qui se remplit depuis le coin haut gauche. Plus de liseré cyan sur les touches. Les menus, l'inventaire et les jauges suivaient déjà ce style.
- **Dépôt progressif** : la Champimaison garde ce qui a été déposé. Coop prévue plus tard : plusieurs joueurs contribuent à la même progression.
- **Panneau « Fabrication » à côté de l'inventaire** (maquette B), liste en haut, détails en bas, E Fabriquer, R Traquer ; à la manette A et Y. Fabriquer = la même touche que déplacer un objet.
- **Objectif et recettes traquées en haut à droite**, à la manière de Palworld.
- **Cailloux** : un nouveau modèle, mêmes règles que les bâtons ; les petits cailloux du décor restent.
- **Lance-pierre**, avec le coup de bâton comme repli si le temps manque.
- **La Champoule est mise de côté** pour cette boucle.
- **Plus de remise à zéro (R)** : recommencer passe par l'accueil.
- **Placeholders** : trou de taupe en cubes de terre autour d'un petit trou ; caillou en cube.
- **L'interface est faite pour du 16:9, en 1920×1080 ou 1280×720, sur PC** (Hugo, 22/11) : pas de console ni de tablette. Tous les canevas suivent la hauteur de l'écran (`Match` = 1) : en 16:9 rien ne change, sur un écran plus large rien n'est coupé en bas, il y a seulement plus d'espace sur les côtés.
- **L'inventaire ouvert cache les jauges et l'objectif**, et ses aides passent sur une ligne, centrée sous ses panneaux (Hugo, 22/11). Ses deux panneaux prennent la largeur d'un écran 16:9 : les jauges, l'objectif et les aides en bas à droite passaient dessous.
- **Les touches de la fabrication ne bougent pas** (Hugo, 22/11) : E Fabriquer toujours à gauche, grisé quand la recette ne peut pas être faite ou qu'aucune n'est ouverte ; R Traquer toujours à droite, grisé quand aucune recette n'est ouverte. Chaque touche est centrée dans sa moitié de la ligne, à la taille des aides du jeu.
- **Quatre tailles de texte** (Hugo, 22/11) : 60 titre d'écran, 36 titre de bloc, 26 texte courant, 22 petit texte, en 1080p (40, 24, 17, 15 dans le canevas de 720 de l'inventaire). Rien en dessous. La règle est dans `CLAUDE.md` (*UI text sizes*) et `UiTextSizeTests` la vérifie. L'objectif en jeu et le titre du panneau de la Champimaison sont en texte courant, pas en titre de bloc : « Entre dans la Champimaison » passerait sur deux lignes.
- **Une plaque sombre semi-transparente derrière l'objectif** (Hugo, 22/11), aux coins coupés comme les panneaux : lisible sur le feuillage clair sans peser comme un panneau en bois. Les aides en bas à droite restent sans fond.
- **Le détail de la recette suit la liste**, sous une ligne pointillée, au lieu d'être collé en bas du panneau : plus de grand vide avec deux recettes. La provenance d'un ingrédient est en sable discret, seul le compteur manquant est en corail.
- **Dans le panneau de la Champimaison, « 2 dans le sac » passe sous le nom** de la ressource : en petit texte, il ne tient plus à côté. Le panneau passe à 420 unités de large.
- **Le premier objectif dit où aller** (Hugo, 22/11) : « Découvre ce qui se passe sous le grand arbre ».
- **Une lueur violet clair qui respire sur la graine** (Hugo, 22/11), pour attirer l'œil : une lumière ponctuelle (`PF_SeedGlow`, `LightPulse`), posée sur la graine de l'arbre géant et partie avec elle. C'est un effet visuel fait par script : Hugo l'a décidé malgré la Charte IA.
- **Les prefabs d'interface restent dans `Assets/Prefabs/UI/`** (Hugo, 22/11), nommés d'après leur composant, comme Unity les range. `scripts/check-art-assets.sh` ne leur applique plus le dossier de domaine ni le préfixe `PF_`, qui visent les prefabs de modèles (ADR-0005). Ceux-là gardent la règle : `Player` et `PlaytestPlayer` deviennent `PF_Player` et `PF_PlaytestPlayer`, même GUID.

Architecture :

- **Données en ScriptableObject, règles en C# pur** (`RequirementProgress`, `Crafter`, `RecipeTracker`, `ObjectiveChain`, `WildSoak`, `CreatureHealth`, `ThreadProduction`), testées en EditMode.
- **`ItemAmount` partagé** par les recettes et les paliers. Pas de système générique de bâtiment améliorable.
- **Le panneau de la Champimaison suit un point du monde en espace écran**, comme `KeyPrompt` : un canvas dans le monde rendrait le texte flou de loin et illisible de biais.
- **Le cadre est un graphisme procédural (`CutCornerGraphic`) en attendant le visuel** : coins fixes, bords et centre étirés, comme une image en neuf tranches. Le jour où un humain dessine le cadre, une `Image` en mode *Sliced* le remplace dans les prefabs, sans toucher au code.
- **Les touches d'un objet qui a son panneau vont dans le pied du panneau** (`IPanelledInteractable`) : pas de touche flottante en double. Les objets sans panneau (bâtons, cailloux, fougères, Spongeus, trous de taupe) gardent la touche flottante.
- **Les touches du pied sont des copies de la touche flottante**, réduites à 32 unités : chaque appareil (clavier, Xbox, PlayStation, souris) s'y dessine de la même façon.
- **Le bord de l'appui long est visible dès que la cible est visée**, vide, au lieu d'apparaître une fois l'appui commencé : il dit à l'avance que la touche se maintient. Change le comportement de `spongeus-fougere`.
- **Pas de système générique de description de panneau en données** : chaque objet remplit son panneau avec son script, à partir des mêmes prefabs. On généralisera au troisième usage.
- **Le caillou du lance-pierre suit `NetTrajectory`** et teste le segment parcouru à chaque image (`SphereCast`), comme le filet : il ne traverse pas sa cible.
- **Les trous de taupe se placent sur les `ChampimaisonSite` de `MysticForest`**, lus au lancement. Hugo (22/11) : les 4 emplacements d'origine sont remplacés par 9, hors des chemins, répartis dans la forêt pour qu'on en croise dès le départ. Seule modification de la forêt : les emplacements ont été déplacés puis le relief et les arbres regénérés avec les outils de la forêt (« Generate relief + forest »). Avant, on a vérifié que ces outils reproduisent le Terrain commité (écart de hauteur max 4 cm, 4 arbres sur 24 290 décalés d'un cheveu) : le changement se limite au sol autour des anciens et des nouveaux emplacements (24 290 → 24 254 arbres).
- **Les Spongeus sauvages naissent à côté des fougères** (Hugo, 22/11 : sinon le joueur ne comprend jamais que le Spongeus se remplit). `CreaturePopulation` place, pour une espèce qui s'imbibe, un Spongeus à `distanceFromSource` (2,5 m) d'une part des plantes qu'elle boit (`shareOfSources`, 0,33), hors tronc et rocher, dans la zone de la brume. Les 6 points fixes sont retirés. Pas une fougère sur deux : le joueur apprend en voyant la règle deux ou trois fois au bon moment ; une fougère sans Spongeus garde de l'eau pour le compagnon, et l'eau des Spongeus abattus reste rare.
- **Les premières fougères croisées sont garanties** : `RefillingPlants` choisit les `firstMetCount` (3) fougères les plus proches de `firstMetFrom` (l'arbre géant, d'où vient le joueur), les remplit au départ, et chacune a son Spongeus.
- **Un Spongeus sauvage ne boit que sous les yeux du joueur** (`soaksWhenPlayerWithin`, 15 m, et `soaksWhenSeenWithinDegrees`, 40°, dans `Spongeus.asset` ; Hugo, 22/11, a demandé la condition du regard) : sans ça, il boit dans les premières secondes, bien avant l'arrivée du joueur, et celui-ci ne trouve que des Spongeus déjà mouillés. Au-delà de 3,5 m il reste calme : on peut le regarder boire. L'imbibition passe de 1,5 à 3 s pour qu'on ait le temps de voir le chapeau changer.
- **Le zoom de visée est partagé** (`AimZoom`) : le filet ne le règle plus qu'au début et à la fin de sa visée. Avant, il le remettait à « pas de visée » à chaque image, même rangé, et le lance-pierre ne zoomait jamais. Le lance-pierre ne garde plus de verrou après un menu : les mains ne lui donnent pas les commandes sous un menu, il reprend la visée dès qu'elles reviennent (comme le filet).
- **L'objectif est calculé à partir de l'état du jeu** (`ObjectiveRules` : graine, plantation, palier, filet), pas de l'ordre des événements : perdre la graine avant de la planter ramène au premier objectif.
- **Les recettes traquées** sont un `RecipeTracker` en C# pur tenu par `GameBootstrap`, partagé par le panneau de fabrication et l'objectif.
- **Le lance-pierre est un composant de la variante `PlaytestPlayer`**, comme le filet sur le joueur : seul le jeu en a besoin.
- **Ce que lâche un champignon abattu** passe par un rappel donné à `CreaturePopulation` : la capture ne dépend pas des objets au sol.
- **Le caillou ignore le collider des champignons** quand il teste le décor : un champignon est touché par sa distance au trajet.
- **Les textes « 1/3 » sont deux nombres** : ils ne passent pas par la table (marqués `i18n-exempt`). « 2 dans le sac » est une entrée avec `{0}`.
- **« +2 Bâton », en bas à gauche, est l'entrée `pickup.line`** (`+{0} {1}`) : chaque langue place le nombre et le nom à sa façon. Sans nom, seul « +2 » s'affiche.
- **Une fabrication dont le résultat ne rentre pas ne bouge rien** : `Crafter` l'essaie d'abord sur une copie de l'inventaire (`PlayerInventory.Copy`). Avant, il rendait les ingrédients après coup, et ils pouvaient changer de case.
- **Prefabs, câblage de `Game.unity` et données créés par un script d'Editor à usage unique** (CLAUDE.md, « Scenes, prefabs and data ») : la branche a été écrite sans Editor. Le script est lancé une fois, son résultat relu et commité, puis il est supprimé.

## Tests

### Automatisés (Unity Test Runner)

Lancés le 22/11/2026 en batchmode sur le PC de Hugo (Unity 6000.3.21f1), après le script de mise en place : tous les EditMode (381/381), et les fixtures PlayMode qui touchent la boucle (`Champimaison`, `CraftingPanel`, `GameScene`, `ItemNames`, `KeyPrompt`, `Slingshot`, `SpongeusCompanion`, `InventoryScreen`, `PlaytestLoop`, `MainMenu`, `PlayModeRunnerSmokeTests`). Les autres fixtures PlayMode (forêt, vent, sons, saut, bancs d'essai…) n'ont pas été relancées.

Corrigé à ce moment-là, sans changer ce que les tests exigent :

- **Compilation** : `PlaytestLoopGame.Game` était masqué par le namespace `Fungiiiii.Game` (renommé `Bootstrap`) ; `ChampimaisonTier` était ambigu entre le jeu et les prototypes dans `PlayModeRunnerSmokeTests` (alias) ; le script de mise en place utilisait `??` sur des objets Unity (règles UNT0007/UNT0008).
- **Script de mise en place** : ouvrir la scène `Game` déchargeait les prefabs qu'il venait d'écrire. Il les recharge maintenant par leur chemin.
- **`GameScenePlayModeTests.Wait`** comptait la durée de l'image où l'attente commence, qui est celle de la fin du chargement (0,31 s) : la visée n'avait pas encore commencé. L'attente compte maintenant les images qui suivent.
- **Exigences changées par la boucle** : `PlaytestLoopPlayModeTests` attend 13 objets autour du joueur (4 cailloux en plus) ; `ItemNamesPlayModeTests` met le filet dans la case (départ mains vides) ; `GameScenePlayModeTests.OnlyTheMushroomAimedAt_IsOutlined_InTheColourOfItsChance` emmène le joueur à 10 m du Spongeus, qui vit maintenant dans la brume.

**Instable** : `SpongeusCompanionPlayModeTests.HoldingE_TakesItBack_SqueezedFirstWhenFull` a échoué une fois sur quatre (le Spongeus n'était pas plein en 5 s), puis a passé deux fois. Cause non cherchée.

| Test | Mode | Couvre | Résultat |
|---|---|---|---|
| `RequirementProgressTests` | EditMode | Jamais plus que ce qui manque ; dépôt partiel gardé ; ligne complète intouchée ; rien de ce qui n'est pas demandé ; `Changed` ; `WouldComplete` | ✅ |
| `FungalNetworkTests` (réécrit) | EditMode | Graine endormie au départ ; dépôt partiel sans pousse ; le dernier dépôt fait pousser et le reste passe au palier suivant ; entrée au dernier palier | ✅ |
| `ThreadProductionTests` | EditMode | Rien avant le démarrage ; un fil par période ; stock plein en attente ; ce qui ne rentre pas reste | ✅ |
| `PlantingRulesTests` | EditMode | Texte ou touche selon la graine | ✅ |
| `CrafterTests` | EditMode | Tout ou rien ; ingrédients rendus si le résultat ne rentre pas, sans qu'aucune case ne bouge ; barre rapide d'abord, affiché comme ramassé | ✅ |
| `ItemNamesPlayModeTests.WhatIsCollected_ShowsBottomLeft_NotAboveTheQuickBar` | PlayMode | En bas à gauche ; « +1 » suivi du nom, tiré de l'entrée `pickup.line` | ✅ |
| `RecipeTrackerTests` | EditMode | Traquer, ne plus traquer, ordre, retirée après fabrication | ✅ |
| `ObjectiveRulesTests` | EditMode | Ordre des objectifs ; un texte par objectif | ✅ |
| `WildSoakTests` | EditMode | Source la plus proche ; sources sèches ou d'un autre objet ignorées ; imbibition ; source qui sèche en route ; interruption | ✅ |
| `CreatureHealthTests` | EditMode | Chute à zéro, une seule fois | ✅ |
| `ChampimaisonPlayModeTests` | PlayMode | Graine à l'arbre géant, avec sa lueur violette ; plantation (sac, main) ; panneau et dépôt ; fil ; entrée et fin de démo | ✅ |
| `CraftingPanelPlayModeTests` | PlayMode | Ouverture sans recette ouverte, E et R grisés ; clic puis deuxième clic ; ingrédient manquant et provenance, E grisé à sa place ; recette traquée en haut à droite jusqu'à sa fabrication | ✅ |
| `GameScenePlayModeTests.TheInventory_HidesTheVitalsAndTheObjective_AndKeepsItsHintsUnderItsPanels` | PlayMode | Jauges et objectif cachés pendant l'inventaire, revenus à sa fermeture ; ses aides sous ses panneaux | ✅ |
| `UiTextSizeTests` | EditMode | Chaque texte des prefabs d'interface a l'une des quatre tailles | ✅ |
| `SlingshotPlayModeTests` | PlayMode | Champignons dans la brume ; un Spongeus boit à une fougère pleine ; un caillou par tir, seulement en main ; eau lâchée seulement par un Spongeus mouillé | ✅ |
| `SlingshotPlayModeTests.TheFirstFernsMet_AreFull_EachWithASpongeus_AndNoSpongeusLivesAwayFromTheFerns` | PlayMode | Les 3 premières fougères croisées sont pleines, un Spongeus chacune ; des Spongeus sur une partie des fougères seulement ; aucun loin d'une fougère | ✅ |
| `SlingshotPlayModeTests.AWildSpongeus_DrinksOnlyNearThePlayer_AndLookedAt` | PlayMode | Joueur loin : 6 s sans boire ; joueur à 8 m, dos tourné : 5 s sans boire ; joueur à 8 m qui le regarde : il boit, la fougère sèche | ✅ |
| `KeyPromptPlayModeTests.AText_IsReadOnItsPanel_WhichGrowsAroundIt` | PlayMode | Le texte du petit panneau est actif et lu, le panneau grandit autour (échouait avant la correction : texte caché) | ✅ |
| `SeedFallIntroPlayModeTests` | PlayMode | Au chargement : intro en cours, ni commandes ni objectif, caméra devant la graine qui regarde le feuillage ; puis la graine tombe, la caméra la suit, la vue se resserre ; à la fin, graine au sol, vue et commandes rendues au joueur, interface revenue. Un premier appui affiche l'indice sans passer, un second passe l'intro, graine posée ; l'indice part après 3 s, un appui plus tard le réaffiche sans passer | ✅ |
| `SlingshotAimPlayModeTests` | PlayMode | Vrai clic droit simulé : le lance-pierre vise et la vue se resserre, encore après l'inventaire (échouait avant la correction : la vue ne se resserrait pas) | ✅ |
| `GameScenePlayModeTests.SticksMushroomsAndPebbles_AreAroundThePlayer` | PlayMode | 4 cailloux à moins de 50 m, comme les bâtons | ✅ |
| `KeyPromptPlayModeTests` (4 tests ajoutés) | PlayMode | Triangle sans bord pour un appui ; bord vide puis rempli, sans triangle, pour un maintien ; les deux ensemble ; texte à la place de la touche | ✅ |
| `ChampimaisonPlayModeTests.Aimed_TheChampimaison_ShowsItsKeysInItsPanel_NotOverIt` | PlayMode | Pas de touche flottante sur la Champimaison visée ; « Déposer » au pied du panneau ; pas de touche d'appui court inutile ; pied caché hors de portée | ✅ |

**Tests existants dont l'exigence change :** chaque chargement de la partie dans les tests passe l'intro (`game.Intro.Skip()`) ; dans `SlingshotPlayModeTests`, le Spongeus qu'on fait boire a le joueur à 8 m (il ne boit que sous ses yeux), et `AFelledWetSpongeus_LetsGoOfWater_ADryOneOfNothing` vide toutes les fougères avant d'abattre le Spongeus sec ; `FungalNetworkTests` (réécrit) ; dans `GameScenePlayModeTests`, le test de départ vérifie les mains vides et l'absence de Champimaison, `TheNet_StartsInTheFirstSlotOfTheQuickBar_InHand` devient `ThePlayer_StartsEmptyHanded_TheNetPutInTheQuickBarIsInHand`, les tests du filet reçoivent le filet (`LoadGameWithNet`), le test de l'objet jeté cherche le filet parmi les objets au sol (la graine y est aussi) ; `ItemNamesPlayModeTests` met le filet dans la case ; les tests de reset (`Reset_*`, `CloseAll_*`, `Clear_*`) sont retirés avec le reset ; le test de capture calme sa cible et reprend le filet lui-même ; `SpongeusCompanionPlayModeTests.Aimed_TheSpongeus_ShowsTheKey_WithoutRingUntilHeld_AndTheHoldHint` devient `Aimed_AnEmptySpongeus_ShowsTheKeyWithItsEmptyBorder_AndTheHoldHint` (le bord est visible avant l'appui, sans triangle) ; `GameScenePlayModeTests.TheKey_ShowsEAboveTheTarget_AndFollowsIt` vérifie en plus le triangle.

### Manuels (dans Unity)
| # | Étapes | Résultat attendu | OK |
|---|---|---|---|
| 1 | `Game.unity`, Play, sans toucher à rien | L'intro : la graine tombe du feuillage de l'arbre géant, la caméra la suit et zoome, puis la vue revient au départ. Ensuite : mains vides, barre rapide vide. Objectif « Découvre ce qui se passe sous le grand arbre » en haut à droite, sur une plaque sombre | ⬜ |
| 2 | Aller à l'arbre géant | La graine, contourée, au pied de l'arbre, sous une lueur violet clair qui respire. E la ramasse, la lueur part avec elle | ⬜ |
| 3 | Viser un trou de taupe, graine dans le sac | « Prends la graine en main » sur un petit panneau en bois | ⬜ |
| 4 | Graine en main, viser le trou, puis E | Touche E avec le triangle sur son bord haut. Puis la Champimaison apparaît, panneau « Réveiller la graine 1/2 », « Eau 0/1 » et « Spores 0/3 » en corail. Les autres trous n'ont plus de contour | ⬜ |
| 5 | Cueillir 3 champignons, viser la Champimaison, maintenir E | Aucune touche ne flotte ; au pied du panneau, E avec son bord, « Maintenir Déposer ». Le bord se remplit depuis le coin haut gauche. Spores 3/3 en vert avec sa coche, Eau 0/1 reste, et le pied disparaît | ⬜ |
| 6 | Ramasser des bâtons et des cailloux, Tab, en 1920×1080 puis en 1280×720 | Panneau « Fabrication » à droite, aucune recette ouverte, E et R grisés. Ni jauges ni objectif ; les aides sur une ligne sous les panneaux, rien ne se recouvre | ⬜ |
| 7 | Cliquer « Filet », R | Détails en bas, « Fil de mycélium 0/3 » en rouge avec sa provenance. Le filet apparaît en haut à droite | ⬜ |
| 8 | Cliquer deux fois « Lance-pierre » | Lance-pierre dans la barre rapide, « +1 Lance-pierre », bâtons et caillou retirés. Compteurs du filet traqué à jour | ⬜ |
| 9 | Depuis l'arbre géant, entrer dans la brume et s'approcher de la première fougère à eau, en s'arrêtant à 5–10 m dos à elle, puis se retourner | Elle est pleine, un Spongeus à côté. Dos tourné, rien. Face à lui, il va y boire, son chapeau fonce peu à peu, la fougère sèche. Visée de près, la fougère affiche « Il faut un Spongeus » en entier sur son panneau | ⬜ |
| 10 | Tirer sur un Spongeus sec jusqu'à l'abattre | Il disparaît, rien au sol | ⬜ |
| 11 | Tirer sur un Spongeus mouillé jusqu'à l'abattre | 1 Eau au sol, E la ramasse | ⬜ |
| 12 | Maintenir E sur la Champimaison | Texte « Réveiller ». Le réseau pousse, objectif « Fabrique un filet » | ⬜ |
| 13 | Attendre, appui court sur E | Fil de mycélium récupéré | ⬜ |
| 14 | Fabriquer le filet, capturer un Spongeus, le presser | Eau dans l'inventaire | ⬜ |
| 15 | Payer le palier 2, appui court sur E | Intérieur, puis « Fin de la démo », planche Accueil | ⬜ |
| 16 | Tout le parcours à la manette | Mêmes résultats ; A fabrique, Y traque | ⬜ |
| 17 | R en jeu | Rien ne se passe. Échap, Accueil, Jouer : nouvelle partie, mains vides, graine au pied de l'arbre | ⬜ |
| 18 | Exécutable Windows, partie complète par quelqu'un qui ne connaît pas le jeu | Fin atteinte en 20 min au plus | ⬜ |
| 19 | Viser un bâton, puis un Spongeus vide sorti, puis un Spongeus plein | Bâton : triangle, pas de bord. Spongeus vide : bord vide, pas de triangle. Plein : les deux. Maintenir E : le bord se remplit | ⬜ |
| 20 | Comparer les touches en jeu au canevas « Panneaux en bois » | Bois clair sans liseré, triangle à cheval sur le bord haut, bord collé à la touche ; touches des aides en bas de l'écran de la même couleur | ⬜ |
| 21 | Depuis le départ, regarder autour, puis suivre le chemin vers l'arbre géant | Un trou de taupe contouré dans une petite clairière à droite du départ, un autre à gauche du chemin à mi-route, un derrière la clairière de l'arbre ; aucun sur un chemin | ⬜ |
| 22 | Lance-pierre en main avec des cailloux : maintenir clic droit ; Tab, Tab ; maintenir clic droit | La vue se resserre et l'arc s'affiche les deux fois ; clic gauche tire pendant la visée | ⬜ |
| 23 | Jouer, appuyer une fois sur une touche pendant que la graine tombe, puis une deuxième fois ; recommencer avec un clic et à la manette | Premier appui : « Appuie encore pour passer » en bas à droite, l'intro continue. Deuxième appui : l'intro s'arrête, la vue revient au joueur, la graine est au sol au pied de l'arbre | ⬜ |

## Limitations connues

- **Compilé et testé en batchmode le 22/11/2026, pas encore joué** : aucune partie n'a été faite, aucun test manuel n'a été vérifié.
- **Mise en page des prefabs d'interface** écrite par code, jamais vue : tailles et placements à régler dans l'Inspector.
- **Au-dessus d'un trou de taupe, la touche flotte seule** : « Planter » est dans les aides en bas à droite, pas à côté de la touche comme sur le canevas. Les blocs Production et Débloque du canevas ne sont pas encore dans le panneau de la Champimaison.
- **La graine posée à l'arbre géant** est dessinée comme un objet jeté (image de l'objet), en attendant son modèle.
- **Nombre de Spongeus sauvages** : environ un tiers des fougères, 24 pour 77 au dernier lancement (`shareOfSources` dans `Game.unity`), à régler en jouant.
- **Dans la brume dense, 15 m peut être trop loin pour bien voir le Spongeus boire** : à vérifier en jouant. Le regard est mesuré au centre du Spongeus, pas à ce qui le cache (un tronc entre les deux ne l'empêche pas de boire).
- **Le texte des petits panneaux en bois était caché depuis leur ajout** (« Il faut un Spongeus », « Nécessite une graine », « Prends la graine en main ») : l'objet `Text` du prefab `KeyPrompt` était désactivé, le panneau ne montrait que ses marges. Corrigé ; vérifié par la taille du panneau (227 px pour « Il faut un Spongeus »), pas encore à l'écran.
- **Signaler l'arbre géant et la graine**, le nombre et la place des trous de taupe : à régler pendant le dev.
- **`scripts/check-art-assets.sh` refuse les prefabs de `Assets/Prefabs/UI/`** (domaine et préfixe `PF_`), y compris ceux qui existaient déjà : les nouveaux prefabs d'interface suivent la convention existante du dossier `UI`.
- Placeholders pour tous les nouveaux modèles et icônes.

## Modifié par
