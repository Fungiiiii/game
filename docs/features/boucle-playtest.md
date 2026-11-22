# Boucle du premier playtest : graine, Champimaison, fabrication, lance-pierre

- **Ticket ClickUp :** aucun. Hugo (owner) a demandé de travailler sans ticket, sur la branche du playtest. Écart volontaire au workflow, noté aussi dans la PR.
- **Note de design validée :** aucune sous-tâche de validation. La note a été validée par Hugo le 22/11, en conversation ; son contenu est dans « Ce que fait la feature » et « Décisions ». Maquette : [`Fabrication.html`](../playtest-retours/maquettes/Fabrication.html).
- **Branche :** `feat/playtest-base`
- **Auteur :** Hugo Bernier
- **Statut :** Planifié. Pas encore codé.

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
- [ ] Visé, un trou passe au contour cyan et affiche « Nécessite une graine » sans graine, « Prends la graine en main » si elle est dans le sac, ou la touche E « Planter » si elle est en main.
- [ ] Planter consomme la graine. Ce trou devient la Champimaison. Les autres trous restent, sans contour ni interaction.

**Champimaison**

- [ ] Au-dessus de la Champimaison, un panneau : une ligne par ressource (icône, nom, « déposé / requis », « N dans le sac »), cochée une fois complète. Dessous, la touche E avec le cercle d'appui long et son texte : « Déposer », « Réveiller » ou « Améliorer ».
- [ ] E maintenu dépose ce que le joueur porte et qui manque encore, jamais plus. La Champimaison garde ce qui a été déposé. Le dernier dépôt déclenche le palier.
- [ ] Palier 1 (1 Eau + 3 Spores) : la graine se réveille, le réseau fongique pousse. La Champimaison produit du fil de mycélium, qu'un appui court sur E récupère.
- [ ] Palier 2 (5 Eaux + 5 Spores + 5 Bois) : la Champimaison devient une entrée. Appui court sur E « Entrer » : un intérieur simple, puis « Fin de la démo, merci d'avoir joué » et la planche **Accueil**.

**Cailloux**

- [ ] Des cailloux ramassables apparaissent autour du joueur, comme les bâtons : 4 à moins de 50 m au départ, aucun à moins de 8 m ; toutes les 5 s, s'il en manque, 1 au plus entre 20 et 50 m, plus souvent devant le joueur ; disparus au-delà de 75 m ; 30 au plus.
- [ ] Ils sortent du sol, hors des chemins, des troncs et des rochers, avec le contour de ce qui se ramasse. Placeholder : un cube gris. Les petits cailloux du décor restent.

**Fabrication**

