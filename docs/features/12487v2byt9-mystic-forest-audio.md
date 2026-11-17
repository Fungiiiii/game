# Forêt mystique — sons d'ambiance : vent, rafales, arbre géant, pierres dressées

- **Ticket ClickUp :** https://app.clickup.com/t/12487v2byt9 (ticket de la Forêt mystique ; les sons n'ont pas de tâche à eux)
- **Note de design validée :** aucune. Le brouillon a été remis à Hugo le 17/11, sans sous-tâche de validation. Hugo a demandé le 17/11 de brancher les sons pour les juger en jeu, puis d'ouvrir la PR : écart assumé au processus.
- **Branche :** `feat/12487v2byt9-mystic-forest-audio`
- **Auteur :** Hugo Bernier
- **Statut :** En cours

## Docs précédents

- [12487v2byt9-mystic-forest](./12487v2byt9-mystic-forest.md) — la scène n'est plus muette : un objet `Audio` s'ajoute à `MysticForest.unity`. Rien d'autre ne change dans la scène.

## Ce que fait la feature

- [x] Au spawn et dans la zone A, on entend un vent léger et lent : le fond de lisière, plus bas que les deux autres.
- [x] Dans la zone B, le fond devient un vent sourd. Dans la zone C, un vent grave et lointain.
- [x] On passe d'un fond à l'autre en marchant, sans coupure ni saut de volume : le fondu se fait sur 50 m entre A et B (z de −120 à −70), sur 40 m entre B et C (z de 130 à 170).
- [x] Partout, une rafale passe toutes les 25 à 70 s. Jamais deux fois la même de suite.
- [x] L'arbre géant émet un son grave qui enfle et retombe. On l'entend à partir de 60 m, de plus en plus fort en approchant.
- [x] Les pierres dressées émettent une note grave qui monte puis s'éteint, audible à partir de 40 m.

## Hors périmètre

- Les cris d'oiseaux et les craquements de tronc. C'est eux qui doivent porter l'idée de la liste : la vie s'éteint vers le nord. Ils ne se synthétisent pas correctement : il faudra des enregistrements.
- Les règles de silence autour des lieux, qui ne servent qu'avec ces sons.
- Le creux, l'amas de rochers, les spores.
- La musique, les pas du joueur.
- Le réglage du volume par le joueur.
- Le vent sur les arbres : ils ne bougent toujours pas.

La liste complète des sons, la carte et le brief musique : https://claude.ai/artifact/717PBut7xUi87ENWmVih2i

## Implémentation

**Scène :** `Assets/Scenes/Biomes/MysticForest.unity`, un objet racine `Audio` (pas `EditorOnly` : il doit rester dans le jeu).

| Objet | Rôle |
|---|---|
| `Audio/Beds` | `ZoneAmbience`, qui écoute la position de `Main Camera`. Volumes : sud 0,25, centre 0,5, nord 0,5. |
| `Audio/Beds/Bed_South`, `Bed_Middle`, `Bed_North` | `AudioSource` 2D en boucle, une par fond. |
| `Audio/Gusts` | `AudioSource` 2D et `RandomOneShotEmitter` : 2 rafales, intervalle de 25 à 70 s, volume de 0,5 à 0,8. |
| `Audio/Places/GiantTree` | `AudioSource` 3D en boucle en (25, −105), 3 m au-dessus du sol. Atténuation linéaire de 8 à 60 m, volume 1. |
| `Audio/Places/StandingStones` | `AudioSource` 3D en boucle en (−60, 215), 2 m au-dessus du sol. Atténuation linéaire de 6 à 40 m, volume 0,5. |

**Sons** (`Assets/Audio/MysticForest/`, WAV 48 kHz 16 bits, LFS) :

| Fichier | Durée | Canaux |
|---|---|---|
| `AMB_MysticForest_ZoneA`, `ZoneB`, `ZoneC` | 60 s, en boucle | stéréo, Vorbis compressé en mémoire |
| `AMB_MysticForest_GiantTree` | 32 s, en boucle | mono |
| `AMB_MysticForest_StandingStones` | 16 s, en boucle | mono |
| `SFX_MysticForest_Gust_01`, `_02` | 7 et 8 s | stéréo |

Ils sont fabriqués par `tools/audio/generate_mystic_forest_ambience.py` (Python, numpy, scipy) : du bruit filtré par bandes et quelques notes pures, sans aucun enregistrement. Le script redonne exactement les mêmes fichiers :

```bash
python tools/audio/generate_mystic_forest_ambience.py Assets/Audio/MysticForest
```

**Scripts :**

| Fichier | Rôle |
|---|---|
| `Runtime/Audio/ZoneMix.cs` | Calcul pur des trois volumes selon la position nord-sud. |
| `Runtime/Audio/ZoneAmbience.cs` | Applique ce calcul aux trois sources, à chaque image. |
| `Runtime/Audio/RandomOneShotEmitter.cs` | Joue un son au hasard après une attente tirée au hasard. |

## Décisions

