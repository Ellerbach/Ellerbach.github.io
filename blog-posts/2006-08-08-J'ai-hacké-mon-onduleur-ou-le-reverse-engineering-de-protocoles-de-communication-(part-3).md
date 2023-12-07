# 2006-08-08 J'ai hacké mon onduleur ou le reverse engineering de protocoles de communication (part 3)

Pour faire suite aux deux premiers article sur le sujet, je vais rentrer un peu dans le dur de l'analyse du protocole de communication de mon onduleur. Pour mémoire, je me suis arrêté en disant que j'avais lancé [PortMon](https://www.sysinternals.com/Utilities/Portmon.html) et que j'avais vu plein de choses intéressantes. Voici donc les résultats :

```text
0 0.00004426 RupsMon.exe IRP_MJ_CREATE Serial0 SUCCESS Options: Open 
1 0.00000178 RupsMon.exe IOCTL_SERIAL_SET_TIMEOUTS Serial0 SUCCESS RI:-1 RM:5 RC:80 WM:0 WC:0 
2 0.00000091 RupsMon.exe IOCTL_SERIAL_GET_BAUD_RATE Serial0 SUCCESS 
3 0.00000104 RupsMon.exe IOCTL_SERIAL_GET_LINE_CONTROL Serial0 SUCCESS 
4 0.00000083 RupsMon.exe IOCTL_SERIAL_GET_CHARS Serial0 SUCCESS 
5 0.00000100 RupsMon.exe IOCTL_SERIAL_GET_HANDFLOW Serial0 SUCCESS 
6 0.00000087 RupsMon.exe IOCTL_SERIAL_GET_BAUD_RATE Serial0 SUCCESS 
7 0.00000082 RupsMon.exe IOCTL_SERIAL_GET_LINE_CONTROL Serial0 SUCCESS 
8 0.00000083 RupsMon.exe IOCTL_SERIAL_GET_CHARS Serial0 SUCCESS 
9 0.00000089 RupsMon.exe IOCTL_SERIAL_GET_HANDFLOW Serial0 SUCCESS 
10 0.00000689 RupsMon.exe IOCTL_SERIAL_SET_BAUD_RATE Serial0 SUCCESS Rate: 2400 
11 0.00000391 RupsMon.exe IOCTL_SERIAL_SET_RTS Serial0 SUCCESS 
12 0.00000351 RupsMon.exe IOCTL_SERIAL_SET_DTR Serial0 SUCCESS 
13 0.00000250 RupsMon.exe IOCTL_SERIAL_SET_LINE_CONTROL Serial0 SUCCESS StopBits: 1 Parity: NONE WordLength: 8 
14 0.00000124 RupsMon.exe IOCTL_SERIAL_SET_CHAR Serial0 SUCCESS EOF:0 ERR:0 BRK:0 EVT:d XON:11 XOFF:13 
15 0.00000238 RupsMon.exe IOCTL_SERIAL_SET_HANDFLOW Serial0 SUCCESS Shake:1 Replace:40 XonLimit:2048 XoffLimit:512 
16 0.00000225 RupsMon.exe IOCTL_SERIAL_SET_WAIT_MASK Serial0 SUCCESS Mask: RXCHAR 
17 0.00000260 RupsMon.exe IOCTL_SERIAL_SET_WAIT_MASK Serial0 SUCCESS Mask: RXFLAG 
```

Alors, comment lire ces résultats ? Et bien c'est simple, à l'ouverture du port série, PortMon intercepte les appels au port série. On y voit un _IOCTL_SERIAL_SET_BAUD_RATE Serial0 SUCCESS Rate: 2400_ qui nous indique que le port a été ouvert à 2400 baud. En lisant les quelques lignes qui suivent, on trouve les autres informations :

* 1 bit de stop
* Pas de parité
* Un mot de 8 bits
    Nous avons donc la première information nécessaire pour communiquer avec l'onduleur. Nous avons eu pas mal de chance car il est possible que les appels ne passent pas en mode user et donc l'espionnage du port série avec un outil tel que PortMon ne fonctionne pas. C'est par exemple le cas du service Windows onduleur. Alors comment faire dans ces cas là ? Et bien, il faut ressortir l'oscilloscope. L'oscilloquoi ? L'oscilloscope. Un instrument qui permet d'analyser des signaux, le truc que l'on utilise en TP au collège. Le principe est simple, il suffit de brancher une des bornes de l'oscilloscope sur la masse et l'autre sur une des bornes de communication. Ensuite, il faut faire parler entre eux les appareils (dans notre cas l'ordinateur et l'onduleur) et capturer les signaux (les oscilloscopes modernes permettent tous de le faire). Ensuite, il suffit d'analyser la longueur d'un bit ou de plusieurs bits et d'en déduire la fréquence. Je vais expliquer comment faire un peu plus loin.

A défaut d'avoir un onduleur, il est possible de capturer les signaux sur une carte son par exemple. Le montage est simple et fonctionne également. Il suffit pour cela de faire la même chose qu'avec l'oscilloscope (la masse de l'entrée micro sur la masse du port série et la borne 2 sur l'entrée gauche et la borne 3 sur l'entrée droite par exemple. Ensuite, il suffit d'enregistrer avec n'importe quel logiciel de capture de son ce qui passe sur le micro. Attention, il faut fixer le gain plutôt faible car 5 volts (tension de sortie du port série) est assez important. Normalement, pas de problème côté carte son mais bon, mieux vaut être prévoyant pour ne pas abîmer le matériel. Au pire, le signal « clipera » (saturera) mais ce n'est pas important dans notre cas, nous essayons seulement de capturer des bits donc des 0 et des 1. Peu importe le niveau de crête. L'analyse se passe de la même façon qu'avec l'oscilloscope. La période de plusieurs bits donne la fréquence. Pour la calculer, c'est simple, il faut juste connaître la fréquence d'échantillonnage (comme sur l'oscilloscope). Imaginons que l'on capture un signal à 44KHz, la période est donc de 2,3 10^-5 secondes. Avec un logiciel permettant de zoomer suffisamment, sur le signal acquis, il est possible de calculer la période du signal. Imaginons que nous ayons 18 points entre deux crêtes, cela nous donne donc une période de 4,09 10^-4 secondes et donc une fréquence de 2440 Hz (rappel, fréquence en Hertz = 1 / période en seconde). Et donc on retombe facilement sur 2400 Hz. Evidemment, la vrai vie, c'est un petit peu plus compliqué que cette théorie. En général, on est obligé de prendre plusieurs bits, de prier pour que les bits ne soient pas doublés (ça peut arrive avec beaucoup de malchance) car sinon la fréquence est multipliée par deux.

Avec un peu d'expérience, on y arrive assez facilement. Avec cette méthode, une fois la fréquence déterminée, il est possible de déterminer la parité (il faut plusieurs mots différents pour être sûr), le nombre de bits de stop et aussi la longueur d'un mot. Ca peut prendre un peu de temps et il faut faire dialoguer les appareils entre eux pas mal de fois pour être sûr de ne pas se tromper. Et c'est là que la norme complète aide beaucoup :-) J'ai eu à faire ce type de manipulation, lorsque je développait [l'application de récupération d'information du PDP](./2006-08-06-J'ai-hacké-mon-onduleur-ou-le-reverse-engineering-de-protocoles-de-communication-(part-1).md). C'était sur le port parallèle. Croyez-moi, ça forme 😊 Une bonne expérience que l'on devrait faire faire à tous les étudiants en TP d'informatique/électronique. En plus, les tests après permettent de valider les informations trouvées par cette expérience.

Maintenant, j'ai toutes les informations nécessaires pour me connecter à l'onduleur, il me faut maintenant déterminer son protocole pour pouvoir dialoguer avec lui. La suite au prochain post !
