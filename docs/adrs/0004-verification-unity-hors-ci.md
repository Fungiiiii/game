# ADR-0004 — La vérification Unity se fait sur les machines des développeurs, pas en CI

## Statut

Proposé

Date : 2026-09-14

## Contexte

Le projet Unity vient d'entrer dans le dépôt. Le workflow
[`.github/workflows/unity-tests.yml`](../../.github/workflows/unity-tests.yml)
devait exécuter les tests EditMode et PlayMode à chaque *pull request*, et
`CLAUDE.md` désignait la CI comme autorité sur « le projet compile » et « les
tests passent ».

La PR d'initialisation est la première chose sur laquelle ce workflow ait
jamais tourné. Il a échoué deux fois, et aucun des deux échecs ne venait du
code :

1. **Manque d'espace disque.** L'image `unityci/editor:ubuntu-6000.3.21f1-linux-il2cpp-3`
   pèse 5,6 Go compressée, bien davantage une fois ses couches extraites,
   contre environ 14 Go libres sur un runner GitHub. Le pull mourait sur
   `failed to register layer: no space left on device`. Corrigé en supprimant
   les chaînes d'outils préinstallées — 39 Go libres ensuite, et le pull
   aboutit.
2. **Aucune licence.** Le run atteint alors l'activation et s'arrête sur
   `Licensing method: <none>`.

Le second point ne se corrige pas dans le dépôt : il exige une licence Unity
dans les secrets GitHub. Il faut donc trancher maintenant, parce que la PR
d'initialisation ne peut pas être fusionnée avec une CI rouge, et parce que la
réponse détermine ce que `CLAUDE.md` a le droit d'affirmer sur la vérification.

## Hypothèses

- Chaque développeur qui touche au code a Unity `6000.3.21f1` installé. C'est
  déjà nécessaire pour travailler sur le projet : l'hypothèse ne coûte rien.
- L'équipe travaille sur des branches de ce dépôt, pas sur des forks. GitHub
  ne transmet aucun secret à une PR issue d'un fork, et ne transmet pas
  davantage un runner self-hosted sans risque.
- Une personne désignée produit les builds de livrable sur sa machine.
- L'équipe reste petite. Trois comptes sont actifs sur l'organisation GitHub
  aujourd'hui.

Si l'équipe grossit, se distribue, ou accueille des contributeurs sans
éditeur, cette décision doit être rouverte.

## Contraintes

- Moteur : Unity 6.3 LTS, patch `6000.3.21f1`. Plateforme V1.0 : PC Windows
  64-bit.
- L'organisation GitHub `Fungiiiii` est sur le plan **Free** : 2 000 minutes
  Actions par mois, et le dépôt est **privé**, donc ces minutes sont
  décomptées.
- Mesuré sur les runs réels : 2 min 15 s pour le seul téléchargement de
  l'image, dans un job qui n'a jamais démarré l'éditeur. Un run complet, import
  du projet compris, se situerait plutôt entre 6 et 12 minutes par job, et il y
  a deux jobs par push.
- Un secret GitHub est lisible par quiconque peut pousser un workflow sur le
  dépôt : le masquage des logs se contourne trivialement.
- Les CGU d'Unity considèrent un Unity ID comme personnel. Un compte partagé
  d'équipe est la pratique courante en CI, mais n'est pas formellement prévu
  par la licence Personal.
- `.github/workflows/conventions.yml` tourne déjà, en quelques secondes et
  sans licence.

## Alternatives envisagées

### Alternative 1 — Compte Unity dédié, identifiants dans les secrets

Créer un compte Unity pour le projet, siège Personal, et poser
`UNITY_EMAIL` / `UNITY_PASSWORD` en secrets d'organisation.

- Avantages : c'est la voie standard de l'écosystème game-ci ; vérification
  réellement indépendante des machines de l'équipe ; aucune infrastructure à
  héberger.
- Inconvénients : des identifiants vivants dans GitHub ; l'activation dépend
  des serveurs de licence Unity à chaque run ; une rotation de mot de passe
  casse la CI sans prévenir ; boucle de retour d'environ dix minutes ; le
  quota de 2 000 minutes part vite à deux jobs par push.
- Risques : fuite du compte, dont le rayon d'explosion dépasse le projet ; CI
  rouge pour des motifs de licence, ce qui apprend à l'équipe à ignorer le
  rouge ; quota épuisé au pire moment, c'est-à-dire près du rendu.

### Alternative 2 — Runner self-hosted pré-activé

Une machine de l'équipe, Unity installé et activé une fois à la main.

- Avantages : aucun identifiant dans GitHub ; minutes gratuites ; plus de pull
  de 5,6 Go ; `Library/` reste chaud, donc des runs bien plus rapides.
- Inconvénients : une machine à héberger, maintenir, mettre à jour et
  sécuriser ; personne n'est aujourd'hui désigné pour l'administrer.
- Risques : point de défaillance unique — machine éteinte, CI morte ; un
  runner self-hosted exécute le code des PR, ce qui suppose de faire confiance
  à tout ce qui est poussé.

### Alternative 3 — Ne pas exécuter Unity en CI (retenue)

La CI garde ce qui est rapide, gratuit et sans licence. La vérification Unity
revient aux machines des développeurs.

