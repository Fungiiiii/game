# ADR-0003 — Architecture des entrées : Input System

## Statut

Proposé

Date : 2026-09-14

> Comme pour [ADR-0002](./0002-pipeline-de-rendu-urp.md), la décision est déjà
> actée par l'équipe dans les Spécifications Techniques (STD V2, juillet 2026),
> qui désignent « Unity Input System ». Cet ADR la rapatrie dans le dépôt pour
> qu'elle soit relue avec le code qu'elle contraint. Le relecteur de la PR
> bascule le statut en « Accepté ».
>
> L'argumentaire est reconstruit à partir des contraintes documentées, pas des
> délibérations d'origine.

## Contexte

Le projet Unity est créé maintenant. L'architecture d'entrées conditionne la
façon dont chaque système de gameplay lit les commandes du joueur. Changer de
système après coup impose de réécrire tous les points de lecture d'entrée du
projet, et de refaire les réglages associés.

Deux systèmes coexistent dans Unity 6 : l'ancien Input Manager (`Input.GetKey`,
`Input.GetAxis`) et le package Input System. Ils peuvent cohabiter, ce qui est
précisément le piège : sans décision explicite, les deux finissent utilisés au
hasard des contributions.

## Hypothèses

- Le jeu reste jouable au clavier-souris et à la manette, comme annoncé dans la
  STD.
- La coopération est en ligne ou en LAN ; aucun écran partagé local n'est prévu
  en V1.0, donc pas de gestion multi-joueurs sur une même machine.
- Le remapping des touches par le joueur est attendu à terme, même s'il n'est
  pas au périmètre engagé d'avril 2027.
- L'équipe compte plusieurs contributeurs sur le gameplay, ce qui rend une
  convention explicite plus utile qu'un usage implicite.

## Contraintes

- Moteur : Unity 6.3 LTS, patch `6000.3.21f1`.
- Plateforme V1.0 : PC Windows 64-bit. Steam Deck envisagé après la V1.0, ce
  qui suppose une manette pleinement supportée.
- Périphériques : clavier, souris, manette (STD §3).
- `CLAUDE.md` impose des actions nommées (`Move`, `Interact`, `Jump`) plutôt que
  des tests de touches bruts, pour le remapping, les manettes,
  l'accessibilité et la testabilité.
- `CLAUDE.md` interdit de mélanger deux architectures d'entrée.
- L'analyzer du projet configure `UNT0025` en erreur : les surcharges
  `Input.GetKey` prenant un `KeyCode` cassent déjà le build.

## Alternatives envisagées

### Alternative 1 — Package Input System (retenue)

- Avantages : actions nommées et Action Maps comme unité de base, ce qui est
  exactement ce que `CLAUDE.md` exige ; remapping à l'exécution fourni ;
  détection et changement de périphérique à chaud (clavier vers manette) sans
  code spécifique ; les entrées sont simulables depuis les tests, donc le
  gameplay lié aux entrées devient testable ; l'asset `.inputactions` est un
  fichier versionné et relisible en PR ; c'est le système que Unity maintient.
- Inconvénients : plus de concepts à apprendre (actions, bindings, maps,
  schémas de contrôle, callbacks) ; la configuration initiale est plus longue
  qu'un `Input.GetAxis` ; le mode « Both » qu'Unity propose laisserait la porte
  ouverte à un mélange accidentel s'il était conservé — voir la Décision.
- Risques : contourner les actions en lisant directement
  `Keyboard.current[Key.X]` reproduit les défauts de l'ancien système avec la
  nouvelle API.

### Alternative 2 — Ancien Input Manager

- Avantages : API immédiate, connue de tous ; aucun package ; suffisant pour un
  prototype.
- Inconvénients : axes configurés dans `ProjectSettings/InputManager.asset`,
  difficiles à relire en diff et à fusionner ; pas de remapping à l'exécution
  sans réimplémentation ; support manette fragile et dépendant de la
  plateforme ; pas de simulation d'entrée pour les tests ; Unity ne le fait
  plus évoluer.
- Risques : le remapping et l'accessibilité deviennent un chantier de
  réécriture tardif, au moment où le code de gameplay est le plus étendu.

### Alternative 3 — Les deux, au cas par cas

