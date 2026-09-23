# Démarrer sur Fungiiiii

Tout ce qu'il faut pour livrer ton premier ticket. Lis cette page une fois en
entier ; ensuite, [la cheat sheet git](./git-cheatsheet.md) suffit au quotidien.

## 1. Installer

1. **Unity Hub**, puis l'éditeur **6000.3.21f1** — exactement cette version
   (elle est écrite dans `ProjectSettings/ProjectVersion.txt`).
2. **Git** et **Git LFS** (les modèles 3D et les textures passent par LFS).
   Et **jq** (`winget install jqlang.jq`) : les garde-fous de Claude en ont
   besoin, sans lui Claude refuse toutes les commandes.
3. Cloner le repo, puis une seule fois :

   ```bash
   ./scripts/setup.sh
   ```

   Il installe les hooks git, LFS et l'outil de merge des scènes Unity. Lis
   ce qu'il affiche : il dit ce qui manque.
4. Ouvrir le dossier du repo dans Unity Hub (*Add project from disk*).

## 2. Les mots à connaître

| Mot | Ce que c'est |
|---|---|
| **`develop`** | La branche où tout le travail se rejoint. On part toujours d'elle, on y revient par une PR. |
| **`main`** | Les versions stables. Seul Ilyes y touche. |
| **Branche de feature** | Ta copie de travail pour un ticket : `feat/<id-clickup>-<description>`. |
| **Rebase** | Replacer tes commits après le dernier `develop`, pour tester ton code avec celui des autres. |
| **`--force-with-lease`** | Le push à faire après un rebase. Refuse d'écraser le travail de quelqu'un d'autre. |
| **PR** (pull request) | La demande de merge de ta branche dans `develop`, relue et testée par un autre dev. |
| **Note de design** | Une page qui dit ce que tu vas faire, avant de le faire. Validée par Joaquim et Hugo. |
| **Doc de feature** | Le fichier `docs/features/…md` qui décrit ce que tu as livré, et comment le tester. |
| **Test Runner** | La fenêtre Unity qui lance les tests automatiques : *Window → General → Test Runner*. |
| **EditMode** | Tests rapides, sans lancer le jeu. Pour la logique : calculs, règles, états. |
| **PlayMode** | Tests qui lancent une scène. Pour ce qui a besoin de la physique ou du cycle de vie Unity. |
| **Test manuel** | Ce qu'aucun test automatique ne vérifie : tu fais Play et tu regardes. Écrit sous forme d'étapes dans la doc. |
| **Playbook** | [`playbook-tests.md`](./playbook-tests.md) : les vérifications manuelles communes à toutes les PR. |
| **Scène prototype** | Chaque feature a sa scène de test dans `Assets/Scenes/Prototype/`. |

## 3. Ton premier ticket, pas à pas

1. **ClickUp** : assigne-toi le ticket.
2. **Note de design** : crée une sous-tâche, assigne-la à Joaquim et Hugo,
   colle le [modèle](#modèle-de-note-de-design) et remplis-le. Attends qu'ils
   la ferment. **Pas de code avant.**
3. **Branche** :

   ```bash
   git switch develop
   git pull
   git switch -c feat/<id-clickup>-<description>
   ```

4. **Lire l'existant** : cherche si la feature a déjà des docs.

   ```bash
   grep -ril "<mot-clé>" docs/features/
   ```

5. **Coder**, avec Claude (voir [les prompts](#travailler-avec-claude)). Commits
   en anglais : `feat(player): add jump`.
6. **Doc** : copie `docs/features/_template.md` en
   `docs/features/<id-clickup>-<description>.md` et remplis-la.
   [Exemple rempli](./features/86c9mnqxa-champimaison.md).
7. **Rebase et test** :

   ```bash
   git fetch origin
   git rebase origin/develop
   ```

   Puis dans Unity : Test Runner (EditMode + PlayMode), le
   [playbook](./playbook-tests.md), et les tests manuels de ta doc. Coche la
   colonne *OK* de ta doc.
8. **Push** : `git push --force-with-lease`
9. **PR** vers `develop` sur GitHub. Remplis le template, assigne un reviewer.
10. **Review** : il teste ta branche dans Unity. S'il demande des changements,
    retour à l'étape 5.
11. **Merge** : c'est lui qui merge. Ensuite, nettoie
    ([cheat sheet §6](./git-cheatsheet.md#6-après-le-merge--nettoyer)) et
    ferme le ticket.

Tu es reviewer ? Voir [cheat sheet §5](./git-cheatsheet.md#5-reviewer--tester-la-pr).

## Travailler avec Claude

Claude connaît les règles du repo (`CLAUDE.md`). Il te rappellera la note de
design, lira les anciennes docs et respectera les conventions. Il ne sait pas
ce que tu as vu dans Unity : c'est toi qui testes.

Prompts utiles :

- *« Voici le ticket ClickUp <lien>. Écris un premier jet de la note de
  design. »*
- *« Avant de coder, lis les docs de `docs/features/` qui concernent
  <feature> et résume-moi ce qui existe. »*
- *« Implémente la note de design validée. Sépare la logique dans une classe
  sans Unity pour qu'elle soit testable en EditMode. »*
- *« Écris les tests EditMode de cette feature, puis remplis la section Tests
  de la doc, avec les étapes des tests manuels. »*
- *« Relis tout mon diff avant la PR (skill ship-it) et remplis la description
  de PR. »*

Ne coche jamais un test que tu n'as pas vu passer toi-même dans Unity.

## Modèle de note de design

À coller dans la sous-tâche ClickUp.

```text
QUOI — ce que le joueur voit / peut faire :

POURQUOI — lien du ticket :

COMMENT — scènes, scripts, prefabs, assets touchés :

HORS PÉRIMÈTRE — ce qu'on ne fait pas :

RISQUES / QUESTIONS OUVERTES :

VÉRIFICATION — tests automatiques prévus, tests manuels prévus :
```

## Si la CI est rouge sur ta PR

| Job | Ce qu'il veut dire |
|---|---|
| *Feature doc present and linked* | Doc manquante, section « Docs précédents » vide (mettre `Aucun.`), ou ancienne doc sans lien retour dans « Modifié par ». Le message donne le fichier et la ligne à ajouter. |
| *Conventional Commits* | Un message de commit n'a pas la forme `type(scope): description`. |
| *Source code is English* | Des accents dans le code ou dans un nom de fichier. |
| *No hardcoded player-facing text* | Un texte affiché au joueur écrit en dur dans le code. |
| *Art assets follow ADR-0005* | Nommage ou dossier d'un asset 3D incorrect — voir [`art-pipeline.md`](./art-pipeline.md). |
| *PR to main comes from develop* | Ta PR vise `main` : change la cible pour `develop`. |

Coincé ? Demande sur Discord avant de forcer quoi que ce soit.
