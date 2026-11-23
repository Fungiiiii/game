# Champonge compagnon et fougère à eau

- **Ticket ClickUp :** aucun. Hugo (owner) a demandé, le 21/11, de travailler sans ticket et sur la même branche que le playtest. C'est un écart volontaire au workflow, qui sera noté aussi dans la PR.
- **Note de design validée :** aucune sous-tâche de validation. Ce document a servi de note de design. Hugo l'a validé en conversation le 21/11 : ses réponses et les cinq propositions sont dans « Décisions ».
- **Branche :** `feat/playtest-base`
- **Auteur :** Hugo Bernier
- **Statut :** En cours. Codé et testé en batch ; pas encore joué par un humain.

## Docs précédents

- [playtest-retours](./playtest-retours.md) — E peut aussi être maintenu, avec un cercle qui se remplit autour de la touche, et la touche au-dessus d'une cible peut laisser la place à un court texte. Trois nouvelles aides de touches. Le fil des objets ramassés affiche l'eau. Le contrôle « tronc ou rocher » des champignons devient `ForestResourceSpots.TouchesTrunkOrRock`, partagé avec les fougères.
- [12487v2nnw4-demo-boucle](./12487v2nnw4-demo-boucle.md) — un Champonge capturé reste un objet de l'inventaire (RG_CA_03), mais on peut maintenant le sortir pour qu'il nous suive.
- [12487v2byt9-mystic-forest](./12487v2byt9-mystic-forest.md) — le générateur de modèles produit aussi la fougère à eau. Les fougères du décor (`ZoneB_Fern`, `ZoneC_Fern` et les deux de `Landmarks`) ne sont plus visibles en jeu : elles sont retirées d'une copie du Terrain propre à la partie. La scène et son Terrain ne sont pas modifiés.

## Pourquoi

Un Champonge capturé finissait dans l'inventaire et ne servait à rien. La brume (zone B et le creux) n'était que du décor. Avec cette feature, capturer un Champonge donne une ressource qu'on ne peut pas obtenir autrement, l'eau, comme le prévoit RG_CA_03 : « le champignon capturé est la source de la ressource spécifique de son espèce ». Et le joueur a une raison d'aller dans la brume.

## Ce que fait la feature

**Sortir et reprendre le Champonge**

- [ ] Avec un Champonge sur la case sélectionnée de la barre rapide, **clic gauche** (gâchette droite) le pose au sol à 1,5 m devant le joueur, tourné vers lui. Il quitte l'inventaire. L'aide « Sortir » s'affiche tant qu'aucun Champonge n'est dehors.
- [ ] Un seul Champonge peut être sorti à la fois. Quand un Champonge est déjà dehors, le clic gauche ne fait rien et l'aide « Sortir » disparaît.
- [ ] **Maintenir E** 0,8 s sur le Champonge sorti le remet dans l'inventaire (« +1 Champonge » dans le fil des objets ramassés). Le cercle cyan n'apparaît autour de la touche qu'une fois passé le temps d'un appui court (0,3 s), puis se remplit jusqu'à 0,8 s, comme dans les autres jeux : un appui rapide ne montre jamais de cercle (Hugo, 21/11). Si on relâche avant la fin, rien ne se passe. Quand l'inventaire est plein, le Champonge reste dehors.
- [ ] Reprendre un Champonge mouillé le presse d'abord : l'eau arrive dans l'inventaire, puis le Champonge. Si l'eau ne rentre pas, il reste dehors, mouillé : l'eau n'est jamais perdue.
- [ ] Jeter un Champonge depuis l'inventaire (Q, clic hors du panneau) fonctionne comme pour tout autre objet : son image tombe au sol. Il ne devient pas un compagnon.

**Le Champonge suit le joueur**