- Avantages : aucune migration, chacun utilise ce qu'il connaît.
- Inconvénients : deux sources de vérité pour une même entrée ; conflits et
  doubles déclenchements ; explicitement interdit par `CLAUDE.md`.
- Risques : bugs d'entrée non reproductibles, dépendants de l'ordre
  d'initialisation et du périphérique branché.

## Décision

Le projet utilise le package **Input System** (`com.unity.inputsystem`,
version `1.20.0`), et lui seul.

`ProjectSettings/ProjectSettings.asset` est réglé sur
`activeInputHandler: 1`, soit « Input System Package (New) » — l'ancien Input
Manager est désactivé, et non simplement laissé de côté. Un appel à l'ancienne
API lève alors une exception à l'exécution au lieu de fonctionner à moitié.

L'asset `Assets/InputSystem_Actions.inputactions`, livré par le template, est
conservé comme point de départ unique des actions du projet.

Toute lecture d'entrée passe par une action nommée. La lecture directe d'un
périphérique (`Keyboard.current`, `Gamepad.current`) est réservée aux cas où
aucune action n'a de sens — par exemple un écran de rebinding — et se justifie
en revue.

## Justification

Input System est la seule alternative qui satisfait la contrainte déjà écrite
dans `CLAUDE.md` : des actions nommées, remappables, testables. L'ancien
système ne peut y répondre qu'au prix d'une couche maison qui réimplémenterait,
moins bien, ce que le package fournit.

Le surcoût d'apprentissage est réel mais payé une fois, en début de projet,
alors que la dette de l'ancien système croît avec chaque système de gameplay
qui lit une entrée.

Désactiver franchement l'ancien Input Manager plutôt que de laisser le mode
« Both » est ce qui rend la décision effective : un mélange accidentel échoue
bruyamment au lieu de s'installer discrètement.

## Dépendances

`com.unity.inputsystem` version `1.20.0`.

- Pourquoi nécessaire : c'est l'implémentation de la décision.
- Pourquoi l'existant est insuffisant : l'Input Manager intégré n'offre ni
  actions remappables à l'exécution, ni simulation d'entrée pour les tests, ni
  gestion fiable du changement de périphérique à chaud.
- Maintenance : package Unity de première partie, activement maintenu, livré
  avec le template officiel d'Unity 6.3.
- Compatibilité Unity : `1.20.0` est la version livrée avec Unity 6.3 LTS.
- Compatibilité plateformes : Windows 64-bit couvert ; manettes Xbox,
  DualShock et DualSense supportées, ce qui couvre le Steam Deck envisagé
  après la V1.0.

## Conséquences

- **Positives :** les actions sont déclarées dans un asset versionné et non
  dispersées dans le code ; le remapping et l'accessibilité restent atteignables
  sans réécriture ; le gameplay lié aux entrées devient testable, ce que le
  skill `unity-testing` demande ; le support manette est acquis dès maintenant.
- **Négatives :** montée en compétence nécessaire pour toute l'équipe ;
  l'ancien Input Manager étant désactivé, tout tutoriel ou asset tiers reposant
  sur `Input.GetAxis` échouera et devra être adapté.
- **Dette technique acceptée :** l'asset `InputSystem_Actions` est celui du
  template et décrit des actions génériques, pas celles de Fungiiiii!. Il devra
  être remplacé par les actions réelles du jeu quand elles seront définies.
  Aucun schéma de contrôle propre au projet n'est encore établi.
- **Impact migration :** aucun, le projet est créé avec cette décision.
- **Impact workflow éditeur :** les actions se modifient dans l'éditeur
  d'`.inputactions`. Ce fichier est un point de contention entre contributeurs :
  le modifier demande de coordonner, comme une scène.
- **Impact runtime :** l'Input System s'initialise de façon asynchrone au
  démarrage. Aucun système ne doit supposer qu'un périphérique est disponible
  dès `Awake`.

## Références

- Spécifications Techniques (STD), §3 Stack technologique —
  https://fungiiiii.atlassian.net/wiki/spaces/docs/pages/27099148
- `CLAUDE.md`, et `.claude/skills/unity-runtime-code/SKILL.md`, section Input
- `docs/unity-init.md`, §1 « Still open » et §6 Packages
- `.editorconfig` — `UNT0025` en erreur
- [ADR-0002](./0002-pipeline-de-rendu-urp.md) — décision prise au même moment
