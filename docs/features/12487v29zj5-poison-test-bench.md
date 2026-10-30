# Banc d'essai développeur du poison

- **Ticket ClickUp :** [12487v29zj5 — Prototype poison](https://app.clickup.com/t/12487v29zj5)
- **Note de design :** [révision du 23/09/2026 dans le ticket parent](https://app.clickup.com/t/12487v29zj5) ; [validation](https://app.clickup.com/t/12487v2bcf8) au statut « validé sprint », mais non fermée au moment de l'implémentation
- **Branche :** `feat/12487v29zj5-poison-test-bench`
- **Statut :** En revue — scène et contrôles visuels vérifiés, vérifications manuelles restantes ci-dessous

## Docs précédents

Aucun.

Le modèle unifié provient de la branche `refactor/12487v29zj5-unify-poison-model` et de sa [PR #22](https://github.com/Fungiiiii/game/pull/22), qui ne requiert pas de doc de feature selon `docs/features/README.md`.

## Ce que fait la feature

Cette scène est un **outil de développeur**, non une interface joueur de production.

- La scène `PoisonScene` montre les jauges du HUD existant et des diagnostics de santé, d'intensité, de dégâts par seconde et d'état.
- Les boutons appliquent ou retirent 10 % de poison, fixent les seuils 49/50/75/100 %, ou retirent le poison sans soigner.
- La pause arrête les dégâts ; le guide suspend temporairement la simulation et restaure l'état de pause précédent.
- À zéro PV, un écran de mort propose un redémarrage ; la remise à zéro restaure PV/endurance, retire le poison et reprend la simulation.
- P/O/R/Espace offrent les raccourcis du banc d'essai après mise au point de la Game View.

## Hors périmètre

Écran joueur de production, intégration dans `TheClearing`, réseau, VFX/audio, réglages d'équilibrage, sauvegarde et exposition de cet outil dans un build de sortie.

## Implémentation

- `Assets/Scenes/Prototype/PoisonScene.unity` : scène dédiée issue du prototype antérieur, avec GUIDs conservés pour la scène et le script pilote.
- `PoisonTestBenchController` : adapter de démonstration autour d'un seul `PlayerVitals` ; il décide si `Tick` est appelé et gère les commandes de l'outil. Aucun second modèle de santé ou de poison.
- `PoisonPrototypeView` : vue et contrôles limités à l'éditeur / aux Development Builds ; elle réutilise `VitalsHud` pour les jauges du joueur et lit les diagnostics du modèle.

## Décisions

- Le banc d'essai est une PR séparée de l'unification métier afin que la refonte du modèle reste vérifiable sans UI de démonstration.
- La pause vit dans le pilote de scène : `PlayerVitals` ne reçoit pas de règle de jeu « pause ».
- Les raccourcis sont dispatchés par une méthode déterministe testable en headless. Le câblage vers un vrai clavier reste à contrôler manuellement dans l'éditeur, car l'ancien test à clavier virtuel était instable en headless.

## Tests

### Automatisés (Unity Test Runner)

| Test | Mode | Couvre | Résultat |
|---|---|---|---|
| `PoisonTestBenchTests.ShortcutDispatcherAppliesRemovesPausesAndResets` | PlayMode | raccourcis, guide, mort et reset | ✅ |
| `PoisonTestBenchTests.ButtonsDriveAuthoritativeVitalsAndDiagnostics` | PlayMode | boutons, seuils, pause, antidote, HUD | ✅ |
| `PoisonTestBenchTests.DeathOffersRestartAndGuidePreservesPauseState` | PlayMode | mort, reprise, restauration de la pause | ✅ |
| `PoisonTestBenchTests.PoisonSceneBootstrapsTestBenchAndPlayerHud` | PlayMode | chargement de la scène et HUD partagé | ✅ |

### Manuels (dans Unity)

| # | Étapes | Résultat attendu | OK |
|---|---|---|---|
| 1 | Ouvrir `Assets/Scenes/Prototype/PoisonScene.unity`, Play, cliquer dans Game View. | Banc visible, diagnostics lisibles, HUD joueur en haut à gauche ; aucune erreur rouge dans la Console. | ☐ |
| 2 | Cliquer 49 %, attendre une seconde ; puis 50 %, attendre une seconde. | À 49 % la santé reste stable ; à 50 % elle diminue et la jauge poison reste affichée. | ✅ |
| 3 | Cliquer 100 %, observer les dégâts, puis « Clear poison ». | Dégâts rapides à 100 % ; l'antidote arrête les dégâts, masque la jauge et ne soigne pas. | ✅ |
| 4 | Activer Pause, attendre, ouvrir/fermer le guide, reprendre. | Santé figée en pause ; le guide suspend temporairement et restaure correctement la pause précédente. | ✅ |
| 5 | Utiliser P/O/R/Espace après avoir cliqué dans Game View. | Application, retrait, remise à zéro et pause conformes aux boutons. | ☐ |
| 6 | Mettre 100 %, laisser mourir, cliquer sur le redémarrage. | Écran de mort visible ; le redémarrage restaure 100 PV, zéro poison et masque la jauge. | ☐ |

## Limitations connues

Les seuils, l'antidote, la pause et le guide ont été observés dans la Game View ; le fonctionnement des boutons a été confirmé manuellement. La Console, les raccourcis au clavier physique et le redémarrage après mort restent à contrôler avant fusion. La scène est un outil de prototype et ne doit pas être ajoutée à l'interface joueur de production.

## Modifié par
