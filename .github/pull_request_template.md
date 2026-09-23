## Résumé

<!-- Ce qui change, en deux ou trois phrases. -->

## Ticket

- ClickUp :
- Note de design validée (sous-tâche fermée) :

## Doc de la feature

<!-- Lien vers docs/features/<id>-<description>.md.
     Fix/refactor/chore sans doc : dire pourquoi, et citer la doc d'origine s'il y en a une. -->

## Implémentation

<!-- Les détails techniques utiles au reviewer. -->

## Impact Unity

<!-- Scènes, prefabs, ScriptableObjects, ProjectSettings, packages touchés. « Aucun » sinon. -->

## Tests

<!-- Lancés APRÈS `git rebase origin/develop`. -->

- Test Runner EditMode :
- Test Runner PlayMode :
- Playbook (tronc commun) :
- Tests manuels de la doc :
- **Non vérifié (et pourquoi) :**

## Risques

<!-- Régressions possibles, ce que le reviewer doit regarder de près. -->

## Checklist

**Dev**

- [ ] Branche rebasée sur `origin/develop`
- [ ] Le projet compile, Console sans nouvelle erreur
- [ ] EditMode et PlayMode verts
- [ ] Playbook + tests manuels de la doc rejoués, colonne « OK » cochée dans la doc
- [ ] Doc de la feature à jour, anciennes docs lues et liées dans les deux sens
- [ ] Diffs de scènes, prefabs, `.meta`, `ProjectSettings` et packages relus
- [ ] Code en anglais, aucun texte joueur en dur
- [ ] Aucun secret, aucune modif hors sujet
- [ ] Reviewer assigné

**Reviewer**

- [ ] Branche récupérée et lancée dans Unity
- [ ] Test Runner vert chez moi
- [ ] Playbook + tests manuels de la doc rejoués
