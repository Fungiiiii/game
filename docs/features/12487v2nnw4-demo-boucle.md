# Démo : boucle de jeu dans la Forêt mystique

- **Ticket ClickUp :** [[DEMO] Boucle de jeu — intégration des prototypes dans la Forêt mystique](https://app.clickup.com/t/12487v2nnw4)
- **Note de design validée :** [[VALIDATION] Note de design — Démo : boucle de jeu dans la Forêt mystique](https://app.clickup.com/t/12487v2nnw8) — rédigée par Ilyes, réécrite avec Hugo et fermée le 19/11.
- **Branche :** `feat/12487v2nnw4-demo-boucle`, **créée depuis `feat/86c9mnqy5-filet-capture`** : le filet n'est pas encore dans `develop`. Cette branche ne peut être fusionnée qu'après celle du filet.
- **Auteur :** Ilyes Benkerri
- **Statut :** En cours

## Docs précédents

- [86c9mnqy5-filet-capture](./86c9mnqy5-filet-capture.md) — le filet est réutilisé tel quel. Son champignon (`NetDemoMushroom`) gagne trois options, sans effet sur `NetThrowScene` : un modèle 3D à la place des primitives, une promenade autour de son point d'apparition, le suivi du relief. Le lanceur (`NetThrower`) prévient quand il ramène une prise, et son entrée de lancer peut être coupée.

## Ce que fait la feature

La boucle : explorer → cueillir → capturer → nourrir le réseau. Scène : `Assets/Scenes/Prototype/DemoLoopScene.unity`, lancée depuis l'Editor, en solo.

- [ ] **Spawn :** le joueur apparaît dans la Forêt mystique, au spawn de Hugo, face à l'arbre géant, en vue à la première personne. Marche, sprint (Maj), accroupi (C), dodge (Espace). La forêt garde son audio de zone, ses spores et sa brume.
- [ ] **HUD :** barres de vie et d'endurance en haut à gauche (HUD actuel du repo). La vie ne bouge pas dans la démo.
- [ ] **Endurance :** le sprint consomme 15 % par seconde, le dodge 20 % par usage. Elle remonte de 20 % par seconde dès que rien n'a été dépensé depuis 1 s, en marchant comme à l'arrêt. À 0 %, plus de sprint ni de dodge jusqu'à 20 %. Lancer le filet ne coûte rien.
- [ ] **Cueillette :** E ramasse ce que le joueur vise, à 2 m au plus au sol, dans un cône de 25° autour du centre de la vue. Ce qui peut être cueilli est entouré d'un anneau cyan au sol, plus épais quand il est visé.
  - **Spores :** les champignons du décor posés par Hugo dans la forêt (2 au spawn, 6 au pied de l'arbre géant), plus 7 du même modèle placés par la démo (5 dans la clairière du spawn, 2 près du trou). 1 spore par champignon.
  - **Bois :** 7 bâtons de Hugo (`PF_Stick`, 1 m) posés au sol, orientés au hasard (5 dans la clairière, 2 près du trou). 1 bois par bâton. Exception temporaire (RG_CR_01).
  - L'objet cueilli disparaît et réapparaît au même endroit après 30 à 60 s, au hasard. Inventaire plein : rien n'est cueilli.
- [ ] **Champignons :** 2 Spongeus et 2 Champoule (modèles de Hugo) se promènent lentement autour de leur point de départ, dans la clairière du spawn. Comme dans le prototype du filet : alerte à 3,5 m (disque jaune sous eux), fuite après 1,5 s (rouge), calme à 6 m (gris). Ils suivent le relief.
- [ ] **Capture :** au filet, tel que validé (clic gauche). Spongeus est un champignon commun (60 % s'il n'a pas repéré le joueur, 40 % sinon), Champoule un rare (30 % / 10 %). Quand le filet revient avec une prise, le champignon entre dans l'inventaire tel quel (RG_CA_03).
- [ ] **Inventaire :** Tab ouvre et ferme l'inventaire, qui démarre vide. Ouvert, la souris sert à l'inventaire : la vue ne tourne plus, le filet ne part pas, E ne fait rien. Chaque objet est un carré de couleur et un nombre : violet = spore, brun = bois, bleu clair = Spongeus, jaune pâle = Champoule.
- [ ] **Champimaison :** un trou sombre dans le sol, sur `ChampimaisonSite_01`, l'emplacement de Hugo le plus proche du spawn (environ 47 m vers le nord-est). E au bord du trou y dépose le bois et les spores de l'étape suivante, et le réseau fongique grandit autour en deux étapes : 7 filaments couleur mycélium (crème, palette « Body light ») posés à plat sur le sol, puis 12 de plus, deux fois plus longs. L'anneau du trou est de la même couleur.
  - Étape 1 : 2 bois + 2 spores. Étape 2 : 4 bois + 4 spores.
  - Sans assez de l'un ou de l'autre, rien n'est pris et l'anneau du trou clignote orange.
  - Une fois l'étape 2 atteinte, l'anneau du trou disparaît.
- [ ] **Reset :** R (touche de développeur) remet tout à zéro : joueur au spawn, endurance pleine, inventaire vide et fermé, ressources revenues, champignons à leur place et calmes, filet en main, trou vide.

La SFD a été mise à jour par Hugo le 19/11 dans ce sens : RG_CR_01 (cueillette), RG_SU_03 (régénération en marchant), RG_CH_01 et RG_CH_04 (trou et réseau), RG_CA_03 (la capture donne le champignon).

## Hors périmètre

- Le réseau multijoueur et l'exécutable : la démo se lance depuis l'Editor.
- La maquette HUD de Léna, la capture à mains nues de Quentin et le CaptureSystem de Pol-Mattis.
- Ce qu'on fait d'un champignon capturé.
- Le champignon de bois, d'autres ressources, le craft.
- Faim, toxicité, poison, combat, mort et respawn.
- Toute modification de la scène `MysticForest` : elle est chargée sans être touchée.
- Un nouvel art : seulement des placeholders (trou, filaments, anneaux), et les modèles existants de Hugo : `Spongeus.fbx`, `Champoule.fbx`, `PF_MushroomCluster` et `PF_Stick`.
- Tout texte affiché au joueur.

## Implémentation

La démo utilise la [Forêt mystique](./12487v2byt9-mystic-forest.md) **sans la modifier** : la scène est chargée telle quelle, et rien n'y est enregistré. Elle n'est donc pas dans « Docs précédents ».

`DemoLoopScene` ne contient qu'un objet, `Demo Loop`, avec `DemoLoopBootstrap`. Elle a été écrite par l'API de l'Editor, pas à la main. Au lancement, le bootstrap :

1. charge `MysticForest` en additif et en fait la scène active (brouillard, ciel et lumière ambiante sont ceux de la forêt) ;
2. désactive le `FirstPersonTestPlayer` de la forêt, après en avoir détaché la `Main Camera` ; la pose du joueur de test devient le spawn ;
3. fait apparaître le prefab `Player` hors ligne, et met `PrototypeFirstPersonCamera` (celle du filet) sur la `Main Camera` de la forêt ;
4. construit le HUD, l'inventaire vide, le trou, les ressources, les champignons et le filet.

**Logique en C# pur, testée en EditMode :**

| Fichier | Rôle |
|---|---|
| `Runtime/Survival/StaminaRegulator.cs` | Règles d'endurance RG_SU_03 et RG_SU_04, appliquées à l'endurance de `PlayerVitals`. |
| `Runtime/Gathering/RespawningResource.cs` | Un point de cueillette : disponible, cueilli, de retour après un délai tiré au hasard (générateur injecté). |
| `Runtime/Gathering/InteractionTargeting.cs` | Choisit ce que le joueur vise de près : portée mesurée au sol, cône autour de la vue. |
| `Runtime/Champimaison/FungalNetwork.cs` | Étapes du réseau et leurs coûts. Une étape est payée en entier ou pas du tout. |
| `Runtime/Capture/MushroomWander.cs` | Promenade d'un champignon calme autour de son point de départ (générateur injecté). |

**Intégration**, dans `Runtime/Demo/` (namespace `Fungiiiii.Demo`) :

| Fichier | Rôle |
|---|---|
| `DemoLoopBootstrap.cs` | Chargement de la forêt, construction de la boucle, Tab (inventaire), R (reset). Porte les réglages de la note. |
| `StaminaDriver.cs` | Relie `StaminaRegulator` à `PlayerMotor`. |
| `PlayerInteractor.cs` | Touche E (`<Keyboard>/e`, `<Gamepad>/buttonNorth`), cible et utilise. |
| `Gatherable.cs` | Un point de cueillette dans la scène : son visuel, son anneau cyan. |
| `FungalHole.cs` | Le trou et les filaments du réseau, en placeholders. |
| `DemoGroundRing.cs` | Anneau au sol qui suit le relief. |
| `DemoItems.cs` | Les objets de la démo : identifiants et couleurs d'inventaire (palette du projet). |
| `IDemoInteractable.cs` | Ce que E peut utiliser. |

**Code existant modifié :**

- `PlayerMotor` : `SprintAllowed` et `DodgeAllowed` (vrais par défaut), `IsSprintMoving`, événement `Dodged`. Sans propriétaire d'endurance, rien ne change : `MouvementJoueurScene` et `NetThrowScene` gardent leur comportement.
- `InventoryDemoController` (Pol-Mattis) : `SeedsDemoItems`, vrai par défaut. Faux, l'inventaire démarre vide. `InventaireScene` ne change pas. Le risque n° 4 de la note est couvert par ce seul ajout : `Inventory.Add` existait déjà.
- `NetDemoMushroom` : `InitializeWithModel`, `EnableWander`, `FollowGround` (voir « Docs précédents »).
- `NetThrower` : événement `CatchReturned`, propriété `ThrowInputEnabled`.

## Décisions

- **La forêt est chargée en additif, puis devient la scène active.** Son brouillard, son ciel et sa lumière ambiante sont des réglages de scène : sans ça, ceux de `DemoLoopScene` s'appliqueraient.
- **Dans l'Editor, la forêt est chargée par `EditorSceneManager.LoadSceneAsyncInPlayMode`.** Ni `MysticForest` ni `DemoLoopScene` ne sont dans les Build Settings, et l'exécutable est hors périmètre : les y ajouter aurait touché `ProjectSettings` sans besoin. Un build chargerait la forêt par `SceneManager`, ce qui demandera de l'ajouter aux Build Settings.
- **Le bois est le bâton de Hugo (`PF_Stick`)**, fusionné dans `develop` le 19/11 (#37), à la place du cylindre placeholder. Il n'a pas de collider : seul E le ramasse, le filet ne s'y pose pas. Son anneau fait 0,6 m de rayon, pour entourer ses 1 m de long.
- **La caméra de la forêt est réutilisée, détachée de son joueur de test.** Elle porte l'écouteur de l'audio de zone (`ZoneAmbience` suit sa position), les spores et la brume au sol. Le joueur de test est désactivé, pas détruit.
- **Le joueur est le prefab réseau `Player`, hors ligne**, comme dans `NetThrowScene` : aucun `NetworkManager`.
- **Les champignons cueillables sont des objets, pas les arbres du Terrain.** Les champignons du décor semés par le Terrain n'ont pas d'objet propre : les rendre cueillables aurait demandé de modifier les données du Terrain de Hugo. La démo utilise ceux que Hugo a posés à la main, et en ajoute du même modèle (`PF_MushroomCluster`). Arbitrage d'Ilyes, 19/11.
- **Le trou est sur `ChampimaisonSite_01`**, l'emplacement de Champimaison le plus proche du spawn, plat et dégagé. Arbitrage d'Ilyes, 19/11.
- **Spongeus est commun, Champoule rare** pour le filet. Arbitrage d'Ilyes, 19/11.
- **Une étape du réseau se paie en une fois.** E au bord du trou prend exactement le coût de l'étape suivante, ou rien. Le trou ne garde pas de dépôt partiel.
- **La portée de cueillette se mesure au sol**, entre la caméra et l'objet : regarder un objet à ses pieds ne l'éloigne pas.
- **Endurance : `StaminaRegulator` agit sur l'endurance de `PlayerVitals`**, qui reste la seule source de vérité, affichée par le HUD existant.
- **Le champignon capturé entre dans l'inventaire quand le filet revient**, au moment où il disparaît dans le filet.
- **Tab et R sont construits en code**, comme `Dodge` dans `PlayerInputReader`. E aussi : l'action `Interact` de l'asset `InputSystem_Actions` n'est pas utilisée, pour ne pas mélanger les deux façons de faire dans la démo.

### Écarts par rapport à la note validée

Ces choix ne figurent pas dans la note, ou la précisent. À confirmer par Hugo :

1. **Cyan de la charte introuvable.** Ni la palette du repo ni Confluence ne donnent ce cyan. L'anneau utilise un cyan provisoire (`#33F2FF`), réglable dans `DemoLoopBootstrap` (`highlightColor`).
2. **Trou à environ 47 m du spawn** (`ChampimaisonSite_01`), et non dans la clairière du spawn : la note dit « près du spawn ».
3. **Spores ajoutées par la démo :** 7 champignons du décor en plus de ceux de Hugo, pour qu'il y ait de quoi payer les deux étapes près du spawn.
4. **Quantités :** 1 spore par champignon, 1 bois par bâton. 7 bâtons, 15 champignons cueillables. La note ne les fixe pas.
5. **Profils de capture :** Spongeus commun, Champoule rare (taux du filet). La note ne les fixe pas.
6. **Promenade :** rayon de 4 m, 0,8 m/s, pause de 1,5 s entre deux points. Fuite à 4 m/s, comme le filet. Les champignons ne sortent pas d'un rectangle autour du spawn et du trou.
7. **Retour visuel d'un dépôt refusé :** l'anneau du trou clignote orange 0,5 s. La note ne dit rien du refus vu par le joueur.
8. **Ce qui montre les états des champignons :** le disque coloré sous eux, repris du filet (gris, jaune, rouge). Pendant le gel du filet, le modèle est éclairci.
9. **Cône de visée de 25°** pour E : la note ne donne que la portée.
10. **Pas d'affichage des coûts du réseau :** aucun texte n'est permis sans le système de localisation, et la maquette HUD n'existe pas encore.
11. **Réseau couleur mycélium, pas cyan.** La note dit « filaments cyan au sol ». À la demande d'Ilyes (19/11), le réseau et l'anneau du trou sont crème, couleur « Body light » (#F2E6CC) de la palette, plus proche d'un vrai mycélium. Les filaments sont des rubans à plat, en matériau éclairé : ils prennent la lumière et le brouillard de la forêt. Réglable dans `DemoLoopBootstrap` (`networkColor`). Les objets cueillables gardent leur anneau cyan.

## Tests

### Automatisés (Unity Test Runner)

Lancés en batchmode avec Unity 6000.3.21f1 le 19/11, sur la machine d'Ilyes. Relancés après le rebase sur `origin/develop` (22c3c96, bâton de Hugo) et le passage à `PF_Stick`, puis après le passage du réseau en couleur mycélium : mêmes résultats. Log et XML lus : **EditMode 203/203, PlayMode 32/32**, tous les tests du projet compris (35 et 3 nouveaux). Aucune erreur ni aucun avertissement de compilation. PlayMode a tourné sans `-nographics`. Pendant les tests de la démo, seul le message « TerrainCollider: MeshCollider is not supported on terrain » est logué : il vient de `MysticForest`, qui le logue aussi dans ses propres tests.

| Test | Mode | Couvre | Résultat |
|---|---|---|---|
| `StaminaRegulatorTests` (10 tests) | EditMode | Sprint à 15 %/s, dodge à 20 %, pas de régénération pendant 1 s, régénération à 20 %/s en marchant, plafond, plus de sprint ni de dodge de 0 à 20 %, dodge refusé sans rien dépenser, sprint maintenu sans bloquer la régénération, dodge qui vide l'endurance, reset | ✅ |
| `GatheringTests` (12 tests) | EditMode | Cueillette et disparition, pas de double cueillette, délai entre 30 et 60 s, réapparition, même graine même délai, reset, plage inversée refusée ; visée : cible au centre, hors de portée, portée au sol, derrière ou hors du cône, désactivée | ✅ |
| `FungalNetworkTests` (8 tests) | EditMode | Trou vide au départ ; étape 1 à 2 + 2 ; refusé sans assez de l'un, sans rien prendre ; refusé inventaire vide ; étape 2 à 4 + 4 ; étape 2 refusée avec les seules ressources de l'étape 1 ; rien de plus après l'étape 2 ; reset | ✅ |
| `MushroomWanderTests` (5 tests) | EditMode | Reste dans son rayon, vitesse respectée et à plat, bouge vraiment, même graine même promenade, revient vers chez lui | ✅ |
| `DemoLoopScenePlayModeTests.SceneLoad_LoadsTheForest_WithOnePlayerAndOneCamera` | PlayMode | La forêt est chargée, un seul joueur, le joueur de test désactivé, une seule caméra active (celle de la forêt, pilotée par la vue FPS), champignons, ressources et trou présents, inventaire vide et fermé | ✅ |
| `DemoLoopScenePlayModeTests.GatheringASpot_PutsOneItemInTheInventory_AndHidesIt` | PlayMode | Cueillir donne 1 objet, le point disparaît et ne se cueille pas deux fois | ✅ |
| `DemoLoopScenePlayModeTests.ACapture_PutsTheMushroomInTheInventory` | PlayMode | Un filet lâché sur un Spongeus finit par le capturer, et il entre dans l'inventaire (jusqu'à 12 essais, reset entre chaque) | ✅ |

Les tests existants ne sont pas modifiés et doivent rester verts, en particulier `NetThrowScenePlayModeTests`, `PlayerMovementDemoPlayModeTests` et `InventoryDemoControllerPlayModeTests`.

### Manuels (dans Unity)

Scène : `Assets/Scenes/Prototype/DemoLoopScene.unity`, puis **Play** et un clic dans la Game View pour capturer la souris. Le chargement de la forêt prend quelques secondes.

| # | Étapes | Résultat attendu | OK |
|---|---|---|---|
| 1 | Ouvrir `DemoLoopScene`, sans Play | Hierarchy : un seul objet, `Demo Loop`. Sur son `DemoLoopBootstrap`, `Player Prefab` = `Player`, `Spongeus Model` = `Spongeus`, `Champoule Model` = `Champoule`, `Decor Mushroom Prefab` = `PF_MushroomCluster`, `Stick Prefab` = `PF_Stick`. Aucun « Missing » | ⬜ |
| 2 | Play | La forêt apparaît, vue à la première personne depuis la clairière, face à la trouée vers l'arbre géant. Brouillard bleu-violet, spores qui tombent, son d'ambiance. Barres de vie et d'endurance pleines en haut à gauche. Aucune erreur rouge dans la Console | ⬜ |
| 3 | Hierarchy pendant le Play | `MysticForest` chargée ; son `Player` (joueur de test) grisé ; une `Main Camera` à la racine de la forêt ; un seul `Player` actif | ⬜ |
| 4 | Se déplacer : ZQSD / WASD, souris | Avancer va vers le regard. Le corps ne fait pas tourner la vue. On ne traverse pas les troncs | ⬜ |
| 5 | Sprinter (Maj) tout droit jusqu'à vider l'endurance | La barre bleue baisse d'environ 15 % par seconde. À 0, on repasse à la vitesse de marche même Maj enfoncé | ⬜ |
| 6 | Suite de #5 : continuer à marcher sans sprinter | Après 1 s, la barre remonte en marchant. Avant 20 %, Maj et Espace ne font rien ; à partir de 20 %, ils remarchent | ⬜ |
| 7 | Barre pleine, Espace trois fois de suite (en attendant la fin de chaque dodge) | Chaque dodge retire environ 20 %. Le dodge sans touche de direction part vers l'avant | ⬜ |
| 8 | Regarder les champignons du décor près du spawn | Anneaux cyan au sol autour d'eux, et autour des bâtons. Les champignons du décor plus loin dans la forêt n'en ont pas | ⬜ |
| 9 | S'approcher d'un champignon à anneau, le viser, E | L'anneau s'épaissit quand il est visé. E : le champignon et son anneau disparaissent | ⬜ |
| 10 | Tab | L'inventaire s'ouvre au centre, avec 1 carré violet. La souris est libre et la vue ne tourne plus. Clic gauche : aucun filet ne part. Tab : il se ferme, la souris est recapturée | ⬜ |
| 11 | Cueillir un bâton (E), puis Tab | Le bâton (modèle de Hugo, couché au sol, couleur des troncs) disparaît avec son anneau. Un carré brun en plus | ⬜ |
| 12 | Viser un objet à plus de 2 m, E | Rien ne se passe, l'anneau ne s'épaissit pas | ⬜ |
| 13 | Attendre à côté d'un objet cueilli | Il réapparaît au même endroit entre 30 et 60 s après | ⬜ |
| 14 | Regarder les Spongeus et les Champoule sans s'approcher | Ils se promènent lentement, posés sur le sol, sans flotter ni s'enfoncer. Ils regardent dans le sens où ils avancent. Disque gris sous eux | ⬜ |
| 15 | S'approcher à moins de 3,5 m | Disque jaune, puis rouge après 1,5 s : il fuit. Plus loin, il se calme (gris) et reprend sa promenade | ⬜ |
| 16 | Lancer le filet (clic gauche) sur un Spongeus calme | Gel de 0,5 s, modèle éclairci. Le plus souvent, capture : il rétrécit dans le filet qui revient. Tab : un carré bleu clair | ⬜ |
| 17 | R, puis lancer plusieurs fois sur un Champoule, R entre chaque essai | Échecs fréquents (rare). Quand il est pris, carré jaune pâle dans l'inventaire | ⬜ |
| 18 | Marcher jusqu'au trou (nord-est, environ 47 m) | Un disque sombre au sol entouré d'un anneau crème, sur un sol plat et dégagé | ⬜ |
| 19 | R, cueillir 1 bois et 1 spore, E au bord du trou | L'anneau clignote orange. Tab : le bois et la spore sont toujours là | ⬜ |
| 20 | Avoir au moins 2 bois et 2 spores, E au bord du trou | Des filaments crème apparaissent, à plat sur le sol autour du trou, éclairés et ombrés comme le sol (ni lumineux, ni noirs). Tab : 2 bois et 2 spores en moins | ⬜ |
| 21 | Avoir au moins 4 bois et 4 spores, E au bord du trou | Le réseau s'étend plus loin. L'anneau du trou disparaît. Tab : 4 bois et 4 spores en moins | ⬜ |
| 22 | R | Joueur au spawn, vue droite devant, endurance pleine, inventaire vide et fermé, tout revenu, champignons à leur place et gris, filet en main, trou vide sans filaments | ⬜ |
| 23 | Console pendant tout le test | Aucune erreur rouge. Noter tout avertissement de Netcode lié au prefab joueur utilisé hors ligne | ⬜ |
| 24 | Ouvrir `MysticForest` seule, Play | La forêt marche comme avant, avec son joueur de test. `git status` : la scène n'est pas modifiée | ⬜ |
| 25 | Ouvrir `NetThrowScene`, `InventaireScene` et `MouvementJoueurScene`, Play | Comme avant : 3 champignons primitifs dans l'arène du filet, inventaire rempli d'exemples, mouvement en vue de dessus | ⬜ |

## Limitations connues

- **Dépendance :** la branche contient celle du filet. Elle ne se fusionne qu'après elle.
- **Lancée depuis l'Editor seulement.** Un build demanderait d'ajouter `DemoLoopScene` et `MysticForest` aux Build Settings.
- **Textes de l'inventaire de Pol-Mattis** (« INVENTORY », « QUICK BAR »…) en dur, en anglais, sans localisation : c'est l'état du prototype existant, pas une nouveauté de la démo.
- **Fermer l'inventaire en tenant une pile au curseur** la garde au curseur jusqu'à la réouverture : comportement du prototype d'inventaire.
- **Les champignons ignorent les obstacles :** ils traversent troncs et rochers en se promenant ou en fuyant (pas de navigation).
- **Orientation des modèles :** la démo tourne Spongeus et Champoule vers leur direction de marche en supposant que leur face regarde l'axe Z. À vérifier en jeu (#14).
- **Éclaircissement d'un modèle dans le filet :** obtenu en multipliant la couleur de base du matériau palette. Rendu à vérifier en jeu (#16).
- **Shaders par `Shader.Find`** pour les placeholders, comme le filet : valable dans l'Editor, pas garanti dans un build.

## Modifié par

- [playtest-base](./playtest-base.md) — la boucle sert de base au playtest : contour du modèle à la place de l'anneau cyan, bâtons et champignons répartis sur toute la map par tick, trou de la Champimaison retiré pour l'instant, menu pause sur Échap.
