# ADR-0006 — Réseau : Netcode for GameObjects, et autorité propriétaire sur le mouvement du joueur

## Statut

Proposé

Date : 2026-09-22

> La technologie réseau est déjà désignée par les Spécifications Techniques
> (STD V2, juillet 2026) : Netcode for GameObjects, Unity Relay, Lobby et
> Authentication. Comme [ADR-0002](./0002-pipeline-de-rendu-urp.md) et
> [ADR-0003](./0003-architecture-des-entrees-input-system.md), cet ADR la
> rapatrie dans le dépôt au moment où le package y entre.
>
> Il tranche en plus une question que la STD laisse ouverte : qui fait autorité
> sur la position du joueur. Le relecteur de la PR bascule le statut en
> « Accepté ».

## Contexte

La sous-tâche ClickUp 12487v2b0jr demande d'ajouter la synchronisation réseau
(`NetworkTransform`) au mouvement du joueur. Aucun package réseau n'était
installé dans le projet : la tâche impose donc d'en adopter un, ce qui engage
toute la suite du développement multijoueur.

Synchroniser une position ne dit pas qui la décide. Les sources du projet ne
tranchent pas explicitement pour le mouvement :

- la STD §2 pose « autorité sur le Host (toute logique critique validée côté
  Host) » ;
- la STD §4.2 synchronise les états globaux en `ServerAuth` et prévoit
  « l'interpolation côté client (NetworkTransform) » ;
- la SFD RG_MU_04 énumère la logique critique : « captures, ressources, état de
  la base ». **Le mouvement n'y figure pas.**

Le prototype de mouvement vient d'être validé sur ses sensations de contrôle.
Le choix d'autorité conditionne directement la conservation de ces sensations
en réseau.

## Hypothèses

- Le jeu est coopératif, de 1 à 4 joueurs. Le PvP est hors périmètre (GDD §16).
  Un joueur qui tricherait sur sa position ne lèserait que la partie qu'il
  partage avec ses coéquipiers.
- Unity Relay ajoute de 20 à 50 ms de latence (STD §4.1), pour une cible
  inférieure à 150 ms au MVP et à 100 ms en V1.0 (STD §8.1).
- Aucune règle critique ne dépend aujourd'hui de la position exacte d'un joueur.
  Si une capture ou une récompense venait à en dépendre, cette règle devrait être
  validée côté Host à ce moment-là.
- Si un mode compétitif apparaissait, l'autorité propriétaire serait à revoir.

## Contraintes

- Unity **6000.3.21f1**, verrouillé par `ProjectSettings/ProjectVersion.txt`.
  Netcode for GameObjects 3.0.0 exige Unity 6000.7 : il est exclu.
- Plateforme cible : PC Windows 64 bits (STD §9).
- Le mouvement repose sur un `CharacterController` piloté par l'Input System
  ([ADR-0003](./0003-architecture-des-entrees-input-system.md)) et simulé
  localement.
- `Resources.Load` n'est pas une architecture de chargement autorisée : le prefab
  du joueur doit être référencé par sérialisation.
- La CI ne lance pas Unity ([ADR-0004](./0004-verification-unity-hors-ci.md)).

## Alternatives envisagées

### Alternative 1 — Netcode for GameObjects, mouvement à autorité propriétaire

- Avantages : aucune latence ressentie sur ses propres déplacements ; aucun code
  de prédiction ni de réconciliation à écrire ; pris en charge nativement par
  `NetworkTransform` (`AuthorityMode = Owner`) ; conforme à la lettre de
  RG_MU_04, qui ne classe pas le mouvement comme logique critique.
- Inconvénients : un client modifié peut annoncer une position fausse ; le Host
  ne valide pas les collisions des autres joueurs.
- Risques : qu'une règle critique finisse par dépendre de la position sans que
  sa validation soit déplacée côté Host.

### Alternative 2 — Netcode for GameObjects, mouvement à autorité serveur

- Avantages : lecture stricte de la STD §2 ; résistance à la triche.
- Inconvénients : chaque déplacement subit l'aller-retour vers le Host, Relay
  compris, sauf à écrire une prédiction côté client avec réconciliation —
  coûteuse à développer et à tester.
- Risques : dégrader la sensation de contrôle que le prototype vient de valider.

### Alternative 3 — Une autre technologie réseau

Netcode for Entities, Mirror, Fish-Net ou Photon.

- Avantages : Netcode for Entities apporte la prédiction côté client ; Photon
  fournit un hébergement géré.
- Inconvénients : Netcode for Entities impose DOTS/ECS, incompatible avec
  l'architecture GameObject de tout le projet. Mirror, Fish-Net et Photon sont
  tiers et sortent de la pile retenue par la STD (Relay, Lobby, Authentication).
- Risques : s'écarter de la STD sans la remplacer.

### Alternative 4 — Ne rien faire

- Inconvénients : la coopération de 1 à 4 joueurs, cœur du produit, reste
  impossible.

