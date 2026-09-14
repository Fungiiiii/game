# ADR-0002 — Pipeline de rendu : URP

## Statut

Proposé

Date : 2026-09-14

> La décision elle-même est déjà actée par l'équipe dans les Spécifications
> Techniques (STD V2, juillet 2026), qui désignent URP. Cet ADR ne rouvre pas
> l'arbitrage : il le rapatrie dans le dépôt, où il peut être relu avec le code
> qu'il contraint. Le relecteur de la PR bascule le statut en « Accepté ».
>
> L'argumentaire ci-dessous est reconstruit à partir des contraintes
> documentées (configuration matérielle cible, cibles de performance, direction
> artistique), et non à partir des délibérations d'origine, qui ne figurent pas
> dans le dépôt.

## Contexte

Le projet Fungiiiii! n'a pas encore de projet Unity. Le pipeline de rendu est
figé au moment de la création : il détermine le format des matériaux, des
shaders, des lumières et des réglages de qualité. En changer ensuite impose de
réécrire chaque matériau et chaque shader du projet.

Il faut donc trancher avant la première création de projet, pas après.

## Hypothèses

- La direction artistique reste low-poly stylisée, telle que décrite au GDD, et
  non photoréaliste.
- La cible matérielle basse (GTX 1060 / RX 580, 4 Go de VRAM) reste supportée
  jusqu'au rendu d'avril 2027.
- L'équipe ne comporte pas de spécialiste rendu dédié à plein temps.
- Les effets fongiques (bioluminescence, nuages de spores) sont réalisés avec
  Shader Graph et VFX Graph, et non avec des shaders écrits à la main.

## Contraintes

- Moteur : Unity 6.3 LTS, patch `6000.3.21f1`.
- Plateforme V1.0 : PC Windows 64-bit uniquement. macOS et Linux (SteamOS,
  Steam Deck) hors périmètre V1.0 mais explicitement envisagés ensuite.
- Configuration minimale : Intel i5-8400 / Ryzen 5 2600, 8 Go de RAM,
  GTX 1060 / RX 580 4 Go de VRAM, pour 30 FPS moyen.
- Configuration recommandée : RTX 2070 / RX 5700 XT 8 Go, pour 60 FPS moyen.
- Taille du build packagé : moins de 4 Go en V1.0.
- Mémoire : moins de 8 Go sur configuration recommandée.
- Shader Graph, VFX Graph et post-processing sont requis par la STD.

## Alternatives envisagées

### Alternative 1 — Universal Render Pipeline, URP (retenue)

- Avantages : couvre l'intégralité de l'écart entre la configuration minimale et
  la configuration recommandée, ce qui est exactement le besoin ; supporte
  Shader Graph et VFX Graph ; coût de rendu prévisible, donc budget 60 FPS
  atteignable sans spécialiste ; le Renderer et le RP Asset sont sérialisés en
  assets versionnables ; portage macOS et Linux ultérieur sans réécriture des
  matériaux ; template officiel fourni avec l'éditeur.
- Inconvénients : moins de fonctionnalités de rendu avancées que HDRP
  (éclairage volumétrique, réflexions en espace écran de qualité) ; certaines
  fonctionnalités de VFX Graph restent limitées ou indisponibles sur URP par
  rapport à HDRP.
- Risques : la tentation d'ajouter des Renderer Features au coup par coup fait
  dériver le budget GPU sans que personne ne le mesure.

### Alternative 2 — High Definition Render Pipeline, HDRP

- Avantages : qualité de rendu supérieure ; éclairage volumétrique et effets
  atmosphériques natifs, séduisants pour une forêt fongique ; VFX Graph y est
  le plus complet.
- Inconvénients : plancher matériel incompatible avec la configuration
  minimale annoncée — HDRP vise le milieu et le haut de gamme, une GTX 1060
  4 Go ne tient pas les cibles ; build plus lourd, contrainte des 4 Go plus
  difficile à tenir ; complexité de réglage (exposition, volumes, éclairage
  physique) disproportionnée pour du low-poly stylisé ; Steam Deck exclu.
- Risques : découvrir tardivement que la configuration minimale publiée est
  intenable, et devoir soit la relever, soit migrer tout le projet vers URP.

### Alternative 3 — Built-in Render Pipeline

- Avantages : le plus simple à démarrer ; abondance de tutoriels et d'assets
  tiers compatibles.
- Inconvénients : Shader Graph et VFX Graph ne le supportent pas, ce qui
  contredit directement la STD ; post-processing via l'ancien Post Processing
  Stack v2, non maintenu ; Unity n'y investit plus.
