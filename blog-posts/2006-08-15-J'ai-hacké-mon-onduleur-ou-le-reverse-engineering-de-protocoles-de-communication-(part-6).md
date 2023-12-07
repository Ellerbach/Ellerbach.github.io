# 2006-08-15 J'ai hacké mon onduleur ou le reverse engineering de protocoles de communication (part 6)

Voici la suite des aventures de l'analyse du protocole de communication expliqué dans la [quatrième partie](./2006-08-09-J'ai-hacké-mon-onduleur-ou-le-reverse-engineering-de-protocoles-de-communication-(part-4).md). J'ai donc essayé à partir des commandes envoyées à l'onduleur de compléter ce que j'ai trouvé en écoutant la conversation entre l'application livrée avec l'onduleur et l'onduleur. J'ai donc utilisé l'application développée précédemment pour envoyer les commandes. Je notais le résultat de chaque commande. Ensuite, j'ai essayé à partir de l'application livrée avec et des informations renvoyées de déchiffrer les résultats. Avoir une application existante aide grandement, cela facilite la comparaison des résultats. Sinon, il faut essayer de deviner par tâtonnement. Moins drôle. Il me reste quelques zones d'incertitude cependant que je vais devoir approfondir. Pour les commandes que je n'avais pas (exemple S), j'ai tout simplement envoyé chaque lettre de l'alphabet et j'ai attendu le retour. La plupart du temps, c'est un écho, assez classique dans les protocoles. Quand rien ne revenait, j'analysais l'onduleur lui-même. Parfois rien ne se passait du tout (exemple envoi de la commande Q). Cela impliquait qu'il pouvait manquer une lettre/chiffre derrière ou que le mode était déjà enclenché (exemple commande C quand on est déjà en courant continue).

## Commande I

 Renvoie l'identifiant de l'onduleur : #BELKIN Master 1.00

 1 caractère # qui identifie le début de la chaîne
 16 caractères qui contiennent la marque de l'onduleur, ici BELKIN
 11 caractères qui contiennent le modèle de l'onduleur, ici Master
 11 caractères qui contiennent la version du firmware ici 1.00

## Commande S

Arrête l'onduleur. Attention, si l'onduleur est alimenté en courant il se remet en marche automatiquement. Commande à utiliser donc de préférence lors d'une coupure de courant si besoin.

## Commande T

Passe l'onduleur en mode batterie test pendant 10s.

## Commande C

Repasse l'onduleur en mode alimentation par secteur. Utile lorsqu'on force l'onduleur à se mettre en mode batterie avec la commande TL ou T. Pas de différence visible avec la commande CT.

## Commande TL

Passe l'onduleur en mode batterie.

## Commande CT

Repasse l'onduleur en mode alimentation par secteur. Voir commande C.

## Commande Q1

Renvoie un statut des tensions, fréquences, températures et charges : (238.0 237.0 236.0 024 50.0 13.9 32.0 00001000

238.0 = Voltage d'alimentation = 238.0 V
237.0 = Voltage tension de sortie primaire ( ?) = 237.0 V
236.0 = Voltage tension de sortie secondaire ( ?) = 236.0 V
024 = charge de l'onduleur en %
50.0 = fréquence en Hertz
13.9 = Charge de la batterie (?), reste à déterminer l'échelle, a priori 14.
32.0 = température en °C
00001000 = statut. 1 bit par info. Pas encore déterminé l'utilité de tous les bits. Tests à effectuer

## Commande F

Renvoie un statut de la batterie : #230.0 2.2 12.00 50.0

230.0 = Voltage de la sortie batterie
2.2 = courant en ampère
12.00 = tension de la batterie
50.0 = Fréquence de sortie

Je suis content de ce premier jet de résultat qu'il me reste à approfondir avec quelques tests complémentaires. Cela vade toute façon me permettre d'avancer rapidement dans l'écriture d'une classe de gestion de l'onduleur. A suivre au prochain post !