- **Les zones sont des bandes nord-sud : le volume des fonds suit la seule coordonnée Z du joueur.** Pas de zones de déclenchement à poser ni à maintenir.
- **Fondu à puissance constante** (la somme des carrés des volumes vaut 1) : avec un fondu linéaire, le volume baissait au milieu du passage.
- **Un volume par fond.** Hugo, en jeu le 17/11 : le vent du spawn était fort et rapide alors que rien ne bouge, et il couvrait l'arbre géant. Le fond de lisière a été refait plus lent, et joue deux fois moins fort que les autres.
- **Sons de lieu : des `AudioSource` 3D sans code**, à atténuation linéaire, pour qu'ils s'éteignent à une distance connue.
- **Les sources sont sous un objet `Audio`, pas sous `Clearings`**, qui est `EditorOnly` et absent du jeu.
- **Sons synthétisés par script** plutôt que tirés d'une banque : pas de licence à vérifier, boucles sans raccord par construction, et chaque son se refait en changeant une ligne. Le vent s'y prête ; les oiseaux, les craquements de bois et le scintillement des spores ont été essayés ou écartés, la synthèse ne les rend pas.
- **Les boucles sont construites en fréquence, sur un nombre entier de périodes** : le dernier échantillon rejoint le premier, sans fondu de raccord.
- **Pas d'AudioMixer.** Il n'y a pas encore de menu d'options qui en aurait besoin.
- **Aucun texte ni parole** : rien à localiser.

## Tests

### Automatisés (Unity Test Runner)

Lancés en batchmode avec Unity 6000.3.21f1 sur la machine de Hugo le 17/11, sur la vraie scène et après le dernier changement. Log et XML lus : **EditMode 90/90, PlayMode 19/19**, aucune erreur de compilation.

| Test | Mode | Couvre | Résultat |
|---|---|---|---|
| `OnlyTheSouthBedAtTheSpawn`, `OnlyTheMiddleBedInTheMiddle`, `OnlyTheNorthBedAtTheStandingStones` | EditMode | un seul fond hors des fondus | ✅ |
| `TwoBedsShareAFadeEqually` | EditMode | fondu entre deux fonds | ✅ |
| `LoudnessStaysEvenFromSouthToNorth` | EditMode | pas de saut de volume du sud au nord | ✅ |
| `NeverTheSameClipTwiceInARow`, `EveryOtherClipCanBePicked`, `ASingleClipIsAlwaysPicked` | EditMode | tirage des rafales | ✅ |
| `OnlyTheSouthBedIsHeardAtTheSpawn` | PlayMode | au spawn, seul le fond de lisière a du volume | ✅ |
| `EverySoundHasItsClipAndLoopsWhereItShould` | PlayMode | chaque source a son clip, les fonds et les lieux bouclent | ✅ |

Aucun test ne vérifie ce qu'on entend.

### Manuels (dans Unity)

Au casque, bouton de son de la fenêtre Game activé.

| # | Étapes | Résultat attendu | OK |
|---|---|---|---|
| 1 | Ouvrir `Assets/Scenes/Biomes/MysticForest.unity`, Play, rester au spawn 30 s. | Un vent léger et lent, discret. Aucune erreur dans la Console. | |
| 2 | Attendre au plus 70 s. | Une rafale passe, puis le calme revient. | |
| 3 | Marcher vers l'arbre géant. | À environ 60 m, un son grave apparaît. Il enfle en approchant et reste audible par-dessus le vent. | |
| 4 | Continuer vers le nord jusqu'en zone B. | Le vent devient plus sourd, sans coupure ni saut de volume. | |
| 5 | Suivre le chemin jusqu'aux pierres dressées. | Le vent devient grave et lointain. À environ 40 m des pierres, une note grave monte puis s'éteint, en boucle. | |
| 6 | Rester 5 min dans une zone. | Aucun raccord audible dans la boucle, aucun détail qui revient. | |
| 7 | Fenêtre **Stats**, au spawn et en zone B. | FPS inchangés par rapport à la scène sans son. Objectif : 120 FPS ou plus sur RX 6800 XT + i5-12400F. | |

## Limitations connues

- Écouté en jeu par Hugo le 17/11 dans la copie prototype : fonds, rafales et arbre géant jugés bons. Les pierres dressées n'ont pas été jugées.
- Les volumes sont réglés à l'oreille par une seule personne, sur un seul casque.
- Un vent audible alors que les arbres ne bougent pas : Hugo l'a relevé. Le fond du spawn a été calmé, mais l'écart reste.
- Les fonds font 60 s. La répétition n'a pas été repérée, mais personne n'a écouté plus de quelques minutes.
- Les rafales ne sont pas calées sur les rafales visibles dans l'herbe.
- Sans oiseaux ni craquements, les trois zones ne se distinguent que par le timbre du vent.
- Les sons sont synthétisés. La Charte IA demande une décision explicite de l'équipe sur l'audio du jeu ; elle n'y est pas encore inscrite.
- 7 fichiers WAV, environ 42 Mo dans LFS.

## Modifié par