- [ ] Une fois sorti, il suit le joueur comme un animal de compagnie (Hugo, 21/11 : « dynamique, sympa à suivre, pas collé au corps ») :
  - **Il ne gigote pas** : au repos près du joueur, il ne repart que quand le joueur est à plus de 5 m. Un pas ou un demi-tour ne le fait pas bouger.
  - **Il a sa place** : 2,5 m derrière le joueur et 1 m sur sa droite, pas dans son dos. « Derrière » se prend dans le sens où le joueur **marche**, pas là où il regarde : tourner la caméra, même en boucle, ne le fait pas bouger (retour de Hugo, 21/11 : il tournait autour du joueur sans fin).
  - **Il s'arrête dès qu'il est à portée** : à moins de 3 m du joueur, où qu'il soit autour de lui, il se pose sans chercher à atteindre sa place exacte.
  - **Il marche, puis court** : 3,5 m/s près du joueur, 1 m/s de plus par mètre de retard au-delà de 5 m, jusqu'à 8 m/s (le joueur sprinte à 7). Derrière un joueur qui marche, il traîne à environ 5,5 m ; qui sprinte, à environ 8,5 m.
  - **Il accélère et freine en douceur** (12 m/s²) et s'arrête net à sa place, sans osciller.
  - **Il a l'air vivant** : il tourne progressivement (540°/s) vers où il va, regarde le joueur quand il est au repos, et fait de petits sauts en marchant (7 cm, 1,5 par mètre), qu'il termine quand il s'arrête.
- [ ] S'il se retrouve à plus de 25 m (coincé contre un tronc, distancé), il réapparaît aussitôt à sa place, derrière le joueur, hors de sa vue.
- [ ] Il ne fuit pas, n'a pas de disque d'alerte, et le filet ne le vise pas.
- [ ] Les Champonge sauvages ne changent pas : ils se promènent et fuient toujours dans la clairière du spawn.

**Les fougères à eau**

- [ ] Une nouvelle plante, la **fougère à eau**, pousse seulement dans la brume, environ une tous les 30 m (échantillonnage de Poisson), hors des chemins, des troncs, des rochers et des pentes de plus de 30°. Chacune reste à sa place pendant toute la partie, comme un buisson à baies. Elle est plus large et plus claire que la fougère du décor, et ondule au même vent.
- [ ] Elle a deux états. **Sèche**, elle garde son vert, n'a pas de contour, et on ne peut pas la viser. **Mouillée**, elle a l'air mouillée comme dans les autres jeux : plus sombre (couleur multipliée par 0,7 / 0,8 / 0,85) avec un léger brillant (0,4), et de temps en temps (environ 0,8 goutte par seconde) une petite goutte bleu pâle tombe du bord de ses frondes.
- [ ] Toutes les 20 s, chaque fougère sèche a 25 % de chance de se mouiller, soit environ 80 s sèche en moyenne. Au début de la partie, une fougère sur deux est mouillée.
- [ ] Une fougère mouillée porte le contour crème fin de ce qui se ramasse. Visée, elle passe au contour cyan, et un court texte remplace la touche au-dessus d'elle : **« Il faut un Champonge »** / **"Needs a Champonge"**. E ne fait rien, et aucune aide de touche ne s'ajoute.
- [ ] Le joueur ne peut pas récolter l'eau lui-même. Seul le Champonge le peut.
- [ ] **Plus aucune fougère de décor** dans la forêt (Hugo, 21/11 : « sinon le joueur sera perdu ») : celles du Terrain (zones B et C) et les deux posées à la main sont retirées pour la partie. Toute fougère qu'on voit est une fougère à eau.

**Le Champonge se remplit d'eau**

