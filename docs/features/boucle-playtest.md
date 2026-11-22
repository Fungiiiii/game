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

**Graine et trous de taupe**

- [ ] La graine est posée au pied de l'arbre géant, avec le contour de ce qui se ramasse. E la ramasse.
- [ ] De petits trous de terre sont placés dans la forêt (au départ : sur les 4 emplacements de Champimaison). Placeholder : des cubes de terre en rond, posés dans le sol, autour d'un petit trou de quelques centimètres qui s'enfonce. Tant qu'aucune graine n'est plantée, ils portent un contour crème fin.
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

- [ ] L'objectif en cours : « Trouve une graine », « Plante la graine », « Réveille la graine », « Fabrique un filet », « Améliore la Champimaison », « Entre dans la Champimaison ».
- [ ] Sous l'objectif, les recettes traquées, avec leurs ingrédients mis à jour pendant le jeu. Une recette fabriquée n'est plus traquée.

**Spongeus sauvages**

- [ ] Plus aucun champignon dans la clairière du spawn. Les Spongeus vivent dans la brume.
- [ ] Calme et sec, un Spongeus va boire à une fougère mouillée proche, s'imbibe, et prend l'aspect mouillé (chapeau sombre et brillant). La fougère sèche.

**Lance-pierre**

- [ ] Lance-pierre en main : clic droit maintenu (gâchette gauche) pour viser, avec l'arc et le zoom du filet ; clic gauche (gâchette droite) pour tirer un caillou. Chaque tir consomme un caillou ; sans caillou, rien ne part.
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
- Toute modification de la scène `MysticForest` et de ses données de Terrain.

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
| Spongeus sauvages | `Runtime/Capture/WildSoak.cs`, `Runtime/Companions/WetLook.cs` (nouveaux) ; `WildMushroom.cs` (imbibition, `Hit`, `Felled`), `CreatureDefinition.cs` (santé, butin, imbibition), `CreaturePopulation.cs`, `Companion.cs` (utilise `WetLook`) ; `Assets/Data/Creatures/Spongeus.asset` ; `Game.unity` (6 Spongeus dans la brume, Champoule retirée) |
| Lance-pierre | `Runtime/Combat/CreatureHealth.cs`, `Slingshot.cs`, `SlingshotSettings.cs` ; `Assets/Data/Settings/SlingshotSettings.asset` |
| Fin de démo | `Runtime/UI/DemoEnd.cs` ; `GameBootstrap` charge `ChampimaisonInterior.unity` ; `Editor/Playtest/PlaytestSetup.cs` (scène ajoutée au build) |
| Reset retiré | action `ResetGame` retirée de `InputSystem_Actions.inputactions` et `GameControls.cs` ; `GameBootstrap.ResetGame` et les méthodes de reset sans autre appelant retirées |
| Entrées | actions `UI/Craft` (E) et `UI/Track` (R, Y) |
| Textes | 41 entrées dans la table `UI` (fr/en) |
| Panneaux en bois et touches | `Runtime/UI/TriangleMarkGraphic.cs`, `DashedLineGraphic.cs`, `TickGraphic.cs`, `ActionLine.cs`, `Runtime/Interaction/IPanelledInteractable.cs` (nouveaux) ; `KeyCapGraphic.cs` (sans liseré, bois clair) ; `HoldRingGraphic.cs` renommé `HoldBorderGraphic.cs` (même GUID : bord en octogone autour de la touche) ; `KeyPrompt.cs` (triangle, bord, texte sur panneau, `SetMarks`) ; `PlayerInteractor.cs` (bord dès le ciblage, pas de touche sur un `IPanelledInteractable`, `HoldProgress` et `InteractGlyph` publics) ; `RequirementPanel.cs`, `RequirementRow.cs`, `IngredientRow.cs` (couleurs d'état, coche) ; `FungalHole.cs` (`IPanelledInteractable`, panneau plus bas) ; prefabs `Hints/HintKey`, `HintFace`, `HintPill`, `HintView` (face bois clair) ; entrée `panel.hold` |
| Mise en place | `Editor/Playtest/PlaytestLoopSetup.cs`, à usage unique |

### Mise en place (à faire une fois, dans l'Editor)

1. Ouvrir le projet, attendre la compilation, lire la Console.
2. Menu **Fungiiiii > Playtest > One-off: Set Up the Playtest Loop**. Il écrit :
   - `Assets/Prefabs/Environment/PF_MoleHole.prefab` et `Assets/Prefabs/Props/PF_Pebble.prefab` (placeholders), leurs matériaux `Assets/Art/Materials/Placeholder*.mat` ;
   - `KeyPrompt.prefab` au style des panneaux en bois : `PressMark` (triangle) sur la touche, `HoldRing` renommé `HoldBorder` et ramené autour de la touche, le texte dans un `TextPanel` en bois avec sa pointe ;
   - les prefabs d'interface `RequirementPanel`, `RequirementRow`, `RecipeRow`, `IngredientRow`, `TrackedIngredientRow`, `TrackedRecipe`, `ObjectiveHud`, `DemoEnd` dans `Assets/Prefabs/UI/` ;
   - le panneau « Fabrication » dans `Inventory.prefab` (l'inventaire se décale à gauche) ;
   - le composant `Slingshot` sur `PlaytestPlayer.prefab` ;
   - dans `Game.unity` : le modèle des trous de taupe, le panneau et l'objectif ;
   - la scène `Assets/Scenes/Game/ChampimaisonInterior.unity` et les scènes du build.
3. Relire le diff (prefabs, scène, `EditorBuildSettings.asset`), régler la mise en page dans l'Inspector si besoin, commiter, puis **supprimer `PlaytestLoopSetup.cs`**.

## Décisions

Validées par Hugo le 22/11 :

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
- **La scène `MysticForest` n'est pas modifiée** : les trous de taupe se placent sur ses `ChampimaisonSite`, lus au lancement.
- **L'objectif est calculé à partir de l'état du jeu** (`ObjectiveRules` : graine, plantation, palier, filet), pas de l'ordre des événements : perdre la graine avant de la planter ramène à « Trouve une graine ».
- **Les recettes traquées** sont un `RecipeTracker` en C# pur tenu par `GameBootstrap`, partagé par le panneau de fabrication et l'objectif.
- **Le lance-pierre est un composant de la variante `PlaytestPlayer`**, comme le filet sur le joueur : seul le jeu en a besoin.
- **Ce que lâche un champignon abattu** passe par un rappel donné à `CreaturePopulation` : la capture ne dépend pas des objets au sol.
- **Le caillou ignore le collider des champignons** quand il teste le décor : un champignon est touché par sa distance au trajet.
- **Les textes « 1/3 » sont deux nombres** : ils ne passent pas par la table (marqués `i18n-exempt`). « 2 dans le sac » est une entrée avec `{0}`.
- **Prefabs, câblage de `Game.unity` et données créés par un script d'Editor à usage unique** (CLAUDE.md, « Scenes, prefabs and data ») : la branche a été écrite sans Editor. Le script est lancé une fois, son résultat relu et commité, puis il est supprimé.

## Tests

### Automatisés (Unity Test Runner)

Écrits sans Editor : **ni compilés ni lancés**. Une vérification de syntaxe (analyseur C#) passe sur tous les fichiers modifiés.

| Test | Mode | Couvre | Résultat |
|---|---|---|---|
| `RequirementProgressTests` | EditMode | Jamais plus que ce qui manque ; dépôt partiel gardé ; ligne complète intouchée ; rien de ce qui n'est pas demandé ; `Changed` ; `WouldComplete` | ⬜ |
| `FungalNetworkTests` (réécrit) | EditMode | Graine endormie au départ ; dépôt partiel sans pousse ; le dernier dépôt fait pousser et le reste passe au palier suivant ; entrée au dernier palier | ⬜ |
| `ThreadProductionTests` | EditMode | Rien avant le démarrage ; un fil par période ; stock plein en attente ; ce qui ne rentre pas reste | ⬜ |
| `PlantingRulesTests` | EditMode | Texte ou touche selon la graine | ⬜ |
| `CrafterTests` | EditMode | Tout ou rien ; ingrédients rendus si le résultat ne rentre pas ; barre rapide d'abord, affiché comme ramassé | ⬜ |
| `RecipeTrackerTests` | EditMode | Traquer, ne plus traquer, ordre, retirée après fabrication | ⬜ |
| `ObjectiveRulesTests` | EditMode | Ordre des objectifs ; un texte par objectif | ⬜ |
| `WildSoakTests` | EditMode | Source la plus proche ; sources sèches ou d'un autre objet ignorées ; imbibition ; source qui sèche en route ; interruption | ⬜ |
| `CreatureHealthTests` | EditMode | Chute à zéro, une seule fois | ⬜ |
| `ChampimaisonPlayModeTests` | PlayMode | Graine à l'arbre géant ; plantation (sac, main) ; panneau et dépôt ; fil ; entrée et fin de démo | ⬜ |
| `CraftingPanelPlayModeTests` | PlayMode | Ouverture sans recette ouverte ; clic puis deuxième clic ; ingrédient manquant et provenance ; recette traquée en haut à droite jusqu'à sa fabrication | ⬜ |
| `SlingshotPlayModeTests` | PlayMode | Champignons dans la brume ; un Spongeus boit à une fougère pleine ; un caillou par tir, seulement en main ; eau lâchée seulement par un Spongeus mouillé | ⬜ |
| `GameScenePlayModeTests.SticksMushroomsAndPebbles_AreAroundThePlayer` | PlayMode | 4 cailloux à moins de 50 m, comme les bâtons | ⬜ |
| `KeyPromptPlayModeTests` (4 tests ajoutés) | PlayMode | Triangle sans bord pour un appui ; bord vide puis rempli, sans triangle, pour un maintien ; les deux ensemble ; texte à la place de la touche | ⬜ |
| `ChampimaisonPlayModeTests.Aimed_TheChampimaison_ShowsItsKeysInItsPanel_NotOverIt` | PlayMode | Pas de touche flottante sur la Champimaison visée ; « Déposer » au pied du panneau ; pas de touche d'appui court inutile ; pied caché hors de portée | ⬜ |

**Tests existants dont l'exigence change :** `FungalNetworkTests` (réécrit) ; dans `GameScenePlayModeTests`, le test de départ vérifie les mains vides et l'absence de Champimaison, `TheNet_StartsInTheFirstSlotOfTheQuickBar_InHand` devient `ThePlayer_StartsEmptyHanded_TheNetPutInTheQuickBarIsInHand`, les tests du filet reçoivent le filet (`LoadGameWithNet`), le test de l'objet jeté cherche le filet parmi les objets au sol (la graine y est aussi) ; `ItemNamesPlayModeTests` met le filet dans la case ; les tests de reset (`Reset_*`, `CloseAll_*`, `Clear_*`) sont retirés avec le reset ; le test de capture calme sa cible et reprend le filet lui-même ; `SpongeusCompanionPlayModeTests.Aimed_TheSpongeus_ShowsTheKey_WithoutRingUntilHeld_AndTheHoldHint` devient `Aimed_AnEmptySpongeus_ShowsTheKeyWithItsEmptyBorder_AndTheHoldHint` (le bord est visible avant l'appui, sans triangle) ; `GameScenePlayModeTests.TheKey_ShowsEAboveTheTarget_AndFollowsIt` vérifie en plus le triangle.

### Manuels (dans Unity)
| # | Étapes | Résultat attendu | OK |
|---|---|---|---|
| 1 | `Game.unity`, Play | Mains vides, barre rapide vide. Objectif « Trouve une graine » en haut à droite | ⬜ |
| 2 | Aller à l'arbre géant | La graine, contourée, au pied de l'arbre. E la ramasse | ⬜ |
| 3 | Viser un trou de taupe, graine dans le sac | « Prends la graine en main » sur un petit panneau en bois | ⬜ |
| 4 | Graine en main, viser le trou, puis E | Touche E avec le triangle sur son bord haut. Puis la Champimaison apparaît, panneau « Réveiller la graine 1/2 », « Eau 0/1 » et « Spores 0/3 » en corail. Les autres trous n'ont plus de contour | ⬜ |
| 5 | Cueillir 3 champignons, viser la Champimaison, maintenir E | Aucune touche ne flotte ; au pied du panneau, E avec son bord, « Maintenir Déposer ». Le bord se remplit depuis le coin haut gauche. Spores 3/3 en vert avec sa coche, Eau 0/1 reste, et le pied disparaît | ⬜ |
| 6 | Ramasser des bâtons et des cailloux, Tab | Panneau « Fabrication » à droite, aucune recette ouverte | ⬜ |
| 7 | Cliquer « Filet », R | Détails en bas, « Fil de mycélium 0/3 » en rouge avec sa provenance. Le filet apparaît en haut à droite | ⬜ |
| 8 | Cliquer deux fois « Lance-pierre » | Lance-pierre dans la barre rapide, « +1 Lance-pierre », bâtons et caillou retirés. Compteurs du filet traqué à jour | ⬜ |
| 9 | Dans la brume, observer un Spongeus | Il va boire à une fougère mouillée, son chapeau devient sombre et brillant | ⬜ |
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

## Limitations connues

- **Rien n'a été compilé ni lancé** : la branche a été écrite dans un conteneur sans Unity. Compilation, tests et partie restent à faire sur la machine de Hugo.
- **Le jeu ne démarre pas avant le script de mise en place** : `GameBootstrap` signale dans la Console ce qui manque (modèles, prefabs, `Slingshot` sur le joueur).
- **Mise en page des prefabs d'interface** écrite par code, jamais vue : tailles et placements à régler dans l'Inspector.
- **E Fabriquer est absent plutôt que grisé** quand une recette ne peut pas être faite : le panneau d'aides de touches ne sait pas griser une ligne.
- **Au-dessus d'un trou de taupe, la touche flotte seule** : « Planter » est dans les aides en bas à droite, pas à côté de la touche comme sur le canevas. Les blocs Production et Débloque du canevas ne sont pas encore dans le panneau de la Champimaison.
- **La graine posée à l'arbre géant** est dessinée comme un objet jeté (image de l'objet), en attendant son modèle.
- **Placement des Spongeus sauvages** : 6 points fixes dans la brume, à régler en jouant. Un Spongeus sec peut boire pendant un test de chute : le test le signale.
- **Signaler l'arbre géant et la graine**, le nombre et la place des trous de taupe : à régler pendant le dev.
- **`scripts/check-art-assets.sh` refuse les prefabs de `Assets/Prefabs/UI/`** (domaine et préfixe `PF_`), y compris ceux qui existaient déjà : les nouveaux prefabs d'interface suivent la convention existante du dossier `UI`.
- Placeholders pour tous les nouveaux modèles et icônes.

## Modifié par
