# Git cheat sheet

Le process complet est dans [`docs/workflow.md`](./workflow.md).

## Une fois par clone

```bash
./scripts/setup.sh
git config --global pull.rebase true
git config --global rerere.enabled true
```

`rerere` retient comment tu as résolu un conflit et le rejoue au prochain rebase.

## 1. Démarrer un ticket

*Seulement après la validation de la note de design.*

```bash
git switch develop
git pull
git switch -c feat/12487v29zj3-mouvement-ia
```

## 2. Pendant le dev

```bash
git status
git add Assets/Scripts/Player/PlayerMovement.cs
git commit -m "feat(player): add ground check"
git push -u origin HEAD
```

Ajouter les fichiers **un par un** ou par dossier : pas de `git add .`
aveugle. Toujours committer le `.meta` avec son asset.

## 3. Avant de push pour la PR

```bash
git fetch origin
git rebase origin/develop
```

→ Unity : Test Runner (EditMode + PlayMode) + [playbook](./playbook-tests.md)
+ tests manuels de ta doc. **Tout vert**, puis :

```bash
git push --force-with-lease
```

Jamais `--force` tout court. Jamais sur `develop` ni `main`.

## Conflit pendant le rebase

```bash
git status
```

Ouvrir chaque fichier en conflit, garder la bonne version, puis :

```bash
git add <fichier>
git rebase --continue
```

Scène ou prefab en conflit : `git mergetool` (UnityYAMLMerge, configuré par
`setup.sh`). Perdu ? On revient à l'état d'avant le rebase :

```bash
git rebase --abort
```

## 4. Ouvrir la PR

Vers **`develop`**. Remplir le template : lien ClickUp, lien de la doc
`docs/features/…`, résultats des tests, ce qui n'a pas été vérifié.

Les labels se posent tout seuls. Seul `bloqué` se met à la main. Pas encore
prêt ? Ouvrir la PR en **Draft**.

## 5. Reviewer : tester la PR

```bash
git fetch origin
git switch feat/12487v29zj3-mouvement-ia
git pull
```

→ Unity : Test Runner + playbook + tests manuels de la doc. Cocher la partie
« Reviewer » de la checklist de la PR, puis approuver sur GitHub et
**merger**. La branche
distante est supprimée automatiquement.

## 6. Après le merge — nettoyer

```bash
git switch develop
git pull
git fetch --prune
git branch -d feat/12487v29zj3-mouvement-ia
```

`-d` refuse si la branche n'est pas mergée : c'est voulu, ne pas le remplacer
par `-D`.

## Dépannage

| Situation | Commande |
|---|---|
| Annuler les modifs d'un fichier | `git restore <fichier>` |
| Retirer un fichier du staging | `git restore --staged <fichier>` |
| Corriger le dernier commit (pas encore pushé) | `git commit --amend` |
| Mettre du travail de côté | `git stash push -m "wip"` puis `git stash pop` |
| Voir l'historique | `git log --oneline --graph -20` |
| Retrouver un commit « perdu » | `git reflog` |
| Mauvaise branche avant de committer | `git stash push -m "x"`, `git switch <bonne>`, `git stash pop` |

## Interdits

- Committer ou pusher sur `develop` ou `main` directement
- `git push --force` (utiliser `--force-with-lease`, sur sa branche)
- `git reset --hard`, `git branch -D`, `git clean -f` sans savoir ce qu'on
  perd
- Merger sa propre PR, ou une PR qu'on n'a pas testée dans Unity
