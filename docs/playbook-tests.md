# Playbook de tests

Ce qu'on vérifie **à la main dans Unity**, parce qu'aucun test automatisé ne le
couvre. La CI ne lance pas Unity : ces vérifications et le Test Runner sont
les seules preuves que le jeu marche.

- Le **dev** le déroule sur sa branche après `git rebase develop`, avant de push.
- Le **reviewer** le déroule sur la branche de la PR, avant d'approuver.

## 1. À chaque PR — tronc commun (5 min)

| # | Étapes | Attendu |
|---|---|---|
| 1 | Ouvrir le projet dans Unity après le rebase | Aucune erreur rouge dans la Console |
| 2 | Window → General → Test Runner → EditMode → Run All | Tout vert |
| 3 | Test Runner → PlayMode → Run All | Tout vert |
| 4 | Ouvrir **la scène prototype de la feature** (`Assets/Scenes/Prototype/…`), Play, l'utiliser 1 minute | Pas d'erreur Console, pas de freeze |
| 5 | Si la PR touche du code partagé (`Core/`, `Player/`, input…) : ouvrir aussi les scènes prototype des features qui s'en servent, Play | Elles marchent toujours |
| 6 | Hierarchy / prefabs touchés par la PR | Aucun « Missing script » ni référence `None` inattendue |

Puis : **les tests manuels de la doc de la feature**
(`docs/features/<branche>.md`, section Tests → Manuels), en entier.

## 2. Avant une release sur `main` — régression complète

Le lead dev lance le Test Runner, puis rejoue les tests manuels de chaque
feature listée ci-dessous, chacune dans sa scène prototype. Quand une feature doit être revérifiée à chaque release, on
l'ajoute ici avec un lien vers sa doc.

| Feature | Doc | Tests à rejouer |
|---|---|---|
| Champimaison | [86c9mnqxa-champimaison](./features/86c9mnqxa-champimaison.md) | #1 à #5 |

## Test automatique ou manuel ?

Automatiser tout ce qui est de la **logique** : calculs, règles, états,
inventaire, dégâts — EditMode, rapide, sans scène. PlayMode quand il faut la
physique ou le cycle de vie Unity.

Garder en manuel ce qui est du **ressenti** : feeling des contrôles, caméra,
animations, lisibilité du HUD, rendu visuel. Un test manuel doit rester
rejouable par quelqu'un d'autre : scène, actions, résultat attendu.
