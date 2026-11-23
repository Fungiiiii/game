# Boucle du premier playtest : graine, Champimaison, fabrication, lance-pierre

- **Ticket ClickUp :** aucun. Hugo (owner) a demandé de travailler sans ticket, sur la branche du playtest. Écart volontaire au workflow, noté aussi dans la PR.
- **Note de design validée :** aucune sous-tâche de validation. La note a été validée par Hugo le 22/11, en conversation ; son contenu est dans « Ce que fait la feature » et « Décisions ». Maquette : [`Fabrication.html`](../playtest-retours/maquettes/Fabrication.html).
- **Branche :** `feat/playtest-base`
- **Auteur :** Hugo Bernier
- **Statut :** En cours. Codé sans Editor Unity : rien n'a encore été compilé ni lancé. Le script de mise en place à usage unique reste à lancer (voir « Mise en place »).

## Docs précédents

- [playtest-base](./playtest-base.md) — l'inventaire de départ est vide : le filet n'est plus donné, il se fabrique. Le trou de la Champimaison revient, planté par le joueur. R ne remet plus la partie à zéro.
- [playtest-retours](./playtest-retours.md) — un panneau « Fabrication » s'ouvre à côté de l'inventaire. Les cailloux rejoignent les bâtons et les champignons autour du joueur. En haut à droite : l'objectif et les recettes suivies. En visant, l'aide reste « Viser ».
- [spongeus-fougere](./spongeus-fougere.md) — les Champonge sauvages vivent dans la brume et vont boire aux fougères mouillées. L'aspect mouillé est partagé entre le compagnon et le Champonge sauvage.
- [12487v2nnw4-demo-boucle](./12487v2nnw4-demo-boucle.md) — la Champimaison se plante dans un trou de taupe avec la graine trouvée au pied de l'arbre géant ; ses paliers se paient en dépôt progressif (eau, spores, bois). Plus de champignons dans la clairière du spawn. Le filet n'est plus en main au départ. R ne remet plus la partie à zéro.

## Ce que fait la feature

**La boucle**

- [ ] Le joueur apparaît les mains vides : inventaire vide, rien en main.
- [ ] Il trouve la graine au pied de l'arbre géant, la plante dans un trou de taupe, réveille la graine (1 Eau + 3 Spores), fabrique le filet avec le fil de la Champimaison, capture un Champonge, paie le palier 2 (5 Eaux + 5 Spores + 5 Bois) et entre dans la Champimaison : fin de la démo.
- [ ] Tuer un Champonge mouillé donne 1 Eau, une fois. Un Champonge capturé donne de l'eau autant de fois qu'il boit.

**Règles générales**

- [ ] Planter, tirer et capturer demandent l'objet en main (case sélectionnée de la barre rapide). Déposer et fabriquer prennent les ressources n'importe où dans l'inventaire.
- [ ] Ce qui porte un contour peut servir. Le texte au-dessus de la cible dit ce qui manque.

**Icônes des objets**

- [ ] Chaque objet a une icône provisoire dans les cases de l'inventaire, la barre rapide, la fabrication et l'objet tenu par la souris : bâton, champignon à spores, filet, Champonge, Champoule, eau, graine, caillou, fil de mycélium, lance-pierre. Formes plates, contour sombre, aux couleurs de l'objet.
- [ ] Posé au sol (la graine, l'eau d'un Champonge, un objet jeté), un objet montre la même icône, ses coins transparents découpés.

**Intro : la graine qui tombe**

- [ ] Au lancement d'une partie (Jouer), la caméra est devant l'arbre géant, côté départ, et regarde son feuillage. La graine en tombe en tournant, la caméra la suit et la vue se resserre sur elle. Une fois la graine au sol, la vue recule de la graine jusqu'aux yeux du joueur en 1,6 s, en s'élargissant, la graine toujours en vue : le joueur la voit droit devant lui quand il prend la main. Environ 5,5 s.
- [ ] Pendant l'intro, aucune interface (barre rapide, objectif, jauges, aides, touche) et aucune commande. Deux appuis la passent, comme dans les autres jeux, pour ne pas la perdre par erreur : au premier appui de n'importe quel bouton (clavier, souris, manette), « Appuie encore pour passer » s'affiche en bas à droite pendant 3 s ; un second appui pendant ce temps passe l'intro. La graine est alors posée au sol, le joueur a la main.

**Graine et trous de taupe**

- [ ] La graine est posée au pied de l'arbre géant, avec le contour de ce qui se ramasse. E la ramasse.
- [ ] De petits trous de terre sont placés dans la forêt, sur les 9 emplacements de Champimaison : chacun dans une petite clairière, à 12–25 m d'un chemin, du départ (à 20 m, sur la droite) jusqu'aux pierres levées. Placeholder : des cubes de terre en rond, posés dans le sol, autour d'un petit trou de quelques centimètres qui s'enfonce. Tant qu'aucune graine n'est plantée, ils portent un contour crème fin.
- [ ] Visé, un trou passe au contour cyan et affiche, sur un petit panneau en bois, « Nécessite une graine » sans graine ou « Prends la graine en main » si elle est dans le sac ; graine en main, la touche E avec le triangle de l'appui simple et « Planter » à sa droite.
- [ ] Planter consomme la graine. Ce trou devient la Champimaison. Les autres trous restent, sans contour ni interaction.

**Champimaison**

- [ ] Au-dessus de la Champimaison, un panneau en bois avec sa pointe vers elle : le titre (« Faire germer la graine », « Améliorer la Champimaison ») et le palier à droite (« 1/2 ») ; une ligne pointillée ; une ligne par ressource : icône dans un creux sombre, nom, « N dans le sac » tant qu'elle n'est pas complète, « déposé/requis » en corail quand le sac n'en a pas, en or quand il en a, en vert avec une coche une fois complète.
- [ ] Quand la Champimaison est visée, le pied du panneau montre ses touches, et aucune touche ne flotte au-dessus d'elle : E avec le triangle et « Entrer » pour l'appui court ; E avec son bord, « Maintenir » et « Déposer », « Faire germer » ou « Améliorer » pour l'appui long. Le bord se remplit pendant l'appui. Une touche qui ne ferait rien n'est pas montrée ; sans touche, le pied et sa ligne pointillée disparaissent.
- [ ] Le panneau ne s'affiche qu'à portée de ses touches : à 5 m au plus du centre de la Champimaison, mesurés au sol depuis les yeux, comme pour l'utiliser. Le regarder vise la Champimaison : qui voit le panneau peut s'en servir (avant : panneau dès 8 m, touches à 3 m).
- [ ] Une fois la graine réveillée, le panneau a une ligne pour le fil, sous les ressources : à gauche la touche E, grisée et sans triangle tant qu'aucun fil n'attend, allumée avec son triangle dès qu'un fil attend ; l'icône et « Fil de mycélium » ; « 0/3 » (en discret, puis « en cours », puis « complet » quand le stock est plein). Dessous, une barre qui se remplit jusqu'au prochain fil et « Prochain dans 12 s ». Stock plein : plus de barre, « Plein : récupère-le pour relancer » en vert.
- [ ] Une fois le dernier palier atteint, le panneau ne garde que la touche « Entrer ».
- [ ] E maintenu dépose ce que le joueur porte et qui manque encore, jamais plus. La Champimaison garde ce qui a été déposé. Le dernier dépôt déclenche le palier.
- [ ] Palier 1 (1 Eau + 3 Spores) : la graine germe, le réseau fongique pousse. La Champimaison produit du fil de mycélium (un toutes les 20 s, 3 au plus), qu'un appui court sur E récupère. Le fil se récupère depuis sa ligne dans le panneau (voir plus bas) ; il n'a plus de ligne à part sous le panneau.
- [ ] Palier 2 (5 Eaux + 5 Spores + 5 Bois) : la Champimaison devient une entrée. Appui court sur E « Entrer » : un intérieur simple, puis « Fin de la démo, merci d'avoir joué » et la planche **Accueil**.

**Cailloux**

- [ ] Des cailloux ramassables apparaissent autour du joueur, comme les bâtons (3 bâtons et 3 champignons à spores gardés autour du joueur, 20 de chaque au plus sur la carte) : 4 cailloux à moins de 50 m au départ, aucun à moins de 8 m ; toutes les 5 s, s'il en manque, 1 au plus entre 20 et 50 m, plus souvent devant le joueur ; disparus au-delà de 75 m ; 30 au plus.
- [ ] Ils sortent du sol, hors des chemins, des troncs et des rochers, avec le contour de ce qui se ramasse. Placeholder : un cube gris. Les petits cailloux du décor restent.
- [ ] Bâtons, champignons à spores, cailloux et objets jetés se ramassent à 3 m (avant : 2 m), mesurés au sol depuis les yeux. La croix n'a pas besoin d'être pile dessus : chaque objet compte 30 cm plus large de tous les côtés.

**Fabrication**