- Avantages : aucun identifiant Unity dans GitHub ; aucune minute Actions
  consommée par Unity ; aucune PR bloquée par un problème d'infrastructure ;
  la boucle de retour redevient celle de l'éditeur, quelques secondes.
- Inconvénients : plus aucune vérification indépendante ; « ça compile »
  repose sur la discipline humaine ; un environnement sans éditeur — agent,
  conteneur — ne peut plus rien affirmer et doit le dire explicitement.
- Risques : une régression fusionnée parce que l'auteur n'a pas lancé les
  tests et a coché la case quand même.

## Décision

**La CI n'exécute pas Unity.**

1. `.github/workflows/conventions.yml` est conservé tel quel : il est rapide,
   gratuit, ne demande aucune licence, et reste le point d'application des
   conventions du dépôt.
2. `.github/workflows/unity-tests.yml` est conservé mais passe en
   **déclenchement manuel uniquement** (`workflow_dispatch`). Il ne s'exécute
   plus sur les *pull requests* et ne bloque donc rien. Il reste utilisable le
   jour où une licence existe, avec le correctif d'espace disque et le contrôle
   de licence déjà en place.
3. **Aucune licence, aucun identifiant Unity n'est stocké dans les secrets
   GitHub.**
4. La vérification Unity — compilation, EditMode, PlayMode — se fait sur la
   machine du développeur, avant d'ouvrir la PR. La commande *headless* est
   documentée dans le skill `unity-testing`.
5. L'auteur d'une PR déclare dans la section *Testing* ce qu'il a réellement
   lancé, et sur quelle machine. Ce qui n'a pas été lancé est dit.
6. Les builds de livrable sont produits par une personne désignée, sur sa
   machine.

## Justification

Les deux échecs rencontrés venaient de l'infrastructure, jamais du code. Une
CI qui échoue pour des motifs d'infrastructure n'enseigne qu'une chose :
ignorer le rouge. C'est exactement ce qu'une petite équipe ne peut pas se
permettre, parce qu'elle n'a personne dont le métier est de maintenir la CI.

Ce que l'alternative 1 achète — la certitude que le projet compile ailleurs
que sur la machine de son auteur — l'équipe l'obtient déjà en ouvrant
l'éditeur, puisque tout le monde en a un. Le prix est disproportionné : des
identifiants vivants dans un système où quiconque peut pousser un workflow
peut les lire, et un quota qui s'épuise près du rendu.

L'alternative 2 est techniquement la meilleure et reste la bonne réponse le
jour où une machine est disponible. Elle suppose un administrateur que
l'équipe n'a pas aujourd'hui.

La décision est peu coûteuse à inverser : le workflow reste dans le dépôt,
fonctionnel. Rouvrir cet ADR suffit.

## Dépendances

Aucune ajoutée. La décision retire au contraire `game-ci/unity-test-runner` du
chemin critique : l'action reste référencée par le workflow manuel, mais plus
aucune PR n'en dépend.

## Conséquences

- **Positives :** aucun identifiant Unity dans GitHub ; zéro minute Actions
  consommée par Unity ; plus aucune PR bloquée par la licence ou par l'espace
  disque d'un runner ; boucle de retour ramenée à celle de l'éditeur.
- **Négatives :** aucune vérification indépendante de la compilation et des
  tests ; un agent ou un environnement sans éditeur ne peut affirmer ni que le
  projet compile ni que les tests passent, et doit le déclarer ; une régression
  peut être fusionnée si l'auteur ne lance pas les tests.
- **Dette technique acceptée :** la Definition of Done repose sur une
  déclaration humaine que rien ne vérifie. Cocher « les tests passent » sans
  les avoir lancés ne déclenche aucune alarme. C'est le coût assumé de cette
  décision, et la raison pour laquelle la section *Testing* d'une PR doit dire
  où les tests ont tourné.
- **Impact migration :** `unity-tests.yml` passe en `workflow_dispatch` ;
  `CLAUDE.md`, [`docs/unity-init.md`](../unity-init.md) et le skill
  `unity-testing` cessent de désigner la CI comme autorité de vérification.
- **Impact workflow éditeur :** avant d'ouvrir une PR, lancer EditMode et
  PlayMode localement, et rapporter le résultat observé.
- **Impact runtime :** aucun.

## Références

- PR [#2](https://github.com/Fungiiiii/game/pull/2) — création du projet Unity.
- Runs ayant motivé la décision : `34844082130` (espace disque),
  `34845543779` (licence), `34846127350` (contrôle de licence).
- [`.github/workflows/unity-tests.yml`](../../.github/workflows/unity-tests.yml)
  et [`.github/workflows/conventions.yml`](../../.github/workflows/conventions.yml)
- [`docs/unity-init.md`](../unity-init.md), section « CI »
- `CLAUDE.md`, section « Honesty about verification »
- `.claude/skills/unity-testing/SKILL.md`
- Stratégies d'activation game-ci : https://game.ci/docs/github/activation
- [ADR-0002](./0002-pipeline-de-rendu-urp.md) et
  [ADR-0003](./0003-architecture-des-entrees-input-system.md), introduits par
  la même PR.
