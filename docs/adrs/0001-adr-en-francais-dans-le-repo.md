# ADR-0001 — ADR en français dans le repo, code en anglais

## Statut

Accepté

Date : 2026-09-14

## Contexte

Le projet impose une source de vérité documentaire unique sur Atlassian, et les règles initiales du projet plaçaient les ADR dans la section ADR d'Atlassian.

Deux besoins entrent en tension :

- le code et les artefacts techniques doivent rester lisibles et contribuables par n'importe quel développeur ou outil, ce qui plaide pour l'anglais ;
- les ADR sont des documents de raisonnement, écrits et relus par une équipe francophone ; les rédiger en anglais appauvrit la nuance de l'argumentaire et décourage leur écriture.

Par ailleurs, un ADR hébergé hors du repo ne suit pas la branche qui l'introduit : il ne peut pas être relu dans la même PR que le code qu'il justifie, ni versionné avec lui.

## Hypothèses

- L'équipe reste francophone à court et moyen terme.
- Les contributeurs externes, s'il y en a, touchent au code et non aux ADR.
- Atlassian reste la source de vérité pour la documentation projet (game design, architecture, onboarding), hors ADR.

## Contraintes

- Aucune duplication de documentation entre le repo et Atlassian : un contenu donné a exactement un emplacement.
- Les ADR doivent être relisibles dans la même PR que le changement qu'ils justifient.
- Un ADR accepté prime sur une documentation contradictoire, tant qu'il n'a pas été remplacé.

## Alternatives envisagées

### Alternative 1 — Tout en anglais, ADR sur Atlassian (situation initiale)

- Avantages : source documentaire unique ; cohérence linguistique totale.
- Inconvénients : ADR décorrélés du code et de la PR ; rédaction en anglais coûteuse pour une équipe francophone, donc ADR moins écrits en pratique.
- Risques : la règle « écrire un ADR » est contournée parce qu'elle est trop coûteuse, et les décisions d'architecture deviennent implicites.

### Alternative 2 — ADR en français dans le repo, code en anglais (retenue)

- Avantages : l'ADR vit dans la branche et se relit avec la PR ; rédaction dans la langue de travail de l'équipe ; historique git des décisions.
- Inconvénients : deux langues coexistent dans le dépôt ; la frontière doit être explicite et outillée.
- Risques : dérive du français vers le code si la frontière n'est pas contrôlée automatiquement.

### Alternative 3 — Tout en français

- Avantages : cohérence interne maximale.
- Inconvénients : incompatible avec les conventions Unity et C#, les bibliothèques tierces et l'outillage ; barrière pour tout contributeur ou outil externe.
- Risques : identifiants mixtes franco-anglais, le pire des deux mondes.

## Décision

Le dépôt est **intégralement en anglais** : identifiants, commentaires, documentation XML, noms de tests, messages de log et d'exception, noms d'assets, scènes, prefabs et ScriptableObjects Unity, noms de branches, messages de commit, descriptions de PR, et Markdown du dépôt.

Les **ADR sont rédigés en français** et vivent dans `docs/adrs/`, nommés `NNNN-titre-en-kebab-case.md`. C'est la seule exception.

Les chaînes affichées au joueur relèvent du système de localisation, pas du code : elles ne sont jamais codées en dur dans un `.cs` et ne sont pas concernées par cette règle.

Cet ADR **remplace** la règle initiale plaçant les ADR dans la section ADR d'Atlassian. Atlassian conserve toute la documentation projet hors ADR.

## Justification

Placer l'ADR dans le repo est ce qui rend la règle « tout changement d'architecture s'accompagne d'un ADR » réellement applicable : la décision est relue dans la PR, avec le code, par le même relecteur humain.

Le français pour les ADR supprime le principal frein à leur rédaction. Le coût — deux langues dans le dépôt — est acceptable parce que la frontière est nette (`docs/adrs/` contre tout le reste) et vérifiable automatiquement par un hook `pre-commit`.

## Dépendances

Aucune.

## Conséquences

- **Positives :** ADR versionnés et relus avec le code ; frontière linguistique nette et outillée ; rédaction d'ADR moins coûteuse, donc plus fréquente.
- **Négatives :** dépôt bilingue ; la section ADR d'Atlassian doit être vidée ou redirigée vers `docs/adrs/`.
- **Dette technique acceptée :** le hook `pre-commit` détecte le français par heuristique (caractères accentués) et non par analyse linguistique ; il peut produire des faux positifs, contournables avec `git commit --no-verify`.
- **Impact migration :** tout ADR existant sur Atlassian doit être déplacé vers `docs/adrs/` ou explicitement marqué comme remplacé.
- **Impact workflow éditeur :** aucun.
- **Impact runtime :** aucun.

## Références

- `CLAUDE.md`, section « Language »
- `.claude/skills/adr/SKILL.md`
- `.githooks/pre-commit`
- Documentation projet : https://fungiiiii.atlassian.net/wiki/spaces/docs/overview?homepageId=950451