- [ ] Tab ouvre l'inventaire et, à sa droite, le panneau « Fabrication ».
- [ ] En haut du panneau, sur toute la largeur : toutes les recettes, même impossibles, avec « Possible » ou « Il manque ». Aucune n'est ouverte à l'ouverture.
- [ ] Cliquer une recette ouvre ses détails en bas du panneau : icône, nom, description, ingrédients « possédé / requis ». Un ingrédient qui manque est en rouge, avec sa provenance.
- [ ] En bas du panneau : **E Fabriquer** (grisé s'il manque un ingrédient) et **R Traquer** / **Ne plus traquer**.
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

Prévue, étape par étape (chemins relatifs à `Assets/Scripts/`). Ce tableau sera mis à jour au fil du code.

| Étape | Fichiers principaux |
|---|---|
| Dépôt progressif | `Runtime/Core/ItemAmount.cs`, `Runtime/Champimaison/RequirementProgress.cs`, `ChampimaisonDefinition.cs` (nouveaux) ; `FungalNetwork.cs`, `FungalHoleSpawner.cs` (modifiés) ; `Assets/Data/Champimaison/Champimaison.asset` |
| Graine et trous de taupe | `Runtime/Champimaison/MoleHole.cs`, `MoleHoles.cs`, `PlantingRules.cs` (nouveaux) ; `Runtime/Items/DroppedItems.cs` (`Place`) ; `Runtime/Game/GameBootstrap.cs` ; `Assets/Data/Items/Seed.asset` ; `Game.unity` |
| Panneau et palier 1 | `Runtime/Champimaison/FungalHole.cs` (`IHoldInteractable`) ; `Runtime/UI/RequirementPanel.cs`, `RequirementRow.cs` + prefabs |
| Cailloux | `Runtime/Gathering/ForestGathering.cs` ; `Assets/Data/Items/Pebble.asset`, `Assets/Data/Gathering/Pebbles.asset` |
| Fabrication | `Runtime/Crafting/CraftingRecipe.cs`, `RecipeBook.cs`, `Crafter.cs`, `RecipeTracker.cs` ; `Runtime/UI/CraftingPanel.cs`, `RecipeRow.cs`, `IngredientRow.cs` + prefabs dans `Inventory.prefab` ; `Runtime/UI/InventoryScreen.cs` ; `Runtime/Core/ItemDefinition.cs` (description, provenance) ; actions `Craft` et `Track` dans `InputSystem_Actions.inputactions` |
| Haut à droite | `Runtime/Game/ObjectiveChain.cs`, `Runtime/UI/ObjectiveHud.cs` + prefab |
| Spongeus sauvages | `Runtime/Capture/WildSoak.cs`, `Runtime/Companions/WetLook.cs` (nouveaux) ; `WildMushroom.cs`, `CreatureDefinition.cs`, `CreaturePopulation.cs`, `Companion.cs` ; `Spongeus.asset` ; `Game.unity` |
| Lance-pierre | `Runtime/Combat/CreatureHealth.cs`, `Slingshot.cs`, `SlingshotShot.cs`, `SlingshotSettings.cs` ; prefab `Player` ; `Assets/Data/Items/Slingshot.asset` |
| Fil, palier 2, fin | `Runtime/Champimaison/ThreadProduction.cs` ; `Runtime/UI/DemoEnd.cs` + prefab ; `Assets/Scenes/Game/ChampimaisonInterior.unity` ; `Editor/Playtest/PlaytestSetup.cs` (Build Settings) |

Textes : table `UI` (fr/en), clés `item.*`, `label.needs_seed`, `label.hold_seed`, `hint.plant`, `hint.awaken`, `hint.upgrade`, `panel.in_bag`, `craft.*`, `hint.craft`, `hint.track`, `hint.untrack`, `objective.*`, `hint.shoot`, `hint.no_pebble`, `hint.collect_thread`, `hint.enter`, `demo_end.*`.

## Décisions

Validées par Hugo le 22/11 :

- **La graine se trouve au pied de l'arbre géant** et se plante dans un trou de taupe, pas n'importe où. Les autres trous serviront aux Champimaisons suivantes.
- **Réveiller la graine coûte 1 Eau + 3 Spores**, le palier 2 coûte 5 Eaux + 5 Spores + 5 Bois. Le fil vient de la Champimaison réveillée : le filet arrive après le palier 1, la capture devient rentable au palier 2.
- **Tuer un Spongeus ne donne de l'eau que s'il est mouillé** : le joueur observe avant de tirer.
- **Les outils en main, les ressources dans le sac.**
- **Panneau de coût au-dessus de la Champimaison**, sur le modèle de beaucoup de jeux : la liste des ressources, puis la touche avec le cercle d'appui long.
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
- **Le panneau de la Champimaison suit un point du monde en espace écran**, comme `KeyPrompt`.
- **Le caillou du lance-pierre suit `NetTrajectory`** et teste le segment parcouru à chaque image (`SphereCast`), comme le filet : il ne traverse pas sa cible.
- **La scène `MysticForest` n'est pas modifiée** : les trous de taupe se placent sur ses `ChampimaisonSite`, lus au lancement.
- **Prefabs, câblage de `Game.unity` et données créés par un script d'Editor à usage unique** (CLAUDE.md, « Scenes, prefabs and data ») : la branche a été écrite sans Editor. Le script est lancé une fois, son résultat relu et commité, puis il est supprimé.

## Tests

### Automatisés (Unity Test Runner)

Pas encore écrits ni lancés.

| Test | Mode | Couvre | Résultat |
|---|---|---|---|
| `RequirementProgressTests` | EditMode | Jamais plus que ce qui manque ; dépôt partiel gardé ; complet au dernier dépôt ; rien ne bouge sans ressources | ⬜ |
| `FungalNetworkTests` (réécrit) | EditMode | Paliers par liste ; `Grew` au dernier dépôt ; remise à zéro | ⬜ |
| `PlantingRulesTests` | EditMode | Texte ou touche selon la graine (en main, dans le sac, aucune, déjà plantée) | ⬜ |
| `CrafterTests` | EditMode | Tout ou rien ; ingrédients rendus si le résultat ne rentre pas ; résultat dans la barre rapide d'abord | ⬜ |
| `RecipeTrackerTests` | EditMode | Traquer, ne plus traquer ; retirée après fabrication | ⬜ |
| `ObjectiveChainTests` | EditMode | Ordre des objectifs et événements qui les font avancer | ⬜ |
| `WildSoakTests` | EditMode | Va à la source la plus proche ; s'imbibe ; mouillé ; la fuite interrompt | ⬜ |
| `CreatureHealthTests` | EditMode | Dégâts, mort | ⬜ |
| `ThreadProductionTests` | EditMode | Un fil toutes les N secondes, stock maximum | ⬜ |
| `GameScenePlayModeTests` (ajouts) | PlayMode | Inventaire vide ; graine au pied de l'arbre ; plantation ; cailloux autour du joueur ; Spongeus sauvage qui boit | ⬜ |
| `ChampimaisonPlayModeTests` | PlayMode | Panneau à jour ; E maintenu dépose ; palier 1 | ⬜ |
| `CraftingPanelPlayModeTests` | PlayMode | Liste, détails, E, deuxième clic, R, manette (A, Y) | ⬜ |
| `SlingshotPlayModeTests` | PlayMode | Caillou consommé ; pas de tir sans caillou ; Spongeus mouillé → 1 Eau ; sec → rien | ⬜ |
| `PlaytestLoopPlayModeTests` (ajout) | PlayMode | Parcours complet jusqu'à la fin de la démo | ⬜ |

**Tests existants dont l'exigence change :** `FungalNetworkTests` (coût en liste, dépôt progressif) ; les tests de `GameScenePlayModeTests` qui supposent le filet dans la première case au départ.

### Manuels (dans Unity)

| # | Étapes | Résultat attendu | OK |
|---|---|---|---|
| 1 | `Game.unity`, Play | Mains vides, barre rapide vide. Objectif « Trouve une graine » en haut à droite | ⬜ |
| 2 | Aller à l'arbre géant | La graine, contourée, au pied de l'arbre. E la ramasse | ⬜ |
| 3 | Viser un trou de taupe, graine dans le sac | « Prends la graine en main » | ⬜ |
| 4 | Graine en main, E sur le trou | La Champimaison apparaît, panneau « Eau 0/1 · Spores 0/3 ». Les autres trous n'ont plus de contour | ⬜ |
| 5 | Cueillir 3 champignons, maintenir E sur la Champimaison | Spores 3/3 cochée, Eau 0/1 reste. Le texte de la touche est « Déposer » | ⬜ |
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

## Limitations connues

- Signaler l'arbre géant et la graine, le nombre et la place des trous de taupe, le nombre de Spongeus sauvages et leur fréquence de boisson : à régler pendant le dev.
- Placeholders pour tous les nouveaux modèles et icônes.

## Modifié par
