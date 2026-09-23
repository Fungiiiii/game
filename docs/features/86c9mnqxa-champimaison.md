# Champimaison — plantation et amélioration (prototype)

> **Doc d'exemple.** Écrite après coup pour montrer à quoi ressemble une doc
> remplie ; la feature est antérieure au process. Les tests n'ont pas été
> rejoués en l'écrivant : la colonne résultat attend le prochain dev qui les
> passe (⬜ = pas encore vérifié).

- **Ticket ClickUp :** https://app.clickup.com/t/86c9mnqxa
- **Note de design validée :** aucune — feature livrée avant le process
- **Branche :** `feat/86c9mnqxa-champimaison` (PR #7)
- **Auteur :** —
- **Statut :** Terminé

## Docs précédents

Aucun.

## Ce que fait la feature

Une parcelle où le joueur plante une graine, puis la fait grandir en
Champimaison. Prototype en formes primitives, avec des ressources fictives.

- [x] La parcelle démarre vide, avec **3 spores** et **2 rosées**.
- [x] **Planter une graine** coûte 1 spore et crée une pousse (Tier 0).
- [x] On ne peut planter qu'une fois.
- [x] **Améliorer en Tier 1** coûte 2 spores + 1 rosée, uniquement depuis le Tier 0.
- [x] Améliorer avant d'avoir planté est refusé, et ne dépense rien.
- [x] Un bouton est grisé quand son action est impossible.
- [x] Le bâtiment 3D change à chaque étape : graine → champignon → maison.
- [x] L'interface affiche le tier, le statut, les ressources, le prochain coût
      et une indication de ce qu'il faut faire.
- [x] **Reset** remet la parcelle et les ressources à zéro.

## Hors périmètre

- Vrais modèles 3D (tout est en primitives colorées).
- Économie réelle : spores et rosée sont fictives, rien n'est sauvegardé.
- Tiers au-delà du Tier 1, plusieurs parcelles, temps de pousse.

## Implémentation

- `Assets/Scripts/Runtime/Champimaison/ChampimaisonState.cs` — les règles :
  ressources, coûts, tiers. Classe C# simple, sans Unity, donc testable en
  EditMode.
- `Assets/Scripts/Runtime/Champimaison/ChampimaisonPrototype.cs` —
  `MonoBehaviour` qui construit le décor, le bâtiment et l'interface au
  lancement, et se redessine quand l'état change (événement `Changed`).
- Scène : `Assets/Scenes/Prototype/ChampimaisonScene.unity`. `SampleScene`
  n'est pas touchée.

## Décisions

- **Règles séparées de l'affichage** : `ChampimaisonState` ne dépend pas de
  Unity. Les règles se testent en millisecondes, sans ouvrir de scène.
- **Tout est créé par le code au lancement** (décor, UI) : pas de prefab ni
  d'asset à maintenir pour un prototype jetable.

## Tests

### Automatisés (Unity Test Runner)

Fichier : `Assets/Scripts/Tests/EditMode/ChampimaisonStateTests.cs`

| Test | Mode | Couvre | Résultat |
|---|---|---|---|
| `NewState_StartsWithEmptyPlotAndFictionalResources` | EditMode | Parcelle vide, 3 spores, 2 rosées | ⬜ |
| `PlantSeed_ConsumesOneSporeAndCreatesTier0` | EditMode | Planter coûte 1 spore, une seule fois | ⬜ |
| `UpgradeFromTier0_ConsumesTwoSporesAndOneDew` | EditMode | Tier 1 coûte 2 spores + 1 rosée | ⬜ |
| `UpgradeBeforePlanting_IsRejectedWithoutChangingResources` | EditMode | Amélioration refusée avant plantation | ⬜ |
| `Reset_RestoresTheInitialFlow` | EditMode | Reset | ⬜ |

Pas de test PlayMode : l'affichage est vérifié à la main ci-dessous.

### Manuels (dans Unity)

Scène : `Assets/Scenes/Prototype/ChampimaisonScene.unity`, puis **Play**.

| # | Étapes | Résultat attendu | OK |
|---|---|---|---|
| 1 | Lancer la scène | Graine jaune sur la parcelle · « SPORES 3 · DEW 2 » · bouton Upgrade grisé | ⬜ |
| 2 | Cliquer **PLANT SEED** | Champignon rouge · « SPORES 2 » · Plant grisé, Upgrade actif | ⬜ |
| 3 | Cliquer **UPGRADE TO TIER 1** | Maison avec toit bleu · « SPORES 0 · DEW 1 » · les deux boutons grisés | ⬜ |
| 4 | Cliquer **Reset** | Retour à l'état de l'étape 1 | ⬜ |
| 5 | Regarder la Console pendant tout le test | Aucune erreur rouge | ⬜ |

## Limitations connues

- Les textes de l'interface sont écrits en dur, en anglais, sans passer par la
  localisation — acceptable pour un prototype, à reprendre avant d'intégrer
  la feature au jeu.
- Les coûts et les ressources de départ sont en dur dans le code.

## Modifié par