## Décision

1. Le projet adopte **Netcode for GameObjects 2.13.3**
   (`com.unity.netcode.gameobjects`) avec **Unity Transport**.
2. **La position du joueur est à autorité propriétaire.** Chaque client simule
   son propre joueur ; les `NetworkTransform` du joueur sont en
   `AuthorityMode = Owner` ; les autres copies ne sont pilotées que par la
   réplication.
3. La logique critique énumérée par RG_MU_04 — captures, ressources, état de la
   base — **reste validée côté Host**. La décision 2 ne concerne que le
   mouvement.

## Justification

La technologie n'est pas rouverte : la STD l'a choisie, et les alternatives
tierces s'écarteraient de la pile Relay/Lobby/Authentication qu'elle retient.
La version 2.13.3 est la plus récente compatible avec la version d'Unity
verrouillée.

Pour l'autorité, la contrainte décisive est la sensation de contrôle. En
coopération sans PvP, le risque de triche est faible et ne lèse personne
d'extérieur à la partie. En face, l'autorité serveur ajouterait la latence de
Relay à chaque pas, ou exigerait une prédiction client dont le coût est sans
commune mesure avec le bénéfice pour ce jeu. La SFD RG_MU_04 ne range pas le
mouvement dans la logique critique : l'autorité propriétaire respecte donc la
lettre de la spécification, tandis que la décision 3 en préserve l'esprit.

## Dépendances

- **`com.unity.netcode.gameobjects` 2.13.3**
  - Nécessaire : Unity n'offre aucune réplication réseau intégrée.
  - Existant insuffisant : aucun package réseau n'était installé.
  - Maintenance : package officiel Unity, version stable (étiquette `latest` du
    registre au 22/09/2026).
  - Compatibilité Unity : Unity 6000.0 et suivants.
  - Compatibilité plateformes : Windows 64 bits.
  - Licence : Unity Companion License.
- **`com.unity.transport`**, dépendance transitive. Minimum requis 2.6.0 ; Unity
  l'a résolue en 2.7.4. Licence : Unity Companion License.
- **`com.unity.nuget.mono-cecil`**, dépendance transitive, déjà présente dans le
  projet en version supérieure au minimum requis.

## Conséquences

- **Positives :** le joueur conserve des déplacements sans latence en réseau ;
  aucun code de prédiction à maintenir ; la voie vers Relay, Lobby et
  Authentication reste celle de la STD.
- **Négatives :**
  - Le joueur n'est plus construit au runtime : il devient un prefab,
    `Assets/Prefabs/Characters/Player.prefab`, seul moyen pour Netcode de
    l'instancier chez tous les pairs. Son nommage suit
    [ADR-0005](./0005-pipeline-et-nommage-des-assets-3d.md).
  - Netcode génère et maintient `Assets/DefaultNetworkPrefabs.asset`, la liste
    des prefabs réseau, versionnée avec le projet.
  - Un client modifié peut mentir sur sa position.
- **Dette technique acceptée :**
  - L'accroupi est répliqué visuellement, par un `NetworkTransform` sur le
    maillage, mais la capsule de collision des copies distantes reste à hauteur
    debout.
  - Le prototype se connecte directement par Unity Transport, sur la machine
    locale. Relay, Lobby et Authentication restent à brancher.
  - La vérification à plusieurs joueurs est manuelle : aucun test automatisé ne
    fait tourner deux pairs.
- **Impact migration :** `NetworkManager` se place de lui-même en
  `DontDestroyOnLoad` et survit aux rechargements de scène ; tout code qui
  recharge une scène doit en tenir compte.
- **Impact workflow éditeur :** tester à plusieurs demande un build lancé à côté
  de l'éditeur, ou le package Multiplayer Play Mode, qui n'est pas adopté ici.
- **Impact runtime :** chaque joueur réplique sa position, sa rotation autour de
  l'axe vertical, ainsi que la hauteur et l'échelle verticale de son maillage.

## Références

- ClickUp : [12487v2b0jr — Ajouter NetworkTransform multijoueur au mouvement du
  joueur](https://app.clickup.com/t/12487v2b0jr), sous-tâche de
  [86c9mnqxm — Mouvement joueur](https://app.clickup.com/t/86c9mnqxm).
- Spécifications Techniques (STD) V2 : §2, §3, §4.1, §4.2, §8.1, §9.
- Spécifications Fonctionnelles (SFD) V2 : RG_MU_01, RG_MU_04.
- GDD V2 : §11 (multijoueur coopératif), §16 (hors périmètre).
- [ADR-0003](./0003-architecture-des-entrees-input-system.md) — Input System.
- [ADR-0004](./0004-verification-unity-hors-ci.md) — vérification Unity hors CI.
- À venir : un ADR sur l'architecture de déplacement (`CharacterController`
  contre `Rigidbody`), que cet ADR ne tranche pas.