- [ ] Quand une fougère mouillée est à moins de 10 m du joueur et que son Champonge est sec, le Champonge y va tout seul (la plus proche de lui d'abord). S'il n'est plus intéressé par elle en route, parce qu'elle a séché ou que le joueur s'est éloigné à plus de 10 m, il reprend la suite du joueur.
- [ ] Arrivé à moins de 0,6 m de la fougère, il s'imbibe en 1 s. Son chapeau prend peu à peu l'air mouillé, comme la fougère : plus sombre (couleur multipliée par 0,65 / 0,65 / 0,7) et un peu brillant (0,4). Puis la fougère redevient sèche : son vert revient, plus rien ne tombe. Le Champonge reprend la suite du joueur.
- [ ] Un Champonge mouillé ne va plus vers les fougères. Il contient une seule charge d'eau.

**Presser le Champonge**

- [ ] **Appui court sur E** (relâché en moins de 0,3 s) sur le Champonge mouillé : il est pressé, **+1 Eau** arrive dans l'inventaire et s'affiche dans le fil des objets ramassés, et son chapeau sèche en 1 s : couleur et brillant d'origine.
- [ ] Sur un Champonge sec, l'appui court ne fait rien.
- [ ] Les aides de touches, Champonge visé : « Presser » (E) s'il est mouillé, puis « Maintenir : reprendre » (E) dans tous les cas.
- [ ] L'eau est un nouvel objet : **Eau** / **Water**, un carré bleu `#4A90C8` dans l'inventaire tant que l'icône n'est pas dessinée, piles de 16. Rien ne l'utilise encore.
- [ ] R remet la partie à zéro : le Champonge sorti disparaît (l'inventaire repart avec le filet seul, comme avant), et les fougères tirent de nouveau leur état de départ.
- [ ] Les textes suivent la langue.

## Hors périmètre

- Ce que l'eau permet de faire : crafts et paliers de la Champimaison viendront plus tard.
- Le réseau : le Champonge sorti n'est pas synchronisé entre joueurs. Le playtest se joue seul.
- Plusieurs Champonge dehors en même temps, et tout autre compagnon (Champoule), même si une nouvelle fiche suffirait.
- Une animation de marche ou d'imbibition pour le Champonge : seul le chapeau change.
- La recherche de chemin (NavMesh) : le Champonge marche en ligne droite et réapparaît s'il est coincé.
- Les icônes d'objets (Eau, Champonge) : un humain les dessinera.
- Des Champonge sauvages dans la brume.
- `RespawningResource` reste inutilisée : elle fait revenir une ressource après un délai, alors que la fougère se mouille par tirage, toutes les 20 s.

## Implémentation

Trois couches, comme le reste du projet : la logique en C# pur, testée en EditMode ; les composants Unity qui la branchent ; les données dans des fiches.

**Logique (C# pur)**

| Fichier | Rôle |
|---|---|
| `Runtime/Gathering/ChanceRefill.cs` | Se remplit par tirage : toutes les N s, une chance de se remplir ; vidé, recommence. Graine injectée. |
| `Runtime/Controls/TapOrHold.cs` | Distingue l'appui court (au relâchement, sous 0,3 s) de l'appui long (au seuil de 0,8 s) ; donne la progression du cercle. |
| `Runtime/Companions/CompanionBrain.cs` | Ce que fait un compagnon : suivre (repos et départ à deux distances, place derrière et à côté du joueur, vitesse selon le retard, accélération et freinage *arrive*), aller vers une source, s'imbiber, revenir ; plein ou vide ; réapparition à sa place au-delà de 25 m. Avec `CompanionTuning` et `CompanionStep` (position et vitesse). |
| `Runtime/Companions/IAbsorbable.cs` | Ce qu'un compagnon peut absorber : position, objet donné, plein ou non. Un compagnon ne va que vers ce qui donne l'objet qu'il porte. |

**Données (fiches)**

| Fichier | Contenu |
|---|---|
| `Runtime/Companions/CompanionDefinition.cs` → `Assets/Data/Creatures/ChampongeCompanion.asset` | La créature (son modèle, et l'objet qu'elle devient), suite, vitesse, réapparition, portée, objet porté (Eau) et quantité, rayon de recherche, durée d'imbibition, matériau de la partie qui a l'air mouillée (`ChampongeCap.mat`), teinte et brillant une fois plein, durée du séchage. |
| `Runtime/Gathering/RefillingPlantDefinition.cs` → `Assets/Data/Gathering/WaterFern.asset` | Prefab, objet donné (Eau), décor qui lui ressemble et qu'on retire (`PF_Fern`), intervalle, chance, part pleine au départ, teinte et brillant une fois pleine, espacement, pente, échelle, portée de visée, clé du texte. |
| `Assets/Data/Items/Water.asset` | L'objet Eau, ajouté à `ItemCatalog`. |

**Composants**

| Fichier | Rôle |
|---|---|
| `Runtime/Companions/Companion.cs` | Le Champonge sorti : applique le cerveau, le pose sur le sol, le tourne, donne l'air mouillé à son chapeau (`_BaseColor` et `_Smoothness` par `MaterialPropertyBlock` sur les cases de matériau `ChampongeCap`). `IInteractable` (appui court : presser) et `IHoldInteractable` (appui long : reprendre). |
| `Runtime/Companions/CompanionKeeper.cs` | Objet de scène « Companions » : la liste des compagnons possibles (Inspector), le compagnon sorti (un seul), sortie depuis la case sélectionnée, reprise, `Clear` pour R. Donne à la scène le comportement en main de chaque objet compagnon. |
| `Runtime/Companions/CompanionInHand.cs` | `IHeldItem` du Champonge en main : le clic gauche le sort ; aide « Sortir » tant qu'aucun n'est dehors. |
| `Runtime/Gathering/RefillingPlant.cs` | Sur la racine du prefab de la plante : son modèle et ce qui en goutte (références de l'Inspector). Pleine : air mouillé (`_BaseColor` et `_Smoothness` du shader de vent par `MaterialPropertyBlock`), gouttes émises, contour visible ; texte à la place de la touche (`ILabelledInteractable`), absorbable par un compagnon (`IAbsorbable`). |
| `Runtime/Gathering/RefillingPlants.cs` | Objet de scène « Refilling Plants » : quelle plante pousse dans quelle zone (Inspector : la fougère à eau dans la zone de la brume), dispersion au chargement, minuterie de toutes les plantes, `RestartAll` pour R. |
| `Runtime/Interaction/IHoldInteractable.cs`, `ILabelledInteractable.cs` | Interfaces facultatives à côté de `IInteractable` : seuls le Champonge et la fougère les implémentent ; le bâton, le champignon et l'objet jeté ne changent pas. |
| `Runtime/Interaction/PlayerInteractor.cs` | Sur une cible qui se maintient, appui court au relâchement et appui long par `TapOrHold` ; ailleurs, à l'appui comme avant. Aides recalculées quand la cible change d'état ; texte à la place de la touche. Durées réglables sur le composant. |
| `Runtime/UI/KeyPrompt.cs`, `HoldRingGraphic.cs` | La touche gagne un cercle de progression (graphisme low poly, piste sombre et cyan) et un mode texte (style des aides de touches). |
| `Runtime/Game/GameBootstrap.cs` | Reçoit les deux objets de scène et leur donne le joueur ; enregistre le Champonge sur les mains ; R les remet à zéro. |
| `Runtime/Gathering/ForestResourceSpots.cs` | `TouchesTrunkOrRock`, sorti de `ForestGathering` pour être partagé avec les plantes ; `TakeLandmarks` cache les objets posés à la main d'un nom donné (avant : `ForestGathering.TakeHandPlacedMushrooms`). La dispersion réutilise `ScatterMushroomSpots`. |
| `Runtime/World/SessionTerrain.cs` | Le Terrain de la forêt pour la partie : ce que le jeu en retire (champignons de décor, fougères de décor) l'est d'une seule copie de ses données, faite la première fois. Remplace la copie que faisait `ForestGathering` (`ForestResourceSpots.TakeDecorMushrooms`). |

**Assets**

- `tools/blender/generate_forest_models.py` : `WaterFern` (48 triangles, six frondes ouvertes, Green / Green dark). Son `.meta` reprend celui de `Fern.fbx` (matériau `FernWind.mat`, pour onduler au vent), en Read/Write pour le contour.
- `Assets/Prefabs/Environment/PF_WaterFern.prefab` : `RefillingPlant`, `LODGroup` (un LOD, 3 %), le modèle, et `Drip`, un `ParticleSystem` : un cercle de 0,6 m à 0,36 m de haut (le bord des frondes), 0,8 goutte par seconde quand la fougère est pleine, chute par la gravité en 0,3 s, rendu étiré (`WaterDrip.mat`, `Particles/Unlit` de URP, bleu pâle `#CFE6F2`), sans ombre.
- `Champonge.fbx` : le chapeau a son propre matériau, `M_Fungi_Cap` (fait par une autre session, géométrie inchangée), remappé sur `Assets/Art/Materials/ChampongeCap.mat`, copie de `Palette.mat` : même rendu, mais une case de matériau que le code reconnaît.
- `KeyPrompt.prefab` : `HoldRing` (70 × 70, sous la touche) et `Text` (copie du libellé de `HintRow` : Fredoka 26, crème, contour sombre).
- `Game` (ex-`DemoLoopScene`) : objets « Refilling Plants » et « Companions », reliés au `GameBootstrap`. À l'enregistrement, Unity a aussi remplacé le champ périmé `boxSize` de `DroppedItems` par `pictureSize` et `floatHeight`, avec les valeurs du code : pas de changement de comportement.
- Table `UI` : `hint.release`, `hint.squeeze`, `hint.take_back`, `label.needs_champonge`, `item.water`, ajoutés aux tables (le script `PlaytestLocalization`, qui les portait dans le code, a été retiré ensuite : voir « Nettoyage » dans playtest-retours).
- Les assets et le câblage ont été écrits par un script d'éditeur à usage unique, lancé en batch, relu puis supprimé (règle *Scenes, prefabs and data*).

## Décisions

- **Le modèle de la fougère à eau (`WaterFern.fbx`) et le matériau du chapeau (`ChampongeCap.mat`) sont faits par script** : c'est Hugo qui l'a décidé (22/11), malgré la Charte IA qui réserve les visuels aux humains.
- **L'eau est la ressource propre au Champonge** (Hugo, 21/11) : elle servira aux crafts et à faire monter la Champimaison. Ça applique RG_CA_03.
- **Seul le Champonge récolte l'eau** (Hugo, 21/11) : si le joueur pouvait la prendre lui-même, le Champonge ne servirait à rien.
- **Un seul Champonge sorti à la fois** (Hugo, 21/11).
- **E court = presser, E maintenu = reprendre** (Hugo, 21/11). Les durées viennent de ce qui se fait ailleurs : l'appui court se décide au relâchement, sous 0,3 s (les guides citent 0,25 à 0,5 s pour séparer court et long), et l'appui long dure 0,8 s, avec un cercle de progression. Reprendre se défait facilement, donc pas besoin des 1 à 3 s qu'on réserve aux actions graves. Seules les cibles qui se maintiennent attendent le relâchement : un bâton se ramasse toujours à l'appui.
- **Le Champonge va seul vers une fougère quand le joueur en est proche** (Hugo, 21/11) : le joueur ne donne pas d'ordre.
- **La fougère mouillée a le même contour que le reste, plus un court texte** (Hugo, 21/11) : le joueur la remarque et comprend tout de suite à quoi elle sert.
- **Proposées par Claude, validées par Hugo le 21/11** :
  - **On sort le Champonge par un clic gauche**, avec lui en main dans la barre rapide : c'est l'action principale de l'objet tenu, comme le filet.
  - **Le texte de la fougère est « Il faut un Champonge »** : il dit en quatre mots pourquoi E ne fait rien.
  - **Une fougère sèche n'a ni gouttes ni contour, et on ne peut pas la viser** : il n'y a rien à faire avec elle.
  - **Reprendre un Champonge mouillé le presse d'abord** : l'eau n'est jamais perdue.
  - **Valeurs de départ** : 10 m pour aller vers une fougère, 25 % toutes les 20 s, appui long de 0,8 s, une fougère tous les 30 m. Elles sont dans les fiches et seront réglées au playtest.
- **Une fougère à eau à part, pas les fougères du décor** : les fougères du décor sont des milliers d'arbres du Terrain, sans script. Les rendre interactives coûterait cher, et le joueur viserait des fougères qui ne servent à rien.
- **Un tirage toutes les 20 s, avec une chance** (Hugo, 21/11 : « timer + chance »), plutôt qu'un délai fixe : les fougères ne se remouillent pas toutes en même temps.
- **Modulaire par les données, pas par un cadre** (Hugo, 21/11 : « faire en sorte que ce soit modulaire ») : un nouveau compagnon est une `CompanionDefinition`, une nouvelle plante une `RefillingPlantDefinition` et une ligne dans `RefillingPlants`, sans code. Un compagnon et une plante se rejoignent par l'objet : le Champonge porte de l'eau, il ne va que vers ce qui donne de l'eau. Rien de plus générique n'a été construit (pas d'ordres, pas de plusieurs compagnons) : YAGNI.
- **Le chapeau a son propre matériau, `ChampongeCap.mat`**, copie de `Palette.mat`, plutôt qu'un shader qui assombrirait selon la hauteur : le code trouve le chapeau par son matériau, sans connaître le modèle, et le rendu ne change pas. Le Champonge sauvage utilise le même modèle et le même rendu.
- **« Mouillé » = plus sombre et un peu brillant, sans shader dédié ni gouttes modélisées** (Hugo, 21/11 : « le modèle est pas top, regarder comment les autres font »). Dans les jeux, une surface mouillée est plus sombre (l'eau remplit son grain) et plus brillante (le film d'eau reflète). L'assombrissement porte le message : il se voit sous toute lumière, même dans la brume, alors que le brillant n'apparaît que sous certains angles. Le brillant reste léger : en low poly, les faces plates clignotent et font plastique. C'est la même règle pour la fougère et le chapeau du Champonge : sombre = de l'eau. Les shaders « mouillés » à ruissellement sont faits pour le réalisme et jureraient ici. Les gouttes modélisées (posées, puis arrondies, puis suspendues) ressemblaient à des cristaux ou à des cailloux : elles sont retirées.
- **Une goutte qui tombe de temps en temps** (Hugo, 21/11 : « si c'est vraiment simple ») : le mouvement se voit de loin et se lit tout de suite comme de l'eau, sans lueur. Un seul `ParticleSystem` par fougère, à l'arrêt quand elle est sèche ; pas d'éclaboussure au sol.
- **Les capacités optionnelles d'une cible sont des interfaces à part** (`IHoldInteractable`, `ILabelledInteractable`) plutôt que de nouveaux membres de `IInteractable` : les cibles existantes ne changent pas, et une future cible prend seulement ce dont elle a besoin.
- **Le Champonge marche en ligne droite**, sans collider ni NavMesh : il traverse le joueur sans le bousculer ; coincé, la réapparition à 25 m le débloque.
- **Sa place suit la marche du joueur, pas sa caméra, et il se pose à portée** (Hugo, 21/11) : sa place se calculait depuis la direction de la vue, si bien qu'en tournant la caméra elle tournait autour du joueur et il la poursuivait sans fin ; et il ne se posait qu'en atteignant cette place exacte. Il prend maintenant la direction des pas du joueur (gardée tant qu'il est immobile) et se pose à 3 m de lui.
- **Il suit comme les compagnons des autres jeux** (Hugo, 21/11) : deux distances pour ne pas gigoter (s'arrêter près, ne repartir que loin), une place décalée derrière et à côté du joueur, une vitesse qui grandit avec le retard, une accélération et un freinage doux (comportement *arrive* de Reynolds). Les sauts et les virages progressifs sont dans le composant, pas dans la logique : ils n'en changent pas les déplacements.
- **Les fougères du décor sont retirées à l'exécution, pas du Terrain de la forêt** (Hugo, 21/11 : les enlever) : comme les champignons de décor, elles sont ôtées d'une copie du Terrain faite pour la partie. Regénérer la forêt sans elles aurait changé `MysticForest` et pu déplacer d'autres calques (les rochers passent après les fougères). La plante dit elle-même ce qui lui ressemble (`lookAlikes`) : une autre plante n'aura qu'à nommer son sosie.
- **Une seule copie du Terrain par partie** (`SessionTerrain`), partagée par tout ce qui en retire quelque chose, plutôt qu'une copie par composant.
- **Pas de note validée dans ClickUp** : voir l'en-tête.

## Tests

### Automatisés (Unity Test Runner)

Lancés en batch sur le PC de Hugo (Unity 6000.3.21f1), le 21/11.

| Test | Mode | Couvre | Résultat |
|---|---|---|---|
| `ChanceRefillTests` (10) | EditMode | Vide au départ, premier tirage, 80 s en moyenne pour 25 % / 20 s, long pas de temps, vider, plein = temps arrêté, départ à 50 %, graine, réglages refusés | ✅ |
| `TapOrHoldTests` (7) | EditMode | Court au relâchement, même image, long au seuil une seule fois, entre les deux rien, progression, annulation, réglages refusés | ✅ |
| `CompanionBrainTests` (10) | EditMode | Suit à 2,5 m, reste près, vitesse et hauteur, réapparition derrière, va à la source la plus proche près du joueur et revient plein, durée d'imbibition, ignore sources vides, lointaines ou d'un autre objet, abandon en route (source vide, joueur parti), presser | ✅ |
| Tous les EditMode | EditMode | Rien d'autre n'a changé | ✅ 341 / 341 |
| `ChampongeCompanionPlayModeTests` | PlayMode | Plus de fougère de décor ; fougères seulement dans la brume, pleines et vides ; fougère pleine : plus sombre, brillante, des gouttes tombent, texte au lieu de la touche, E sans effet, sans aide ; fougère vide : verte, rien ne tombe, pas visée ; clic gauche sort un seul Champonge, aide « Sortir » ; il suit et réapparaît ; il s'imbibe près du joueur, le chapeau s'assombrit, presser donne l'eau et le chapeau s'éclaircit ; visé : touche, cercle, aide « Maintenir : reprendre » ; maintenir le reprend, pressé d'abord ; R l'enlève (10 tests) | ✅ |
| Tous les PlayMode | PlayMode | Rien d'autre n'a changé | ✅ 105 / 107, 0 échec ; les 2 ignorés sont des tests du package Input System (`IntegrationTests.WindowsInput_*`) |

### Manuels (dans Unity)

| # | Étapes | Résultat attendu | OK |
|---|---|---|---|
| 1 | Capturer un Champonge, le mettre sur la barre rapide, clic gauche | Il apparaît devant soi et quitte l'inventaire ; « Sortir » disparaît ; un deuxième clic gauche avec un autre Champonge ne fait rien | ⬜ |
| 2 | Marcher, sprinter, faire demi-tour | Il suit à quelques mètres, sur le sol, sans flotter ni s'enfoncer, tourné vers où il va | ⬜ |
| 3 | Le semer derrière des troncs | Il réapparaît derrière soi, hors de vue | ⬜ |
| 4 | Aller dans la brume, chercher une fougère mouillée | Plus sombre, un peu brillante, une goutte tombe de temps en temps ; contour crème ; visée : contour cyan et « Il faut un Champonge » au-dessus ; E ne fait rien. Une fougère sèche : verte, rien ne tombe | ⬜ |
| 5 | S'approcher à moins de 10 m d'une fougère mouillée | Le Champonge y va seul, son chapeau devient sombre et un peu brillant, la fougère redevient verte et ne goutte plus, il revient | ⬜ |
| 6 | Viser le Champonge mouillé, appui court sur E | « +1 Eau » au-dessus de la barre rapide, le chapeau s'éclaircit en 1 s | ⬜ |
| 7 | Maintenir E sur le Champonge | Le cercle se remplit autour de la touche ; à la fin, il revient dans l'inventaire. Relâché avant la fin : rien | ⬜ |
| 8 | Attendre près de fougères sèches environ 2 minutes | Certaines se mouillent, pas toutes en même temps | ⬜ |
| 9 | Noter les FPS dans la brume avec le Champonge sorti | 120 FPS ou plus sur la machine de référence | ⬜ |
| 10 | Passer en anglais | « Water », "Needs a Champonge", "Take out", "Squeeze", "Hold: take back" | ⬜ |
| 11 | Le Champonge sauvage de la clairière | Même rendu qu'avant, chapeau compris (le modèle a un matériau de plus) | ⬜ |
| 12 | Parcourir les zones B et C | Aucune fougère de décor ; les seules fougères sont les fougères à eau, dans la brume | ⬜ |

## Limitations connues

- Rien n'a été joué par un humain : l'air mouillé de la fougère et du chapeau, la goutte qui tombe, le cercle et le texte n'ont été vérifiés que par les tests (valeurs), pas à l'œil. Les réglages (teinte, brillant, taille et fréquence des gouttes) sont à ajuster en jeu.
- Le Champonge peut rester quelques secondes contre un tronc avant de réapparaître : pas de NavMesh.
- Les FPS dans la brume n'ont pas été mesurés avec les fougères à eau (environ 140 dans la bande 580 × 180 m).
- Rien n'a été testé dans un exécutable.

## Modifié par

- [boucle-playtest](./boucle-playtest.md) — Champonge sauvages dans la brume, qui vont boire aux fougères mouillées ; aspect mouillé partagé avec le compagnon (`WetLook`).