- [ ] Tab ouvre l'inventaire et, à sa droite, le panneau « Fabrication ».
- [ ] En haut du panneau, sur toute la largeur : toutes les recettes, même impossibles, avec « Prête » ou « À compléter ». Aucune n'est ouverte à l'ouverture.
- [ ] Cliquer une recette ouvre ses détails en bas du panneau ; à la manette, la recette sélectionnée s'ouvre : icône, nom, description, ingrédients « possédé / requis ». Un ingrédient qui manque est en corail, avec sa provenance ; un ingrédient en quantité suffisante est en vert, avec une coche.
- [ ] En bas du panneau : **E Fabriquer** (absent tant qu'il manque un ingrédient) et **R Suivre** / **Ne plus suivre**.
- [ ] Fabriquer utilise la même touche que déplacer un objet : deuxième clic sur la recette ouverte, ou E ; **A** à la manette. Suivre : R ; **Y** à la manette. Sur une case de l'inventaire, A, X et Y gardent leur rôle.
- [ ] Fabriquer prend 1 s, comme dans d'autres jeux : une barre se remplit sous l'image de la recette, puis l'objet arrive. Ouvrir une autre recette ou fermer l'inventaire arrête la fabrication, rien n'est pris. Un appui pendant la fabrication ne la relance pas.
- [ ] Fabriquer prend tous les ingrédients d'un coup, à la fin. L'objet va d'abord dans la barre rapide et s'affiche dans le fil des objets ramassés. Si l'objet ne rentre pas, rien n'est pris.
- [ ] Recettes : Lance-pierre = 2 Bois + 1 Caillou ; Filet = 3 Fils de mycélium + 2 Bois.

**Haut à droite**

- [ ] L'objectif en cours : « Découvre le secret du grand arbre », « Plante la graine », « Fais germer la graine », « Améliore la Champimaison », « Entre dans la Champimaison ».
- [ ] Une fois la graine plantée, sous l'objectif, ce que le palier de la Champimaison demande : une ligne par ressource avec son icône, son nom, « possédé/requis » (déposé et porté comptent) et, tant qu'il en manque, d'où elle vient (« Sur des plantes, dans la brume »).
- [ ] Après la première amélioration (le palier qui demande 5 Eaux), et tant qu'aucun Champonge n'a été capturé, une ligne passe avant les ressources : « (optionnel) Capture un Champonge avec le filet », l'icône du filet, et dessous « Pour l’eau : pose-le, il boit aux fougères ». Elle disparaît pour de bon dès qu'un Champonge entre dans l'inventaire.
- [ ] Sous l'objectif, les recettes suivies, avec leurs ingrédients mis à jour pendant le jeu. Une recette fabriquée n'est plus suivie. Elles sont séparées de la quête par une ligne pointillée et le titre « Fabrication », comme « Objectif » au-dessus de la quête ; ligne et titre n'apparaissent que si une recette est suivie.

**Champonge sauvages**

- [ ] Les fougères à eau font entre 0,6 et 0,8 fois la taille de leur modèle : le Champonge à côté reste bien visible.
- [ ] Plus aucun champignon dans la clairière du spawn. Les Champonge vivent dans la brume, et seulement à côté des fougères à eau : un seul, à 2,5 m, sur environ 1 fougère sur 6. Seuls les 6 plus proches du joueur, à moins de 50 m, sont présents ; au-delà de 60 m, un Champonge disparaît, et revient à sa place quand le joueur revient. Les 3 fougères les plus proches de l'arbre géant (les premières qu'on croise en entrant dans la brume) ont toujours leur Champonge et sont pleines au départ.
- [ ] Calme et sec, un Champonge va boire à une fougère mouillée proche, mais seulement quand le joueur est à moins de 15 m et le regarde (à moins de 40° du centre de la vue) : il boit sous ses yeux, jamais à l'autre bout de la forêt ni dans son dos. Parti boire, il continue tant que le joueur reste à moins de 15 m. Il s'imbibe en 3 s, l'éponge passe du jaune clair au jaune foncé et brille peu à peu ; plein, des gouttes en tombent, comme de la fougère pleine. La fougère sèche.
- [ ] Regardé, à moins de 15 m, un Champonge (sauvage ou sorti) montre son nom au-dessus de sa tête : il suffit de regarder vers lui, à moins de 25° du centre de la vue (avant : 6°, il fallait presque le viser). Le nom seul, en petit texte crème avec un contour sombre, sans panneau, qui apparaît et disparaît en fondu (0,15 s). Plein d'eau (il goutte), une goutte d'eau à gauche du nom, l'icône de l'Eau. Rien à travers un tronc.
- [ ] Le joueur s'accroupit en maintenant Ctrl (avant : C). Accroupi derrière un Champonge calme, hors de ce qu'il voit (plus de 100° de l'avant), le joueur n'est pas remarqué. Si le Champonge a bu, E « Presser » à moins de 1,8 m donne 1 Eau ; il fuit, sec. Debout, ou de face, il remarque le joueur à 3,5 m comme avant. Pas de capture sans filet.
- [ ] Tant que le joueur n'a jamais pressé de Champonge : debout, s'il regarde (à moins de 10° du centre de la vue) un Champonge plein, celui qui goutte, entre 4 et 10 m, la touche Ctrl s'affiche à droite du Champonge avec « Maintenir · S’approcher discrètement ». Elle disparaît quand il s'accroupit ou détourne le regard, et ne revient plus après le premier Champonge pressé. Une touche au nom long (« Ctrl ») s'élargit au lieu de déborder.

**Lance-pierre**

- [ ] Lance-pierre en main : clic droit maintenu (gâchette gauche) pour viser, avec l'arc et le zoom du filet, y compris après avoir ouvert l'inventaire ou la pause ; clic gauche (gâchette droite) pour tirer un caillou. Chaque tir consomme un caillou ; sans caillou, rien ne part.
- [ ] Un champignon touché perd des points de vie et fuit. Abattu, il disparaît ; un Champonge mouillé lâche 1 Eau au sol, un Champonge sec ne lâche rien. E ramasse ce qui est au sol.

**Panneaux en bois et touches (design system)**

Le canevas Claude Design « Panneaux en bois » (https://claude.ai/artifact/6QKkEEsLLfLJGbdY9hCTdV) est la référence : anatomie, blocs, cas en jeu.

- [ ] Toutes les touches (au-dessus des cibles, dans les panneaux, dans les aides en bas de l'écran) sont des carrés de bois clair à coins coupés avec un dessous sombre, sans liseré cyan.
- [ ] Une touche qui fait quelque chose sur un appui simple porte un petit triangle cyan qui pointe vers le bas, posé à cheval sur son bord haut.
- [ ] Une touche qui peut être maintenue porte un bord collé autour d'elle, aux coins coupés comme elle, visible dès que la cible est visée : piste crème transparente, remplie de cyan pendant l'appui, depuis le coin haut gauche et dans le sens des aiguilles d'une montre. Une touche qui fait les deux (Champonge plein : presser ou reprendre) porte les deux.
- [ ] Un panneau en bois a des coins coupés, une arête claire en haut, un dessous sombre, une facette plus sombre sur sa partie droite, et une pointe sous lui vers ce qu'il désigne.
- [ ] Les textes qui remplacent la touche (« Nécessite une graine », « Nécessite un Champonge ») sont sur un petit panneau en bois.
- [ ] Les aides de touches en bas de l'écran ne portent ni triangle ni bord : elles listent, elles ne désignent pas une cible. Ce que fait E sur une cible n'y est plus : il est écrit à droite de la touche flottante, « Presser », « Ramasser »…, et dessous « Maintenir » et ce que fait l'appui long.

**Boussole**

- [ ] En haut au centre, une bande sombre où glissent N, E, S, O quand le joueur tourne, et un « ! » jaune là où est l'objectif : la graine au pied de l'arbre géant ; le trou de taupe le plus proche pour la planter ; tant qu'il manque de l'eau pour le palier, la fougère pleine la plus proche ; sinon la Champimaison. Objectif derrière : le « ! » reste collé au bord de la bande, du côté où tourner. Cachée pendant l'intro et les menus.

**Inventaire et objets**

- [ ] La case survolée montre, sous le nom de l'objet, à quoi il sert et, s'il se trouve quelque part, où (« Au pied du grand arbre »).
- [ ] La graine s'appelle « Graine de Champimaison ».
- [ ] La liste des objets ramassés est à gauche de la barre rapide, alignée sur son bas, et non plus en bas à gauche de l'écran : le nom de l'objet en main, au-dessus de la barre, reste libre.
- [ ] Posé au sol, un objet montre son icône sans fond noir, aussi dans l'exécutable.

**Fin de la démo**

- [ ] Sous « Menu principal », une planche « Quitter le jeu ».
- [ ] Échap, ou Start ou B à la manette, revient à l'écran d'accueil, comme la planche « Menu principal ». Seule la planche ferme le jeu.

**Plus de remise à zéro**

- [ ] R ne remet plus la partie à zéro : l'action de développement `ResetGame` est retirée. Recommencer une partie passe par Pause → Menu principal → Jouer, qui recharge tout.

## Hors périmètre

- Plusieurs Champimaisons, la téléportation entre elles, les paliers au-delà du 2.
- La Champoule.
- Une IA des champignons au-delà de : se promener, fuir, boire, ne pas voir derrière soi.
- Une carte (Hugo, 22/11 : la boussole pour l'instant) et un écran des commandes (à voir si c'est utile).
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
| Champonge sauvages | `Runtime/Capture/WildSoak.cs`, `Runtime/Companions/WetLook.cs` (nouveaux) ; `WildMushroom.cs` (imbibition, `Hit`, `Felled`), `CreatureDefinition.cs` (santé, butin, imbibition), `CreaturePopulation.cs`, `Companion.cs` (utilise `WetLook`) ; `Assets/Data/Creatures/Champonge.asset` ; `Game.unity` (Champonge à côté d'une fougère sur trois, Champoule retirée) ; `RefillingPlants.cs` (premières fougères croisées) |
| Lance-pierre | `Runtime/Combat/CreatureHealth.cs`, `Slingshot.cs`, `SlingshotSettings.cs` ; `Assets/Data/Settings/SlingshotSettings.asset` |
| Fin de démo | `Runtime/UI/DemoEnd.cs` ; `GameBootstrap` charge `ChampimaisonInterior.unity` ; `Editor/Playtest/PlaytestSetup.cs` (scène ajoutée au build) |
| Reset retiré | action `ResetGame` retirée de `InputSystem_Actions.inputactions` et `GameControls.cs` ; `GameBootstrap.ResetGame` et les méthodes de reset sans autre appelant retirées |
| Entrées | actions `UI/Craft` (E) et `UI/Track` (R, Y) |
| Textes | 42 entrées dans la table `UI` (fr/en), dont `pickup.line` |
| Panneaux en bois et touches | `Runtime/UI/TriangleMarkGraphic.cs`, `DashedLineGraphic.cs`, `TickGraphic.cs`, `ActionLine.cs`, `Runtime/Interaction/IPanelledInteractable.cs` (nouveaux) ; `KeyCapGraphic.cs` (sans liseré, bois clair) ; `HoldRingGraphic.cs` renommé `HoldBorderGraphic.cs` (même GUID : bord en octogone autour de la touche) ; `KeyPrompt.cs` (triangle, bord, texte sur panneau, `SetMarks`) ; `PlayerInteractor.cs` (bord dès le ciblage, pas de touche sur un `IPanelledInteractable`, `HoldProgress` et `InteractGlyph` publics) ; `RequirementPanel.cs`, `RequirementRow.cs`, `IngredientRow.cs` (couleurs d'état, coche) ; `FungalHole.cs` (`IPanelledInteractable`, panneau plus bas) ; prefabs `Hints/HintKey`, `HintFace`, `HintPill`, `HintView` (face bois clair) ; entrée `panel.hold` |
| Mise en place | `Editor/Playtest/PlaytestLoopSetup.cs`, à usage unique |
| Retours du playtest du 22/11 (2) | `Runtime/Items/DroppedItems.cs` (matériau `DroppedIcon.mat`) ; `Runtime/UI/DemoEnd.cs` ; `RequirementPanel.prefab` (450 de large) ; action `Crouch` sur Ctrl ; `Runtime/Capture/CreaturePopulation.cs` (6 au plus, cachés au loin), `WildMushroom.cs` (`IsFree`, `IsSneakedUpOn`, `Squeeze`), `WildSqueeze.cs` (nouveau), `WildSoak.cs` (`Squeeze`), `CreatureDefinition.cs` (`wetDrip`, `seesWithinDegrees`, `squeezeReach`) ; `Runtime/Companions/WetLook.cs` (couleur du matériau, gouttes) ; `PF_WaterDrip.prefab` (copie des gouttes de la fougère) ; `ChampongeCap.mat` (jaune éponge) ; `Runtime/UI/KeyPrompt.cs` (textes à droite, pulsation), `PlayerInteractor.cs` (plus d'aides en bas pour E), `CreatureNameTag.cs` (et son prefab), `ThreadTag.cs`, `Compass.cs`, `SneakHint.cs`, `Runtime/Capture/ModelHeight.cs` (nouveaux), `ObjectiveHud.cs` (besoins du palier), `ObjectiveNeed.prefab`, `CraftingPanel.cs` (1 s), `ItemNamePlate.cs` (description, provenance) ; `Runtime/Game/ObjectiveTarget.cs` (nouveau), `SeedFallIntro.cs` (recul), `Runtime/Player/FirstPersonCamera.cs` (`ViewPose`, `LookAt`) ; `PickupFeed.prefab` ; données des bâtons et champignons ; textes dans la table `UI` |
| Retours du playtest du 23/11 | `Runtime/Interaction/InteractionTargeting.cs` (marge de la croix, `IsWithinReach` public), `PlayerInteractor.cs` (`crosshairMargin`) ; `Runtime/Champimaison/FungalHole.cs` (visée par le panneau) ; `RequirementPanel.cs`, `ThreadTag.cs` (portée de la Champimaison au lieu de leur distance) ; `CreatureNameTag.cs` et son prefab (25°, goutte) ; `ObjectiveHud.cs`, `IngredientRow.cs` (`ShowGoal`), `ObjectiveNeed.prefab` et `TrackedIngredientRow.prefab` (icônes, hauteur, provenance) ; ligne du fil : `ThreadProduction.cs`, `ProductionRow.cs` (nouveau), `RequirementPanel.cs` et son prefab, `FungalHole.ThreadItem` ; style bois : `ObjectiveHud.prefab`, `Compass.prefab`, `PickupLine.prefab`, `Hotbar.prefab` (étiquette du nom en main), `IntroSkipHint.prefab`, `CreatureNameTag.prefab`, `PauseMenu.prefab`, `Inventory.prefab`, `DemoEnd.prefab` ; `DemoEnd.cs` (Échap) ; `GatheringSettings.asset`, `Game.unity` (portées) ; entrées `objective.optional_capture` et `objective.optional_capture.hint` |

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
- **Tuer un Champonge ne donne de l'eau que s'il est mouillé** : le joueur observe avant de tirer.
- **Les outils en main, les ressources dans le sac.**
- **Panneau de coût au-dessus de la Champimaison**, sur le modèle de beaucoup de jeux : la liste des ressources, puis ses touches.
- **Design system des panneaux en bois** (canevas Claude Design « Panneaux en bois ») : un cadre qui prend la taille de son contenu, des blocs empilés (en-tête, lignes de ressource, pied d'actions). Appui simple : triangle posé sur le bord haut de la touche. Appui long : bord collé à la touche qui se remplit depuis le coin haut gauche. Plus de liseré cyan sur les touches. Les menus, l'inventaire et les jauges suivaient déjà ce style.
- **Dépôt progressif** : la Champimaison garde ce qui a été déposé. Coop prévue plus tard : plusieurs joueurs contribuent à la même progression.
- **Panneau « Fabrication » à côté de l'inventaire** (maquette B), liste en haut, détails en bas, E Fabriquer, R Suivre ; à la manette A et Y. Fabriquer = la même touche que déplacer un objet.
- **Objectif et recettes suivies en haut à droite**, à la manière de Palworld.
- **Cailloux** : un nouveau modèle, mêmes règles que les bâtons ; les petits cailloux du décor restent.
- **Lance-pierre**, avec le coup de bâton comme repli si le temps manque.
- **La Champoule est mise de côté** pour cette boucle.
- **Plus de remise à zéro (R)** : recommencer passe par l'accueil.
- **Placeholders** : trou de taupe en cubes de terre autour d'un petit trou ; caillou en cube.
- **L'interface est faite pour du 16:9, en 1920×1080 ou 1280×720, sur PC** (Hugo, 22/11) : pas de console ni de tablette. Tous les canevas suivent la hauteur de l'écran (`Match` = 1) : en 16:9 rien ne change, sur un écran plus large rien n'est coupé en bas, il y a seulement plus d'espace sur les côtés.
- **L'inventaire ouvert cache les jauges et l'objectif**, et ses aides passent sur une ligne, centrée sous ses panneaux (Hugo, 22/11). Ses deux panneaux prennent la largeur d'un écran 16:9 : les jauges, l'objectif et les aides en bas à droite passaient dessous.
- **Icônes provisoires des objets** (Hugo, 22/11 : « je t'autorise à faire des SVG juste pour ça ») : la Charte IA interdit l'IA pour ce qui se voit en jeu ; Hugo autorise ces placeholders, volontairement simples. Sources dans `tools/placeholder-icons/*.svg`, rendues en `Assets/Art/Textures/Icons/Icon_<Objet>.png` (sprites 256 px, sans mipmaps), branchées sur le champ `icon` de chaque `ItemDefinition`. Au sol, l'image d'un objet découpe ses coins transparents (`_ALPHATEST_ON` sur URP Unlit, par le matériau `DroppedIcon.mat` : voir les retours du 22/11 (2)).
- **Relecture des textes en français** (Hugo, 22/11) : « Suivre / Suivie / Ne plus suivre » remplacent « Traquer » (calque de l'anglais *track*) ; « Menu principal » et « Quitter le jeu » en pause ; « Améliorer la Champimaison » partout, au lieu d'« Agrandir » dans le titre du panneau ; « dans l’inventaire » au lieu de « dans le sac », comme le reste du jeu ; « Champignon à spores », pour ne pas confondre la ressource avec les Champonge ; « Reprendre » seul, puisque « Maintenir » est déjà affiché à côté ; espace insécable avant « ! ». Puis : « Diviser » pour le clic droit dans l'inventaire (il prend aussi un seul objet quand on en tient déjà, trop long à dire) ; « Poser » pour sortir un Champonge, en paire avec « Reprendre » ; « Faire germer » au lieu de « Réveiller » pour la graine (touche, titre et objectif) ; « Prête » / « À compléter » sur les recettes (« Manque des ingrédients » ne tient pas dans l'étiquette) ; « Nécessite une graine » et « Nécessite un Champonge » (Hugo : « Nécessite » sur les deux panneaux) ; « Aucun caillou » ; provenances qui disent où et comment ; premier objectif raccourci en « Découvre le secret du grand arbre ». Pendant la visée, l'aide reste « Viser » : « Relâcher : ne plus viser » n'apportait rien, l'entrée est retirée de la table. Le tutoiement reste pour les objectifs et les consignes, l'infinitif pour les actions des touches.
- **Les touches de la fabrication ne bougent pas** (Hugo, 22/11) : E Fabriquer toujours à gauche, grisé quand la recette ne peut pas être faite ou qu'aucune n'est ouverte ; R Suivre toujours à droite, grisé quand aucune recette n'est ouverte. Chaque touche est centrée dans sa moitié de la ligne, à la taille des aides du jeu.
- **Quatre tailles de texte** (Hugo, 22/11) : 60 titre d'écran, 36 titre de bloc, 26 texte courant, 22 petit texte, en 1080p (40, 24, 17, 15 dans le canevas de 720 de l'inventaire). Rien en dessous. La règle est dans `CLAUDE.md` (*UI text sizes*) et `UiTextSizeTests` la vérifie. L'objectif en jeu et le titre du panneau de la Champimaison sont en texte courant, pas en titre de bloc : « Entre dans la Champimaison » passerait sur deux lignes.
- **Une plaque sombre semi-transparente derrière l'objectif** (Hugo, 22/11), aux coins coupés comme les panneaux : lisible sur le feuillage clair sans peser comme un panneau en bois. Les aides en bas à droite restent sans fond.
- **Le détail de la recette suit la liste**, sous une ligne pointillée, au lieu d'être collé en bas du panneau : plus de grand vide avec deux recettes. La provenance d'un ingrédient est en sable discret, seul le compteur manquant est en corail.
- **Dans le panneau de la Champimaison, « 2 dans l’inventaire » passe sous le nom** de la ressource : en petit texte, il ne tient plus à côté. Le panneau passe à 420 unités de large.
- **Le premier objectif dit où aller** (Hugo, 22/11) : « Découvre le secret du grand arbre ».
- **Une lueur violet clair qui respire sur la graine** (Hugo, 22/11), pour attirer l'œil : une lumière ponctuelle (`PF_SeedGlow`, `LightPulse`), posée sur la graine de l'arbre géant et partie avec elle. C'est un effet visuel fait par script : Hugo l'a décidé malgré la Charte IA.
- **Les prefabs d'interface restent dans `Assets/Prefabs/UI/`** (Hugo, 22/11), nommés d'après leur composant, comme Unity les range. `scripts/check-art-assets.sh` ne leur applique plus le dossier de domaine ni le préfixe `PF_`, qui visent les prefabs de modèles (ADR-0005). Ceux-là gardent la règle : `Player` et `PlaytestPlayer` deviennent `PF_Player` et `PF_PlaytestPlayer`, même GUID.

Architecture :

- **Données en ScriptableObject, règles en C# pur** (`RequirementProgress`, `Crafter`, `RecipeTracker`, `ObjectiveChain`, `WildSoak`, `CreatureHealth`, `ThreadProduction`), testées en EditMode.
- **`ItemAmount` partagé** par les recettes et les paliers. Pas de système générique de bâtiment améliorable.
- **Le panneau de la Champimaison suit un point du monde en espace écran**, comme `KeyPrompt` : un canvas dans le monde rendrait le texte flou de loin et illisible de biais.
- **Le cadre est un graphisme procédural (`CutCornerGraphic`) en attendant le visuel** : coins fixes, bords et centre étirés, comme une image en neuf tranches. Le jour où un humain dessine le cadre, une `Image` en mode *Sliced* le remplace dans les prefabs, sans toucher au code.
- **Les touches d'un objet qui a son panneau vont dans le pied du panneau** (`IPanelledInteractable`) : pas de touche flottante en double. Les objets sans panneau (bâtons, cailloux, fougères, Champonge, trous de taupe) gardent la touche flottante.
- **Les touches du pied sont des copies de la touche flottante**, réduites à 32 unités : chaque appareil (clavier, Xbox, PlayStation, souris) s'y dessine de la même façon.
- **Le bord de l'appui long est visible dès que la cible est visée**, vide, au lieu d'apparaître une fois l'appui commencé : il dit à l'avance que la touche se maintient. Change le comportement de `spongeus-fougere`.
- **Pas de système générique de description de panneau en données** : chaque objet remplit son panneau avec son script, à partir des mêmes prefabs. On généralisera au troisième usage.
- **Le caillou du lance-pierre suit `NetTrajectory`** et teste le segment parcouru à chaque image (`SphereCast`), comme le filet : il ne traverse pas sa cible.
- **Les trous de taupe se placent sur les `ChampimaisonSite` de `MysticForest`**, lus au lancement. Hugo (22/11) : les 4 emplacements d'origine sont remplacés par 9, hors des chemins, répartis dans la forêt pour qu'on en croise dès le départ. Seule modification de la forêt : les emplacements ont été déplacés puis le relief et les arbres regénérés avec les outils de la forêt (« Generate relief + forest »). Avant, on a vérifié que ces outils reproduisent le Terrain commité (écart de hauteur max 4 cm, 4 arbres sur 24 290 décalés d'un cheveu) : le changement se limite au sol autour des anciens et des nouveaux emplacements (24 290 → 24 254 arbres).
- **Les Champonge sauvages naissent à côté des fougères** (Hugo, 22/11 : sinon le joueur ne comprend jamais que le Champonge se remplit). `CreaturePopulation` place, pour une espèce qui s'imbibe, un Champonge à `distanceFromSource` (2,5 m) d'une part des plantes qu'elle boit (`shareOfSources`, 0,33, puis 0,17 après le playtest du 22/11 (2)), hors tronc et rocher, dans la zone de la brume. Les 6 points fixes sont retirés. Pas une fougère sur deux : le joueur apprend en voyant la règle deux ou trois fois au bon moment ; une fougère sans Champonge garde de l'eau pour le compagnon, et l'eau des Champonge abattus reste rare.
- **Les premières fougères croisées sont garanties** : `RefillingPlants` choisit les `firstMetCount` (3) fougères les plus proches de `firstMetFrom` (l'arbre géant, d'où vient le joueur), les remplit au départ, et chacune a son Champonge.
- **Un Champonge sauvage ne boit que sous les yeux du joueur** (`soaksWhenPlayerWithin`, 15 m, et `soaksWhenSeenWithinDegrees`, 40°, dans `Champonge.asset` ; Hugo, 22/11, a demandé la condition du regard) : sans ça, il boit dans les premières secondes, bien avant l'arrivée du joueur, et celui-ci ne trouve que des Champonge déjà mouillés. Au-delà de 3,5 m il reste calme : on peut le regarder boire. L'imbibition passe de 1,5 à 3 s pour qu'on ait le temps de voir le chapeau changer.
- **Le zoom de visée est partagé** (`AimZoom`) : le filet ne le règle plus qu'au début et à la fin de sa visée. Avant, il le remettait à « pas de visée » à chaque image, même rangé, et le lance-pierre ne zoomait jamais. Le lance-pierre ne garde plus de verrou après un menu : les mains ne lui donnent pas les commandes sous un menu, il reprend la visée dès qu'elles reviennent (comme le filet).
- **L'objectif est calculé à partir de l'état du jeu** (`ObjectiveRules` : graine, plantation, palier, filet), pas de l'ordre des événements : perdre la graine avant de la planter ramène au premier objectif.
- **Les recettes suivies** sont un `RecipeTracker` en C# pur tenu par `GameBootstrap`, partagé par le panneau de fabrication et l'objectif.
- **Le lance-pierre est un composant de la variante `PlaytestPlayer`**, comme le filet sur le joueur : seul le jeu en a besoin.
- **Ce que lâche un champignon abattu** passe par un rappel donné à `CreaturePopulation` : la capture ne dépend pas des objets au sol.
- **Le caillou ignore le collider des champignons** quand il teste le décor : un champignon est touché par sa distance au trajet.
- **Les textes « 1/3 » sont deux nombres** : ils ne passent pas par la table (marqués `i18n-exempt`). « 2 dans l’inventaire » est une entrée avec `{0}`.
- **« +2 Bâton », à gauche de la barre rapide, est l'entrée `pickup.line`** (`+{0} {1}`) : chaque langue place le nombre et le nom à sa façon. Sans nom, seul « +2 » s'affiche.
- **Une fabrication dont le résultat ne rentre pas ne bouge rien** : `Crafter` l'essaie d'abord sur une copie de l'inventaire (`PlayerInventory.Copy`). Avant, il rendait les ingrédients après coup, et ils pouvaient changer de case.
- **Retours du playtest du 22/11 (2)** (Hugo, après une partie de test) :
  - **Fond noir des objets au sol, seulement dans l'exécutable** : à la construction, Unity ne garde que les variantes de shader utilisées par un matériau du projet (log du build : « After built-in stripping: 4 » sur 32 pour URP Unlit, malgré la liste *Always Included*). Le matériau fait par code demandait `_ALPHATEST_ON`, que rien n'utilisait. `DroppedItems` copie maintenant un vrai matériau, `DroppedIcon.mat`, qui garde la variante dans le build.
  - **« Champignon à spores » sur deux lignes** dans le panneau de la Champimaison : il demande 239 unités, la ligne en avait 227 ; le panneau passe à 450 de large.
  - **Champonge** : une fougère sur six, 6 au plus autour du joueur, qui disparaissent au-delà de 60 m (Hugo : « et faire disparaître si trop loin »). Ils ne sont pas détruits : cachés, ils gardent leur place et leur état, et la liste des champignons ne change jamais, ce qui garde les contours du filet valables. Couleur éponge, jaune clair puis jaune foncé (retour de playtest) : `ChampongeCap.mat` perd la texture de palette pour une couleur unie, `WetLook` part de la couleur du matériau au lieu du blanc. Plein, des gouttes tombent, avec les réglages des gouttes de la fougère (copiés dans `PF_WaterDrip`).
  - **Ce que fait E est écrit à droite de la touche** (Hugo : « sur le texte des aides au bouton », plutôt qu'en bas à gauche), pour toutes les cibles avec une touche flottante, et l'interacteur n'ajoute plus rien aux aides en bas.
  - **« Récupérer le fil » sort du panneau** et a sa ligne, sous lui, qui pulse (Hugo : « sur une autre ligne, pas sur le même panneau, et effet de clignotement »). Remplacé le 23/11 par la ligne de production dans le panneau (voir plus bas).
  - **Le nom du Champonge se lit de loin** (Hugo : pas besoin d'être collé), quand il est visé à moins de 15 m. D'abord sur le petit panneau en bois des messages ; Hugo, en le voyant : « trop visible, devrait être un petit nametag ». Il devient le nom seul en petit texte (22), sans panneau, au-dessus de la tête, en fondu, dans son prefab `CreatureNameTag` : le panneau cachait le haut de l'éponge, celui qui change de couleur.
  - **La boussole plutôt qu'une carte** (Hugo : « boussole pour l'instant, avec un ! comme tous les jeux »). Pour l'eau, le « ! » montre une fougère pleine : Hugo voulait que ce soit clair, et le jaune seul ne disait pas où aller.
  - **Presser par derrière, accroupi, sans capturer** (Hugo : « c'est de le presser, pas de capture en crouch sans filet ») : `WildSqueeze`, une cible de plus pour E, n'existe que sur les espèces qui s'imbibent. L'eau dit comment l'obtenir : « Un Champonge plein en garde. Approche-le accroupi, par derrière. »
  - **Apprendre à presser par derrière** (Hugo : « un nouveau joueur, comment il sait ? il ne le sait pas » ; « seulement au premier récoltage » ; « et en regardant le Spongeus ») : `SneakHint` montre la touche d'accroupissement près d'un Champonge plein regardé, debout, entre 4 et 10 m, jusqu'au premier Champonge pressé (`CreaturePopulation.Squeezed`). La suite (E « Presser ») est déjà dite par la touche du Champonge. L'enchaînement repose sur ce que le joueur voit déjà : il goutte, il est plein. La hauteur d'un Champonge est calculée au même endroit pour le nom, l'indice et le pressage (`ModelHeight`).
  - **L'indice de l'eau reste mystérieux** (Hugo : « trop clair, un peu moins ? ») : « Sur des plantes, dans la brume ».
  - **Fabriquer prend 1 s après l'appui** (Hugo : « on attend 1 s après le press simple de E »), pas un appui maintenu.
  - **Objets ramassés à gauche de la barre**, comme dans Valheim, Subnautica ou Zelda, qui les empilent sur le côté, loin du nom de l'objet en main.
  - **Fougères à eau plus petites** (Hugo, en jeu : trop grandes à côté du Champonge) : leur taille est tirée entre 0,6 et 0,8 fois le modèle au lieu de 0,9 et 1,2 (`scale` dans `WaterFern.asset`), sans toucher au modèle. Les gouttes suivent la fougère.
  - **Moins de champignons, autant de bâtons** : 3 champignons autour du joueur au lieu de 5 (arrondi au-dessus de la moitié, pour les 8 spores de la boucle). Les bâtons, d'abord passés de 4 à 2, remontent à 3 : Hugo n'en trouvait plus (« autant de bâtons que de champis »).
  - **L'intro finit par un recul jusqu'au joueur**, qui regarde la graine (Hugo : pour que le joueur voie qu'elle est devant lui).
  - **Fin de démo** : Échap ne faisait rien et le joueur voulait quitter ; la planche « Quitter le jeu » s'ajoute.
- **Retours du playtest du 23/11** (Hugo, après la partie d'une autre personne) :
  - **Panneau de la Champimaison** : « si on voit le panneau, on doit pouvoir cliquer dessus direct, sinon on affiche quelque chose qui ne sert à rien ». Il s'affichait à 8 m, ses touches marchaient à 3 m. Hugo : 5 m pour les deux (« sinon c'est étrange »), et la récupération du fil aussi. Le panneau et la ligne du fil n'ont plus leur propre distance : ils demandent à la Champimaison si le joueur est à portée (`InteractionTargeting.IsWithinReach`, la même mesure que pour l'utiliser), ce qui empêche les deux de se décaler de nouveau. La Champimaison est visée par son panneau (point visé au pied du panneau, boîte de 2 × 2,5 m sur le trou) : sinon, de près, regarder le panneau ne la visait pas.
  - **Ramassage** : « trop court en distance, et trop précis sur la souris ». Hugo : 3 m pour les plantes ; les objets jetés suivent. La boîte de chaque cible compte 30 cm plus large pour la croix (`crosshairMargin`, sur le joueur), pour toutes les cibles avec un modèle.
  - **Le filet sert à l'eau, dans l'objectif** : la personne n'avait pas compris qu'on pouvait capturer un Champonge pour l'eau. Une ligne optionnelle, à partir du palier à 5 Eaux (Hugo : « quand on a déjà amélioré une fois la maison »), jusqu'à la première capture. C'est une ligne de besoin (`IngredientRow.ShowGoal`) sans compte, pour garder le même style. Capturé = un objet compagnon a été dans l'inventaire au moins une fois.
  - **Nom du Champonge** : « le voir tout le temps tant que tu regardes vers lui, pas être directement sur lui ». 25° au lieu de 6°, comme le cône de visée. Plein d'eau, une goutte à gauche du nom, comme les icônes d'état de la plupart des jeux ; elle suit les gouttes qui tombent (`IsDripping`), ce que le joueur voit.
  - **Fin de démo impossible à quitter avec Échap** : Échap revient à l'accueil, comme le « retour » des autres écrans ; quitter reste sur la planche, pour ne pas fermer le jeu par erreur. `DemoEnd` éteint ses contrôles avant de les libérer : libérés allumés, ils restaient dans le système d'entrées et faisaient échouer les tests suivants qui simulent Ctrl.
  - **Plus d'objectif « Fabrique un filet »** (Hugo, en voyant la liste : « l'objectif, c'est améliorer la Champimaison, le filet c'est un moyen de faire ») : une fois la graine germée, l'objectif est « Améliore la Champimaison ». Le filet n'est plus que la ligne optionnelle. `ObjectiveRules` ne suit plus le filet (`HasHadNet` retiré), la boussole ne montre plus la Champimaison pour son fil avant le filet, et l'entrée `objective.craft_net` est retirée de la table.
  - **Lignes sous l'objectif** (Hugo, sur sa capture : la ligne optionnelle sur deux lignes chevauchait son indice, icônes trop petites) : les icônes passent de 14 à 24 (lignes des besoins et des recettes suivies). La ligne prend la hauteur de ses textes (30 au moins, au lieu de 30 fixes). La provenance passe sous le nom, décalée après l'icône, plus terne (opacité 0,6 au lieu de 0,8). Hugo voulait la provenance plus petite : elle est déjà en 22, le minimum (« UI text sizes »), et passer les noms en 26 les aurait mis à la taille de l'objectif ; la différence se fait donc par la couleur et le décalage. L'icône du filet reste sur la ligne optionnelle.
  - **Quête et fabrication séparées** (Hugo : « ajouter une séparation entre quête et craft item, c'est pas assez clair la différence ») : une ligne pointillée et le titre « Fabrication » (entrée `craft.title`, déjà dans la table) au-dessus des recettes suivies, dans le style du titre « Objectif ». Seulement quand une recette est suivie.
  - **Le Spongeus s'appelle maintenant Champonge** (Hugo : « renomme spongeus en Champonge, assets etc compris »). Partout : textes affichés (fr, en), clés de la table (`item.champonge`, `item.champonge.description`, `item.champonge.source`, `label.needs_champonge`), identifiant de l'objet (`champonge`), assets renommés avec leur `.meta`, donc leurs GUID et leurs références intactes (`Champonge.fbx`, `ChampongeCap.mat`, `Icon_Champonge.png`, `Data/Creatures/Champonge.asset`, `ChampongeCompanion.asset`, `Data/Items/Champonge.asset`, `tools/placeholder-icons/Champonge.svg`), code et tests (`ChampongeCompanionPlayModeTests`, `ChampongePopulationPlayModeTests`, `ChampongeSqueezePlayModeTests`), docs pas encore fusionnées et canevas « Panneaux en bois ». Restent sous l'ancien nom : les docs déjà fusionnées dans `develop` (on ne les réécrit pas), le nom de fichier de la doc `spongeus-fougere.md` (celui de sa branche), la capture `docs/screenshots/spongeus.png`, et les noms à l'intérieur du modèle `Champonge.fbx`, fait par un humain : son fichier source Blender et ses objets s'appellent encore Spongeus, à renommer par l'artiste au prochain export.
  - **Quand le prochain fil arrive** (Hugo : « une ligne d'interface, avec une barre de chargement, pour savoir quand le fil est récupérable »). Dans le panneau (Hugo l'a choisi plutôt que sur la ligne du fil), avec le stock et les secondes ; la ligne qui pulsait sous le panneau est supprimée (Hugo : « oui, supprime-la ») : `ThreadTag.cs`, son objet « Thread Tag » dans `Game.unity` et son câblage dans `GameBootstrap`, et la récupération sur cette ligne : E à gauche, grisé quand il n'y a pas de fil, « pour montrer au joueur que c'est récupérable, mais pas maintenant ». Dessiné d'abord dans le canevas (« Proposition : la production du fil »), selon le bloc « Production » du système. `ThreadProduction` donne `NextProgress` et `SecondsToNext` ; `ProductionRow` (nouveau) affiche la ligne ; le prefab du panneau la reçoit d'un script d'Editor à usage unique (`TmpProductionRow`, supprimé), fait avec les pièces existantes (la touche de la ligne d'action, le creux et les textes de la ligne de ressource). Entrées `panel.thread_next` (`Prochain dans {0} s`) et `panel.thread_full`.
  - **Toute l'interface en bois** (Hugo : « notre jeu, on dirait une interface Roblox »). Relevé et propositions dans le canevas « Panneaux en bois » (rangée « Proposition : toute l'interface en bois »), puis « oui, tout » :
    - l'objectif passe dans le cadre bois des panneaux (face, arête, dessous, facette), « Objectif » en « texte discret » (#CDB896) au lieu du cyan, réservé aux touches ;
    - la boussole devient une planche, ses lettres sans contour, le « ! » en « en cours » (#E8CF7E) ;
    - ce qui reste à l'écran passe sur une petite étiquette en bois, sans contour : les objets ramassés, le nom de l'objet en main, « Appuie encore pour passer » (Hugo : « mélange A et B ») ;
    - le nom du Champonge reste un texte seul au-dessus de lui, pour ne pas le cacher, avec une ombre douce au lieu du contour ;
    - les voiles derrière la pause, l'inventaire et la fin de démo passent du bleu ardoise au brun « creux » (#2B2723), même opacité ;
    - la barre de fabrication reprend celle du bloc « Production » : creux brun, remplissage crème.
    La police (Fredoka) et les icônes provisoires ne changent pas : choix de direction artistique pour l'équipe (Charte IA pour les icônes).
  - **La touche E suit une cible qui bouge seule**, comme un Champonge sorti qui marche : elle n'était replacée que quand la cible changeait ou que la vue bougeait. Trouvé par `TheKey_ShowsEAboveTheTarget_AndFollowsIt` avec la portée à 3 m : l'objet du test était déjà visé avant d'être déplacé.
- **Prefabs, câblage de `Game.unity` et données créés par un script d'Editor à usage unique** (CLAUDE.md, « Scenes, prefabs and data ») : la branche a été écrite sans Editor. Le script est lancé une fois, son résultat relu et commité, puis il est supprimé.

## Tests

### Automatisés (Unity Test Runner)

Lancés le 22/11/2026 en batchmode sur le PC de Hugo (Unity 6000.3.21f1), après le script de mise en place : tous les EditMode (381/381), et les fixtures PlayMode qui touchent la boucle (`Champimaison`, `CraftingPanel`, `GameScene`, `ItemNames`, `KeyPrompt`, `Slingshot`, `ChampongeCompanion`, `InventoryScreen`, `PlaytestLoop`, `MainMenu`, `PlayModeRunnerSmokeTests`). Les autres fixtures PlayMode (forêt, vent, sons, saut, bancs d'essai…) n'ont pas été relancées.

Corrigé à ce moment-là, sans changer ce que les tests exigent :

- **Compilation** : `PlaytestLoopGame.Game` était masqué par le namespace `Fungiiiii.Game` (renommé `Bootstrap`) ; `ChampimaisonTier` était ambigu entre le jeu et les prototypes dans `PlayModeRunnerSmokeTests` (alias) ; le script de mise en place utilisait `??` sur des objets Unity (règles UNT0007/UNT0008).
- **Script de mise en place** : ouvrir la scène `Game` déchargeait les prefabs qu'il venait d'écrire. Il les recharge maintenant par leur chemin.
- **`GameScenePlayModeTests.Wait`** comptait la durée de l'image où l'attente commence, qui est celle de la fin du chargement (0,31 s) : la visée n'avait pas encore commencé. L'attente compte maintenant les images qui suivent.
- **Exigences changées par la boucle** : `PlaytestLoopPlayModeTests` attend 13 objets autour du joueur (4 cailloux en plus) ; `ItemNamesPlayModeTests` met le filet dans la case (départ mains vides) ; `GameScenePlayModeTests.OnlyTheMushroomAimedAt_IsOutlined_InTheColourOfItsChance` emmène le joueur à 10 m du Champonge, qui vit maintenant dans la brume.

**Instable** : `ChampongeCompanionPlayModeTests.HoldingE_TakesItBack_SqueezedFirstWhenFull` a échoué une fois sur quatre (le Champonge n'était pas plein en 5 s), puis a passé deux fois. Cause non cherchée.

| Test | Mode | Couvre | Résultat |
|---|---|---|---|
| `RequirementProgressTests` | EditMode | Jamais plus que ce qui manque ; dépôt partiel gardé ; ligne complète intouchée ; rien de ce qui n'est pas demandé ; `Changed` ; `WouldComplete` | ✅ |
| `FungalNetworkTests` (réécrit) | EditMode | Graine endormie au départ ; dépôt partiel sans pousse ; le dernier dépôt fait pousser et le reste passe au palier suivant ; entrée au dernier palier | ✅ |
| `ThreadProductionTests` | EditMode | Rien avant le démarrage ; un fil par période ; stock plein en attente ; ce qui ne rentre pas reste | ✅ |
| `PlantingRulesTests` | EditMode | Texte ou touche selon la graine | ✅ |
| `CrafterTests` | EditMode | Tout ou rien ; ingrédients rendus si le résultat ne rentre pas, sans qu'aucune case ne bouge ; barre rapide d'abord, affiché comme ramassé | ✅ |
| `ItemNamesPlayModeTests.WhatIsCollected_ShowsLeftOfTheQuickBar_NotAboveIt` | PlayMode | À gauche de la barre rapide, sur son bas ; « +1 » suivi du nom, tiré de l'entrée `pickup.line` | ✅ |
| `RecipeTrackerTests` | EditMode | Suivre, ne plus suivre, ordre, retirée après fabrication | ✅ |
| `ObjectiveRulesTests` | EditMode | Ordre des objectifs ; un texte par objectif | ✅ |
| `ObjectiveRulesTests.OnceAwake_TheChampimaisonIsToBeGrown_WithoutAnObjectiveForTheNet` | EditMode | Germée, l'objectif est d'améliorer la Champimaison, sans étape pour le filet (remplace `OnceAwake_TheNetIsToBeCrafted_ThenTheChampimaisonGrown`) | ✅ |
| `ThreadProductionTests.TheNextThread_IsMeasured_AsProgressAndSecondsLeft` | EditMode | Progression et secondes arrondies au-dessus ; rien avant le départ ni avec un stock plein | ✅ |
| `ChampimaisonPlayModeTests.OnceAwake_ThePanel_ShowsTheThreadComing_WithItsKeyGreyedUntilOneWaits` | PlayMode | Pas de ligne avant le réveil ; après, touche grisée, « 0/3 », barre qui avance, « Prochain dans » ; stock plein : touche allumée, « 3/3 », plus de barre, « Plein » | ✅ |
| `WildSoakTests` | EditMode | Source la plus proche ; sources sèches ou d'un autre objet ignorées ; imbibition ; source qui sèche en route ; interruption | ✅ |
| `CreatureHealthTests` | EditMode | Chute à zéro, une seule fois | ✅ |
| `ChampimaisonPlayModeTests` | PlayMode | Graine à l'arbre géant, avec sa lueur violette ; plantation (sac, main) ; panneau et dépôt ; fil ; entrée et fin de démo | ✅ |
| `CraftingPanelPlayModeTests` | PlayMode | Ouverture sans recette ouverte, E et R grisés ; clic puis deuxième clic ; ingrédient manquant et provenance, E grisé à sa place ; recette suivie en haut à droite jusqu'à sa fabrication | ✅ |
| `GameScenePlayModeTests.TheInventory_HidesTheVitalsAndTheObjective_AndKeepsItsHintsUnderItsPanels` | PlayMode | Jauges et objectif cachés pendant l'inventaire, revenus à sa fermeture ; ses aides sous ses panneaux | ✅ |
| `UiTextSizeTests` | EditMode | Chaque texte des prefabs d'interface a l'une des quatre tailles | ✅ |
| `SlingshotPlayModeTests` | PlayMode | Champignons dans la brume ; un Champonge boit à une fougère pleine ; un caillou par tir, seulement en main ; eau lâchée seulement par un Champonge mouillé | ✅ |
| `SlingshotPlayModeTests.TheFirstFernsMet_AreFull_EachWithAChamponge_AndNoChampongeLivesAwayFromTheFerns` | PlayMode | Les 3 premières fougères croisées sont pleines, un Champonge chacune ; des Champonge sur une partie des fougères seulement ; aucun loin d'une fougère | ✅ |
| `SlingshotPlayModeTests.AWildChamponge_DrinksOnlyNearThePlayer_AndLookedAt` | PlayMode | Joueur loin : 6 s sans boire ; joueur à 8 m, dos tourné : 5 s sans boire ; joueur à 8 m qui le regarde : il boit, la fougère sèche | ✅ |
| `KeyPromptPlayModeTests.AText_IsReadOnItsPanel_WhichGrowsAroundIt` | PlayMode | Le texte du petit panneau est actif et lu, le panneau grandit autour (échouait avant la correction : texte caché) | ✅ |
| `SeedFallIntroPlayModeTests` | PlayMode | Au chargement : intro en cours, ni commandes ni objectif, caméra devant la graine qui regarde le feuillage ; puis la graine tombe, la caméra la suit, la vue se resserre ; à la fin, graine au sol, vue et commandes rendues au joueur, interface revenue. Un premier appui affiche l'indice sans passer, un second passe l'intro, graine posée ; l'indice part après 3 s, un appui plus tard le réaffiche sans passer | ✅ |
| `SlingshotAimPlayModeTests` | PlayMode | Vrai clic droit simulé : le lance-pierre vise et la vue se resserre, encore après l'inventaire (échouait avant la correction : la vue ne se resserrait pas) | ✅ |
| `GameScenePlayModeTests.SticksMushroomsAndPebbles_AreAroundThePlayer` | PlayMode | 3 bâtons, 3 champignons, 4 cailloux à moins de 50 m | ✅ |
| `ItemIconTests.TheDroppedIconMaterial_CutsOutTheTransparentCorners` | EditMode | Le matériau des objets au sol est un asset qui découpe la transparence | ✅ |
| `ControlMapsTests.Crouching_IsHeldOnCtrl_NotOnC` | EditMode | Ctrl accroupit, C non | ✅ |
| `ItemTextsTests` | EditMode | Chaque objet a une description dans la table, et sa provenance quand il en a une | ✅ |
| `ChampongePopulationPlayModeTests` | PlayMode | 1 à 6 Champonge présents, tous près du joueur ; un Champonge part quand le joueur est à 90 m, sans bouger, et revient à 20 m | ✅ |
| `ChampimaisonPlayModeTests` (fin de démo, fil) | PlayMode | Planche « Quitter le jeu » ; ligne du fil à part, qui pulse, absente du panneau, partie une fois le fil récupéré | ✅ |
| `ChampimaisonPlayModeTests.OncePlanted_TheObjective_ListsWhatTheSeedNeeds_WithWhereToFindIt` | PlayMode | Besoins du palier sous l'objectif, avec provenance ; porté compte ; palier suivant | ✅ |
| `KeyPromptPlayModeTests` (2 tests ajoutés) | PlayMode | Textes à droite de la touche, vidés en cachant ; pulsation | ✅ |
| `CreatureNameTagPlayModeTests` | PlayMode | Nom visé à 12 m, fondu entrant, caché en se retournant, fondu sortant ; rien à 25 m | ✅ |
| `ChampongeSqueezePlayModeTests` | PlayMode | Accroupi derrière : non remarqué, « Presser », 1 Eau, il fuit sec ; debout ou de face : rien | ✅ |
| `ChampongeSqueezePlayModeTests.LookingAtAFullChamponge_FromSevenMetres_ShowsTheCrouchKey_UntilTheFirstSqueeze` | PlayMode | Ctrl et « S’approcher discrètement » en regardant un Champonge plein à 7 m ; caché en détournant le regard, en s'accroupissant ; fini après le premier pressage | ✅ |
| `CompassPlayModeTests` | PlayMode | « ! » au milieu face à la graine, au bord dos à elle ; cachée avec l'inventaire ; après la plantation, une fougère pleine puis la Champimaison | ✅ |
| `CraftingPanelPlayModeTests` (fabrication en 1 s) | PlayMode | L'objet arrive après la barre ; un clic pendant ne relance pas ; autre recette ou inventaire fermé : rien de pris | ✅ |
| `SeedFallIntroPlayModeTests.TheShot_DrawsBackFromTheSeed_ToThePlayer_BeforeGivingTheControls` | PlayMode | Le recul jusqu'au joueur dure plusieurs images ; à la fin, la graine est devant lui | ✅ |
| `KeyPromptPlayModeTests` (4 tests ajoutés) | PlayMode | Triangle sans bord pour un appui ; bord vide puis rempli, sans triangle, pour un maintien ; les deux ensemble ; texte à la place de la touche | ✅ |
| `ChampimaisonPlayModeTests.Aimed_TheChampimaison_ShowsItsKeysInItsPanel_NotOverIt` | PlayMode | Pas de touche flottante sur la Champimaison visée ; « Déposer » au pied du panneau ; pas de touche d'appui court inutile ; pied caché hors de portée | ✅ |
| `GatheringTests.Targeting_ASmallStickJustBesideTheCrosshair_IsTargeted_WithTheMargin` | EditMode | Un bâton de 10 cm à 25 cm de la croix : manqué sans marge, visé avec 30 cm | ✅ |
| `ChampimaisonPlayModeTests.ThePanel_ShowsOnlyWhereItsKeysWork_AndReadingItAimsAtTheChampimaison` | PlayMode | À 4,5 m, le panneau s'affiche, le regarder vise la Champimaison, « Déposer » au pied ; à 6,5 m, ni panneau ni visée | ✅ |
| `ChampimaisonPlayModeTests.OnceGrownOnce_TheObjective_SuggestsCatchingAChampongeWithTheNet_UntilOneIsCaught` | PlayMode | Pas de ligne avant la germination ; après, en premier, texte et indice pour l'eau, sans compte ; partie à la capture, et pas revenue quand le Champonge quitte l'inventaire | ✅ |
| `CreatureNameTagPlayModeTests.LookingTwentyDegreesBesideAChamponge_StillShowsItsName_WithoutADropWhileDry` | PlayMode | Nom montré à 20° du centre de la vue ; pas de goutte sur un Champonge sec | ✅ |
| `ChampongeSqueezePlayModeTests.AFullChamponge_HasADropBesideItsName` | PlayMode | Un Champonge plein a la goutte à côté de son nom | ✅ |
| `DemoEndPlayModeTests.Escape_AtTheEndOfTheDemo_GoesBackToTheHomeScreen` | PlayMode | Échap à la fin de la démo charge l'écran d'accueil | ✅ |

**Tests existants dont l'exigence change :** `GameScenePlayModeTests.AnItemDroppedFromTheInventory_LiesInFront_ThenCanBePickedUpAgain` vérifie l'icône du filet au sol, plus son carré de couleur ; le test du Champonge qui boit tourne la vue vers lui (il se promène autour de sa fougère) ; chaque chargement de la partie dans les tests passe l'intro (`game.Intro.Skip()`) ; dans `SlingshotPlayModeTests`, le Champonge qu'on fait boire a le joueur à 8 m (il ne boit que sous ses yeux), et `AFelledWetChamponge_LetsGoOfWater_ADryOneOfNothing` vide toutes les fougères avant d'abattre le Champonge sec ; `FungalNetworkTests` (réécrit) ; dans `GameScenePlayModeTests`, le test de départ vérifie les mains vides et l'absence de Champimaison, `TheNet_StartsInTheFirstSlotOfTheQuickBar_InHand` devient `ThePlayer_StartsEmptyHanded_TheNetPutInTheQuickBarIsInHand`, les tests du filet reçoivent le filet (`LoadGameWithNet`), le test de l'objet jeté cherche le filet parmi les objets au sol (la graine y est aussi) ; `ItemNamesPlayModeTests` met le filet dans la case ; les tests de reset (`Reset_*`, `CloseAll_*`, `Clear_*`) sont retirés avec le reset ; le test de capture calme sa cible et reprend le filet lui-même ; `ChampongeCompanionPlayModeTests.Aimed_TheChamponge_ShowsTheKey_WithoutRingUntilHeld_AndTheHoldHint` devient `Aimed_AnEmptyChamponge_ShowsTheKeyWithItsEmptyBorder_AndTheHoldHint` (le bord est visible avant l'appui, sans triangle) ; `GameScenePlayModeTests.TheKey_ShowsEAboveTheTarget_AndFollowsIt` vérifie en plus le triangle. Depuis le playtest du 23/11 : `ChampimaisonPlayModeTests.Aimed_TheChampimaison_ShowsItsKeysInItsPanel_NotOverIt` ne vérifie plus le panneau à 7 m (il n'y est plus montré : c'est le nouveau test du panneau qui couvre la distance), et le test du fil se place à 4 m au lieu de 5 m (5 m est la limite exacte de la portée). `ChampimaisonPlayModeTests.ThePanel_ListsTheCost_AndHoldingTheKey_DropsWhatIsMissing` attend « Améliore la Champimaison » après la germination, plus « Fabrique un filet ». `AnAwakeChampimaison_ProducesThread_ThatAShortPressCollects` vérifie la touche de la ligne de production, allumée puis grisée après la récupération, au lieu de la ligne qui pulsait. Dans `ChampongeSqueezePlayModeTests`, le test du nom et celui de l'indice « S’approcher discrètement » se placent là où rien ne cache la créature (`StandInSight`) : à 7 m derrière elle, une bosse du terrain la cachait parfois, et le jeu n'affichait rien, comme il doit (une fois sur deux pour le nom, vu par un diagnostic : la ligne de vue touchait le terrain).

### Manuels (dans Unity)
| # | Étapes | Résultat attendu | OK |
|---|---|---|---|
| 1 | `Game.unity`, Play, sans toucher à rien | L'intro : la graine tombe du feuillage de l'arbre géant, la caméra la suit et zoome, puis la vue revient au départ. Ensuite : mains vides, barre rapide vide. Objectif « Découvre le secret du grand arbre » en haut à droite, sur une plaque sombre | ⬜ |
| 2 | Aller à l'arbre géant | La graine, contourée, au pied de l'arbre, sous une lueur violet clair qui respire. E la ramasse, la lueur part avec elle | ⬜ |
| 3 | Viser un trou de taupe, graine dans le sac | « Prends la graine en main » sur un petit panneau en bois | ⬜ |
| 4 | Graine en main, viser le trou, puis E | Touche E avec le triangle sur son bord haut. Puis la Champimaison apparaît, panneau « Faire germer la graine 1/2 », « Eau 0/1 » et « Spores 0/3 » en corail. Les autres trous n'ont plus de contour | ⬜ |
| 5 | Cueillir 3 champignons, viser la Champimaison, maintenir E | Aucune touche ne flotte ; au pied du panneau, E avec son bord, « Maintenir Déposer ». Le bord se remplit depuis le coin haut gauche. Spores 3/3 en vert avec sa coche, Eau 0/1 reste, et le pied disparaît | ⬜ |
| 6 | Ramasser des bâtons et des cailloux, Tab, en 1920×1080 puis en 1280×720 | Panneau « Fabrication » à droite, aucune recette ouverte, E et R grisés. Ni jauges ni objectif ; les aides sur une ligne sous les panneaux, rien ne se recouvre | ⬜ |
| 7 | Cliquer « Filet », R | Détails en bas, « Fil de mycélium 0/3 » en rouge avec sa provenance. Le filet apparaît en haut à droite | ⬜ |
| 8 | Cliquer deux fois « Lance-pierre » | Lance-pierre dans la barre rapide, « +1 Lance-pierre », bâtons et caillou retirés. Compteurs du filet suivi à jour | ⬜ |
| 9 | Depuis l'arbre géant, entrer dans la brume et s'approcher de la première fougère à eau, en s'arrêtant à 5–10 m dos à elle, puis se retourner | Elle est pleine, un Champonge à côté. Dos tourné, rien. Face à lui, il va y boire, son chapeau fonce peu à peu, la fougère sèche. Visée de près, la fougère affiche « Nécessite un Champonge » en entier sur son panneau | ⬜ |
| 10 | Tirer sur un Champonge sec jusqu'à l'abattre | Il disparaît, rien au sol | ⬜ |
| 11 | Tirer sur un Champonge mouillé jusqu'à l'abattre | 1 Eau au sol, E la ramasse | ⬜ |
| 12 | Maintenir E sur la Champimaison | Texte « Faire germer ». Le réseau pousse, objectif « Améliore la Champimaison », avec la ligne optionnelle du filet | ⬜ |
| 13 | Attendre, appui court sur E | Fil de mycélium récupéré | ⬜ |
| 14 | Fabriquer le filet, capturer un Champonge, le presser | Eau dans l'inventaire | ⬜ |
| 15 | Payer le palier 2, appui court sur E | Intérieur, puis « Fin de la démo », planche Menu principal | ⬜ |
| 16 | Tout le parcours à la manette | Mêmes résultats ; A fabrique, Y suit | ⬜ |
| 17 | R en jeu | Rien ne se passe. Échap, Menu principal, Jouer : nouvelle partie, mains vides, graine au pied de l'arbre | ⬜ |
| 18 | Exécutable Windows, partie complète par quelqu'un qui ne connaît pas le jeu | Fin atteinte en 20 min au plus | ⬜ |
| 19 | Viser un bâton, puis un Champonge vide sorti, puis un Champonge plein | Bâton : triangle, pas de bord. Champonge vide : bord vide, pas de triangle. Plein : les deux. Maintenir E : le bord se remplit | ⬜ |
| 20 | Comparer les touches en jeu au canevas « Panneaux en bois » | Bois clair sans liseré, triangle à cheval sur le bord haut, bord collé à la touche ; touches des aides en bas de l'écran de la même couleur | ⬜ |
| 21 | Depuis le départ, regarder autour, puis suivre le chemin vers l'arbre géant | Un trou de taupe contouré dans une petite clairière à droite du départ, un autre à gauche du chemin à mi-route, un derrière la clairière de l'arbre ; aucun sur un chemin | ⬜ |
| 22 | Lance-pierre en main avec des cailloux : maintenir clic droit ; Tab, Tab ; maintenir clic droit | La vue se resserre et l'arc s'affiche les deux fois ; clic gauche tire pendant la visée | ⬜ |
| 23 | Jouer, appuyer une fois sur une touche pendant que la graine tombe, puis une deuxième fois ; recommencer avec un clic et à la manette | Premier appui : « Appuie encore pour passer » en bas à droite, l'intro continue. Deuxième appui : l'intro s'arrête, la vue revient au joueur, la graine est au sol au pied de l'arbre | ⬜ |
| 24 | Ramasser bâtons, cailloux, champignons, la graine ; Tab ; fabriquer le lance-pierre ; jeter un objet | Chaque case montre l'icône de son objet, la barre rapide aussi, et la fabrication. L'objet jeté au sol montre son icône sans carré noir autour | ⬜ |
| 25 | Dans l'exécutable : la graine à l'arbre géant, un objet jeté | Icône sans fond noir | ⬜ |
| 26 | Aller à la fin de la démo, cliquer « Quitter le jeu » | Le jeu se ferme | ⬜ |
| 27 | Viser un Champonge de 12 m, puis s'en approcher ; le regarder boire | Son nom en petit, au-dessus de sa tête sans le cacher, en fondu ; il passe du jaune clair au jaune foncé ; plein, des gouttes tombent | ⬜ |
| 28 | Maintenir Ctrl, contourner un Champonge mouillé par derrière, E | Il ne fuit pas ; « Presser » à droite de E ; 1 Eau, il fuit. Debout ou de face : il fuit avant | ⬜ |
| 29 | Tourner sur soi pendant chaque objectif | « ! » jaune vers la graine, puis un trou de taupe, une fougère pleine, la Champimaison ; collé au bord quand l'objectif est derrière | ⬜ |
| 30 | Planter la graine | Sous l'objectif : Eau 0/1 et Champignon à spores 0/3, chacun avec où le trouver, sur une ligne | ⬜ |
| 31 | Attendre 20 s près de la Champimaison germée | Le fil se récupère depuis sa ligne dans le panneau (test 44) ; aucune ligne ne pulse sous le panneau | ⬜ |
| 32 | Tab, survoler chaque objet ; fabriquer le lance-pierre | Nom, description, provenance ; la barre se remplit en 1 s avant que l'objet arrive | ⬜ |
| 33 | Ramasser quelque chose | « +1 … » à gauche de la barre rapide | ⬜ |
| 34 | Jouer jusqu'à la fin de l'intro | La vue recule de la graine jusqu'au joueur, qui la voit devant lui | ⬜ |
| 35 | Entrer dans la brume, regarder une fougère et son Champonge | La fougère ne cache plus le Champonge ; ses gouttes tombent de ses feuilles quand elle est pleine | ⬜ |
| 36 | Nouvelle partie : regarder debout un Champonge plein (qui goutte) à 6–8 m ; s'accroupir ; le presser ; en regarder un autre plein | Ctrl, sur une touche assez large, avec « Maintenir · S’approcher discrètement » à droite du Champonge ; disparaît accroupi ; après le premier pressage, plus jamais | ⬜ |
| 37 | S'approcher de la Champimaison en la regardant, depuis 10 m | Le panneau apparaît vers 5 m ; dès qu'il est là, regarder le panneau montre les touches à son pied, et maintenir E dépose | ⬜ |
| 38 | Ramasser des bâtons et des champignons sans viser pile dessus, à 2–3 m | Le contour et E apparaissent ; E ramasse | ⬜ |
| 39 | Améliorer la Champimaison une fois | En haut de la liste sous l'objectif : « (optionnel) Capture un Champonge avec le filet », icône du filet, « Pour l’eau : pose-le, il boit aux fougères » ; disparaît à la capture | ⬜ |
| 40 | Regarder vers un Champonge sans le viser, à 10 m ; un Champonge plein | Son nom reste affiché tant qu'on regarde vers lui ; une goutte à gauche du nom quand il goutte | ⬜ |
| 41 | À la fin de la démo, Échap | Retour à l'écran d'accueil | ⬜ |
| 42 | Améliorer la Champimaison une fois, en 1920×1080 puis en 1280×720 | Sous l'objectif, icônes lisibles ; la ligne optionnelle sur deux lignes ne chevauche pas son indice ; chaque provenance est sous son nom, décalée, plus terne que lui | ⬜ |
| 43 | Jouer : intro passée en deux appuis, ramasser des objets, changer d'objet en main, Échap, Tab, finir la démo | « Appuie encore pour passer », les objets ramassés et le nom de l'objet en main sur de petites étiquettes en bois ; l'objectif et la boussole en bois, « Objectif » non cyan ; le nom du Champonge sans contour, lisible dans la brume ; voiles bruns derrière la pause, l'inventaire et la fin ; barre de fabrication brune et crème | ⬜ |
| 44 | Réveiller la graine, rester devant la Champimaison 20 s, appuyer sur E, attendre 60 s | E grisé, la barre se remplit, « Prochain dans … » décompte ; au premier fil, E s'allume avec son triangle et « 1/3 » ; E récupère le fil, E se grise ; à 3/3, plus de barre et « Plein : récupère-le pour relancer » | ⬜ |
| 45 | Jouer : regarder la créature des fougères, la capturer, ouvrir l'inventaire | Son nom est « Champonge » au-dessus d'elle, dans l'inventaire, dans l'objectif et sur la fougère (« Nécessite un Champonge ») ; plus aucun « Spongeus » à l'écran | ⬜ |
| 46 | Suivre une recette (R dans l'inventaire), fermer l'inventaire ; la fabriquer | En haut à droite, sous la quête : une ligne pointillée, « Fabrication », puis la recette ; une fois fabriquée, ligne et titre disparaissent | ⬜ |

## Limitations connues

- **Compilé et testé en batchmode le 22/11/2026, pas encore joué** : aucune partie n'a été faite, aucun test manuel n'a été vérifié.
- **Mise en page des prefabs d'interface** écrite par code, jamais vue : tailles et placements à régler dans l'Inspector.
- **Au-dessus d'un trou de taupe, la touche flotte seule** : « Planter » est dans les aides en bas à droite, pas à côté de la touche comme sur le canevas. Les blocs Production et Débloque du canevas ne sont pas encore dans le panneau de la Champimaison.
- **La graine posée à l'arbre géant** est dessinée comme un objet jeté (son icône), en attendant son modèle.
- **Les icônes des objets sont provisoires** : des SVG simples tracés en code (`tools/placeholder-icons/`, rendus en PNG 256 px par `render.sh`, ImageMagick), à remplacer par des icônes faites par un humain (Charte IA).
- **Nombre de Champonge sauvages** : environ une fougère sur six (`shareOfSources`, 0,17), 6 présents au plus (`maxShown`), à régler en jouant.
- **Retours du 22/11 (2) testés en batchmode, pas encore joués** : écrits sans lancer Unity après un plantage du PC pendant les tests, puis lancés au go de Hugo, un Unity à la fois : les prefabs de la boussole et de la plaquette des objets par un script à usage unique (`TmpFeedback4`, supprimé), tous les EditMode (387/387) et les fixtures PlayMode de la boucle (113/113). Les autres fixtures PlayMode (forêt, vent, sons, saut, bancs d'essai) n'ont pas été relancées. La mise en page de la boussole, des textes à droite des touches et de la plaquette n'a pas été vue à l'écran.
- **Retours du 23/11 testés en batchmode le 23/11, pas encore joués** : EditMode `GatheringTests`, `UiTextSizeTests`, `ItemTextsTests`, `ControlMapsTests` (29/29) ; PlayMode, en un seul Unity, les fixtures Champimaison, nom, pressage, fin de démo, scène de jeu, boucle, noms d'objets, compagnon, touches, boussole et lance-pierre (83/84 au dernier passage). Deux tests de `GameScenePlayModeTests` échouent parfois quand d'autres fixtures tournent avant, et passent seuls ou dans leur fixture : `TheKey_ShowsEAboveTheTarget_AndFollowsIt` (touche à 350 px de la cible, 2 fois sur 6 passages, pas revu depuis la correction de la touche qui suit sa cible) et `OnlyTheMushroomAimedAt_IsOutlined_InTheColourOfItsChance` (couleur de la chance du filet, 1 fois). Cause non trouvée ; le code du filet n'a pas changé. Les autres EditMode et PlayMode n'ont pas été relancés. Ces retours ont été écrits sans l'Editor : la goutte du nom a été ajoutée au prefab à la main (YAML), pas dans l'Editor ; sa place, à gauche du nom selon la largeur du texte, n'a pas été vue à l'écran. La ligne optionnelle de l'objectif, plus longue que les autres, passe sur deux lignes si elle ne tient pas.
- **Couleur éponge du Champonge** : issue d'un retour de playtest, à valider en jeu ; le modèle fait par un humain n'est pas touché, seule la couleur de son matériau change.
- **Dans la brume dense, 15 m peut être trop loin pour bien voir le Champonge boire** : à vérifier en jouant. Le regard est mesuré au centre du Champonge, pas à ce qui le cache (un tronc entre les deux ne l'empêche pas de boire).
- **Le texte des petits panneaux en bois était caché depuis leur ajout** (« Nécessite un Champonge », « Nécessite une graine », « Prends la graine en main ») : l'objet `Text` du prefab `KeyPrompt` était désactivé, le panneau ne montrait que ses marges. Corrigé ; vérifié par la taille du panneau (227 px pour « Nécessite un Champonge »), pas encore à l'écran.
- **Signaler l'arbre géant et la graine**, le nombre et la place des trous de taupe : à régler pendant le dev.
- **`scripts/check-art-assets.sh` refuse les prefabs de `Assets/Prefabs/UI/`** (domaine et préfixe `PF_`), y compris ceux qui existaient déjà : les nouveaux prefabs d'interface suivent la convention existante du dossier `UI`.
- Placeholders pour tous les nouveaux modèles et icônes.

## Modifié par
