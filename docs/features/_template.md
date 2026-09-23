# <Nom de la feature>

<!-- Copier ce fichier en docs/features/<suffixe-de-la-branche>.md
     Branche feat/12487v29zj3-mouvement-ia  →  docs/features/12487v29zj3-mouvement-ia.md
     Pas d'accent ni d'espace dans le nom du fichier. -->

- **Ticket ClickUp :** <lien>
- **Note de design validée :** <lien vers la sous-tâche de validation fermée>
- **Branche :** `feat/<id>-<description>`
- **Auteur :** <dev>
- **Statut :** En cours · Terminé

## Docs précédents

<!-- OBLIGATOIRE. Avant d'écrire une ligne de code, cherche dans docs/features/
     tout ce qui concerne cette feature (grep sur le nom, le système, la scène).
     Liste chaque doc que cette branche étend ou modifie, en lien relatif :
       - [12487v29zj3-mouvement-ia](./12487v29zj3-mouvement-ia.md) — ce qui change
     Et ajoute dans CHAQUE doc listé une ligne dans sa section « Modifié par ».
     La CI vérifie les deux sens.
     Si rien n'existe, écris exactement : Aucun. -->

Aucun.

## Ce que fait la feature

<!-- Liste complète, du point de vue du joueur. Une ligne par comportement.
     C'est cette liste que les tests ci-dessous doivent couvrir. -->

- [ ] ...

## Hors périmètre

<!-- Ce que la feature ne fait volontairement pas. -->

## Implémentation

<!-- Scènes, prefabs, scripts, ScriptableObjects, assets touchés.
     Assez pour qu'un autre dev s'y retrouve, pas plus. -->

## Décisions

<!-- Les choix d'architecture pris pendant la feature, et pourquoi.
     Remplace les ADR : une ligne ou deux par décision suffit.
       - Mouvement en CharacterController plutôt que Rigidbody : pas de physique
         réaliste voulue, et collisions plus prévisibles. -->

## Tests

### Automatisés (Unity Test Runner)

| Test | Mode | Couvre | Résultat |
|---|---|---|---|
| `NomDuTest` | EditMode / PlayMode | ligne de « Ce que fait la feature » | ✅ / ❌ |

### Manuels (dans Unity)

<!-- Tout ce qu'un test automatisé ne vérifie pas. Des étapes qu'un autre dev
     peut suivre sans te demander : scène à ouvrir, quoi faire, quoi observer.
     Le reviewer rejoue cette liste et le coche dans la checklist de la PR
     (pas ici : un commit après l'approbation la ferait sauter). Reporter aussi les étapes dans
     docs/playbook-tests.md si elles doivent être revérifiées à chaque release. -->

| # | Étapes | Résultat attendu | OK |
|---|---|---|---|
| 1 | Ouvrir `Assets/Scenes/...`, Play, ... | ... | ✅ |

## Limitations connues

## Modifié par

<!-- Rempli par les branches suivantes qui modifient cette feature.
       - [<id>-<description>](./<id>-<description>.md) — ce qui a changé -->