- Risques : impasse technique sur les effets fongiques, qui sont l'identité
  visuelle du jeu, et migration forcée en cours de projet.

### Alternative 4 — Ne rien décider et créer le projet en Built-in par défaut

- Avantages : aucun effort immédiat.
- Inconvénients : le choix est fait quand même, mais par défaut et sans trace.
- Risques : le pire des cas — la réécriture des matériaux arrive de toute
  façon, plus tard et plus chère.

## Décision

Le projet utilise **URP (Universal Render Pipeline)**, version `17.3.0`,
fournie par `com.unity.render-pipelines.universal`.

Le projet est créé à partir du template officiel **3D Cross-Platform** de
Unity 6.3, qui est le template URP. Les assets de pipeline livrés par ce
template sont conservés tels quels dans `Assets/Settings/` :
`PC_RPAsset` et `PC_Renderer`, `Mobile_RPAsset` et `Mobile_Renderer`,
`UniversalRenderPipelineGlobalSettings`, ainsi que les profils de volume.

Toute modification d'un RP Asset, d'un Renderer, ou l'ajout d'une Renderer
Feature, passe par une PR relue, au même titre que du code.

## Justification

URP est la seule des trois options qui couvre la plage matérielle publiée. La
configuration minimale (GTX 1060 4 Go, 30 FPS) et la configuration recommandée
(RTX 2070, 60 FPS) tiennent toutes deux dans son enveloppe, là où HDRP exclut
la première et où Built-in interdit l'outillage exigé par la STD.

Le rendu low-poly stylisé n'a pas besoin de ce que HDRP apporte en plus. Payer
la complexité de HDRP pour une direction artistique qui ne l'exploite pas
serait un coût permanent sans contrepartie.

Enfin, le portage macOS et Linux est déjà annoncé comme possible après la V1.0.
URP le rend envisageable sans retoucher les matériaux ; HDRP l'exclut de fait
sur Steam Deck.

## Dépendances

`com.unity.render-pipelines.universal` version `17.3.0`.

- Pourquoi nécessaire : c'est l'implémentation d'URP ; la décision ne peut pas
  être appliquée sans elle.
- Pourquoi l'existant est insuffisant : le pipeline Built-in ne supporte ni
  Shader Graph ni VFX Graph, que la STD exige.
- Maintenance : package Unity de première partie, versionné avec l'éditeur.
  Sa version suit celle d'Unity 6.3 et ne doit pas être mise à jour
  indépendamment de l'éditeur.
- Compatibilité Unity : `17.3.0` est la version livrée avec Unity 6.3 LTS.
- Compatibilité plateformes : Windows 64-bit couvert ; macOS, Linux et
  Steam Deck couverts pour un portage ultérieur.

Amène également `com.unity.shadergraph` en dépendance transitive.

## Conséquences

- **Positives :** la plage matérielle annoncée est tenable ; Shader Graph et
  VFX Graph sont disponibles immédiatement ; les réglages de rendu sont des
  assets versionnés, donc relisibles en PR ; le portage hors Windows reste
  ouvert.
- **Négatives :** les effets de rendu haut de gamme sont hors d'atteinte ;
  certains assets du Store écrits pour Built-in nécessiteront une conversion
  de matériaux.
- **Dette technique acceptée :** les RP Assets et Renderers du template sont
  repris sans audit de leurs réglages par défaut. Une passe de réglage
  (ombres, MSAA, résolution des ombres, niveaux de qualité) reste à faire, et
  doit s'appuyer sur le Profiler plutôt que sur des valeurs choisies à vue.
  Les assets `Mobile_*` sont conservés bien qu'aucune plateforme mobile ne soit
  visée : les retirer touche aux Quality Settings et mérite sa propre PR.
- **Impact migration :** aucun, le projet est créé avec cette décision.
- **Impact workflow éditeur :** les matériaux sont créés dans Unity avec des
  shaders URP. Les matériaux importés depuis Blender ne sont pas utilisés,
  conformément à `docs/art-pipeline.md`.
- **Impact runtime :** le pipeline de rendu conditionne le budget GPU. Les
  cibles de 30 et 60 FPS de la STD se vérifient au Unity Profiler, pas à l'œil.

## Références

- Spécifications Techniques (STD), §3 Stack technologique et §9 Configuration
  matérielle cible — https://fungiiiii.atlassian.net/wiki/spaces/docs/pages/27099148
- `docs/unity-init.md`, §1 « Still open »
- `.claude/skills/unity-project-config/SKILL.md`, section Rendering
- [ADR-0003](./0003-architecture-des-entrees-input-system.md) — décision prise
  au même moment, pour la même raison de calendrier
