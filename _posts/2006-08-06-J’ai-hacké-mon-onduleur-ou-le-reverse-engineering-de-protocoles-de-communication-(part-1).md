---
layout: post
title:  "J’ai hacké mon onduleur ou le reverse engineering de protocoles de communication (part 1)"
date: 2006-08-06 15:40:00 +0100
---
C’est un titre un peu long mais bon, il exprime le fond de mon problème : j’ai acheté un onduleur (un [Belkin](http://www.belkin.com/)) pour mettre sur mon serveur et un de mes PC à la maison. Par contre, faudra être un peu patient car il va y avoir plusieurs post sur le sujet. 

Le truc, c’est que l’application de gestion livrée avec est… comment dire… sans dire de gros mots… un peu vieillissante et inutilisable pour faire de l’administration. Une vieille appli au look de la fin des années 90, pas d’accès à distance, pas de SNMP, pas de page Web, bref rien :-(. Pour ne pas polémiquer, je préfère ne pas mettre de capture d’écran. Mais bon, pour les curieux, je vous les tiens à disposition… 

Du coup, je me suis lancé dans la recherche d’application un peu mieux. Au bout de quelques heures de recherche, quelques applications téléchargées et installées, je m’aperçois que tous les onduleurs ont un protocole de communication totalement différent. Les logiciels d’un constructeur ne fonctionnent qu’avec leur propre onduleur. Les applications sont d’ailleurs toutes très variées, seules les versions pro proposent du SNMP et quasi pas de soft qui permette d’avoir un état dans une page Web. 

Je me suis donc rabattu sur le service « Onduleur » de Windows (on le trouve dans « Gestion d’énergie » sauf sur les portables). Et là, pas mieux. Un service de base où mis à part si l’on possède un [APC](http://www.apc.com/index.cfm?ISOCountryCode=fr), il n’y a pas grand-chose. Visiblement n’importe quel onduleur possède des instructions minimums qui permettent de vérifier son état : sur secteur, sur batterie, batterie faible. Les possibilités sont très limitées : arrêter la machine et exécuter un programme. Point barre. 

Je suis donc parti sur l’écriture d’un programme en ligne de commande qui permette d’envoyer un mail en SMTP. Cela me permettra en cas de panne électrique d’être alerté par mail de la coupure de courant. Mieux que rien. Je me suis dit que ça allait me faire du bien, presque 2 ans et demi sans écrire une ligne de code. Ca va me dérouiller un peu. 

Côté outil, je pars donc sur [Visual Basic Express 2005](http://www.microsoft.com/france/msdn/vstudio/express/vbasicexpress.mspx). Une solution parfaite pour m’y remettre. Parfaite car j’adore Visual Basic (je « parle » aussi C/C++,Java, Pascal, Fortran, C#, ADA et « baragouine un peu » en Eiffel, Python, PhP). Parfaite aussi pour les étudiants, les débutants et les passionnés : le produit est gratuit, permet de développer en .NET 2.0 et offre des possibilités de recherche super sympa (j’y reviendrais plus loin). Après 15 minutes de téléchargement (OK, j’ai une bonne ligne ADSL), l’installation se passe sans encombre. Je prends évidemment la librairie MSDN avec moi. Pas besoin de [SQL Express](http://www.microsoft.com/france/msdn/vstudio/express/sqlexpress.mspx) pour mon besoin (pour l’instant). SQL Express est une base de données, basée sur SQL Server et gratuite. SQL Express est idéal pour les développements, les tests, une utilisation en production avec quelques utilisateurs simultanés. 

Evidemment, [j’active](http://www.microsoft.com/france/msdn/vstudio/express/register.mspx) mon Visual Basic Express 2005, qui permet d’avoir accès à de nombreux avantages : 


  * Corbis Image Pack — Un assortiment de 250 images Corbis gratuites à inclure dans vos sites Web et vos applications   
    
  * La suite IconBuffet Studio Edition Icon — Une collection de plus de 100 icônes IconBuffet.com, professionnelles et gratuites   
    
  * Une variété de composants — Une sélection de composants à utiliser dans vos applications   
    
  * Des livres électroniques et des articles — Des livres électroniques Microsoft Press complets ainsi que des articles intéressants pour les débutants, les amateurs, et les personnes essayant les outils de développement Microsoft pour la première fois.   
    D’ailleurs en ce moment, il y a un [jeu Activ'Express avec 20 lots à gagner chaque mois](http://www.microsoft.com/france/msdn/vstudio/express/jeu/default.mspx). Bref aucune excuse pour ne pas activer ses version de Visual Studio Express. 

Côté développement, je me dis que j’allais commencer par chercher dans l’aide. Toujours commencer par réfléchir avant de coder :-). D’ailleurs la bonne règle, c’est autant de temps à réfléchir qu’à coder. Un nouveau menu est présent « Communauté » et « Recherche dans les communautés ». A l’intérieur, on trouve des recherches sur des exemples. Je lance donc la recherche qui m’ouvre une nouvelle fenêtre, je tape « envoi mail smtp ». J’obtiens de nombreux résultats dont un très bien sur MSDN intitulé « [Envoi de courrier, exemple](http://msdn2.microsoft.com/fr-fr/library/ms173026.aspx) » en ce qui concerne Microsoft et un autre très bien aussi sur le réseau [Codes-Sources](http://www.codes-sources.com) (superbement piloté par mon ami [Nix](blogs.developpeur.org/nix)) intitulé « [VB.NET envoi de mail pas SMTP avec authentification](http://www.vbfrance.com/code.aspx?ID=28622) ». 

Résultat : avec ces deux exemples, en moins de 10 minutes, j’avais fait mon programme. Très cool côté productivité, moins cool côté « me remettre à faire un peu de code ». Du coup, c’est là que me vient l’idée d’aller plus loin et de réécrire un logiciel qui permette de gérer mon onduleur ! Mon onduleur possède deux ports de communication : série et USB. Je me lance pour série, je connais bien… 

Cela me rappellera mon jeune temps de développeur. J’étais à l’époque étudiant à [l’ENSAM](http://www.ensam.fr/sommaire/rubrique_sommaire.htm) (les Arts et Métiers). Je passais mon temps libre et mes vacances à travailler pour la société [SAMx](http://www.samx.com/). Cet éditeur de logiciel est spécialisé dans les logiciels d’analyse scientifique. Leurs logiciels communiquent avec des microsondes et spectromètres électroniques. La plupart de ces appareils à l’époque ne communiquait qu’en port série et/ou parallèle. Je me souviens d’un développement où l’objectif était de récupérer des données (des images représentant des cartographies) depuis des [PDP](http://histoire.info.online.fr/pdp11.html) vers des PC (à l’époque en 1996 sous Windows 95). C’était l’époque des premiers ports parallèle [EPP et ECP](http://en.wikipedia.org/wiki/IEEE_1284). La technologie n’était tellement pas sèche qu’il m’a fallut développer un dongle spécifique pour réussir à fonctionner avec l’ensemble des chipsets disponibles sur le marché. J’ai appris à cette époque à vraiment me servir d’un oscilloscope… Au final, la solution fonctionnait parfaitement avec un port série sur lequel les informations de commande étaient envoyées et un port parallèle sur lequel les informations étaient reçues. Pour les autres développements, je n’avais que rarement accès aux matériels (une microsonde, cela prend une pièce complète, SAMx n’en possédait pas, nous allions chez des clients faire les test) et je devait donc d’une part bien développer en respectant les protocoles de communication (documentés dans des superbes docs comme celles des standards) et d’autre part bien émuler les tests ! C’est à cette époque que la [VT100](http://en.wikipedia.org/wiki/Vt100) est devenue une de mes meilleure amie. 

De là, j’ai gardé de très bons restes. Restes que j’ai appliqués à mon onduleur. Et vous comprendrez le titre de ce post au prochain post :-) 



