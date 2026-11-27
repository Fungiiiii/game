# Forêt mystique — vent sur les arbres, calé sur les rafales du son

- **Ticket ClickUp :** [Vent sur les arbres, calé sur les rafales du son](https://app.clickup.com/t/12487v2nbcz) (sous-tâche du ticket Forêt mystique, https://app.clickup.com/t/12487v2byt9). La note de design est dans sa description.
- **Note de design validée :** [[VALIDATION] Note de design — vent sur les arbres](https://app.clickup.com/t/12487v2nbd1). Hugo l'a validée dans la conversation le 17/11, puis a demandé une révision après son premier essai (troncs immobiles, feuillage seul, moins d'amplitude). La note révisée est sur le ticket. Au moment de la PR, la sous-tâche n'est pas fermée.
- **Branche :** `feat/12487v2byt9-mystic-forest-tree-wind`, partie de `feat/12487v2byt9-mystic-forest-audio` (PR #33), dont elle utilise l'émetteur de rafales.
- **Auteur :** Hugo Bernier
- **Statut :** En cours

## Docs précédents

- [12487v2byt9-mystic-forest](./12487v2byt9-mystic-forest.md) — les arbres ne sont plus immobiles. L'herbe se penche aussi pendant les rafales que l'on entend, et `GrassWind` lit maintenant une valeur de rafale partagée.
- [12487v2byt9-mystic-forest-audio](./12487v2byt9-mystic-forest-audio.md) — `RandomOneShotEmitter` signale chaque rafale jouée. Un composant `WindGustDriver` s'ajoute sur `Audio/Gusts`. Les rafales entendues sont maintenant visibles dans le feuillage et l'herbe.

## Ce que fait la feature

- [x] Les troncs et les branches des 4 arbres (`Tree`, `TreeBroad`, `TreePine`, `TreeGiant`) ne bougent jamais.
- [x] Leur feuillage ondule doucement, en continu : la couronne se balance lentement dans le sens du vent, et chaque touffe bouge un peu de son côté. Environ 10 à 20 cm.
- [x] Les touffes bougent presque d'un bloc : les facettes low poly restent nettes.
- [x] Deux arbres voisins ne se balancent pas en même temps.
- [x] Quand une rafale se fait entendre, le feuillage et l'herbe se penchent davantage. Le mouvement monte, culmine et retombe avec le volume de la rafale : environ 30 cm de plus sur les arbres au plus fort.
- [x] Une rafale jouée plus fort fait davantage pencher.
- [x] L'arbre géant bouge un peu moins que les autres.
- [x] L'ombre du feuillage suit son mouvement ; l'ombre des troncs reste fixe.
- [x] Hors rafale, l'herbe bouge comme avant.

## Hors périmètre

- Buissons, fougères, champignons : ils restent immobiles.
- Un vent de fond qui changerait selon la zone.
- Une rafale qui traverse la forêt depuis une direction : le son des rafales est 2D, tout le feuillage réagit en même temps (l'herbe garde son ondulation qui avance).
- Feuilles qui tombent, bruit de feuillage, craquements.
- Troncs qui plient.

## Implémentation

**Shaders** (`Assets/Art/Shaders/`) :

| Fichier | Rôle |
|---|---|
| `Wind.hlsl` | Partagé par l'herbe et les arbres : la direction du vent et la valeur globale `_WindGust` (0 sans rafale, jusqu'à environ 1). |
| `TreeWind.shader` | `Fungiiiii/TreeWind` : URP Lit, dont le feuillage bouge. Passes ForwardLit, ShadowCaster, DepthOnly, DepthNormals. |
| `GrassWind.shader` | Ajoute `_GustLean` (0,9) : penché supplémentaire, multiplié par `_WindGust`. |

**Matériaux** (`Assets/Art/Materials/`) :

| Matériau | Modèles | Couronne | Touffes | Rafale | Vitesse | Frémissement |
|---|---|---|---|---|---|---|
| `TreeWind.mat` | `Tree`, `TreeBroad`, `TreePine` | 0,12 m | 0,06 m | 0,3 m | 0,6 | 0,015 m |
| `TreeWindGiant.mat` | `TreeGiant` | 0,08 m | 0,05 m | 0,2 m | 0,5 | 0,015 m |

Chaque `.fbx.meta` des 4 arbres remappe `M_Fungi_Palette` sur l'un de ces matériaux au lieu de `Palette.mat`. Texture, couleur, lissé et métallique sont ceux de `Palette.mat`.

**Scripts** (`Assets/Scripts/Runtime/Audio/`) :

| Fichier | Rôle |
|---|---|
| `GustEnvelope.cs` | Calcul pur : le volume d'un son au fil du temps, de 0 à 1, mesuré par tranches de 0,1 s (RMS sur tous les canaux). |
| `WindGustDriver.cs` | Sur `Audio/Gusts`. Mesure chaque son de rafale une fois, à l'`Awake`. Quand l'émetteur joue une rafale, il règle `_WindGust` = volume mesuré × volume de lecture à chaque image, puis le remet à 0. |
| `RandomOneShotEmitter.cs` | Ajoute l'événement `Played(clip, volume)` et la liste `Clips`. |

**Scène :** `Assets/Scenes/Biomes/MysticForest.unity`, un composant `WindGustDriver` ajouté sur `Audio/Gusts`. Rien d'autre.

## Décisions

- **Seul le feuillage bouge.** Hugo, au premier essai le 17/11, où tout l'arbre pliait depuis le pied : « le tronc bouge, les branches ne suivent pas », « ils bougent beaucoup », « les feuillages doivent bien bouger, les troncs non », « rester dans l'ambiance low poly ».
- **Deux couches de mouvement**, comme dans la plupart des jeux (le *main bending* et le *detail bending* de Crysis, GPU Gems 3 ch. 16) : un balancement lent de la couronne, et une onde lente qui traverse le feuillage. L'onde est longue par rapport à une touffe, donc les sommets d'une même touffe bougent presque ensemble et les facettes restent nettes.
- **Le bois est reconnu par sa case de palette, pas par des couleurs de sommet.** Les jeux peignent d'habitude un masque dans les couleurs de sommet du modèle. Ici, chaque face vise déjà une case de `Palette.png` : les cases Wood (3,2), Wood dark (3,3) et Bark pale (6,4) sont immobiles. Les modèles, dont `Tree.fbx` qui n'est pas généré par script, ne changent pas.
- **Les rafales viennent du son.** Hugo, 17/11 : seules les rafales sont calées, pas le vent de fond par zone. L'herbe suit aussi.
- **Le volume est mesuré dans le fichier, une fois**, plutôt qu'écouté en direct sur la sortie audio : c'est déterministe, testable, et indépendant du volume général. Les clips de rafale doivent rester en *Decompress On Load*, sinon `WindGustDriver` ne peut pas les lire et le signale dans la Console.
- **Shader écrit à la main sur les fichiers de URP Lit**, pas en Shader Graph : une fonction déplace le sommet puis appelle la fonction de sommet de Lit, inchangée. Les arbres gardent l'éclairage de `Palette.mat`.
- **Shader incompatible avec le SRP Batcher** : les réglages de vent sont hors du bloc `UnityPerMaterial`, que `LitInput.hlsl` déclare lui-même. Les arbres du Terrain sont dessinés en GPU instancing, qui reste actif.
- **Deux matériaux** : le géant, mis à l'échelle ×1,8 et aux grandes couronnes plates, est réglé à part.
- **Pas d'`AudioMixer`, pas de gestionnaire global** : un composant à côté de l'émetteur, qui écrit une seule valeur globale de shader et la remet à 0 quand il est désactivé.

## Tests

### Automatisés (Unity Test Runner)

Lancés en batchmode avec Unity 6000.3.21f1 sur la machine de Hugo le 17/11, après le dernier changement de shader. Log et XML lus : **EditMode 94/94, PlayMode 20/20**, aucune erreur de compilation. Le shader `TreeWind` compile sans erreur ni avertissement. Les tests n'ont pas été relancés après un rebase sur `origin/develop` : la branche part de la PR #33.

| Test | Mode | Couvre | Résultat |
|---|---|---|---|
| `SilenceMakesNoGust` | EditMode | un son muet ne donne aucune rafale | ✅ |
| `TheLoudestMomentReachesOne` | EditMode | le pic du son vaut 1, le début est faible, la courbe monte | ✅ |
| `NoGustBeforeOrAfterTheSound` | EditMode | 0 avant et après le son | ✅ |
| `ASoundOnOneChannelOnlyStillCounts` | EditMode | un son sur un seul canal stéréo compte | ✅ |
| `TheVisibleWindRisesAndFallsWithAHeardGust` | PlayMode | dans la scène, une rafale fait monter `_WindGust` au-dessus de 0,9 puis le ramène à 0 | ✅ |

Aucun test automatisé ne vérifie ce qu'on voit : le mouvement est un réglage visuel.

### Manuels (dans Unity)

| # | Étapes | Résultat attendu | OK |
|---|---|---|---|
| 1 | Ouvrir `Assets/Scenes/Biomes/MysticForest.unity`, Play, regarder les arbres au spawn sans bouger. | Les troncs et les branches sont immobiles. Le feuillage ondule doucement, les arbres voisins pas en même temps. Aucune erreur dans la Console. | |
| 2 | Attendre une rafale (au plus 70 s), au casque. | Le feuillage et l'herbe se penchent en même temps que le son, puis reviennent quand il retombe. | |
| 3 | Regarder la jonction entre branches et feuillage, en rafale. | Le feuillage ne se détache pas des branches de façon visible. | |
| 4 | Regarder les ombres au sol, en rafale. | L'ombre du feuillage bouge, celle des troncs reste fixe. | |
| 5 | Marcher jusqu'à l'arbre géant, puis en zone B (pins). | Le géant bouge moins que les autres. Les étages des pins bougent, leur tronc non. | |
| 6 | Fenêtre **Stats**, au spawn et en zone B. | Noter les FPS dans la PR. Objectif : 120 FPS ou plus sur RX 6800 XT + i5-12400F. | |

## Limitations connues

- Vu en jeu par Hugo au premier essai le 17/11 : version rejetée (tout l'arbre pliait). La version actuelle n'a été vue qu'en rendus batchmode : images fixes et une animation de 48 images, avec une rafale simulée.
- Les amplitudes ont été réglées à l'œil sur ces rendus, sans les avoir vues en mouvement en jeu.
- FPS non mesurés.
- Un nouvel arbre qui utilise une autre case de bois aurait un tronc qui bouge : il faut ajouter la case dans `FoliageWeight` du shader.
- Le feuillage glisse un peu autour des branches immobiles. Ça ne se voit pas sur les rendus, aux amplitudes actuelles.
- Les normales ne suivent pas le mouvement : l'éclairage des facettes ne change pas quand le feuillage se déplace. Avec quelques centimètres de mouvement, ça ne se voit pas.

## Modifié par

- [playtest-base](./playtest-base.md) — nouveau vent : tout l'arbre penche depuis le pied, rien ne tremble, le vent traverse la forêt. La décision « seul le feuillage bouge » est remplacée.
