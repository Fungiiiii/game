# Docs de features

Une doc par branche qui ajoute ou modifie un comportement du jeu. Elle décrit la
feature **exacte** telle qu'elle a été livrée, et la liste des tests qui le
prouvent. La doc générale du projet (game design, univers, specs) reste sur
[Confluence](https://fungiiiii.atlassian.net/wiki/spaces/docs/overview?homepageId=950451).

Le process complet est dans [`docs/workflow.md`](../workflow.md).

## Nom du fichier

Le suffixe de la branche, sans le préfixe :

| Branche | Doc |
|---|---|
| `feat/12487v29zj3-mouvement-ia` | `docs/features/12487v29zj3-mouvement-ia.md` |
| `fix/12487v2c1ab-ia-traverse-murs` | `docs/features/12487v2c1ab-ia-traverse-murs.md` |

L'identifiant ClickUp retrouve le ticket, la description retrouve la branche.
Pas d'accent, pas d'espace. Partir de [`_template.md`](./_template.md).

**Exemple rempli :** [`86c9mnqxa-champimaison.md`](./86c9mnqxa-champimaison.md).

## Quand une doc est obligatoire

| Branche | Doc |
|---|---|
| `feat/` | **Toujours.** La CI bloque la PR sinon. |
| `fix/` | **Si le comportement visible change** (la doc d'origine devient fausse sans elle) : nouvelle doc qui cite l'ancienne. Si le fix rétablit simplement ce que la doc décrivait déjà : pas de doc, mais un test de régression et la doc d'origine citée dans la PR. |
| `refactor/` `test/` `chore/` `docs/` | Non. Un refactor ne change pas de comportement — s'il en change un, c'est un `feat/` ou un `fix/`. |
| Assets 3D (`chore/asset-…`) | Non : template de PR asset (capture d'écran). |

## Modifier une feature existante

On ne réécrit pas l'ancienne doc : elle décrit ce qui a été livré à l'époque.

1. **Lire les docs existantes avant de coder.** Chercher dans `docs/features/`
   le nom de la feature, du système, de la scène. Lire aussi leur section
   « Modifié par » : la version actuelle est au bout de la chaîne.
2. Créer la nouvelle doc. Section **Docs précédents** : un lien vers chaque
   doc étendue ou modifiée, et ce qui change.
3. Dans chaque doc citée, ajouter une ligne dans **Modifié par** qui pointe
   vers la nouvelle.

La CI vérifie que la section « Docs précédents » est remplie (un lien ou
`Aucun.`), que les liens existent, et que chaque doc citée renvoie bien vers
la nouvelle.
