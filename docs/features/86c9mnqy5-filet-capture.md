# Filet de capture (Net Throw) — prototype

- **Ticket ClickUp :** [[CAPTURE] Filet de capture (Net Throw)](https://app.clickup.com/t/86c9mnqy5)
- **Note de design validée :** [[VALIDATION] Note de design — Filet de capture (Net Throw)](https://app.clickup.com/t/12487v2nkb9) — réécrite et fermée par Hugo le 19/11. Révision de Hugo le 19/11, transmise par Ilyes : couleur du cercle en dégradé et fil entre la main et le filet.
- **Branche :** `feat/86c9mnqy5-filet-capture`
- **Auteur :** Ilyes Benkerri
- **Statut :** En cours — écrit sans Unity, **ni compilé ni testé** (voir « Limitations connues »)

## Docs précédents

Aucun.

La branche touche deux prototypes qui n'ont pas de doc de feature, car ils sont antérieurs au process : le mouvement joueur (`PlayerMotor`, PR #9) et la capture (`MushroomCaptureStateMachine`, PR #5). Ce qui change chez eux est décrit dans « Implémentation ».

## Ce que fait la feature

Le filet sert à capturer à distance les champignons rapides ou en fuite. C'est un outil unique et permanent, sans durabilité.

- [ ] Le joueur vise à la première personne, à la souris ou au stick droit. Il se déplace avec le vrai mouvement : marche, sprint, accroupi, dodge. Avancer va dans la direction du regard.
- [ ] Tant que le filet est en main, un petit disque est visible en bas à droite de l'écran. Un arc en pointillés montre la trajectoire, et un cercle de 1,5 m de rayon montre la zone d'impact.
- [ ] Le cercle prend une couleur en dégradé selon les chances du champignon qui serait visé, pour qu'on voie tout de suite à quoi on a affaire :
  - **rouge** : 0 %, capture impossible ;
  - **orange** : dès que la chance dépasse 0 %, chance basse mais pas impossible ;
  - **jaune** : 50 % ;
  - **le vert le plus vert** : 100 %.

  La couleur passe en continu de l'orange au jaune, puis du jaune au vert. Le cercle est **gris** si aucun champignon n'est dans la zone. Aucun texte n'est affiché.
- [ ] Clic gauche ou gâchette droite : le filet part dans la direction du regard à 11 m/s. Il suit un arc et retombe au premier sol, obstacle ou champignon rencontré. Il ne s'arrête jamais sur le joueur lui-même.
- [ ] Si un champignon est dans la zone à l'atterrissage, il y a une seule tentative de capture : sur le champignon le plus proche du centre, mesuré au sol.
- [ ] Les chances de réussite sont provisoires :

  | Champignon | A repéré le joueur | Ne l'a pas repéré (bonus de surprise +20 points) |
  |---|---|---|
  | Commun | 40 % | 60 % |
  | Commun affaibli | 80 % | 100 % |
  | Rare | 10 % | 30 % |

  « Ne l'a pas repéré » veut dire que le champignon est encore calme (gris). Un champignon en alerte (jaune) ou en fuite (rouge) n'a pas le bonus.
- [ ] Quand le filet touche un champignon, le filet et le champignon se figent 0,5 s. Le champignon est mis en surbrillance avec des couleurs plus claires. Le joueur reste libre de bouger.
- [ ] Tant que le filet est sorti (en vol, au sol, au retour), un fil relie la main au filet, avec un léger affaissement.
- [ ] **Réussite :** après le gel, le filet se referme en boule et revient vers le joueur en 1 s. Le champignon est dedans et rétrécit jusqu'à disparaître à l'arrivée.
- [ ] **Échec :** après le gel, le filet revient vide en 1 s et le champignon fuit loin du filet.
- [ ] **Lancer à côté :** le filet revient vide tout de suite, en 1 s. Les champignons à moins de 3 m de l'impact fuient loin du filet.
- [ ] Un champignon qui fuit après un filet s'en éloigne jusqu'à 6 m, puis se calme.
- [ ] On ne peut pas relancer tant que le filet n'est pas revenu en main : le retour fait office de recharge.
- [ ] Un champignon en fuite peut être visé.
- [ ] Un filet lancé dans le vide revient vide au bout de 3 s de vol.
- [ ] Les champignons perçoivent le joueur comme dans le prototype de capture : calme (gris), alerte à 3,5 m (jaune), fuite après 1,5 s d'alerte (rouge), calme à nouveau à 6 m. Ils fuient à 4 m/s et ne sortent pas de l'arène.
- [ ] La scène contient 3 champignons à profil fixe :
  - un **commun**, au chapeau rouge ;
  - un **commun affaibli**, au chapeau terne et penché ;
  - un **rare**, au chapeau violet et plus gros.
- [ ] **R** ou **Start** remet la démo à zéro : filet en main, champignons à leur place et calmes, joueur au point de départ.

## Hors périmètre

- Le CaptureSystem (ticket de Pol-Mattis) : vrais taux par espèce, état affaibli réel, récompenses, XP.
- Le combat qui affaiblit les champignons.
- Le réseau : synchronisation du lancer et validation par l'hôte.
- Le craft du filet et son lien avec l'inventaire.
- Les filets améliorés (portée, rayon, taux).
- La caméra FPS définitive (ticket Mouvement joueur).
- Le son, les vrais visuels et les animations : uniquement des placeholders en primitives (ticket VFX, Charte IA).
- L'intégration dans la Forêt mystique ou The Clearing.
- Tout texte affiché au joueur.

## Implémentation

Scène : `Assets/Scenes/Prototype/NetThrowScene.unity`. C'est une copie de `CaptureChampignonScene` (caméra, lumière, volume), dont l'objet de démo est remplacé par `Net Throw Prototype` avec le composant `NetThrowDemo`. Tout le reste est construit au lancement.

**Logique en C# pur**, dans `Assets/Scripts/Runtime/Capture/Net/` (namespace `Fungiiiii.Capture`) :

| Fichier | Rôle |
|---|---|
| `NetTrajectory.cs` | Vol balistique du filet : position et vitesse à l'instant t. Recherche du point d'atterrissage par pas, à travers un lancer de segment injecté. |
| `NetCycle.cs` | Cycle du filet : en main → en vol → gel → retour → en main. Refuse un lancer hors de la main. Événements `Thrown`, `Landed`, `CaptureSucceeded`, `CaptureFailed`, `Returned`. |
| `NetImpact.cs` | Choix de la cible (la plus proche du centre, bord inclus, distance au sol) et liste des champignons dans un rayon. |
| `NetCaptureRates.cs` | **Provisoire.** Table des taux par profil, bonus de surprise, tirage avec un générateur injecté. |
| `NetChanceColors.cs` | Dégradé de couleur du cercle selon la chance : rouge à 0 %, puis orange, jaune à 50 %, vert à 100 %. Ne calcule pas la chance elle-même. |
| `NetLandingResolver.cs` | Ce que fait un filet posé : tentative sur une cible (tirage), ou liste des champignons effrayés si le lancer est à côté. Aperçu sans tirage pour la visée. |

**Scène prototype**, dans `Assets/Scripts/Runtime/Capture/Net/Demo/` :

| Fichier | Rôle |
|---|---|
| `NetThrowDemo.cs` | Construit l'arène, le joueur, les 3 champignons et le lanceur. Porte les réglages (valeurs de la note) et le reset. |
| `NetThrower.cs` | Entrée `ThrowNet`, aperçu de l'arc et du cercle, vol, gel, retour, rétrécissement de la prise, fil entre la main et le filet (`LineRenderer`). |
| `NetDemoMushroom.cs` | Champignon placeholder : perception par la machine à états de capture, fuite, surbrillance, transport dans le filet. |
| `PrototypeFirstPersonCamera.cs` | Caméra FPS du prototype : entrée `Look`, place les yeux sur le haut de la capsule, donne son lacet à `PlayerMotor`. |
| `NetDemoPrimitives.cs` | Primitives et matériaux placeholder. |

**Code existant modifié :**
- `MushroomCaptureStateMachine.ResolveCaptureAttempt(bool succeeded)` : une réussite fait passer à Captured et déclenche `Captured`, un échec fait passer à Flee. Accepté depuis tous les états sauf Captured. Rien d'autre ne change dans la machine à états.
- `PlayerMotor.MovementYawDegrees` passe d'une constante privée à 0 à une propriété publique, qui vaut 0 par défaut. Le déplacement et le dodge sans direction l'utilisent comme avant. `MouvementJoueurScene` garde donc exactement son comportement.

## Décisions

- **Le joueur est le prefab réseau `Player`, utilisé hors ligne.** Aucun `NetworkManager` n'est créé dans la scène, donc rien n'est spawné : `PlayerNetworkAuthority` ne s'exécute pas, et le moteur et la lecture des entrées restent actifs comme dans le prefab. Cela évite un second prefab joueur à maintenir.
- **Le tirage a lieu à l'atterrissage, et le résultat est montré à la fin du gel.** Le taux utilise l'état du champignon au moment où le filet le touche.
- **« N'a pas repéré le joueur » veut dire l'état Idle** de la machine à états de capture.
- **Un lancer à côté passe par `ResolveCaptureAttempt(false)`** sur les champignons proches. Pour la machine à états, c'est une tentative ratée qui fait fuir. Aucune autre méthode n'est ajoutée au domaine de Pol-Mattis.
- **Un champignon fuit loin du filet, pas du joueur,** après un échec ou un lancer à côté. Sinon, un filet lancé de plus de 6 m le calmerait à l'image suivante : le joueur est au-delà de la distance de calme.
- **Les champignons gardent leurs colliders.** Un filet lancé sur eux s'y pose au lieu de les traverser. Ils sont désactivés pendant que la prise revient dans le filet.
- **La couleur ne dépend que de la chance.** Selon Hugo, la chance sera à terme un rapport entre la rareté, le niveau et la vie du champignon, et l'objet utilisé : le filet n'est que le premier outil, d'autres plus forts viendront. Cette formule relève du CaptureSystem. `NetChanceColors` reste valable quelle que soit la façon dont la chance est calculée.
- **`ThrowNet` et `Look` sont construites en code**, comme `Dodge` dans `PlayerInputReader`. L'asset partagé `InputSystem_Actions` n'est pas modifié. `ThrowNet` devra y être ajoutée avant la production.

### Écarts par rapport à la note validée

Ces choix ne figurent pas dans la note, ou la précisent. Ils sont à confirmer par Hugo :

1. **Le cercle est gris** quand aucun champignon n'est dans la zone : « aucune cible » n'est pas « impossible ». Couleurs exactes et point jaune à 50 % : choix de placeholder.
2. **Le rare affaibli n'existe pas.** La note n'en donne pas le taux, et la scène n'a que les 3 profils demandés.
3. **Le filet en main est visible** (petit disque en bas à droite), pour montrer qu'on peut relancer.
4. **Fermeture et rétrécissement :** le filet se referme pendant le premier quart du retour, et la prise rétrécit pendant tout le retour. La note ne donne pas de durée.
5. **Vol maximal de 3 s :** un filet lancé dans le vide revient vide.
6. **Valeurs non fixées par la note :** gravité du filet 9,81 m/s² (à régler au feeling), fuite des champignons à 4 m/s, sensibilité de la visée (0,1°/pixel, 150°/s au stick).
7. **Un événement `Returned` en plus** des quatre demandés, pour savoir quand le filet est de nouveau en main.

## Tests

### Automatisés (Unity Test Runner)

**Pas encore exécutés dans Unity.** Les tests EditMode de la logique pure ont tourné hors Unity, avec .NET 8 et des substituts minimaux de `UnityEngine` : 55 sur 55 au vert, tests existants de la machine à états compris. Ça ne remplace pas le Test Runner : à lancer dans Unity avant toute revue. Les tests PlayMode n'ont pas tourné.

| Test | Mode | Couvre | Résultat |
|---|---|---|---|
| `NetTrajectoryTests` (5 tests) | EditMode | Parabole pour une vitesse et une gravité données ; portée à 45° ; point et temps d'atterrissage ; vol sans impact | ⬜ |
| `NetCycleTests` (9 tests) | EditMode | Lancer refusé en vol, pendant le gel et pendant le retour, accepté une fois le filet revenu ; résultat montré à la fin du gel ; retour avec ou sans prise ; reset | ⬜ |
| `NetImpactTests` (7 tests) | EditMode | Aucune cible, une seule, plusieurs (la plus proche gagne), cible pile sur le bord, distance au sol, liste dans un rayon | ⬜ |
| `NetCaptureRatesTests` (7 méthodes) | EditMode | Les trois profils, le bonus de surprise, plafond à 100 %, tirage déterministe avec générateur fixé | ⬜ |
| `NetChanceColorsTests` (6 tests) | EditMode | Rouge à 0 % (et pour une chance inconnue), orange dès une chance infime, jaune à 50 %, vert à 100 %, le rouge diminue quand la chance monte | ⬜ |
| `NetLandingResolverTests` (6 tests) | EditMode | Un lancer à côté fait fuir les champignons à moins de 3 m ; une touche ne vise que le plus proche et n'en effraie aucun ; profil et surprise de la cible ; même graine, mêmes résultats ; aperçu sans tirage | ⬜ |
| `MushroomCaptureResolveAttemptTests` (5 tests) | EditMode | Une réussite passe à Captured et déclenche `Captured` une seule fois ; un échec passe à Flee ; un champignon en Idle ou en fuite est accepté ; un champignon capturé ne bouge plus | ⬜ |
| `NetThrowScenePlayModeTests.SceneLoad_BuildsThePlayerTheNetAndThreeMushrooms` | PlayMode | La scène se charge avec le joueur, le filet en main et 3 champignons | ⬜ |
| `NetThrowScenePlayModeTests.ANetDroppedOnAMushroom_LandsOnIt_AndChangesItsState` | PlayMode | Un lancer déclenché par code atterrit sur un champignon et son état change (Captured ou Flee) | ⬜ |

Les tests existants de `MushroomCaptureStateMachineTests` et `PlayerMovementDemoPlayModeTests` ne sont pas modifiés. Ils doivent rester verts.

### Manuels (dans Unity)

Scène : `Assets/Scenes/Prototype/NetThrowScene.unity`, puis **Play** et un clic dans la Game View pour capturer la souris.

| # | Étapes | Résultat attendu | OK |
|---|---|---|---|
| 1 | Ouvrir la scène, sans lancer Play | Hierarchy : `Main Camera`, `Directional Light`, `Global Volume`, `Net Throw Prototype`. Sur ce dernier, `NetThrowDemo` a `Player Prefab` = `Player`. Aucun « Missing script » | ⬜ |
| 2 | Play | Vue à la première personne, sol vert, 4 blocs, 3 champignons devant (rouge à gauche, terne et penché au centre, violet plus gros au fond). Disque du filet en bas à droite. Aucune erreur rouge dans la Console | ⬜ |
| 3 | Bouger la souris, puis ZQSD / WASD | La vue tourne, sans dépasser la verticale. Avancer va vers le regard. Shift sprinte, C accroupit (la vue descend), Espace fait un dodge | ⬜ |
| 4 | Regarder le sol devant soi | Arc en pointillés blancs et cercle **gris** au point d'impact. Le cercle suit le regard | ⬜ |
| 5 | Viser le champignon rouge sans s'approcher (il reste gris) | Cercle **jaune tirant sur le vert** (60 %). En s'approchant à moins de 3,5 m, le champignon passe au jaune et le cercle à un **jaune orangé** (40 %) | ⬜ |
| 6 | Viser le champignon violet, de loin puis de près | Jamais rouge : **orange jaune** s'il est calme (30 %), **orange** franc en alerte (10 %) | ⬜ |
| 7 | Viser le champignon terne de loin | Cercle du **vert le plus vert** (100 %). Même champignon en alerte : vert un peu moins franc (80 %) | ⬜ |
| 8 | Lancer (clic gauche) sur le champignon terne | Un fil relie la main au filet pendant tout le vol. Le filet vole en arc et se pose sur lui. Filet et champignon se figent environ 0,5 s, champignon plus clair. On peut bouger pendant ce temps | ⬜ |
| 9 | Suite de #8 | Le filet se referme en boule et revient en 1 s, le fil raccourcit avec lui. Le champignon rétrécit dedans et disparaît. Le disque réapparaît en bas à droite et le fil disparaît | ⬜ |
| 10 | Cliquer plusieurs fois pendant le vol et le retour | Aucun second filet ne part avant que le disque soit revenu | ⬜ |
| 11 | R, puis lancer plusieurs fois sur le champignon violet, R entre chaque essai | La plupart du temps, échec : le filet revient vide et le champignon devient rouge et s'éloigne du point d'impact. Il se calme (gris) environ 6 m plus loin | ⬜ |
| 12 | R, puis lancer à environ 2 m du champignon rouge, sans le toucher | Cercle gris avant le lancer. Le filet revient vide tout de suite. Le champignon fuit | ⬜ |
| 13 | R, puis lancer à plus de 3 m de tout champignon | Le filet revient vide. Aucun champignon ne fuit à cause du filet | ⬜ |
| 14 | Approcher un champignon jusqu'à ce qu'il fuie, puis lancer sur lui pendant sa fuite | La tentative a lieu : gel, puis capture ou fuite | ⬜ |
| 15 | Lancer vers le ciel | Le filet retombe au sol et revient | ⬜ |
| 16 | Regarder vers le bas et lancer à ses pieds | Le filet ne s'arrête pas sur le joueur, il tombe au sol devant | ⬜ |
| 17 | R | Filet en main, 3 champignons à leur place et gris, joueur au départ, vue droite devant | ⬜ |
| 18 | Ouvrir `MouvementJoueurScene`, Play, F1, se déplacer | Le mouvement en vue de dessus marche comme avant (le lacet de `PlayerMotor` reste à 0) | ⬜ |
| 19 | Ouvrir `CaptureChampignonScene`, Play, approcher et capturer avec E | Le prototype de capture marche comme avant | ⬜ |
| 20 | Console pendant tout le test | Aucune erreur rouge. Noter tout avertissement de Netcode lié au prefab joueur utilisé hors ligne | ⬜ |

## Limitations connues

- **Rien n'a été compilé ni exécuté.** La branche a été écrite dans un conteneur sans Unity, et la CI ne lance pas Unity (ADR-0004). La scène et les `.meta` ont été écrits à la main : la première ouverture dans l'Editor est le vrai premier test.
- **Code jetable :** la table de taux et le tirage seront remplacés par le `TryCapture` du CaptureSystem. `ResolveCaptureAttempt` est dans le domaine de Pol-Mattis : à coordonner au merge.
- **Prefab réseau hors ligne :** utiliser `Player` sans `NetworkManager` n'a pas été vérifié. Netcode peut logger des avertissements.
- **Caméra FPS et `PlayerMotor` :** le corps tourne toujours vers la direction de déplacement, et un dodge sans direction part vers l'avant du corps, pas vers le regard. À vérifier en jeu.
- **SFD à mettre à jour** (note de Hugo) : RG_CR_04 (durabilité) et RG_CA_02 (taux unique).
- Placeholders partout : `Shader.Find` sur les shaders URP, valable dans l'Editor, pas garanti dans un build.

## Modifié par
